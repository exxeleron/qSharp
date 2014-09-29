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
        private const string PROTOCOL_DEBUG_ENV = "QSHARP_PROTOCOL_DEBUG";

        private readonly Encoding encoding;
        private readonly Stream stream;

        private byte[] header;
        private byte[] rawData;
        private EndianBinaryReader reader;

        private int maxReadingChunk;

        /// <summary>
        ///     Initializes a new QReader instance.
        /// </summary>
        /// <param name="stream">Input stream containing serialized messages</param>
        /// <param name="encoding">Encoding used for deserialization of string data</param>
        /// <param name="maxReadingChunk">Maxium number of bytes read in a single chunk from stream</param>
        public QReader(Stream stream, Encoding encoding, int maxReadingChunk)
        {
            this.stream = stream;
            this.encoding = encoding;
            this.maxReadingChunk = maxReadingChunk;
        }

        /// <summary>
        ///     Reads next message from the stream and returns a deserialized object.
        /// </summary>
        /// <param name="raw">indicates whether reply should be parsed or return as raw data</param>
        /// <returns>QMessage instance encapsulating a deserialized message.</returns>
        public QMessage Read(bool raw = false)
        {
            header = ReadFully(8);
            Endianess endianess = (Endianess)header[0];
            MessageType messageType = (MessageType)header[1];
            bool compressed = header[2] == 1;

            reader = new EndianBinaryReader(header) { Endianess = endianess };
            reader.Seek(4, SeekOrigin.Begin);
            int messageSize = reader.ReadInt32();
            int dataSize = Math.Max(messageSize - 8, 0);

            rawData = ReadFully(dataSize);
            if (raw)
            {
                return new QMessage(rawData, messageType, endianess, compressed, raw, messageSize, dataSize);
            }

            var data = rawData;
            if (compressed)
            {
                data = Uncompress(rawData, endianess);
                dataSize = data.Length;
            }

            reader = new EndianBinaryReader(data, endianess);

            try
            {
                return new QMessage(ReadObject(), messageType, endianess, compressed, raw, messageSize, dataSize);
            }
            catch (QReaderException e)
            {
                ProtocolDebug(e);
                throw e;
            }
            catch (QException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                ProtocolDebug(e);
                throw e;
            }
        }

        private void ProtocolDebug(Exception e)
        {
            var debugPath = System.Environment.GetEnvironmentVariable(PROTOCOL_DEBUG_ENV);
            if (!string.IsNullOrEmpty(debugPath) && debugPath.Trim() != "")
            {
                debugPath += "\\" + PROTOCOL_DEBUG_ENV + "." + DateTime.UtcNow.Ticks;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(debugPath))
                {
                    file.Write(BitConverter.ToString(header).Replace("-", ""));
                    file.WriteLine(BitConverter.ToString(rawData).Replace("-", ""));
                    file.WriteLine(e.ToString());
                }
            }
        }

        private byte[] ReadFully(int length)
        {
            var buffer = new byte[length];

            int read = 0;
            int chunk;

            while ((chunk = stream.Read(buffer, read, Math.Min(maxReadingChunk, buffer.Length - read))) > 0)
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
            int uncompressedSize = -8 +
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
            int n = 0;
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
                    int r = buffer[0xff & compressed[d++]];
                    uncompressed[s++] = uncompressed[r++];
                    uncompressed[s++] = uncompressed[r++];
                    n = 0xff & compressed[d++];
                    for (int m = 0; m < n; m++)
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
            QType qtype = (QType)reader.ReadSByte();

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
                    return reader.ReadBoolean();
                case QType.Byte:
                    return reader.ReadByte();
                case QType.Guid:
                    return reader.ReadGuid();
                case QType.Short:
                    return reader.ReadInt16();
                case QType.Int:
                    return reader.ReadInt32();
                case QType.Long:
                    return reader.ReadInt64();
                case QType.Float:
                    return reader.ReadSingle();
                case QType.Double:
                    return reader.ReadDouble();
                case QType.Char:
                    return reader.ReadChar();
                case QType.Timestamp:
                    return new QTimestamp(reader.ReadInt64());
                case QType.Month:
                    return new QMonth(reader.ReadInt32());
                case QType.Date:
                    return new QDate(reader.ReadInt32());
                case QType.Datetime:
                    return new QDateTime(reader.ReadDouble());
                case QType.Timespan:
                    return new QTimespan(reader.ReadInt64());
                case QType.Minute:
                    return new QMinute(reader.ReadInt32());
                case QType.Second:
                    return new QSecond(reader.ReadInt32());
                case QType.Time:
                    return new QTime(reader.ReadInt32());
            }

            throw new QReaderException("Unable to deserialize q atom of type: " + qtype);
        }

        private object[] ReadGeneralList()
        {
            reader.ReadByte(); // attributes
            int length = reader.ReadInt32();
            var list = new object[length];

            for (int i = 0; i < length; i++)
            {
                list[i] = ReadObject();
            }

            return list;
        }

        private object ReadList(QType qtype)
        {
            reader.ReadByte(); // attributes
            int length = reader.ReadInt32();

            switch (qtype)
            {
                case QType.BoolList:
                    {
                        var list = new bool[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadBoolean();
                        }
                        return list;
                    }
                case QType.ByteList:
                    {
                        var list = new byte[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadByte();
                        }
                        return list;
                    }
                case QType.GuidList:
                    {
                        var list = new Guid[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadGuid();
                        }
                        return list;
                    }
                case QType.ShortList:
                    {
                        var list = new short[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadInt16();
                        }
                        return list;
                    }
                case QType.IntList:
                    {
                        var list = new int[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadInt32();
                        }
                        return list;
                    }
                case QType.LongList:
                    {
                        var list = new long[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadInt64();
                        }
                        return list;
                    }
                case QType.FloatList:
                    {
                        var list = new float[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadSingle();
                        }
                        return list;
                    }
                case QType.DoubleList:
                    {
                        var list = new double[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = reader.ReadDouble();
                        }
                        return list;
                    }
                case QType.String:
                    {
                        return encoding.GetChars(reader.ReadBytes(length));
                    }
                case QType.SymbolList:
                    {
                        var list = new string[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = ReadSymbol();
                        }
                        return list;
                    }
                case QType.TimestampList:
                    {
                        var list = new QTimestamp[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QTimestamp(reader.ReadInt64());
                        }
                        return list;
                    }
                case QType.MonthList:
                    {
                        var list = new QMonth[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QMonth(reader.ReadInt32());
                        }
                        return list;
                    }
                case QType.DateList:
                    {
                        var list = new QDate[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QDate(reader.ReadInt32());
                        }
                        return list;
                    }
                case QType.DatetimeList:
                    {
                        var list = new QDateTime[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QDateTime(reader.ReadDouble());
                        }
                        return list;
                    }
                case QType.TimespanList:
                    {
                        var list = new QTimespan[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QTimespan(reader.ReadInt64());
                        }
                        return list;
                    }
                case QType.MinuteList:
                    {
                        var list = new QMinute[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QMinute(reader.ReadInt32());
                        }
                        return list;
                    }
                case QType.SecondList:
                    {
                        var list = new QSecond[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QSecond(reader.ReadInt32());
                        }
                        return list;
                    }
                case QType.TimeList:
                    {
                        var list = new QTime[length];
                        for (int i = 0; i < length; i++)
                        {
                            list[i] = new QTime(reader.ReadInt32());
                        }
                        return list;
                    }
            }
            throw new QReaderException("Unable to deserialize q vector of type: " + qtype);
        }

        private string ReadSymbol()
        {
            return reader.ReadSymbol(encoding);
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
                        reader.ReadSymbol(encoding); // ignore context
                        var expression = (char[])ReadObject();
                        return new QLambda(new string(expression));
                    }
                case QType.Projection:
                    {
                        int length = reader.ReadInt32();
                        var parameters = new object[length];
                        for (int i = 0; i < length; i++)
                        {
                            parameters[i] = ReadObject();
                        }
                        return new QProjection(parameters);
                    }
                case QType.UnaryPrimitiveFunc:
                    {
                        var code = reader.ReadByte();
                        return code == 0 ? null : QFunction.Create((byte)qtype);
                    }
                case QType.BinaryPrimitiveFunc:
                case QType.TernaryOperatorFunc:
                    {
                        var code = reader.ReadByte(); // ignore
                        return QFunction.Create((byte)qtype);
                    }
                case QType.CompositionFunc:
                    {
                        int length = reader.ReadInt32();
                        var parameters = new object[length];
                        for (int i = 0; i < length; i++)
                        {
                            parameters[i] = ReadObject();
                        }
                        return QFunction.Create((byte)qtype);
                    }
                default:
                    {
                        ReadObject(); // ignore
                        return QFunction.Create((byte)qtype);
                    }
            }
        }

        private object ReadDictionary()
        {
            object keys = ReadObject();
            object values = ReadObject();

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
            reader.ReadByte(); // attributes
            reader.ReadByte(); // dict type stamp
            return new QTable(ReadObject() as string[], ReadObject() as Array);
        }
    }
}