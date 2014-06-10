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
                int millis = Math.Abs(Value);
                int seconds = millis / 1000;
                int minutes = seconds / 60;
                int hours = minutes / 60;

                return String.Format("{0}{1:00}:{2:00}:{3:00}.{4:000}", Value < 0 ? "-" : "", hours, minutes % 60, seconds % 60, millis % 1000);
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
            if (date == null || date.Length == 0 || date.Equals(NullRepresentation))
            {
                return new QTime(int.MinValue);
            }

            try
            {
                String[] parts = date.Split(new char[]{ ':' , '.'});
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);
                int millis = int.Parse(parts[3]);
                return new QTime((millis + 1000 * seconds + 60000 * minutes + 3600000 * Math.Abs(hours)) * (hours > 0 ? 1 : -1));
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