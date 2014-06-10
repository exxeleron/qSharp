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
    ///     Represents q minute type.
    /// </summary>
    public struct QMinute : IDateTime
    {
        private const string NullRepresentation = "0Nu";

        private DateTime datetime;

        /// <summary>
        ///     Creates new QMinute instance using specified q minute value.
        /// </summary>
        /// <param name="value">a count of minutes from midnight</param>
        public QMinute(int value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///     Creates new QMinute instance using specified q minute value.
        /// </summary>
        /// <param name="datetime">DateTime to be set</param>
        public QMinute(DateTime datetime)
            : this()
        {
            this.datetime = datetime;
            Value = (int) datetime.TimeOfDay.TotalMinutes;
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
        ///     Converts QMinute object to .NET DateTime type.
        /// </summary>
        public DateTime ToDateTime()
        {
            if (datetime == DateTime.MinValue)
            {
                datetime = new DateTime().AddMinutes(Value);
            }
            return datetime;
        }

        /// <summary>
        ///     Returns the string representation of QMinute instance.
        /// </summary>
        public override string ToString()
        {
            if (Value != int.MinValue)
            {
                int minutes = Math.Abs(Value);
                int hours = minutes / 60;

                return String.Format("{0}{1:00}:{2:00}", Value < 0 ? "-" : "", hours, minutes % 60);
            }
            return NullRepresentation;
        }

        /// <summary>
        /// Returns a QMinute represented by a given string.
        /// </summary>
        /// <param name="date">string representation</param>
        /// <returns>a QMinute instance</returns>
        public static QMinute FromString(string date)
        {
            if (date == null || date.Length == 0 || date.Equals(NullRepresentation))
            {
                return new QMinute(int.MinValue);
            }

            try
            {
                String[] parts = date.Split(':');
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);

                return new QMinute((minutes + 60 * Math.Abs(hours)) * (hours > 0 ? 1 : -1));
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse QMinute from: " + date, e);
            }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QMinute.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QMinute.</param>
        /// <returns>true if the specified System.Object is equal to the current QMinute; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is QMinute))
            {
                return false;
            }

            return Value == ((QMinute)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}