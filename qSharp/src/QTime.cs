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
    ///     Represents q time type.
    /// </summary>
    public struct QTime : IDateTime
    {
        private const string DateFormat = "HH:mm:ss.fff";
        private const string NullRepresentation = "0Nt";

        private DateTime datetime;

        /// <summary>
        ///     Creates new QTime instance using specified q time value.
        /// </summary>
        /// <param name="value">a count of milliseconds from midnight</param>
        public QTime(int value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///     Creates new QTime instance using specified DateTime.
        /// </summary>
        /// <param name="datetime">DateTime to be set</param>
        public QTime(DateTime datetime)
            : this()
        {
            this.datetime = datetime;
            Value = (int)datetime.TimeOfDay.TotalMilliseconds;
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
        ///     Converts QTime object to .NET DateTime type.
        /// </summary>
        public DateTime ToDateTime()
        {
            if (datetime == DateTime.MinValue)
            {
                datetime = new DateTime().AddMilliseconds(Value);
            }
            return datetime;
        }

        /// <summary>
        ///     Returns the string representation of QTime instance.
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
        /// Returns a QTime represented by a given string.
        /// </summary>
        /// <param name="date">string representation</param>
        /// <returns>a QTime instance</returns>
        public static QTime FromString(string date)
        {
            try
            {
                return date == null || date.Length == 0 || date.Equals(NullRepresentation) ? new QTime(int.MinValue) : new QTime(DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse QTime from: " + date, e);
            }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QTime.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QTime.</param>
        /// <returns>true if the specified System.Object is equal to the current QTime; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is QTime))
            {
                return false;
            }

            return Value == ((QTime)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}