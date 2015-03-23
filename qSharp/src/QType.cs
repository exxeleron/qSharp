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
using System.Collections.Generic;

namespace qSharp
{
    /// <summary>
    ///     Enumerates all supported q types.
    /// </summary>
    public enum QType
    {
        NullItem = 101,
        Error = -128,
        GeneralList = 0,
        Bool = -1,
        BoolList = 1,
        Guid = -2,
        GuidList = 2,
        Byte = -4,
        ByteList = 4,
        Short = -5,
        ShortList = 5,
        Int = -6,
        IntList = 6,
        Long = -7,
        LongList = 7,
        Float = -8,
        FloatList = 8,
        Double = -9,
        DoubleList = 9,
        Char = -10,
        String = 10,
        Symbol = -11,
        SymbolList = 11,
        Timestamp = -12,
        TimestampList = 12,
        Month = -13,
        MonthList = 13,
        Date = -14,
        DateList = 14,
        Datetime = -15,
        DatetimeList = 15,
        Timespan = -16,
        TimespanList = 16,
        Minute = -17,
        MinuteList = 17,
        Second = -18,
        SecondList = 18,
        Time = -19,
        TimeList = 19,
        Table = 98,
        KeyedTable = 99,
        Dictionary = 99,
        Lambda = 100,
        LambdaPart = 104,
        Projection = 104,
        UnaryPrimitiveFunc = 101,
        BinaryPrimitiveFunc = 102,
        TernaryOperatorFunc = 103,
        CompositionFunc = 105,
        AdverbFunc106 = 106,
        AdverbFunc107 = 107,
        AdverbFunc108 = 108,
        AdverbFunc109 = 109,
        AdverbFunc110 = 110,
        AdverbFunc111 = 111,
    }

    /// <summary>
    ///     Utility class for conversions between q and .NET types.
    /// </summary>
    public static class QTypes
    {
        internal static readonly DateTime QEpoch = new DateTime(2000, 1, 1);

        private static readonly Dictionary<Type, QType> toQ = new Dictionary<Type, QType>
            {
                {typeof (object[]), QType.GeneralList},
                {typeof (bool), QType.Bool},
                {typeof (bool[]), QType.BoolList},
                {typeof (Guid), QType.Guid},
                {typeof (Guid[]), QType.GuidList},
                {typeof (byte), QType.Byte},
                {typeof (byte[]), QType.ByteList},
                {typeof (short), QType.Short},
                {typeof (short[]), QType.ShortList},
                {typeof (int), QType.Int},
                {typeof (int[]), QType.IntList},
                {typeof (long), QType.Long},
                {typeof (long[]), QType.LongList},
                {typeof (float), QType.Float},
                {typeof (float[]), QType.FloatList},
                {typeof (double), QType.Double},
                {typeof (double[]), QType.DoubleList},
                {typeof (char), QType.Char},
                {typeof (char[]), QType.String},
                {typeof (char[][]), QType.GeneralList},
                {typeof (string), QType.Symbol},
                {typeof (string[]), QType.SymbolList},
                {typeof (QTimestamp), QType.Timestamp},
                {typeof (QTimestamp[]), QType.TimestampList},
                {typeof (QMonth), QType.Month},
                {typeof (QMonth[]), QType.MonthList},
                {typeof (QDate), QType.Date},
                {typeof (QDate[]), QType.DateList},
                {typeof (QDateTime), QType.Datetime},
                {typeof (QDateTime[]), QType.DatetimeList},
                {typeof (QTimespan), QType.Timespan},
                {typeof (QTimespan[]), QType.TimespanList},
                {typeof (QMinute), QType.Minute},
                {typeof (QMinute[]), QType.MinuteList},
                {typeof (QSecond), QType.Second},
                {typeof (QSecond[]), QType.SecondList},
                {typeof (QTime), QType.Time},
                {typeof (QTime[]), QType.TimeList},
                {typeof (QLambda), QType.Lambda},
                {typeof (QProjection), QType.Projection},
                {typeof (QException), QType.Error},
                {typeof (QDictionary), QType.Dictionary},
                {typeof (QTable), QType.Table},
                {typeof (QKeyedTable), QType.KeyedTable},
            };

