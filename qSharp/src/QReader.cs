//
//   Copyright (c) 2011-2014 Exxeleron GmbH
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.IO;
using System.Text;

namespace qSharp
{
    /// <summary>
    ///     Provides deserialization from q IPC protocol.
    ///     Methods of QReader are not thread safe.
    /// </summary>
    public sealed class QReader
    {
        private const string ProtocolDebugEnv = "QSHARP_PROTOCOL_DEBUG";
        private readonly Encoding _encoding;
        private readonly int _maxReadingChunk;
        private readonly Stream _stream;
        private byte[] _header;
        private byte[] _rawData;
        private EndianBinaryReader _reader;

        /// <summary>
        ///     Initializes a new QReader instance.
        /// </summary>
        /// <param name="stream">Input stream containing serialized messages</param>
        /// <param name="encoding">Encoding used for deserialization of string data</param>
        /// <param name="maxReadingChunk">Maxium number of bytes read in a single chunk from stream</param>
        public QReader(Stream stream, Encoding encoding, int maxReadingChunk)
        {
            _stream = stream;
            _encoding = encoding;
            _maxReadingChunk = maxReadingChunk;
        }

        /// <summary>
        ///     Reads next message from the stream and returns a deserialized object.
        /// </summary>
        /// <param name="raw">indicates whether reply should be parsed or return as raw data</param>
        /// <returns>QMessage instance encapsulating a deserialized message.</returns>
        public QMessage Read(bool raw = false)
        {
            _header = ReadFully(8);
            var endianess = (Endianess) _header[0];
            var messageType = (MessageType) _header[1];
            var compressed = _header[2] == 1;

            _reader = new EndianBinaryReader(_header) {Endianess = endianess};
            _reader.Seek(4, SeekOrigin.Begin);
            var messageSize = _reader.ReadInt32();
            var dataSize = Math.Max(messageSize - 8, 0);

            _rawData = ReadFully(dataSize);
            if (raw)
            {
                return new QMessage(_rawData, messageType, endianess, compressed, raw, messageSize, dataSize);
            }

            var data = _rawData;
            if (compressed)
            {
                data = Uncompress(_rawData, endianess);
                dataSize = data.Length;
            }

            _reader = new EndianBinaryReader(data, endianess);

            try
            {
                return new QMessage(ReadObject(), messageType, endianess, compressed, raw, messageSize, dataSize);
            }
            catch (QReaderException e)
            {
                ProtocolDebug(e);
                throw;
            }
            catch (QException)
            {
                throw;
            }
            catch (Exception e)
            {
                ProtocolDebug(e);
                throw;
            }
        }

        private void ProtocolDebug(Exception e)
        {
            var debugPath = Environment.GetEnvironmentVariable(ProtocolDebugEnv);
            if (!string.IsNullOrEmpty(debugPath) && debugPath.Trim() != "")
            {
                debugPath += "\\" + ProtocolDebugEnv + "." + DateTime.UtcNow.Ticks;
                using (var file = new StreamWriter(debugPath))
                {
                    file.Write(BitConverter.ToString(_header).Replace("-", ""));
                    file.WriteLine(BitConverter.ToString(_rawData).Replace("-", ""));
                    file.WriteLine(e.ToString());
                }
            }
        }

        private byte[] ReadFully(int length)
        {
            var buffer = new byte[length];

            var read = 0;
            int chunk;

            while ((chunk = _stream.Read(buffer, read, Math.Min(_maxReadingChunk, buffer.Length - read))) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    break;
                }
            }
            return buffer;
        }

        private byte[] Uncompress(byte[] compressed, Endianess endianess)
        {
            // size of the uncompressed message is encoded on first 4 bytes
            // size has to be decreased by header length (8 bytes)
            var uncompressedSize = -8 +
                                   (int)
                                       (endianess == Endianess.BigEndian
                                           ? EndianBinaryReader.FromBigEndian(compressed, 0, 4)
                                           : EndianBinaryReader.FromLittleEndian(compressed, 0, 4));

            if (uncompressedSize <= 0)
            {
                throw new QReaderException("Error while data uncompression.");
            }

            var uncompressed = new byte[uncompressedSize];
            var buffer = new int[256];
            short i = 0;
            var n = 0;
            int f = 0, s = 0, p = 0, d = 4;

            while (s < uncompressedSize)
            {
                if (i == 0)
                {
                    f = 0xff & compressed[d++];
                    i = 1;
                }
                if ((f & i) != 0)
                {
                    var r = buffer[0xff & compressed[d++]];
                    uncompressed[s++] = uncompressed[r++];
                    uncompressed[s++] = uncompressed[r++];
                    n = 0xff & compressed[d++];
                    for (var m = 0; m < n; m++)
                    {
                        uncompressed[s + m] = uncompressed[r + m];
                    }
                }
                else
                {
                    uncompressed[s++] = compressed[d++];
                }
                while (p < s - 1)
                {
                    buffer[(0xff & uncompressed[p]) ^ (0xff & uncompressed[p + 1])] = p++;
                }
                if ((f & i) != 0)
                {
                    p = s += n;
                }
                i *= 2;
                if (i == 256)
                {
                    i = 0;
                }
            }

            return uncompressed;
        }

