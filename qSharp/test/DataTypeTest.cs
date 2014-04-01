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

using NUnit.Framework;

namespace qSharp.test
{
    [TestFixture]
    internal class DataTypeTest
    {
        [Test]
        public void TestQKeyedTableConstruction()
        {
            var kt1 = new QKeyedTable(new QTable(new[] {"eid"}, new object[] {new[] {1001, 1002, 1003}}),
                                      new QTable(new[] {"pos", "dates"},
                                                 new object[]
                                                     {
                                                         new[] {"d1", "d2", "d3"},
                                                         new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)}
                                                     }));
            var kt2 = new QKeyedTable(new[] {"pos", "dates", "eid"}, new[] {"eid"},
                                      new object[]
                                          {
                                              new[] {"d1", "d2", "d3"},
                                              new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)},
                                              new[] {1001, 1002, 1003}
                                          });
            Assert.IsTrue(kt1.Equals(kt2));
        }
    }
}