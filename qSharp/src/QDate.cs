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
    ///     Represents q date type.
    /// </summary>
    public struct QDate : IDateTime
    {
        private const string DateFormat = "yyyy.MM.dd";
        private const string NullRepresentation = "0Nd";
        private DateTime _datetime;

        /// <summary>
        ///     Creates new QDate instance using specified q date value.
        /// </summary>
        /// <param name="value">a count of days since 2000.01.01</param>
        public QDate(int value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///     Creates new QDate instance using specified DateTime.
        /// </summary>
        /// <param name="datetime">DateTime to be set</param>
        public QDate(DateTime datetime)
            : this()
        {
            this._datetime = datetime;
            Value = (int) (datetime - QTypes.QEpoch).TotalDays;
        }

        public int Value { get; private set; }

        /// <summary>
        ///     Returns internal q representation.
        /// </summary>
        public object GetValue()
        {
            return Value;
        }

        /// <summary>
        ///     Converts QDate object to .NET DateTime type.
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
        ///     Returns the string representation of QDate instance.
        /// </summary>
        public override string ToString()
        {
            if (Value != int.MinValue)
            {
                return ToDateTime().ToString(DateFormat);
            }
            return NullRepresentation;
        }

        /// <summary>
        ///     Returns a QDate represented by a given string.
        /// </summary>
        /// <param name="date">string representation</param>
        /// <returns>a QDate instance</returns>
        public static QDate FromString(string date)
        {
            try
            {
                return date == null || date.Length == 0 || date.Equals(NullRepresentation)
                    ? new QDate(int.MinValue)
                    : new QDate(DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse QDate from: " + date, e);
            }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QDate.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QDate.</param>
        /// <returns>true if the specified System.Object is equal to the current QDate; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is QDate))
            {
                return false;
            }

            return Value == ((QDate) obj).Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}