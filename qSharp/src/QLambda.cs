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
using System.Text.RegularExpressions;

namespace qSharp
{
    /// <summary>
    ///     Represents a q lambda expression.
    /// </summary>
    public sealed class QLambda : QFunction
    {
        private const string LambdaRegex = @"\s*(k\))?\s*\{.*\}";

        private readonly string expression;

        /// <summary>
        ///     Creates new QLambda instance with given body. Note that expression is trimmed and required to be enclosed
        ///     in { and } brackets.
        /// </summary>
        public QLambda(string expression)
            : base((byte)QType.Lambda)
        {
            if (expression == null)
            {
                throw new ArgumentException("Lambda expression cannot be null");
            }
            expression = expression.Trim();

            if (expression.Length == 0)
            {
                throw new ArgumentException("Lambda expression cannot be empty");
            }

            if (!Regex.IsMatch(expression, LambdaRegex, RegexOptions.Compiled))
            {
                throw new ArgumentException("Invalid lambda expresion: " + expression);
            }

            this.expression = expression;
        }

        /// <summary>
        ///     Gets body of a q lambda expression.
        /// </summary>
        public string Expression
        {
            get { return expression; }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QLambda.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QLambda.</param>
        /// <returns>true if the specified System.Object is equal to the current QLambda; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var l = obj as QLambda;
            if (l == null)
            {
                return false;
            }

            return expression.Equals(l.expression);
        }

        /// <summary>
        ///     Determines whether the specified QLambda is equal to the current QLambda.
        /// </summary>
        /// <param name="l">The QLambda to compare with the current QLambda.</param>
        /// <returns>true if the specified QLambda is equal to the current QLambda; otherwise, false</returns>
        public bool Equals(QLambda l)
        {
            if (l == null)
            {
                return false;
            }

            return expression.Equals(l.expression);
        }

        /// <summary>
        ///     Serves as a hash function for a QLambda type.
        /// </summary>
        /// <returns>A hash code for the current QLambda</returns>
        public override int GetHashCode()
        {
            return expression.GetHashCode();
        }

        /// <summary>
        ///     Returns a System.String that represents the current QLambda.
        /// </summary>
        /// <returns>A System.String that represents the current QLambda</returns>
        public override string ToString()
        {
            return "QLambda: " + expression;
        }
    }
}