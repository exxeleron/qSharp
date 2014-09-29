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
    ///     Represents a q projection.
    /// </summary>
    public sealed class QProjection : QFunction
    {
        private readonly Array parameters;

        /// <summary>
        ///     Creates new QProjection instance with given parameters.
        /// </summary>
        public QProjection(Array parameters)
            : base((byte) QType.Projection)
        {
            this.parameters = parameters;
        }

        /// <summary>
        ///     Gets parameters of a q lambda expression.
        /// </summary>
        public Array Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        ///     Determines whether the specified System.Object is equal to the current QProjection.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current QProjection.</param>
        /// <returns>true if the specified System.Object is equal to the current QProjection; otherwise, false</returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var p = obj as QProjection;
            if (p == null)
            {
                return false;
            }

            return Utils.ArrayEquals(parameters, p.parameters);
        }

        /// <summary>
        ///     Determines whether the specified QProjection is equal to the current QProjection.
        /// </summary>
        /// <param name="l">The QProjection to compare with the current QProjection.</param>
        /// <returns>true if the specified QProjection is equal to the current QProjection; otherwise, false</returns>
        public bool Equals(QProjection p)
        {
            if (p == null)
            {
                return false;
            }

            return Utils.ArrayEquals(parameters, p.parameters);
        }

        /// <summary>
        ///     Serves as a hash function for a QProjection type.
        /// </summary>
        /// <returns>A hash code for the current QProjection</returns>
        public override int GetHashCode()
        {
            return parameters == null ? 0 : parameters.Length;
        }

        /// <summary>
        ///     Returns a System.String that represents the current QLambda.
        /// </summary>
        /// <returns>A System.String that represents the current QLambda</returns>
        public override string ToString()
        {
            return "QProjection: " + (parameters == null ? "<null>" : Utils.ArrayToString(parameters));
        }
    }
}