        private object ReadObject()
        {
            var qtype = (QType) _reader.ReadSByte();

            switch (qtype)
            {
                case QType.Bool:
                case QType.Byte:
                case QType.Guid:
                case QType.Short:
                case QType.Int:
                case QType.Long:
                case QType.Float:
                case QType.Double:
                case QType.Char:
                case QType.Timestamp:
                case QType.Month:
                case QType.Date:
                case QType.Datetime:
                case QType.Timespan:
                case QType.Minute:
                case QType.Second:
                case QType.Time:
                    return ReadAtom(qtype);
                case QType.Symbol:
                    return ReadSymbol();
                case QType.BoolList:
                case QType.ByteList:
                case QType.GuidList:
                case QType.ShortList:
                case QType.IntList:
                case QType.LongList:
                case QType.FloatList:
                case QType.DoubleList:
                case QType.String:
                case QType.SymbolList:
                case QType.TimestampList:
                case QType.MonthList:
                case QType.DateList:
                case QType.DatetimeList:
                case QType.TimespanList:
                case QType.MinuteList:
                case QType.SecondList:
                case QType.TimeList:
                    return ReadList(qtype);
                case QType.GeneralList:
                    return ReadGeneralList();
                case QType.Error:
                    return ReadError();
                case QType.Lambda:
                case QType.Projection:
                case QType.UnaryPrimitiveFunc:
                case QType.BinaryPrimitiveFunc:
                case QType.TernaryOperatorFunc:
                case QType.CompositionFunc:
                case QType.AdverbFunc106:
                case QType.AdverbFunc107:
                case QType.AdverbFunc108:
                case QType.AdverbFunc109:
                case QType.AdverbFunc110:
                case QType.AdverbFunc111:
                    return ReadFunction(qtype);
                case QType.Dictionary:
                    return ReadDictionary();
                case QType.Table:
                    return ReadTable();
            }

            throw new QReaderException("Unable to deserialize q type: " + qtype);
        }

        private object ReadAtom(QType qtype)
        {
            switch (qtype)
            {
                case QType.Bool:
                    return _reader.ReadBoolean();
                case QType.Byte:
                    return _reader.ReadByte();
                case QType.Guid:
                    return _reader.ReadGuid();
                case QType.Short:
                    return _reader.ReadInt16();
                case QType.Int:
                    return _reader.ReadInt32();
                case QType.Long:
                    return _reader.ReadInt64();
                case QType.Float:
                    return _reader.ReadSingle();
                case QType.Double:
                    return _reader.ReadDouble();
                case QType.Char:
                    return _reader.ReadChar();
                case QType.Timestamp:
                    return new QTimestamp(_reader.ReadInt64());
                case QType.Month:
                    return new QMonth(_reader.ReadInt32());
                case QType.Date:
                    return new QDate(_reader.ReadInt32());
                case QType.Datetime:
                    return new QDateTime(_reader.ReadDouble());
                case QType.Timespan:
                    return new QTimespan(_reader.ReadInt64());
                case QType.Minute:
                    return new QMinute(_reader.ReadInt32());
                case QType.Second:
                    return new QSecond(_reader.ReadInt32());
                case QType.Time:
                    return new QTime(_reader.ReadInt32());
            }

            throw new QReaderException("Unable to deserialize q atom of type: " + qtype);
        }

        private object[] ReadGeneralList()
        {
            _reader.ReadByte(); // attributes
            var length = _reader.ReadInt32();
            var list = new object[length];

            for (var i = 0; i < length; i++)
            {
                list[i] = ReadObject();
            }

            return list;
        }

        private object ReadList(QType qtype)
        {
            _reader.ReadByte(); // attributes
            var length = _reader.ReadInt32();

