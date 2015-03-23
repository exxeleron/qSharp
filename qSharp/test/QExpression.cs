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
using System.Collections.Generic;
using System.IO;

namespace qSharp.test
{
    internal class QExpressions
    {
        private readonly Dictionary<string, Dictionary<string, byte[]>> _expressions =
            new Dictionary<string, Dictionary<string, byte[]>>();

        private readonly Dictionary<string, Dictionary<string, object>> reference = new Dictionary
            <string, Dictionary<string, object>>
            {
                {
                    "q2",
                    new Dictionary<string, object>
                        {
                            {"1+`", new QException("type")},
                            {"()", new object[0]},
                            {"1", 1},
                            {"1i", 1},
                            {"-234h", (short) -234},
                            {"1b", true},
                            {"0x2a", (byte) 0x2a},
                            {"89421099511627575j", 89421099511627575},
                            {"3.234", 3.234},
                            {"5.5e", (float) 5.5},
                            {"\"0\"", '0'},
                            {"\"abc\"", "abc".ToCharArray()},
                            {"\"\"", "".ToCharArray()},
                            {
                                "\"quick brown fox jumps over a lazy dog\"",
                                "quick brown fox jumps over a lazy dog".ToCharArray()
                            },
                            {"`abc", "abc"},
                            {"`quickbrownfoxjumpsoveralazydog", "quickbrownfoxjumpsoveralazydog"},
                            {"2000.01.04D05:36:57.600", new QTimestamp(279417600000000)},
                            {"2001.01m", new QMonth(12)},
                            {"2001.01.01", new QDate(366)},
                            {"2000.05.01", new QDate(121)},
                            {"2000.01.04T05:36:57.600", new QDateTime(3.234)},
                            {"0D05:36:57.600", new QTimespan(20217600000000)},
                            {"12:01", new QMinute(721)},
                            {"12:05:00", new QSecond(43500)},
                            {"12:04:59.123", new QTime(43499123)},
                            {"0b", QTypes.GetQNull(QType.Bool)},
                            {"0x00", QTypes.GetQNull(QType.Byte)},
                            {"0Nh", QTypes.GetQNull(QType.Short)},
                            {"0N", QTypes.GetQNull(QType.Int)},
                            {"0Ni", QTypes.GetQNull(QType.Int)},
                            {"0Nj", QTypes.GetQNull(QType.Long)},
                            {"0Ne", QTypes.GetQNull(QType.Float)},
                            {"0n", QTypes.GetQNull(QType.Double)},
                            {"\" \"", QTypes.GetQNull(QType.Char)},
                            {"`", QTypes.GetQNull(QType.Symbol)},
                            {"0Np", QTypes.GetQNull(QType.Timestamp)},
                            {"0Nm", QTypes.GetQNull(QType.Month)},
                            {"0Nd", QTypes.GetQNull(QType.Date)},
                            {"0Nz", QTypes.GetQNull(QType.Datetime)},
                            {"0Nn", QTypes.GetQNull(QType.Timespan)},
                            {"0Nu", QTypes.GetQNull(QType.Minute)},
                            {"0Nv", QTypes.GetQNull(QType.Second)},
                            {"0Nt", QTypes.GetQNull(QType.Time)},

                            {"(0b;1b;0b)", new[] {false, true, false}},
                            {"(0x01;0x02;0xff)", new byte[] {1, 2, 255}},
                            {"(1h;2h;3h)", new short[] {1, 2, 3}},
                            {"(1i;2i;3i)", new[] {1, 2, 3}},
                            {"1 2 3", new[] {1, 2, 3}},
                            {"(1j;2j;3j)", new long[] {1, 2, 3}},
                            {"(5.5e; 8.5e)", new[] {5.5f, 8.5f}},
                            {"3.23 6.46", new[] {3.23, 6.46}},

                            {"(0b;1b;1b)", new Boolean[] {false, true, true}},
                            {"(0x02;0x02;0xff)", new Byte[] {2, 2, 255}},
                            {"(2h;2h;3h)", new Int16[] {2, 2, 3}},
                            {"(2i;2i;3i)", new Int32[] {2, 2, 3}},
                            {"2 2 3", new Int64[] {2, 2, 3}},
                            {"(2j;2j;3j)", new Int64[] {2, 2, 3}},
                            {"(6.5e; 8.5e)", new Single[] {6.5f, 8.5f}},
                            {"3.233 6.46", new Double[] {3.233, 6.46}},

                            {"(1;`bcd;\"0bc\";5.5e)", new object[] {1, "bcd", "0bc".ToCharArray(), (float) 5.5}},
                            {"(42;::;`foo)", new object[] { 42, null, "foo" }},
                            {"(enlist 1h; 2; enlist 3j)", new object[] {new short[] {1}, 2, new long[] {3}}},
                            {"(2;`bcd;\"0bc\";5.5e)", new object[] {1, "bcd", "0bc".ToCharArray(), (Single) 5.5}},
                            {"(enlist 2h; 2; enlist 3j)", new object[] {new Int16[] {1}, 2, new long[] {3}}},
                            {"`the`quick`brown`fox", new[] {"the", "quick", "brown", "fox"}},
                            {"``quick``fox", new[] {"", "quick", "", "fox" }},
                            {"``", new[] {"", ""}},
                            {"(\"quick\"; \"brown\"; \"fox\"; \"jumps\"; \"over\"; \"a lazy\"; \"dog\")", new object[] {"quick".ToCharArray(), "brown".ToCharArray(), "fox".ToCharArray(), "jumps".ToCharArray(), "over".ToCharArray(), "a lazy".ToCharArray(), "dog".ToCharArray() }},
                            {"(\"quick\"; \"brown\"; \"fox\")", new char[][] {"quick".ToCharArray(), "brown".ToCharArray(), "fox".ToCharArray() }},
                            {
                                "2000.01.04D05:36:57.600 0Np",
                                new[] {new QTimestamp(279417600000000), new QTimestamp(long.MinValue)}
                            },
                            {"(2001.01m; 0Nm)", new[] {new QMonth(12), new QMonth(int.MinValue)}},
                            {
                                "2001.01.01 2000.05.01 0Nd",
                                new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)}
                            },
                            {"2000.01.04T05:36:57.600 0Nz", new[] {new QDateTime(3.234), new QDateTime(double.NaN)}},
                            {"0D05:36:57.600 0Nn", new[] {new QTimespan(20217600000000), new QTimespan(long.MinValue)}},
                            {"12:01 0Nu", new[] {new QMinute(721), new QMinute(int.MinValue)}},
                            {"12:05:00 0Nv", new[] {new QSecond(43500), new QSecond(int.MinValue)}},
                            {"12:04:59.123 0Nt", new[] {new QTime(43499123), new QTime(int.MinValue)}},
                            {"::", null},
                            {"{x+y}", new QLambda("{x+y}")},
                            {"{x+y}[3]", new QProjection( new object[]{new QLambda("{x+y}"), 3})},
                            {"(enlist `a)!(enlist 1)", new QDictionary(new[] {"a"}, new[] {1})},
                            {"1 2!`abc`cdefgh", new QDictionary(new[] {1, 2}, new[] {"abc", "cdefgh"})},
                            {
                                "`abc`def`gh!([] one: 1 2 3; two: 4 5 6)", 
                                new QDictionary(new string[] { "abc", "def", "gh" },
                                                new QTable(new[] {"one", "two"}, new object[] {new[] {1L, 2L, 3L}, new[] {4L, 5L, 6L}}))
                            },
                            {"(`x`y!(`a;2))", new QDictionary(new string[] {"x", "y"},
                                                              new object[] {"a", 2})
                            },
                            {
                                "(1;2h;3.3;\"4\")!(`one;2 3;\"456\";(7;8 9))",
                                new QDictionary(new object[] {1, (short) 2, 3.3, '4'},
                                                new object[]
                                                    {
                                                        "one", new[] {2, 3}, "456".ToCharArray(),
                                                        new object[] {7, new[] {8, 9}}
                                                    })
                            },
                            {
                                "(0 1; 2 3)!`first`second",
                                new QDictionary(new object[] {new[] {0, 1}, new[] {2, 3}}, new[] {"first", "second"})
                            },
                            {
                                "`A`B`C!((1;2.2;3);(`x`y!(`a;2));5.5)",
                                new QDictionary(new[] {"A", "B", "C"},
                                                new object[]
                                                    {
                                                        new object[] {1, 2.2, 3},
                                                        new QDictionary(new[] {"x", "y"}, new object[] {"a", 2}), 5.5
                                                    })
                            },
                            {
                                "flip `abc`def!(1 2 3; 4 5 6)",
                                new QTable(new[] {"abc", "def"}, new object[] {new[] {1, 2, 3}, new[] {4, 5, 6}})
                            },
                            {
                                "flip `name`iq!(`Dent`Beeblebrox`Prefect;98 42 126)",
                                new QTable(new[] {"name", "iq"},
                                           new object[] {new[] {"Dent", "Beeblebrox", "Prefect"}, new[] {98, 42, 126}})
                            },
                            {
                                "flip `name`iq`grade!(`Dent`Beeblebrox`Prefect;98 42 126;\"a c\")",
                                new QTable(new[] {"name", "iq", "grade"},
                                           new object[] {new[] {"Dent", "Beeblebrox", "Prefect"}, new[] {98, 42, 126}, new[] {'a', ' ', 'c'}})
                            },
                            {
                                "flip `name`iq`fullname!(`Dent`Beeblebrox`Prefect;98 42 126;(\"Arthur Dent\"; \"Zaphod Beeblebrox\"; \"Ford Prefect\"))",
                                new QTable(new[] {"name", "iq", "fullname"},
                                           new object[] {new[] {"Dent", "Beeblebrox", "Prefect"}, new[] {98, 42, 126}, new object[]{"Arthur Dent".ToCharArray(), "Zaphod Beeblebrox".ToCharArray(), "Ford Prefect".ToCharArray()} })
                            },
                            {
                                "([] sc:1 2 3; nsc:(1 2; 3 4; 5 6 7))",
                                new QTable(new[] {"sc", "nsc"},
                                           new object[]
                                               {
                                                   new[] {1, 2, 3},
                                                   new object[] {new[] {1, 2}, new[] {3, 4}, new[] {5, 6, 7}}
                                               })
                            },
                            {
                                "([] name:`symbol$(); iq:`int$())",
                                new QTable(new[] {"name", "iq"}, new object[] {new string[] {}, new int[] {}})
                            },
                            {
                                "([] pos:`d1`d2`d3;dates:(2001.01.01;2000.05.01;0Nd))",
                                new QTable(new[] {"pos", "dates"},
                                           new object[]
                                               {
                                                   new[] {"d1", "d2", "d3"},
                                                   new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)}
                                               })
                            },
                            {
                                "([eid:1001 1002 1003] pos:`d1`d2`d3;dates:(2001.01.01;2000.05.01;0Nd))",
                                new QKeyedTable(new QTable(new[] {"eid"}, new object[] {new[] {1001, 1002, 1003}}),
                                                new QTable(new[] {"pos", "dates"},
                                                           new object[]
                                                               {
                                                                   new[] {"d1", "d2", "d3"},
                                                                   new[]
                                                                       {
                                                                           new QDate(366), new QDate(121),
                                                                           new QDate(int.MinValue)
                                                                       }
                                                               }))
                            },
                        }
                },
                {
                    "q3",
                    new Dictionary<string, object>
                        {
                            {"1+`", new QException("type")},
                            {"()", new object[0]},
                            {"1", (long) 1},
                            {"1i", 1},
                            {"-234h", (short) -234},
                            {"1b", true},
                            {"0x2a", (byte) 0x2a},
                            {"89421099511627575j", 89421099511627575},
                            {"3.234", 3.234},
                            {"5.5e", (float) 5.5},
                            {"\"0\"", '0'},
                            {"\"abc\"", "abc".ToCharArray()},
                            {"\"\"", "".ToCharArray()},
                            {
                                "\"quick brown fox jumps over a lazy dog\"",
                                "quick brown fox jumps over a lazy dog".ToCharArray()
                            },
                            {"`abc", "abc"},
                            {"`quickbrownfoxjumpsoveralazydog", "quickbrownfoxjumpsoveralazydog"},
                            {"2000.01.04D05:36:57.600", new QTimestamp(279417600000000)},
                            {"2001.01m", new QMonth(12)},
                            {"2001.01.01", new QDate(366)},
                            {"2000.05.01", new QDate(121)},
                            {"2000.01.04T05:36:57.600", new QDateTime(3.234)},
                            {"0D05:36:57.600", new QTimespan(20217600000000)},
                            {"12:01", new QMinute(721)},
                            {"12:05:00", new QSecond(43500)},
                            {"12:04:59.123", new QTime(43499123)},
                            {"0b", QTypes.GetQNull(QType.Bool)},
                            {"0x00", QTypes.GetQNull(QType.Byte)},
                            {"0Nh", QTypes.GetQNull(QType.Short)},
                            {"0N", QTypes.GetQNull(QType.Long)},
                            {"0Ni", QTypes.GetQNull(QType.Int)},
                            {"0Nj", QTypes.GetQNull(QType.Long)},
                            {"0Ne", QTypes.GetQNull(QType.Float)},
                            {"0n", QTypes.GetQNull(QType.Double)},
                            {"\" \"", QTypes.GetQNull(QType.Char)},
                            {"`", QTypes.GetQNull(QType.Symbol)},
                            {"0Np", QTypes.GetQNull(QType.Timestamp)},
                            {"0Nm", QTypes.GetQNull(QType.Month)},
                            {"0Nd", QTypes.GetQNull(QType.Date)},
                            {"0Nz", QTypes.GetQNull(QType.Datetime)},
                            {"0Nn", QTypes.GetQNull(QType.Timespan)},
                            {"0Nu", QTypes.GetQNull(QType.Minute)},
                            {"0Nv", QTypes.GetQNull(QType.Second)},
                            {"0Nt", QTypes.GetQNull(QType.Time)},

                            {"(0b;1b;0b)", new[] {false, true, false}},
                            {"(0x01;0x02;0xff)", new byte[] {1, 2, 255}},
                            {"(1h;2h;3h)", new short[] {1, 2, 3}},
                            {"(1i;2i;3i)", new[] {1, 2, 3}},
                            {"1 2 3", new long[] {1, 2, 3}},
                            {"(1j;2j;3j)", new long[] {1, 2, 3}},
                            {"(5.5e; 8.5e)", new[] {5.5f, 8.5f}},
                            {"3.23 6.46", new[] {3.23, 6.46}},

                            {"(0b;1b;1b)", new Boolean[] {false, true, true}},
                            {"(0x02;0x02;0xff)", new Byte[] {2, 2, 255}},
                            {"(2h;2h;3h)", new Int16[] {2, 2, 3}},
                            {"(2i;2i;3i)", new Int32[] {2, 2, 3}},
                            {"2 2 3", new Int64[] {2, 2, 3}},
                            {"(2j;2j;3j)", new Int64[] {2, 2, 3}},
                            {"(6.5e; 8.5e)", new Single[] {6.5f, 8.5f}},
                            {"3.233 6.46", new Double[] {3.233, 6.46}},

                            {"(1;`bcd;\"0bc\";5.5e)", new object[] {(long) 1, "bcd", "0bc".ToCharArray(), (float) 5.5}},
                            {"(42;::;`foo)", new object[] { 42L, null, "foo" }},
                            {"(enlist 1h; 2; enlist 3j)", new object[] {new short[] {1}, (long) 2, new long[] {3}}},
                            {"(2;`bcd;\"0bc\";5.5e)", new object[] {1, "bcd", "0bc".ToCharArray(), (Single) 5.5}},
                            {"(enlist 2h; 2; enlist 3j)", new object[] {new Int16[] {1}, 2, new long[] {3}}},
                            {"`the`quick`brown`fox", new[] {"the", "quick", "brown", "fox"}},
                            {"``quick``fox", new[] {"", "quick", "", "fox" }},
                            {"``", new[] {"", ""}},
                            {"(\"quick\"; \"brown\"; \"fox\"; \"jumps\"; \"over\"; \"a lazy\"; \"dog\")", new object[] {"quick".ToCharArray(), "brown".ToCharArray(), "fox".ToCharArray(), "jumps".ToCharArray(), "over".ToCharArray(), "a lazy".ToCharArray(), "dog".ToCharArray() }},
                            {"(\"quick\"; \"brown\"; \"fox\")", new char[][] {"quick".ToCharArray(), "brown".ToCharArray(), "fox".ToCharArray() }},
                            {
                                "2000.01.04D05:36:57.600 0Np",
                                new[] {new QTimestamp(279417600000000), new QTimestamp(long.MinValue)}
                            },
                            {"(2001.01m; 0Nm)", new[] {new QMonth(12), new QMonth(int.MinValue)}},
                            {
                                "2001.01.01 2000.05.01 0Nd",
                                new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)}
                            },
                            {"2000.01.04T05:36:57.600 0Nz", new[] {new QDateTime(3.234), new QDateTime(double.NaN)}},
                            {"0D05:36:57.600 0Nn", new[] {new QTimespan(20217600000000), new QTimespan(long.MinValue)}},
                            {"12:01 0Nu", new[] {new QMinute(721), new QMinute(int.MinValue)}},
                            {"12:05:00 0Nv", new[] {new QSecond(43500), new QSecond(int.MinValue)}},
                            {"12:04:59.123 0Nt", new[] {new QTime(43499123), new QTime(int.MinValue)}},
                            {"::", null},
                            {"{x+y}", new QLambda("{x+y}")},
                            {"{x+y}[3]", new QProjection( new object[] {new QLambda("{x+y}"), (long) 3})},
                            {"(enlist `a)!(enlist 1)", new QDictionary(new[] {"a"}, new long[] {1})},
                            {"1 2!`abc`cdefgh", new QDictionary(new long[] {1, 2}, new[] {"abc", "cdefgh"})},
                                                        {
                                "`abc`def`gh!([] one: 1 2 3; two: 4 5 6)", 
                                new QDictionary(new string[] { "abc", "def", "gh" },
                                                new QTable(new[] {"one", "two"}, new object[] {new[] {1L, 2L, 3L}, new[] {4L, 5L, 6L}}))
                            },
                            {"(`x`y!(`a;2))", new QDictionary(new string[] {"x", "y"},
                                                              new object[] {"a", 2L})
                            },
                            {
                                "(1;2h;3.3;\"4\")!(`one;2 3;\"456\";(7;8 9))",
                                new QDictionary(new object[] {(long) 1, (short) 2, 3.3, '4'},
                                                new object[]
                                                    {
                                                        "one", new long[] {2, 3}, "456".ToCharArray(),
                                                        new object[] {(long) 7, new long[] {8, 9}}
                                                    })
                            },
                            {
                                "(0 1; 2 3)!`first`second",
                                new QDictionary(new object[] {new long[] {0, 1}, new long[] {2, 3}},
                                                new[] {"first", "second"})
                            },
                            {
                                "`A`B`C!((1;2.2;3);(`x`y!(`a;2));5.5)",
                                new QDictionary(new[] {"A", "B", "C"},
                                                new object[]
                                                    {
                                                        new object[] {(long) 1, 2.2, (long) 3},
                                                        new QDictionary(new[] {"x", "y"}, new object[] {"a", (long) 2}),
                                                        5.5
                                                    })
                            },
                            {
                                "flip `abc`def!(1 2 3; 4 5 6)",
                                new QTable(new[] {"abc", "def"},
                                           new object[] {new long[] {1, 2, 3}, new long[] {4, 5, 6}})
                            },
                            {
                                "flip `name`iq!(`Dent`Beeblebrox`Prefect;98 42 126)",
                                new QTable(new[] {"name", "iq"},
                                           new object[]
                                               {new[] {"Dent", "Beeblebrox", "Prefect"}, new long[] {98, 42, 126}})
                            },
                            {
                                "flip `name`iq`grade!(`Dent`Beeblebrox`Prefect;98 42 126;\"a c\")",
                                new QTable(new[] {"name", "iq", "grade"},
                                           new object[] {new[] {"Dent", "Beeblebrox", "Prefect"}, new long[] {98, 42, 126}, new[] {'a', ' ', 'c'}})
                            },
                            {
                                "flip `name`iq`fullname!(`Dent`Beeblebrox`Prefect;98 42 126;(\"Arthur Dent\"; \"Zaphod Beeblebrox\"; \"Ford Prefect\"))",
                                new QTable(new[] {"name", "iq", "fullname"},
                                           new object[] {new[] {"Dent", "Beeblebrox", "Prefect"}, new long[] {98, 42, 126}, new object[]{"Arthur Dent".ToCharArray(), "Zaphod Beeblebrox".ToCharArray(), "Ford Prefect".ToCharArray()} })
                            },
                            {
                                "([] sc:1 2 3; nsc:(1 2; 3 4; 5 6 7))",
                                new QTable(new[] {"sc", "nsc"},
                                           new object[]
                                               {
                                                   new long[] {1, 2, 3},
                                                   new object[]
                                                       {new long[] {1, 2}, new long[] {3, 4}, new long[] {5, 6, 7}}
                                               })
                            },
                            {
                                "([] name:`symbol$(); iq:`int$())",
                                new QTable(new[] {"name", "iq"}, new object[] {new string[] {}, new int[] {}})
                            },
                            {
                                "([] pos:`d1`d2`d3;dates:(2001.01.01;2000.05.01;0Nd))",
                                new QTable(new[] {"pos", "dates"},
                                           new object[]
                                               {
                                                   new[] {"d1", "d2", "d3"},
                                                   new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)}
                                               })
                            },
                            {
                                "([eid:1001 1002 1003] pos:`d1`d2`d3;dates:(2001.01.01;2000.05.01;0Nd))",
                                new QKeyedTable(
                                    new QTable(new[] {"eid"}, new object[] {new long[] {1001, 1002, 1003}}),
                                    new QTable(new[] {"pos", "dates"},
                                               new object[]
                                                   {
                                                       new[] {"d1", "d2", "d3"},
                                                       new[] {new QDate(366), new QDate(121), new QDate(int.MinValue)}
                                                   }))
                            },
                            {
                                "\"G\"$\"8c680a01-5a49-5aab-5a65-d4bfddb6a661\"",
                                new Guid("8c680a01-5a49-5aab-5a65-d4bfddb6a661")
                            },
                            {
                                "(\"G\"$\"8c680a01-5a49-5aab-5a65-d4bfddb6a661\"; 0Ng)",
                                new[]
                                    {
                                        new Guid("8c680a01-5a49-5aab-5a65-d4bfddb6a661"),
                                        Guid.Empty
                                    }
                            },
                        }
                }
            };

        public QExpressions(Dictionary<string, string> files)
        {
            foreach (var pair in files)
            {
                _expressions[pair.Key] = new Dictionary<string, byte[]>();
                var expressionsFile = new StreamReader(pair.Value);

                using (expressionsFile)
                {
                    while (true)
                    {
                        string query = expressionsFile.ReadLine();
                        string expression = expressionsFile.ReadLine();

                        if (query != null && expression != null)
                        {
                            _expressions[pair.Key][query] = StringToByteArray(expression);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetExpressions(string version)
        {
            return _expressions[version].Keys;
        }


        public byte[] GetBinaryExpression(string version, string key)
        {
            return _expressions[version][key];
        }

        public object GetReferenceObject(string version, string key)
        {
            try
            {
                return reference[version][key];
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(string.Format("Cannot retrieve key: {0}", key));
                throw e;
            }
        }

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            var bytes = new byte[NumberChars/2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}