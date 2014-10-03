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
        private static readonly int[] guidByteOrder = { 3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15 };

        private readonly Encoding encoding;
        private readonly int protocolVersion;

        private readonly Stream stream;
        private BinaryWriter writer;
        private int messageSize;

        /// <summary>
        ///     Initializes a new QWriter instance.
        /// </summary>
        /// <param name="stream">Output stream for serialized messages</param>
        /// <param name="encoding">Encoding to be used</param>
        /// <param name="protocolVersion">kdb+ protocol version</param>
        public QWriter(Stream stream, Encoding encoding, int protocolVersion)
        {
            this.stream = stream;
            this.encoding = encoding;
            this.protocolVersion = protocolVersion;
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
            writer = new BinaryWriter(memStream);

            // write header
            var header = new byte[8];
            header[0] = 1; // endianness
            header[1] = (byte)msgType;
            writer.Write(header);
            // serialize object
            WriteObject(obj);
            // write size
            messageSize = (int)memStream.Position;

            writer.Seek(4, SeekOrigin.Begin);
            writer.Write(messageSize);

            memStream.WriteTo(stream);
            stream.Flush();

            return messageSize;
        }

        private void WriteObject(object obj)
        {
            QType qtype = QTypes.GetQType(obj);
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
                    WriteList((Array)obj, qtype);
                    return;
                case QType.String:
                    WriteString(obj as char[]);
                    return;
                case QType.GeneralList:
                    WriteGeneralList((Array)obj);
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
                    if (obj is QDictionary)
                    {
                        WriteDictionary(obj as QDictionary);
                    }
                    else if (obj is QKeyedTable)
                    {
                        WriteKeyedTable(obj as QKeyedTable);
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
            writer.Write((sbyte)QType.Error);
            WriteSymbol(exception.Message);
        }

        private void WriteAtom(object obj, QType qtype)
        {
            writer.Write((sbyte)qtype);
            switch (qtype)
            {
                case QType.Bool:
                    writer.Write((bool)obj);
                    return;
                case QType.Byte:
                    writer.Write((byte)obj);
                    return;
                case QType.Guid:
                    if (protocolVersion < 3)
                    {
                        throw new QWriterException("kdb+ protocol version violation: guid not supported pre kdb+ v3.0");
                    }
                    WriteGuid((Guid)obj);
                    return;
                case QType.Short:
                    writer.Write((short)obj);
                    return;
                case QType.Int:
                    writer.Write((int)obj);
                    return;
                case QType.Long:
                    writer.Write((long)obj);
                    return;
                case QType.Float:
                    writer.Write((float)obj);
                    return;
                case QType.Double:
                    writer.Write((double)obj);
                    return;
                case QType.Char:
                    writer.Write((char)obj);
                    return;
                case QType.Symbol:
                    WriteSymbol(obj as string);
                    return;
                case QType.Timestamp:
                    if (protocolVersion < 1)
                    {
                        throw new QWriterException("kdb+ protocol version violation: timestamp not supported pre kdb+ v2.6");
                    }
                    writer.Write(((QTimestamp)obj).Value);
                    return;
                case QType.Month:
                    writer.Write(((QMonth)obj).Value);
                    return;
                case QType.Date:
                    writer.Write(((QDate)obj).Value);
                    return;
                case QType.Datetime:
                    writer.Write(((QDateTime)obj).Value);
                    return;
                case QType.Timespan:
                    if (protocolVersion < 1)
                    {
                        throw new QWriterException("kdb+ protocol version violation: timespan not supported pre kdb+ v2.6");
                    }
                    writer.Write(((QTimespan)obj).Value);
                    return;
                case QType.Minute:
                    writer.Write(((QMinute)obj).Value);
                    return;
                case QType.Second:
                    writer.Write(((QSecond)obj).Value);
                    return;
                case QType.Time:
                    writer.Write(((QTime)obj).Value);
                    return;
            }
            throw new QWriterException("Unable to serialize q atom of type: " + qtype);
        }

        private void WriteList(Array list, QType qtype)
        {
            writer.Write((sbyte)qtype);
            writer.Write((byte)0); // attributes
            writer.Write(list.Length);
            switch (qtype)
            {
                case QType.BoolList:
                    {
                        var _list = list as bool[];
                        if (_list != null)
                            foreach (bool a in _list)
                            {
                                writer.Write(a);
                            }
                        return;
                    }
                case QType.ByteList:
                    {
                        var _list = list as byte[];
                        if (_list != null)
                            foreach (byte a in _list)
                            {
                                writer.Write(a);
                            }
                        return;
                    }
                case QType.GuidList:
                    {
                        if (protocolVersion < 3)
                        {
                            throw new QWriterException("kdb+ protocol version violation: guid not supported pre kdb+ v3.0");
                        }
                        var _list = list as Guid[];
                        if (_list != null)
                            foreach (Guid a in _list)
                            {
                                WriteGuid(a);
                            }
                        return;
                    }
                case QType.ShortList:
                    {
                        var _list = list as short[];
                        if (_list != null)
                            foreach (short a in _list)
                            {
                                writer.Write(a);
                            }
                        return;
                    }
                case QType.IntList:
                    {
                        var _list = list as int[];
                        if (_list != null)
                            foreach (int a in _list)
                            {
                                writer.Write(a);
                            }
                        return;
                    }
                case QType.LongList:
                    {
                        var _list = list as long[];
                        if (_list != null)
                            foreach (long a in _list)
                            {
                                writer.Write(a);
                            }
                        return;
                    }
                case QType.FloatList:
                    {
                        var _list = list as float[];
                        if (_list != null)
                            foreach (float a in _list)
                            {
                                writer.Write(a);
                            }
                        return;
                    }
                case QType.DoubleList:
                    {
                        var _list = list as double[];
                        if (_list != null)
                            foreach (double a in _list)
                            {
                                writer.Write(a);
                            }
                        return;
                    }
                case QType.SymbolList:
                    {
                        var _list = list as string[];
                        if (_list != null)
                            foreach (string a in _list)
                            {
                                WriteSymbol(a);
                            }
                        return;
                    }
                case QType.TimestampList:
                    {
                        if (protocolVersion < 1)
                        {
                            throw new QWriterException("kdb+ protocol version violation: timestamp not supported pre kdb+ v2.6");
                        }
                        var _list = list as QTimestamp[];
                        if (_list != null)
                            foreach (QTimestamp a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
                case QType.MonthList:
                    {
                        var _list = list as QMonth[];
                        if (_list != null)
                            foreach (QMonth a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
                case QType.DateList:
                    {
                        var _list = list as QDate[];
                        if (_list != null)
                            foreach (QDate a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
                case QType.DatetimeList:
                    {
                        var _list = list as QDateTime[];
                        if (_list != null)
                            foreach (QDateTime a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
                case QType.TimespanList:
                    {
                        if (protocolVersion < 1)
                        {
                            throw new QWriterException("kdb+ protocol version violation: timespan not supported pre kdb+ v2.6");
                        }
                        var _list = list as QTimespan[];
                        if (_list != null)
                            foreach (QTimespan a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
                case QType.MinuteList:
                    {
                        var _list = list as QMinute[];
                        if (_list != null)
                            foreach (QMinute a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
                case QType.SecondList:
                    {
                        var _list = list as QSecond[];
                        if (_list != null)
                            foreach (QSecond a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
                case QType.TimeList:
                    {
                        var _list = list as QTime[];
                        if (_list != null)
                            foreach (QTime a in _list)
                            {
                                writer.Write((a).Value);
                            }
                        return;
                    }
            }
            throw new QWriterException("Unable to serialize q vector of type: " + qtype);
        }

        private void WriteGeneralList(Array list)
        {
            writer.Write((sbyte)QType.GeneralList);
            writer.Write((byte)0); // attributes
            writer.Write(list.Length);
            foreach (object obj in list)
            {
                WriteObject(obj);
            }
        }

        private void WriteString(char[] s)
        {
            writer.Write((sbyte)QType.String);
            writer.Write((byte)0); // attributes
            byte[] encoded = encoding.GetBytes(new string(s));
            writer.Write(encoded.Length);
            writer.Write(encoded);
        }

        private void WriteSymbol(string s)
        {
            writer.Write(encoding.GetBytes(s));
            writer.Write((byte)0);
        }


        private void WriteGuid(Guid g)
        {
            if (protocolVersion < 3)
            {
                throw new QWriterException("kdb+ protocol version violation: Guid not supported pre kdb+ v3.0");
            }

            byte[] b = g.ToByteArray();
            for (int i = 0; i < b.Length; i++)
            {
                int index = guidByteOrder[i];
                writer.Write(b[index]);
            }
        }


        private void WriteNullItem()
        {
            writer.Write((sbyte)QType.NullItem);
            writer.Write((byte)0);
        }

        private void WriteLambda(QLambda l)
        {
            writer.Write((sbyte)QType.Lambda);
            writer.Write((byte)0);
            WriteString(l.Expression.ToCharArray());
        }

        private void WriteProjection(QProjection p)
        {
            writer.Write((sbyte)QType.LambdaPart);
            writer.Write(p.Parameters.Length);
            foreach (object parameter in p.Parameters)
            {
                WriteObject(parameter);
            }
        }

        private void WriteDictionary(QDictionary d)
        {
            writer.Write((sbyte)QType.Dictionary);
            WriteObject(d.Keys);
            WriteObject(d.Values);
        }

        private void WriteKeyedTable(QKeyedTable kt)
        {
            writer.Write((sbyte)QType.KeyedTable);
            WriteObject(kt.Keys);
            WriteObject(kt.Values);
        }

        private void WriteTable(QTable t)
        {
            writer.Write((sbyte)QType.Table);
            writer.Write((byte)0); // attributes
            writer.Write((sbyte)QType.Dictionary);
            WriteObject(t.Columns);
            WriteObject(t.Data);
        }
    }
}