            switch (qtype)
            {
                case QType.BoolList:
                {
                    var list = new bool[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadBoolean();
                    }
                    return list;
                }
                case QType.ByteList:
                {
                    var list = new byte[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadByte();
                    }
                    return list;
                }
                case QType.GuidList:
                {
                    var list = new Guid[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadGuid();
                    }
                    return list;
                }
                case QType.ShortList:
                {
                    var list = new short[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadInt16();
                    }
                    return list;
                }
                case QType.IntList:
                {
                    var list = new int[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadInt32();
                    }
                    return list;
                }
                case QType.LongList:
                {
                    var list = new long[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadInt64();
                    }
                    return list;
                }
                case QType.FloatList:
                {
                    var list = new float[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadSingle();
                    }
                    return list;
                }
                case QType.DoubleList:
                {
                    var list = new double[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = _reader.ReadDouble();
                    }
                    return list;
                }
                case QType.String:
                {
                    return _encoding.GetChars(_reader.ReadBytes(length));
                }
                case QType.SymbolList:
                {
                    var list = new string[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = ReadSymbol();
                    }
                    return list;
                }
                case QType.TimestampList:
                {
                    var list = new QTimestamp[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QTimestamp(_reader.ReadInt64());
                    }
                    return list;
                }
                case QType.MonthList:
                {
                    var list = new QMonth[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QMonth(_reader.ReadInt32());
                    }
                    return list;
                }
                case QType.DateList:
                {
                    var list = new QDate[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QDate(_reader.ReadInt32());
                    }
                    return list;
                }
                case QType.DatetimeList:
                {
                    var list = new QDateTime[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QDateTime(_reader.ReadDouble());
                    }
                    return list;
                }
                case QType.TimespanList:
                {
                    var list = new QTimespan[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QTimespan(_reader.ReadInt64());
                    }
                    return list;
                }
                case QType.MinuteList:
                {
                    var list = new QMinute[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QMinute(_reader.ReadInt32());
                    }
                    return list;
                }
                case QType.SecondList:
                {
                    var list = new QSecond[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QSecond(_reader.ReadInt32());
                    }
                    return list;
                }
                case QType.TimeList:
                {
                    var list = new QTime[length];
                    for (var i = 0; i < length; i++)
                    {
                        list[i] = new QTime(_reader.ReadInt32());
                    }
                    return list;
                }
            }
            throw new QReaderException("Unable to deserialize q vector of type: " + qtype);
        }

        private string ReadSymbol()
        {
            return _reader.ReadSymbol(_encoding);
        }

        private object ReadError()
        {
            throw new QException(ReadSymbol());
        }

        private QFunction ReadFunction(QType qtype)
        {
            switch (qtype)
            {
                case QType.Lambda:
                {
                    _reader.ReadSymbol(_encoding); // ignore context
                    var expression = (char[]) ReadObject();
                    return new QLambda(new string(expression));
                }
                case QType.Projection:
                {
                    var length = _reader.ReadInt32();
                    var parameters = new object[length];
                    for (var i = 0; i < length; i++)
                    {
                        parameters[i] = ReadObject();
                    }
                    return new QProjection(parameters);
                }
                case QType.UnaryPrimitiveFunc:
                {
                    var code = _reader.ReadByte();
                    return code == 0 ? null : QFunction.Create((byte) qtype);
                }
                case QType.BinaryPrimitiveFunc:
                case QType.TernaryOperatorFunc:
                {
                    var code = _reader.ReadByte(); // ignore
                    return QFunction.Create((byte) qtype);
                }
                case QType.CompositionFunc:
                {
                    var length = _reader.ReadInt32();
                    var parameters = new object[length];
                    for (var i = 0; i < length; i++)
                    {
                        parameters[i] = ReadObject();
                    }
                    return QFunction.Create((byte) qtype);
                }
                default:
                {
                    ReadObject(); // ignore
                    return QFunction.Create((byte) qtype);
                }
            }
        }

        private object ReadDictionary()
        {
            var keys = ReadObject();
            var values = ReadObject();

            if (keys is Array && values is Array)
            {
                return new QDictionary(keys as Array, values as Array);
            }
            if (keys is Array && values is QTable)
            {
                return new QDictionary(keys as Array, values as QTable);
            }
            if (keys is QTable && values is QTable)
            {
                return new QKeyedTable(keys as QTable, values as QTable);
            }

            throw new QReaderException("Cannot create valid dictionary object from mapping: " + keys + " to " + values);
        }

        private QTable ReadTable()
        {
            _reader.ReadByte(); // attributes
            _reader.ReadByte(); // dict type stamp
            return new QTable(ReadObject() as string[], ReadObject() as Array);
        }
    }
}