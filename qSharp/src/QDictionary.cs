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

namespace qSharp
{
    /// <summary>
    ///     Represents a q dictionary type.
    /// </summary>
    public sealed class QDictionary : IEnumerable
    {
        private readonly bool _areValuesArray;
        private readonly Array _keys;
        private readonly object _values;

        /// <summary>
        ///     Creates new QDictionary instance with given keys and values arrays.
        /// </summary>
        public QDictionary(Array keys, Array values)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException("Keys array cannot be null or 0-length");
            }

            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Values array cannot be null or 0-length");
            }

            if (keys.Length != values.Length)
            {
                throw new ArgumentException("Keys and value arrays cannot have different length");
            }

            _keys = keys;
            _values = values;
            _areValuesArray = true;
        }

        /// <summary>
        ///     Creates new QDictionary instance with given keys array and table values.
        /// </summary>
        public QDictionary(Array keys, QTable values)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException("Keys array cannot be null or 0-length");
            }

            if (values == null || values.RowsCount == 0)
            {
                throw new ArgumentException("Values table cannot be null or 0-length");
            }

            if (keys.Length != values.RowsCount)
            {
                throw new ArgumentException("Keys and value arrays cannot have different length");
            }

            _keys = keys;
            _values = values;
            _areValuesArray = false;
        }

        /// <summary>
        ///     Gets an array with dictionary keys.
        /// </summary>
        public Array Keys
        {
            get { return _keys; }
        }

        /// <summary>
        ///     Gets an array with dictionary values.
        /// </summary>
        public object Values
        {
            get { return _values; }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a dictionary keys and values.
        /// </summary>
        /// <returns>An QDictionaryEnumerator object that can be used to iterate through the dictionary</returns>
        public IEnumerator GetEnumerator()
        {
            return new QDictionaryEnumerator(this);
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QDictionary.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QDictionary.</param>
        /// <returns>true if the specified System.Object is equal to the current QDictionary; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            var d = obj as QDictionary;
            if (d == null)
            {
                return false;
            }

            if (_areValuesArray)
            {
                return Utils.ArrayEquals(Keys, d.Keys) && Utils.ArrayEquals(Values as Array, d.Values as Array);
            }
            var qTable = Values as QTable;
            return qTable != null && (Utils.ArrayEquals(Keys, d.Keys) && qTable.Equals(d.Values));
        }

        public override int GetHashCode()
        {
            return 31*Keys.GetHashCode() + Values.GetHashCode();
        }

        /// <summary>
        ///     Determines whether the specified QDictionary is equal to the current QDictionary.
        /// </summary>
        /// <param name="d">The QDictionary to compare with the current QDictionary.</param>
        /// <returns>true if the specified QDictionary is equal to the current QDictionary; otherwise, false</returns>
        public bool Equals(QDictionary d)
        {
            if (d == null)
            {
                return false;
            }

            if (_areValuesArray)
            {
                return Utils.ArrayEquals(Keys, d.Keys) && Utils.ArrayEquals(Values as Array, d.Values as Array);
            }
            var qTable = Values as QTable;
            return qTable != null && (Utils.ArrayEquals(Keys, d.Keys) && qTable.Equals(d.Values));
        }

        /// <summary>
        ///     Returns a System.String that represents the current QDictionary.
        /// </summary>
        /// <returns>A System.String that represents the current QDictionary</returns>
        public override string ToString()
        {
            if (_areValuesArray)
            {
                return "QDictionary: " + Utils.ArrayToString(Keys) + "!" + Utils.ArrayToString(Values as Array);
            }
            return "QDictionary: " + Utils.ArrayToString(Keys) + "!" + (Values as QTable);
        }

        /// <summary>
        ///     Defines a key/value pair that can be retrieved.
        /// </summary>
        public struct KeyValuePair
        {
            private readonly QDictionary _dictionary;
            private readonly int _index;

            /// <summary>
            ///     Initializes a new instance of the KeyValuePair.
            /// </summary>
            public KeyValuePair(QDictionary dictionary, int index)
            {
                _dictionary = dictionary;
                _index = index;
            }

            /// <summary>
            ///     Gets the key in the key/value pair.
            /// </summary>
            public object Key
            {
                get { return _dictionary._keys.GetValue(_index); }
            }

            /// <summary>
            ///     Gets the value in the key/value pair.
            /// </summary>
            public object Value
            {
                get
                {
                    if (!_dictionary._areValuesArray)
                    {
                        var qTable = _dictionary._values as QTable;
                        if (qTable != null) return qTable[_index];
                    }
                    var table = _dictionary._values as QTable;
                    if (table != null)
                        return table[_index];

                    throw new NotFiniteNumberException(string.Format("Type {0} is unsupported.", _dictionary._values != null ? _dictionary._values.GetType().Name : "null"));
                }
            }
        }

        /// <summary>
        ///     Iterator over pairs [key, value] stored in a dictionary.
        /// </summary>
        private sealed class QDictionaryEnumerator : IEnumerator
        {
            private readonly QDictionary _dictionary;
            private int _index = -1;

            public QDictionaryEnumerator(QDictionary dictionary)
            {
                _dictionary = dictionary;
            }

            public object Current
            {
                get { return new KeyValuePair(_dictionary, _index); }
            }

            public bool MoveNext()
            {
                _index++;
                return _index < _dictionary._keys.Length;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}