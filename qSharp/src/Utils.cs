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

namespace qSharp
{
    /// <summary>
    ///     Utility methods
    /// </summary>
    public static class Utils
    {
        /// <summary>
        ///     Returns string representation of the array.
        /// </summary>
        /// <param name="list">Array to be converted</param>
        /// <returns>A System.String object representing given array</returns>
        public static string ArrayToString(Array list)
        {
            if (list == null || list.Length == 0)
            {
                return "[]";
            }
            var values = new string[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                object obj = list.GetValue(i);
                values[i] = obj == null
                                ? null
                                : obj.GetType().IsArray ? ArrayToString(obj as Array) : obj.ToString();
            }
            return "[" + string.Join("; ", values) + "]";
        }

        /// <summary>
        ///     Compares to arrays for values equality.
        /// </summary>
        /// <returns>true if both arrays contain the same values, false otherwise</returns>
        internal static bool ArrayEquals(Array l, Array r)
        {
            if (l == null && r == null)
            {
                return true;
            }

            if (l == null || r == null)
            {
                return false;
            }

            if (l.GetType() != r.GetType())
            {
                return false;
            }

            if (l.Length != r.Length)
            {
                return false;
            }

            for (int i = 0; i < l.Length; i++)
            {
                object lv = l.GetValue(i);
                object rv = r.GetValue(i);

                if (lv == rv || lv == null && rv == null)
                {
                    continue;
                }

                if (lv == null || rv == null )
                {
                    return false;
                }

                if (lv.GetType().IsArray)
                {
                    if (!ArrayEquals(lv as Array, rv as Array))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!lv.Equals(rv))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}