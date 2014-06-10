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
    ///     Represents q second type.
    /// </summary>
    public struct QSecond : IDateTime
    {
        private const string NullRepresentation = "0Nv";

        private DateTime datetime;

        /// <summary>
        ///     Creates new QSecond instance using specified q second value.
        /// </summary>
        /// <param name="value">a count of seconds from midnight</param>
        public QSecond(int value)
            : this()
        {
            Value = value;
        }

        /// <summary>
        ///     Creates new QSecond instance using specified DateTime.
        /// </summary>
        /// <param name="datetime">DateTime to be set</param>
        public QSecond(DateTime datetime)
            : this()
        {
            this.datetime = datetime;
            Value = (int)datetime.TimeOfDay.TotalSeconds;
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
        ///     Converts QSecond object to .NET DateTime type.
        /// </summary>
        public DateTime ToDateTime()
        {
            if (datetime == DateTime.MinValue)
            {
                datetime = new DateTime().AddSeconds(Value);
            }
            return datetime;
        }

        /// <summary>
        ///     Returns the string representation of QSecond instance.
        /// </summary>
        public override string ToString()
        {
            if (Value != int.MinValue)
            {
                int seconds = Math.Abs(Value);
                int minutes = seconds / 60;
                int hours = minutes / 60;

                return String.Format("{0}{1:00}:{2:00}:{3:00}", Value < 0 ? "-" : "", hours, minutes % 60, seconds % 60);
            }
            return NullRepresentation;
        }

        /// <summary>
        /// Returns a QSecond represented by a given string.
        /// </summary>
        /// <param name="date">string representation</param>
        /// <returns>a QSecond instance</returns>
        public static QSecond FromString(string date)
        {
            if (date == null || date.Length == 0 || date.Equals(NullRepresentation))
            {
                return new QSecond(int.MinValue);
            }

            try
            {
                String[] parts = date.Split(':');
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);

                return new QSecond((seconds + 60 * minutes + 3600 * Math.Abs(hours)) * (hours > 0 ? 1 : -1));
            }
            catch (Exception e)
            {
                throw new ArgumentException("Cannot parse QSecond from: " + date, e);
            }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QSecond.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QSecond.</param>
        /// <returns>true if the specified System.Object is equal to the current QSecond; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is QSecond))
            {
                return false;
            }

            return Value == ((QSecond)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}