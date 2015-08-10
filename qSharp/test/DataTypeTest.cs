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
            var kt1 = new QKeyedTable(new QTable(new[] { "eid" }, new object[] { new[] { 1001, 1002, 1003 } }),
                new QTable(new[] { "pos", "dates" },
                    new object[]
                    {
                        new[] {"d1", "d2", "d3"},
                        new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)}
                    }));
            var kt2 = new QKeyedTable(new[] { "pos", "dates", "eid" }, new[] { "eid" },
                new object[]
                {
                    new[] {"d1", "d2", "d3"},
                    new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)},
                    new[] {1001, 1002, 1003}
                });
            Assert.IsTrue(kt1.Equals(kt2));
        }

        [Test]
        public void TestQDictionary()
        {
            string[] keys = new string[] { "foo", "bar", "z" };
            object[] values = new object[] { 1, "val", null };

            var d = new QDictionary(keys, values);
            Assert.IsTrue(d.Equals(new QDictionary(keys, values)));

            var e = d.GetEnumerator();
            int i = 0;
            while (e.MoveNext())
            {
                var kv = e.Current;
                Assert.AreEqual(keys[i], kv.Key);
                Assert.AreEqual(values[i], kv.Value);
                i++;
            }

            Assert.AreEqual(i, d.Count);

            var table = new QTable(new[] { "eid" }, new object[] { new[] { 1001, 1002, 1003 } });
            d = new QDictionary(keys, table);
            Assert.IsTrue(d.Equals(new QDictionary(keys, table)));

            e = d.GetEnumerator();
            i = 0;
            while (e.MoveNext())
            {
                var kv = e.Current;
                Assert.AreEqual(keys[i], kv.Key);
                Assert.AreEqual(table[i].ToArray(), ((QTable.Row)kv.Value).ToArray());
                i++;
            }

            Assert.AreEqual(i, d.Count);
        }

        [Test]
        public void TestQTable()
        {
            var columns = new[] { "pos", "dates" };
            var data = new object[] { new[] { "d1", "d2", "d3" }, new[] { 1001, 1002, 1003 } };

            var t = new QTable(columns, data);
            Assert.AreEqual(t, new QTable(columns, data));

            int i = 0;
            var e = t.GetEnumerator();

            while (e.MoveNext())
            {
                var r = e.Current;
                Assert.AreEqual(t[i].ToArray(), r.ToArray());
                i++;
            }

            Assert.AreEqual(i, t.RowsCount);
        }

        [Test]
        public void TestQKeyedTable()
        {
            var columns = new[] { "pos", "dates", "eid" };
            var keyColumns = new[] { "eid" };
            var data = new object[] { new[] { "d1", "d2", "d3" }, new[] { new QDate(366), new QDate(121), new QDate(int.MinValue) }, new[] { 1001, 1002, 1003 } };

            var kt = new QKeyedTable(columns, keyColumns, data);
            Assert.AreEqual(kt, new QKeyedTable(columns, keyColumns, data));

            int i = 0;
            var e = kt.GetEnumerator();

            while (e.MoveNext())
            {
                var ktp = e.Current;
                Assert.AreEqual(kt.Keys[i], ktp.Key);
                Assert.AreEqual(kt.Values[i], ktp.Value);
                i++;
            }

            Assert.AreEqual(i, kt.RowsCount);
        }
    }
}