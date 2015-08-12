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

namespace qSharp
{
    /// <summary>
    ///     Represents a q dictionary type.
    /// </summary>
    public sealed class QDictionary : IEnumerable<QDictionary.KeyValuePair>
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
            Count = keys.Length;
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
            Count = keys.Length;
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
        ///     Gets a number of entries in the dictionary.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Returns an enumerator that iterates through a dictionary keys and values.
        /// </summary>
        /// <returns>An QDictionaryEnumerator object that can be used to iterate through the dictionary</returns>
        public IEnumerator<QDictionary.KeyValuePair> GetEnumerator()
        {
            return new QDictionaryEnumerator(this);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a dictionary keys and values.
        /// </summary>
        /// <returns>An QDictionaryEnumerator object that can be used to iterate through the dictionary</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
            public int Index { get; internal set; }

            /// <summary>
            ///     Initializes a new instance of the KeyValuePair.
            /// </summary>
            public KeyValuePair(QDictionary dictionary, int index) : this()
            {
                _dictionary = dictionary;
                Index = index;
            }

            /// <summary>
            ///     Gets the key in the key/value pair.
            /// </summary>
            public object Key
            {
                get { return _dictionary._keys.GetValue(Index); }
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
                        
                        return qTable[Index];
                    }
                    else
                    {
                        return (_dictionary._values as Array).GetValue(Index);
                    }
                }
            }
        }

        /// <summary>
        ///     Iterator over pairs [key, value] stored in a dictionary.
        /// </summary>
        private sealed class QDictionaryEnumerator : IEnumerator<QDictionary.KeyValuePair>
        {
            private readonly QDictionary _dictionary;
            private int _index = -1;
            private QDictionary.KeyValuePair _current;

            public QDictionaryEnumerator(QDictionary dictionary)
            {
                _dictionary = dictionary;
                _current = new QDictionary.KeyValuePair(_dictionary, _index);
            }

            public QDictionary.KeyValuePair Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                _index++;
                _current.Index = _index;
                return _index < _dictionary._keys.Length;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
                _index = -1;
            }
        }
    }
}