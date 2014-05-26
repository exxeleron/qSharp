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
using System.Collections.Specialized;
using System.Linq;

namespace qSharp
{
    /// <summary>
    ///     Represents a q table type.
    /// </summary>
    public sealed class QTable : IEnumerable
    {
        private readonly string[] columns;
        private readonly ListDictionary columnsMap;
        private Array data;

        /// <summary>
        ///     Initializes a new instance of the QTable with specified column names and data matrix.
        /// </summary>
        public QTable(string[] columns, Array data)
        {
            if (columns == null || columns.Length == 0)
            {
                throw new ArgumentException("Columns array cannot be null or 0-length");
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

            columnsMap = new ListDictionary();
            for (int i = 0; i < columns.Length; i++)
            {
                columnsMap[columns[i]] = i;
            }

            this.columns = columns;
            this.data = data;
            RowsCount = ((Array)data.GetValue(0)).Length;
        }

        /// <summary>
        ///     Gets a number of rows in current QTable.
        /// </summary>
        public int RowsCount { get; private set; }

        /// <summary>
        ///     Gets a number of columns in current QTable.
        /// </summary>
        public int ColumnsCount
        {
            get { return Columns.Length; }
        }

        /// <summary>
        ///     Gets an array of columns in current QTable.
        /// </summary>
        public string[] Columns
        {
            get { return columns; }
        }

        /// <summary>
        ///     Gets a data matrix in current QTable.
        /// </summary>
        public Array Data
        {
            get { return data; }
        }

        /// <summary>
        ///     Gets a row of data from current QTable.
        /// </summary>
        /// <param name="index">0 based row index</param>
        /// <returns>Row object representing a row in current QTable</returns>
        public Row this[int index]
        {
            get { return new Row(this, index); }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through rows in a table.
        /// </summary>
        /// <returns>An QTableEnumerator object that can be used to iterate through the table</returns>
        public IEnumerator GetEnumerator()
        {
            return new QTableEnumerator(this);
        }

        /// <summary>
        ///     Gets a column index for specified name.
        /// </summary>
        /// <param name="column">Name of the column</param>
        /// <returns>0 based column index</returns>
        public int GetColumnIndex(string column)
        {
            return (int)columnsMap[column];
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QTable.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QTable.</param>
        /// <returns>true if the specified System.Object is equal to the current QTable; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var t = obj as QTable;
            if (t == null)
            {
                return false;
            }

            return Utils.ArrayEquals(Columns, t.Columns) && Utils.ArrayEquals(Data, t.Data);
        }

        public override int GetHashCode()
        {
            return 31 * Columns.GetHashCode() + Data.GetHashCode();
        }

        /// <summary>
        ///     Determines whether the specified QTable is equal to the current QTable.
        /// </summary>
        /// <param name="t">The QTable to compare with the current QTable.</param>
        /// <returns>true if the specified QTable is equal to the current QTable; otherwise, false</returns>
        public bool Equals(QTable t)
        {
            if (t == null)
            {
                return false;
            }

            return Utils.ArrayEquals(Columns, t.Columns) && Utils.ArrayEquals(Data, t.Data);
        }

        /// <summary>
        ///     Returns a System.String that represents the current QTable.
        /// </summary>
        /// <returns>A System.String that represents the current QTable</returns>
        public override string ToString()
        {
            return "QTable: " + Utils.ArrayToString(Columns) + "!" + Utils.ArrayToString(Data);
        }

        /// <summary>
        ///     Iterator over rows in a table.
        /// </summary>
        private sealed class QTableEnumerator : IEnumerator
        {
            private readonly QTable _table;
            private int _index = -1;

            public QTableEnumerator(QTable table)
            {
                _table = table;
            }

            public object Current
            {
                get { return new Row(_table, _index); }
            }

            public bool MoveNext()
            {
                _index++;
                return _index < _table.RowsCount;
            }

            public void Reset()
            {
                _index = -1;
            }
        }

        /// <summary>
        ///     Represents single row in a table.
        /// </summary>
        public struct Row : IEnumerable
        {
            private readonly int _rowIndex;
            private readonly QTable _table;

            /// <summary>
            ///     Initializes a new instance of the Row.
            /// </summary>
            public Row(QTable table, int rowIndex)
            {
                if (rowIndex < 0 || rowIndex > table.RowsCount)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _table = table;
                _rowIndex = rowIndex;
            }

            /// <summary>
            ///     Gets a number of columns in a row.
            /// </summary>
            public int Length
            {
                get { return _table.Columns.Length; }
            }

            /// <summary>
            ///     Gets an object stored under specifix index.
            /// </summary>
            /// <param name="index">0 based index</param>
            /// <returns>object</returns>
            public object this[int index]
            {
                get { return ((Array)_table.data.GetValue(index)).GetValue(_rowIndex); }
                set { ((Array)_table.data.GetValue(index)).SetValue(value, _rowIndex); }
            }

            /// <summary>
            ///     Returns an enumerator that iterates through a row.
            /// </summary>
            /// <returns>An RowEnumerator object that can be used to iterate through the row</returns>
            public IEnumerator GetEnumerator()
            {
                return new RowEnumerator(this);
            }

            /// <summary>
            ///     Creates a copy of entire row and returns it as an object array.
            /// </summary>
            /// <returns>object[] with copy of entire row</returns>
            public object[] ToArray()
            {
                int length = Length;
                var row = new object[length];

                for (int i = 0; i < length; i++)
                {
                    row[i] = this[i];
                }

                return row;
            }

            /// <summary>
            ///     Returns a System.String that represents the current QTable row.
            /// </summary>
            /// <returns>A System.String that represents the current QTable row</returns>
            public override string ToString()
            {
                return Utils.ArrayToString(_table.Columns) + "!" + Utils.ArrayToString(ToArray());
            }
        }

        /// <summary>
        ///     Iterates over a row.
        /// </summary>
        private sealed class RowEnumerator : IEnumerator
        {
            private int _colIndex = -1;
            private Row _row;

            public RowEnumerator(Row row)
            {
                _row = row;
            }

            public object Current
            {
                get { return _row[_colIndex]; }
            }

            public bool MoveNext()
            {
                _colIndex++;
                return _colIndex < _row.Length;
            }

            public void Reset()
            {
                _colIndex = -1;
            }
        }
    }
}