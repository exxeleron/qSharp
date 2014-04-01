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
using System.Globalization;

namespace qSharp
{
    /// <summary>
    ///     Represents q timespan type.
    /// </summary>
    public struct QTimespan : IDateTime
    {
        private const string DateFormat = "'D'HH:mm:ss.fff";
        private const string NanosFormat = "{0:000000}";
        private const string NullRepresentation = "0Nn";
        private const long NanosPerDay = 86400000000000L;

        private DateTime datetime;

        /// <summary>
        ///     Creates new QTimespan instance using specified q timespan value.
        /// </summary>
        /// <param name="value">a count of nanoseconds from midnight</param>
        public QTimespan(long value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///     Creates new QTimespan instance using specified DateTime.
        /// </summary>
        /// <param name="datetime">DateTime to be set</param>
        public QTimespan(DateTime datetime)
            : this()
        {
            this.datetime = datetime;
            Value = (long)(1e6 * (datetime - QTypes.QEpoch).TotalMilliseconds);
        }

        public long Value { get; private set; }

        /// <summary>
        ///     Returns internal q representation.
        /// </summary>
        public object GetValue()
        {
            return Value;
        }

        /// <summary>
        ///     Converts QTimespan object to .NET DateTime type. Nanoseconds are lost during conversion.
        /// </summary>
        public DateTime ToDateTime()
        {
            if (datetime == DateTime.MinValue)
            {
                datetime = QTypes.QEpoch.AddMilliseconds(Math.Abs((double)Value) / 1000000L);
            }
            return datetime;
        }

        /// <summary>
        ///     Returns the string representation of QTimespan instance.
        /// </summary>
        public override string ToString()
        {
            if (Value != long.MinValue)
            {
                return (Value < 0 ? "-" : "") + (Math.Abs(Value) / NanosPerDay) + ToDateTime().ToString(DateFormat) + String.Format(NanosFormat, Math.Abs(Value % 1000000L));
            }
            return NullRepresentation;
        }

        /// <summary>
        /// Returns a QTimespan represented by a given string.
        /// </summary>
        /// <param name="date">string representation</param>
        /// <returns>a QTimespan instance</returns>
        public static QTimespan FromString(string date)
        {
            try
            {

                if (date == null || date.Length == 0 || date.Equals(NullRepresentation))
                {
                    return new QTimespan(long.MinValue);
                }
                else
                {
                    long nanos = (long)(1e6 * DateTime.ParseExact(date.Substring(date.IndexOf("D"), 13), DateFormat, CultureInfo.InvariantCulture).TimeOfDay.TotalMilliseconds)
                        + long.Parse(date.Substring(date.LastIndexOf(".") + 3));
                    return new QTimespan(int.Parse(date.Substring(0, date.IndexOf("D"))) * NanosPerDay + (date.StartsWith("-") ? -1 : 1) * nanos);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse QTimespan from: " + date, e);
            }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QTimespan.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QTimespan.</param>
        /// <returns>true if the specified System.Object is equal to the current QTimespan; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is QTimespan))
            {
                return false;
            }

            return Value == ((QTimespan)obj).Value;
        }

        public override int GetHashCode()
        {
            return ((int)Value) ^ ((int)(Value >> 32));
        }
    }
}