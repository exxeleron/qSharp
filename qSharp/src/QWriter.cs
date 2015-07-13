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
    ///     Provides serialization to q IPC protocol.
    ///     Methods of QWriter are not thread safe.
    /// </summary>
    public sealed class QWriter
    {
        /// order of uid read/write
        private static readonly int[] GuidByteOrder = {3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15};

        private readonly Encoding _encoding;
        private readonly int _protocolVersion;
        private readonly Stream _stream;
        private int _messageSize;
        private BinaryWriter _writer;

        /// <summary>
        ///     Initializes a new QWriter instance.
        /// </summary>
        /// <param name="stream">Output stream for serialized messages</param>
        /// <param name="encoding">Encoding to be used</param>
        /// <param name="protocolVersion">kdb+ protocol version</param>
        public QWriter(Stream stream, Encoding encoding, int protocolVersion)
        {
            _stream = stream;
            _encoding = encoding;
            _protocolVersion = protocolVersion;
        }

        /// <summary>
        ///     Serializes .NET object to q IPC protocol and writes as a message to the output stream.
        /// </summary>
        /// <param name="obj">Object to be serialized</param>
        /// <param name="msgType">Message type</param>
        /// <returns>total size of the message, includes header (8 bytes) and data payload</returns>
        public int Write(object obj, MessageType msgType)
        {
            var memStream = new MemoryStream();
            _writer = new BinaryWriter(memStream);

            // write header
            var header = new byte[8];
            header[0] = 1; // endianness
            header[1] = (byte) msgType;
            _writer.Write(header);
            // serialize object
            WriteObject(obj);
            // write size
            _messageSize = (int) memStream.Position;

            _writer.Seek(4, SeekOrigin.Begin);
            _writer.Write(_messageSize);

            memStream.WriteTo(_stream);
            _stream.Flush();

            return _messageSize;
        }

        private void WriteObject(object obj)
        {
            var qtype = QTypes.GetQType(obj);
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
                case QType.Symbol:
                case QType.Timestamp:
                case QType.Month:
                case QType.Date:
                case QType.Datetime:
                case QType.Timespan:
                case QType.Minute:
                case QType.Second:
                case QType.Time:
                    WriteAtom(obj, qtype);
                    return;
                case QType.BoolList:
                case QType.ByteList:
                case QType.GuidList:
                case QType.ShortList:
                case QType.IntList:
                case QType.LongList:
                case QType.FloatList:
                case QType.DoubleList:
                case QType.SymbolList:
                case QType.TimestampList:
                case QType.MonthList:
                case QType.DateList:
                case QType.DatetimeList:
                case QType.TimespanList:
                case QType.MinuteList:
                case QType.SecondList:
                case QType.TimeList:
                    WriteList((Array) obj, qtype);
                    return;
                case QType.String:
                    WriteString(obj as char[]);
                    return;
                case QType.GeneralList:
                    WriteGeneralList((Array) obj);
                    return;
                case QType.NullItem:
                    WriteNullItem();
                    return;
                case QType.Lambda:
                    WriteLambda(obj as QLambda);
                    return;
                case QType.Projection:
                    WriteProjection(obj as QProjection);
                    return;
                case QType.Error:
                    WriteError(obj as Exception);
                    return;
                case QType.Dictionary: // and QType.KeyedTable (both share the same q type - 99)
                    var dictionary = obj as QDictionary;
                    if (dictionary != null)
                    {
                        WriteDictionary(dictionary);
                    }
                    else
                    {
                        var kt = obj as QKeyedTable;
                        if (kt != null)
                        {
                            WriteKeyedTable(kt);
                        }
                    }
                    return;
                case QType.Table:
                    WriteTable(obj as QTable);
                    return;
            }

            throw new QWriterException("Unable to serialize q type: " + qtype);
        }

        private void WriteError(Exception exception)
        {
            _writer.Write((sbyte) QType.Error);
            WriteSymbol(exception.Message);
        }

        private void WriteAtom(object obj, QType qtype)
        {
            _writer.Write((sbyte) qtype);
            switch (qtype)
            {
                case QType.Bool:
                    _writer.Write((bool) obj);
                    return;
                case QType.Byte:
                    _writer.Write((byte) obj);
                    return;
                case QType.Guid:
                    if (_protocolVersion < 3)
                    {
                        throw new QWriterException("kdb+ protocol version violation: guid not supported pre kdb+ v3.0");
                    }
                    WriteGuid((Guid) obj);
                    return;
                case QType.Short:
                    _writer.Write((short) obj);
                    return;
                case QType.Int:
                    _writer.Write((int) obj);
                    return;
                case QType.Long:
                    _writer.Write((long) obj);
                    return;
                case QType.Float:
                    _writer.Write((float) obj);
                    return;
                case QType.Double:
                    _writer.Write((double) obj);
                    return;
                case QType.Char:
                    _writer.Write((char) obj);
                    return;
                case QType.Symbol:
                    WriteSymbol(obj as string);
                    return;
                case QType.Timestamp:
                    if (_protocolVersion < 1)
                    {
                        throw new QWriterException(
                            "kdb+ protocol version violation: timestamp not supported pre kdb+ v2.6");
                    }
                    _writer.Write(((QTimestamp) obj).Value);
                    return;
                case QType.Month:
                    _writer.Write(((QMonth) obj).Value);
                    return;
                case QType.Date:
                    _writer.Write(((QDate) obj).Value);
                    return;
                case QType.Datetime:
                    _writer.Write(((QDateTime) obj).Value);
                    return;
                case QType.Timespan:
                    if (_protocolVersion < 1)
                    {
                        throw new QWriterException(
                            "kdb+ protocol version violation: timespan not supported pre kdb+ v2.6");
                    }
                    _writer.Write(((QTimespan) obj).Value);
                    return;
                case QType.Minute:
                    _writer.Write(((QMinute) obj).Value);
                    return;
                case QType.Second:
                    _writer.Write(((QSecond) obj).Value);
                    return;
                case QType.Time:
                    _writer.Write(((QTime) obj).Value);
                    return;
            }
            throw new QWriterException("Unable to serialize q atom of type: " + qtype);
        }

        private void WriteList(Array list, QType qtype)
        {
            _writer.Write((sbyte) qtype);
            _writer.Write((byte) 0); // attributes
            _writer.Write(list.Length);
            switch (qtype)
            {
                case QType.BoolList:
                {
                    var lst = list as bool[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write(a);
                    }
                    return;
                }
                case QType.ByteList:
                {
                    var lst = list as byte[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write(a);
                    }
                    return;
                }
                case QType.GuidList:
                {
                    if (_protocolVersion < 3)
                    {
                        throw new QWriterException("kdb+ protocol version violation: guid not supported pre kdb+ v3.0");
                    }
                    var lst = list as Guid[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        WriteGuid(a);
                    }
                    return;
                }
                case QType.ShortList:
                {
                    var lst = list as short[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write(a);
                    }
                    return;
                }
                case QType.IntList:
                {
                    var lst = list as int[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write(a);
                    }
                    return;
                }
                case QType.LongList:
                {
                    var lst = list as long[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write(a);
                    }
                    return;
                }
                case QType.FloatList:
                {
                    var lst = list as float[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write(a);
                    }
                    return;
                }
                case QType.DoubleList:
                {
                    var lst = list as double[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write(a);
                    }
                    return;
                }
                case QType.SymbolList:
                {
                    var lst = list as string[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        WriteSymbol(a);
                    }
                    return;
                }
                case QType.TimestampList:
                {
                    if (_protocolVersion < 1)
                    {
                        throw new QWriterException(
                            "kdb+ protocol version violation: timestamp not supported pre kdb+ v2.6");
                    }
                    var lst = list as QTimestamp[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write((a).Value);
                    }
                    return;
                }
                case QType.MonthList:
                {
                    var lst = list as QMonth[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write((a).Value);
                    }
                    return;
                }
                case QType.DateList:
                {
                    var lst = list as QDate[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write((a).Value);
                    }
                    return;
                }
                case QType.DatetimeList:
                {
                    var lst = list as QDateTime[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write((a).Value);
                    }
                    return;
                }
                case QType.TimespanList:
                {
                    if (_protocolVersion < 1)
                    {
                        throw new QWriterException(
                            "kdb+ protocol version violation: timespan not supported pre kdb+ v2.6");
                    }
                    var lst = list as QTimespan[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write((a).Value);
                    }
                    return;
                }
                case QType.MinuteList:
                {
                    var lst = list as QMinute[];
                    if (lst != null)
                        foreach (var a in lst)
                        {
                            _writer.Write((a).Value);
                        }
                    return;
                }
                case QType.SecondList:
                {
                    var lst = list as QSecond[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write((a).Value);
                    }
                    return;
                }
                case QType.TimeList:
                {
                    var lst = list as QTime[];
                    if (lst == null) return;
                    foreach (var a in lst)
                    {
                        _writer.Write((a).Value);
                    }
                    return;
                }
            }
            throw new QWriterException("Unable to serialize q vector of type: " + qtype);
        }

        private void WriteGeneralList(Array list)
        {
            _writer.Write((sbyte) QType.GeneralList);
            _writer.Write((byte) 0); // attributes
            _writer.Write(list.Length);
            foreach (var obj in list)
            {
                WriteObject(obj);
            }
        }

        private void WriteString(char[] s)
        {
            _writer.Write((sbyte) QType.String);
            _writer.Write((byte) 0); // attributes
            var encoded = _encoding.GetBytes(new string(s));
            _writer.Write(encoded.Length);
            _writer.Write(encoded);
        }

        private void WriteSymbol(string s)
        {
            _writer.Write(_encoding.GetBytes(s));
            _writer.Write((byte) 0);
        }

        private void WriteGuid(Guid g)
        {
            if (_protocolVersion < 3)
            {
                throw new QWriterException("kdb+ protocol version violation: Guid not supported pre kdb+ v3.0");
            }

            var b = g.ToByteArray();
            for (var i = 0; i < b.Length; i++)
            {
                var index = GuidByteOrder[i];
                _writer.Write(b[index]);
            }
        }

        private void WriteNullItem()
        {
            _writer.Write((sbyte) QType.NullItem);
            _writer.Write((byte) 0);
        }

        private void WriteLambda(QLambda l)
        {
            _writer.Write((sbyte) QType.Lambda);
            _writer.Write((byte) 0);
            WriteString(l.Expression.ToCharArray());
        }

        private void WriteProjection(QProjection p)
        {
            _writer.Write((sbyte) QType.LambdaPart);
            _writer.Write(p.Parameters.Length);
            foreach (var parameter in p.Parameters)
            {
                WriteObject(parameter);
            }
        }

        private void WriteDictionary(QDictionary d)
        {
            _writer.Write((sbyte) QType.Dictionary);
            WriteObject(d.Keys);
            WriteObject(d.Values);
        }

        private void WriteKeyedTable(QKeyedTable kt)
        {
            _writer.Write((sbyte) QType.KeyedTable);
            WriteObject(kt.Keys);
            WriteObject(kt.Values);
        }

        private void WriteTable(QTable t)
        {
            _writer.Write((sbyte) QType.Table);
            _writer.Write((byte) 0); // attributes
            _writer.Write((sbyte) QType.Dictionary);
            WriteObject(t.Columns);
            WriteObject(t.Data);
        }
    }
}