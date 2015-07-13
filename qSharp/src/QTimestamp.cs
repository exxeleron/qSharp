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
    ///     Represents q timestamp type.
    /// </summary>
    public struct QTimestamp : IDateTime
    {
        private const string DateFormat = "yyyy.MM.dd'D'HH:mm:ss.fff";
        private const string NanosFormat = "{0:000000}";
        private const string NullRepresentation = "0Np";
        private const long NanosPerDay = 86400000000000L;
        private DateTime _datetime;

        /// <summary>
        ///     Creates new QTimestamp instance using specified q timestamp value.
        /// </summary>
        /// <param name="value">a count of nanoseconds from midnight 2000.01.01</param>
        public QTimestamp(long value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///     Creates new QTimestamp instance using specified DateTime.
        /// </summary>
        /// <param name="datetime">DateTime to be set</param>
        public QTimestamp(DateTime datetime)
            : this()
        {
            _datetime = datetime;
            Value = (long) (1e6*(datetime - QTypes.QEpoch).TotalMilliseconds);
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
        ///     Converts QTimestamp object to .NET DateTime type. Nanoseconds are lost during conversion.
        /// </summary>
        public DateTime ToDateTime()
        {
            if (_datetime == DateTime.MinValue)
            {
                _datetime = QTypes.QEpoch.AddMilliseconds((double) Value/1000000L);
            }
            return _datetime;
        }

        /// <summary>
        ///     Returns the string representation of QTimestamp instance.
        /// </summary>
        public override string ToString()
        {
            if (Value != long.MinValue)
            {
                return ToDateTime().ToString(DateFormat) + string.Format(NanosFormat, Value%1000000L);
            }
            return NullRepresentation;
        }

        /// <summary>
        ///     Returns a QTimestamp represented by a given string.
        /// </summary>
        /// <param name="date">string representation</param>
        /// <returns>a QTimestamp instance</returns>
        public static QTimestamp FromString(string date)
        {
            try
            {
                if (date == null || date.Length == 0 || date.Equals(NullRepresentation))
                {
                    return new QTimestamp(long.MinValue);
                }
                return
                    new QTimestamp(
                        (long)
                            (1e6*
                             (DateTime.ParseExact(date.Substring(0, 23), DateFormat, CultureInfo.InvariantCulture) -
                              QTypes.QEpoch).TotalMilliseconds) + long.Parse(date.Substring(23)));
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse QTimestamp from: " + date, e);
            }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QTimestamp.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QTimestamp.</param>
        /// <returns>true if the specified System.Object is equal to the current QTimestamp; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is QTimestamp))
            {
                return false;
            }

            return Value == ((QTimestamp) obj).Value;
        }

        public override int GetHashCode()
        {
            return ((int) Value) ^ ((int) (Value >> 32));
        }
    }
}