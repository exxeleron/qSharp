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
    ///     Represents q datetime type.
    /// </summary>
    public struct QDateTime : IDateTime
    {
        private const string DateFormat = "yyyy.MM.dd'T'HH:mm:ss.fff";
        private const string NullRepresentation = "0Nz";
        private DateTime _datetime;

        /// <summary>
        ///     Creates new QDatetime instance using specified q datetime value.
        /// </summary>
        /// <param name="value">a fractional day count from midnight 2000.01.01</param>
        public QDateTime(double value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///     Creates new QDatetime instance using specified DateTime.
        /// </summary>
        /// <param name="datetime">DateTime to be set</param>
        public QDateTime(DateTime datetime)
            : this()
        {
            _datetime = datetime;
            Value = (datetime - QTypes.QEpoch).TotalDays;
        }

        public double Value { get; private set; }

        /// <summary>
        ///     Returns internal q representation.
        /// </summary>
        public object GetValue()
        {
            return Value;
        }

        /// <summary>
        ///     Converts DateTime object to .NET DateTime type.
        /// </summary>
        public DateTime ToDateTime()
        {
            if (_datetime == DateTime.MinValue)
            {
                _datetime = new DateTime(2000, 1, 1).AddDays(Value);
            }
            return _datetime;
        }

        /// <summary>
        ///     Returns the string representation of DateTime instance.
        /// </summary>
        public override string ToString()
        {
            return !double.IsNaN(Value) ? ToDateTime().ToString(DateFormat) : NullRepresentation;
        }

        /// <summary>
        ///     Returns a QDateTime represented by a given string.
        /// </summary>
        /// <param name="date">string representation</param>
        /// <returns>a QDateTime instance</returns>
        public static QDateTime FromString(string date)
        {
            try
            {
                return string.IsNullOrWhiteSpace(date) || date.Equals(NullRepresentation)
                    ? new QDateTime(double.NaN)
                    : new QDateTime(DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse QDateTime from: " + date, e);
            }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QDateTime.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QDateTime.</param>
        /// <returns>true if the specified System.Object is equal to the current QDateTime; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is QDateTime))
            {
                return false;
            }

            return (double.IsNaN(Value) && double.IsNaN(((QDateTime) obj).Value)) || Math.Abs(Value - ((QDateTime) obj).Value) < double.Epsilon;
        }

        public override int GetHashCode()
        {
            var data = BitConverter.GetBytes(Value);
            var x = BitConverter.ToInt32(data, 0);
            var y = BitConverter.ToInt32(data, 4);
            return x ^ y;
        }
    }
}