        private static readonly Dictionary<QType, Type> fromQ = new Dictionary<QType, Type>
            {
                {QType.GeneralList, typeof (object[])},
                {QType.Bool, typeof (bool)},
                {QType.BoolList, typeof (bool[])},
                {QType.Guid, typeof (Guid)},
                {QType.GuidList, typeof (Guid[])},
                {QType.Byte, typeof (byte)},
                {QType.ByteList, typeof (byte[])},
                {QType.Short, typeof (short)},
                {QType.ShortList, typeof (short[])},
                {QType.Int, typeof (int)},
                {QType.IntList, typeof (int[])},
                {QType.Long, typeof (long)},
                {QType.LongList, typeof (long[])},
                {QType.Float, typeof (float)},
                {QType.FloatList, typeof (float[])},
                {QType.Double, typeof (double)},
                {QType.DoubleList, typeof (double[])},
                {QType.Char, typeof (char)},
                {QType.String, typeof (char[])},
                {QType.Symbol, typeof (string)},
                {QType.SymbolList, typeof (string[])},
                {QType.Timestamp, typeof (QTimestamp)},
                {QType.TimestampList, typeof (QTimestamp[])},
                {QType.Month, typeof (QMonth)},
                {QType.MonthList, typeof (QMonth[])},
                {QType.Date, typeof (QDate)},
                {QType.DateList, typeof (QDate[])},
                {QType.Datetime, typeof (QDateTime)},
                {QType.DatetimeList, typeof (QDateTime[])},
                {QType.Timespan, typeof (QTimespan)},
                {QType.TimespanList, typeof (QTimespan[])},
                {QType.Minute, typeof (QMinute)},
                {QType.MinuteList, typeof (QMinute[])},
                {QType.Second, typeof (QSecond)},
                {QType.SecondList, typeof (QSecond[])},
                {QType.Time, typeof (QTime)},
                {QType.TimeList, typeof (QTime[])},
                {QType.Lambda, typeof (QLambda)},
                {QType.Projection, typeof (QProjection)},
                {QType.Error, typeof (Exception)},
            };

        private static readonly Dictionary<QType, object> qNulls = new Dictionary<QType, object>
            {
                {QType.Bool, false},
                {QType.Byte, (byte) 0},
                {QType.Guid, Guid.Empty},
                {QType.Short, short.MinValue},
                {QType.Int, int.MinValue},
                {QType.Long, long.MinValue},
                {QType.Float, float.NaN},
                {QType.Double, double.NaN},
                {QType.Char, ' '},
                {QType.Symbol, ""},
                {QType.Timestamp, new QTimestamp(long.MinValue)},
                {QType.Month, new QMonth(int.MinValue)},
                {QType.Date, new QDate(int.MinValue)},
                {QType.Datetime, new QDateTime(double.NaN)},
                {QType.Timespan, new QTimespan(long.MinValue)},
                {QType.Minute, new QMinute(int.MinValue)},
                {QType.Second, new QSecond(int.MinValue)},
                {QType.Time, new QTime(int.MinValue)},
            };

        /// <summary>
        ///     Returns default mapping for particular .NET object to representative q type.
        /// </summary>
        /// <param name="obj">Requested object</param>
        /// <returns>QType enum being a result of q serialization</returns>
        public static QType GetQType(object obj)
        {
            if (obj == null)
            {
                return QType.NullItem;
            }
            if (toQ.ContainsKey(obj.GetType()))
            {
                return toQ[obj.GetType()];
            }
            throw new QWriterException("Cannot serialize object of type: " + obj.GetType().FullName);
        }

        /// <summary>
        ///     Returns default mapping for particular q type.
        /// </summary>
        /// <param name="type">Requested q type</param>
        /// <returns>Type of the object being a result of q message deserialization</returns>
        public static Type GetType(QType type)
        {
            if (fromQ.ContainsKey(type))
            {
                return fromQ[type];
            }
            switch (type)
            {
                case QType.NullItem:
                    return null;
                default:
                    throw new QReaderException("Cannot deserialize object of type: " + type);
            }
        }

        /// <summary>
        ///     Returns object representing q null of particular type.
        /// </summary>
        /// <param name="type">Requested null type</param>
        /// <returns>object representing q null</returns>
        public static object GetQNull(QType type)
        {
            if (qNulls.ContainsKey(type))
            {
                return qNulls[type];
            }
            throw new QException("Cannot find null value of type: " + type);
        }
    }
}