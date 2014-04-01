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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qSharp
{
    /// <summary>
    ///     Represents a q keyed table type.
    /// </summary>
    public sealed class QKeyedTable : IEnumerable
    {
        private readonly QTable keys;
        private readonly QTable values;

        /// <summary>
        ///     Creates new QKeyedTable instance with given keys and values arrays.
        /// </summary>
        public QKeyedTable(QTable keys, QTable values)
        {
            if (keys == null || keys.RowsCount == 0)
            {
                throw new ArgumentException("Keys table cannot be null or 0-length");
            }

            if (values == null || values.RowsCount == 0)
            {
                throw new ArgumentException("Values table cannot be null or 0-length");
            }

            if (keys.RowsCount != values.RowsCount)
            {
                throw new ArgumentException("Keys and value tables cannot have different length");
            }

            this.keys = keys;
            this.values = values;
        }

        /// <summary>
        ///     Initializes a new instance of the QKeyedTable with specified column names and data matrix.
        /// </summary>
        public QKeyedTable(string[] columns, string[] keyColumns, Array data)
        {
            if (columns == null || columns.Length == 0)
            {
                throw new ArgumentException("Columns array cannot be null or 0-length");
            }

            if (keyColumns == null || keyColumns.Length == 0)
            {
                throw new ArgumentException("Key columns array cannot be null or 0-length");
            }

            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("Data matrix cannot be null or 0-length");
            }

            if (columns.Length != data.Length)
            {
                throw new ArgumentException("Columns array and data matrix cannot have different length");
            }

            if (data.Cast<object>().Any(col => !col.GetType().IsArray))
            {
                throw new ArgumentException("Non array column found in data matrix");
            }

            if (keyColumns.Any(keyCol => !columns.Contains(keyCol)))
            {
                throw new ArgumentException("Non array column found in data matrix");
            }

            var keyIndices = new SortedSet<int>();
            for (int i = 0; i < columns.Length; i++)
            {
                if (keyColumns.Contains(columns[i]))
                {
                    keyIndices.Add(i);
                }
            }

            var keyArrays = new object[keyIndices.Count];
            var keyHeaders = new string[keyIndices.Count];
            var dataArrays = new object[data.Length - keyIndices.Count];
            var dataHeaders = new string[data.Length - keyIndices.Count];

            int ki = 0;
            int di = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (keyIndices.Contains(i))
                {
                    keyHeaders[ki] = columns[i];
                    keyArrays[ki++] = data.GetValue(i);
                }
                else
                {
                    dataHeaders[di] = columns[i];
                    dataArrays[di++] = data.GetValue(i);
                }
            }

            keys = new QTable(keyHeaders, keyArrays);
            values = new QTable(dataHeaders, dataArrays);
        }

        /// <summary>
        ///     Gets an array with keys.
        /// </summary>
        public QTable Keys
        {
            get { return keys; }
        }

        /// <summary>
        ///     Gets an array with values.
        /// </summary>
        public QTable Values
        {
            get { return values; }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a table keys and values.
        /// </summary>
        /// <returns>An QKeyedTableEnumerator object that can be used to iterate through the table</returns>
        public IEnumerator GetEnumerator()
        {
            return new QKeyedTableEnumerator(this);
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QKeyedTable.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QKeyedTable.</param>
        /// <returns>true if the specified System.Object is equal to the current QKeyedTable; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var kt = obj as QKeyedTable;
            if (kt == null)
            {
                return false;
            }

            return Keys.Equals(kt.Keys) && Values.Equals(kt.Values);
        }

        /// <summary>
        ///     Determines whether the specified QKeyedTable is equal to the current QKeyedTable.
        /// </summary>
        /// <param name="kt">The QKeyedTable to compare with the current QKeyedTable.</param>
        /// <returns>true if the specified QKeyedTable is equal to the current QKeyedTable; otherwise, false</returns>
        public bool Equals(QKeyedTable kt)
        {
            if (kt == null)
            {
                return false;
            }

            return Keys.Equals(kt.Keys) && Values.Equals(kt.Values);
        }

        public override int GetHashCode()
        {
            return 31*Keys.GetHashCode() + Values.GetHashCode();
        }

        /// <summary>
        ///     Returns a System.String that represents the current QKeyedTable.
        /// </summary>
        /// <returns>A System.String that represents the current QKeyedTable</returns>
        public override string ToString()
        {
            return "QKeyedTable: " + Keys + "|" + Values;
        }

        /// <summary>
        ///     Defines a key/value pair that can be retrieved.
        /// </summary>
        public struct KeyValuePair
        {
            private readonly int _index;
            private readonly QKeyedTable _kt;

            /// <summary>
            ///     Initializes a new instance of the KeyValuePair.
            /// </summary>
            public KeyValuePair(QKeyedTable table, int index)
            {
                _kt = table;
                _index = index;
            }

            /// <summary>
            ///     Gets the key in the key/value pair.
            /// </summary>
            public object Key
            {
                get { return _kt.keys[_index]; }
            }

            /// <summary>
            ///     Gets the value in the key/value pair.
            /// </summary>
            public object Value
            {
                get { return _kt.values[_index]; }
            }
        }

        /// <summary>
        ///     Iterator over pairs [key, value] stored in a keyed table.
        /// </summary>
        private sealed class QKeyedTableEnumerator : IEnumerator
        {
            private readonly QKeyedTable _kt;
            private int _index = -1;

            public QKeyedTableEnumerator(QKeyedTable table)
            {
                _kt = table;
            }

            public object Current
            {
                get { return new KeyValuePair(_kt, _index); }
            }

            public bool MoveNext()
            {
                _index++;
                return _index < _kt.keys.RowsCount;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}