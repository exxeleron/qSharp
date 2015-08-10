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
    ///     Common interface for the q table types.
    /// </summary>
    public interface IQTable
    {
        /// <summary>
        ///     Gets a number of rows in table.
        /// </summary>
        int RowsCount { get; }

        /// <summary>
        ///     Gets a number of columns in table.
        /// </summary>
        int ColumnsCount { get; }
    }
}