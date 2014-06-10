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
using NUnit.Framework;

namespace qSharp.test
{
    [TestFixture]
    internal class DateTimeTest
    {
        [Test]
        public void TestQDate()
        {
            Assert.AreEqual(-1645, new QDate(new DateTime(1995, 7, 1)).Value);
            Assert.AreEqual(-365, new QDate(new DateTime(1999, 1, 1)).Value);
            Assert.AreEqual(0, new QDate(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(2008, new QDate(new DateTime(2005, 7, 1)).Value);
            Assert.AreEqual(3653, new QDate(new DateTime(2010, 1, 1)).Value);
        }

        [Test]
        public void testQDateToString()
        {
            Assert.AreEqual("1995.07.01", new QDate(-1645).ToString());
            Assert.AreEqual("1999.01.01", new QDate(-365).ToString());
            Assert.AreEqual("2000.01.01", new QDate(0).ToString());
            Assert.AreEqual("2005.07.01", new QDate(2008).ToString());
            Assert.AreEqual("2010.01.01", new QDate(3653).ToString());

            Assert.AreEqual("0Nd", new QDate(int.MinValue).ToString());
        }

        [Test]
        public void testQDateFromString()
        {
            Assert.AreEqual(new QDate(-1645), QDate.FromString("1995.07.01"));
            Assert.AreEqual(new QDate(-365), QDate.FromString("1999.01.01"));
            Assert.AreEqual(new QDate(0), QDate.FromString("2000.01.01"));
            Assert.AreEqual(new QDate(2008), QDate.FromString("2005.07.01"));
            Assert.AreEqual(new QDate(3653), QDate.FromString("2010.01.01"));

            Assert.AreEqual(new QDate(int.MinValue), QDate.FromString(null));
            Assert.AreEqual(new QDate(int.MinValue), QDate.FromString(""));
            Assert.AreEqual(new QDate(int.MinValue), QDate.FromString("0Nd"));
        }

        [Test]
        public void TestQDateTime()
        {
            Assert.AreEqual(-364.0000116, new QDateTime(new DateTime(1999, 1, 1, 23, 59, 59)).Value, delta: 0.1);
            Assert.AreEqual(0, new QDateTime(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(2008.0833218, new QDateTime(new DateTime(2005, 7, 1, 1, 59, 59)).Value, delta: 0.1);
            Assert.AreEqual(3653.5, new QDateTime(new DateTime(2010, 1, 1, 14, 23, 42)).Value, delta: 0.1);
        }

        [Test]
        public void testQDateTimeToString()
        {
            Assert.AreEqual("1999.01.01T23:59:59.006", new QDateTime(-364.0000115).ToString());
            Assert.AreEqual("2000.01.01T00:00:00.000", new QDateTime(0.0).ToString());
            Assert.AreEqual("2005.07.01T01:59:59.004", new QDateTime(2008.0833218).ToString());
            Assert.AreEqual("2010.01.01T14:23:42.029", new QDateTime(3653.599792).ToString());

            Assert.AreEqual("0Nz", new QDateTime(Double.NaN).ToString());
        }

        [Test]
        public void testQDateTimeFromString()
        {
            Assert.AreEqual(new QDateTime(-364.0000115).Value, QDateTime.FromString("1999.01.01T23:59:59.000").Value, 0.001);
            Assert.AreEqual(new QDateTime(0.0).Value, QDateTime.FromString("2000.01.01T00:00:00.000").Value, 0.001);
            Assert.AreEqual(new QDateTime(2008.0833218).Value, QDateTime.FromString("2005.07.01T01:59:59.000").Value, 0.001);
            Assert.AreEqual(new QDateTime(3653.599792).Value, QDateTime.FromString("2010.01.01T14:23:42.000").Value, 0.001);

            Assert.AreEqual(new QDateTime(Double.NaN), QDateTime.FromString(null));
            Assert.AreEqual(new QDateTime(Double.NaN), QDateTime.FromString(""));
            Assert.AreEqual(new QDateTime(Double.NaN), QDateTime.FromString("0Nz"));
        }

        [Test]
        public void TestQMinute()
        {
            Assert.AreEqual(0, new QMinute(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(810, new QMinute(new DateTime(2000, 1, 1, 13, 30, 13)).Value);
            Assert.AreEqual(1439, new QMinute(new DateTime(2000, 1, 1, 23, 59, 59)).Value);
        }

        [Test]
        public void testQMinuteToString()
        {
            Assert.AreEqual("00:00", new QMinute(0).ToString());
            Assert.AreEqual("13:30", new QMinute(810).ToString());
            Assert.AreEqual("23:59", new QMinute(1439).ToString());

            Assert.AreEqual("-13:30", new QMinute(-810).ToString());
            Assert.AreEqual("52:23", new QMinute(3143).ToString());

            Assert.AreEqual("0Nu", new QMinute(int.MinValue).ToString());
        }

        [Test]
        public void testQMinuteFromString()
        {
            Assert.AreEqual(new QMinute(0), QMinute.FromString("00:00"));
            Assert.AreEqual(new QMinute(810), QMinute.FromString("13:30"));
            Assert.AreEqual(new QMinute(1439), QMinute.FromString("23:59"));

            Assert.AreEqual(new QMinute(-810), QMinute.FromString("-13:30"));
            Assert.AreEqual(new QMinute(3143), QMinute.FromString("52:23"));

            Assert.AreEqual(new QMinute(int.MinValue), QMinute.FromString(null));
            Assert.AreEqual(new QMinute(int.MinValue), QMinute.FromString(""));
            Assert.AreEqual(new QMinute(int.MinValue), QMinute.FromString("0Nu"));
        }

        [Test]
        public void TestQMonth()
        {
            Assert.AreEqual(-60, new QMonth(new DateTime(1995, 1, 1)).Value);
            Assert.AreEqual(-54, new QMonth(new DateTime(1995, 7, 1)).Value);
            Assert.AreEqual(0, new QMonth(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(66, new QMonth(new DateTime(2005, 7, 1)).Value);
            Assert.AreEqual(120, new QMonth(new DateTime(2010, 1, 1)).Value);
        }

        [Test]
        public void testQMonthToString()
        {
            Assert.AreEqual("1995.01m", new QMonth(-60).ToString());
            Assert.AreEqual("1995.07m", new QMonth(-54).ToString());
            Assert.AreEqual("2000.01m", new QMonth(0).ToString());
            Assert.AreEqual("2005.07m", new QMonth(66).ToString());
            Assert.AreEqual("2010.01m", new QMonth(120).ToString());

            Assert.AreEqual("0Nm", new QMonth(int.MinValue).ToString());
        }

        [Test]
        public void testQMonthFromString()
        {
            Assert.AreEqual(new QMonth(-60), QMonth.FromString("1995.01m"));
            Assert.AreEqual(new QMonth(-54), QMonth.FromString("1995.07m"));
            Assert.AreEqual(new QMonth(0), QMonth.FromString("2000.01m"));
            Assert.AreEqual(new QMonth(66), QMonth.FromString("2005.07m"));
            Assert.AreEqual(new QMonth(120), QMonth.FromString("2010.01m"));

            Assert.AreEqual(new QMonth(int.MinValue), QMonth.FromString(null));
            Assert.AreEqual(new QMonth(int.MinValue), QMonth.FromString(""));
            Assert.AreEqual(new QMonth(int.MinValue), QMonth.FromString("0Nm"));
        }

        [Test]
        public void TestQSecond()
        {
            Assert.AreEqual(0, new QSecond(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(48613L, new QSecond(new DateTime(2000, 1, 1, 13, 30, 13)).Value);
            Assert.AreEqual(86399L, new QSecond(new DateTime(2000, 1, 1, 23, 59, 59)).Value);
        }

        [Test]
        public void testQSecondToString()
        {
            Assert.AreEqual("00:00:00", new QSecond(0).ToString());
            Assert.AreEqual("13:30:13", new QSecond(48613).ToString());
            Assert.AreEqual("23:59:59", new QSecond(86399).ToString());

            Assert.AreEqual("51:46:39", new QSecond(186399).ToString());
            Assert.AreEqual("-23:59:59", new QSecond(-86399).ToString());

            Assert.AreEqual("0Nv", new QSecond(int.MinValue).ToString());
        }

        [Test]
        public void testQSecondFromString()
        {
            Assert.AreEqual(new QSecond(0), QSecond.FromString("00:00:00"));
            Assert.AreEqual(new QSecond(48613), QSecond.FromString("13:30:13"));
            Assert.AreEqual(new QSecond(86399), QSecond.FromString("23:59:59"));

            Assert.AreEqual(new QSecond(186399), QSecond.FromString("51:46:39"));
            Assert.AreEqual(new QSecond(-86399), QSecond.FromString("-23:59:59"));

            Assert.AreEqual(new QSecond(int.MinValue), QSecond.FromString(null));
            Assert.AreEqual(new QSecond(int.MinValue), QSecond.FromString(""));
            Assert.AreEqual(new QSecond(int.MinValue), QSecond.FromString("0Nv"));
        }

        [Test]
        public void TestQTime()
        {
            Assert.AreEqual(0, new QTime(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(48613000L, new QTime(new DateTime(2000, 1, 1, 13, 30, 13)).Value);
            Assert.AreEqual(86399000L, new QTime(new DateTime(2000, 1, 1, 23, 59, 59)).Value);
        }

        [Test]
        public void testQTimeToString()
        {
            Assert.AreEqual("00:00:00.000", new QTime(0).ToString());
            Assert.AreEqual("13:30:13.000", new QTime(48613000).ToString());
            Assert.AreEqual("23:59:59.000", new QTime(86399000).ToString());

            Assert.AreEqual("51:46:39.050", new QTime(186399050).ToString());
            Assert.AreEqual("-23:59:59.100", new QTime(-86399100).ToString());

            Assert.AreEqual("0Nt", new QTime(int.MinValue).ToString());
        }

        [Test]
        public void testQTimeFromString()
        {
            Assert.AreEqual(new QTime(48613000), QTime.FromString("13:30:13.000"));
            Assert.AreEqual(new QTime(0), QTime.FromString("00:00:00.000"));
            Assert.AreEqual(new QTime(86399000), QTime.FromString("23:59:59.000"));

            Assert.AreEqual(new QTime(186399001), QTime.FromString("51:46:39.001"));
            Assert.AreEqual(new QTime(-86399000), QTime.FromString("-23:59:59.000"));

            Assert.AreEqual(new QTime(int.MinValue), QTime.FromString(null));
            Assert.AreEqual(new QTime(int.MinValue), QTime.FromString(""));
            Assert.AreEqual(new QTime(int.MinValue), QTime.FromString("0Nt"));
        }

        [Test]
        public void TestQTimespan()
        {
            Assert.AreEqual(0, new QTimespan(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(48613000000000L, new QTimespan(new DateTime(2000, 1, 1, 13, 30, 13)).Value);
            Assert.AreEqual(86399000000000L, new QTimespan(new DateTime(2000, 1, 1, 23, 59, 59)).Value);
        }

        [Test]
        public void testQTimespanToString()
        {
            Assert.AreEqual("0D00:00:00.000000000", new QTimespan(0L).ToString());
            Assert.AreEqual("0D13:30:13.000000000", new QTimespan(48613000000000L).ToString());
            Assert.AreEqual("0D13:30:13.000000100", new QTimespan(48613000000100L).ToString());
            Assert.AreEqual("-0D13:30:13.000001000", new QTimespan(-48613000001000L).ToString());
            Assert.AreEqual("1D13:30:13.000000000", new QTimespan(135013000000000L).ToString());
            Assert.AreEqual("0D23:59:59.000000000", new QTimespan(86399000000000L).ToString());
            Assert.AreEqual("2D23:59:59.000000000", new QTimespan(259199000000000L).ToString());
            Assert.AreEqual("-2D23:59:59.000000000", new QTimespan(-259199000000000L).ToString());

            Assert.AreEqual("0Nn", new QTimespan(long.MinValue).ToString());
        }

        [Test]
        public void testQTimespanFromString()
        {
            Assert.AreEqual(new QTimespan(0L), QTimespan.FromString("0D00:00:00.000000000"));
            Assert.AreEqual(new QTimespan(48613000000000L), QTimespan.FromString("0D13:30:13.000000000"));
            Assert.AreEqual(new QTimespan(48613000000100L), QTimespan.FromString("0D13:30:13.000000100"));
            Assert.AreEqual(new QTimespan(-48613000000000L), QTimespan.FromString("-0D13:30:13.000000000"));
            Assert.AreEqual(new QTimespan(-48613000010000L), QTimespan.FromString("-0D13:30:13.000010000"));
            Assert.AreEqual(new QTimespan(135013000000000L), QTimespan.FromString("1D13:30:13.000000000"));
            Assert.AreEqual(new QTimespan(86399000000000L), QTimespan.FromString("0D23:59:59.000000000"));
            Assert.AreEqual(new QTimespan(259199000000000L), QTimespan.FromString("2D23:59:59.000000000"));
            Assert.AreEqual(new QTimespan(-259199000000000L), QTimespan.FromString("-2D23:59:59.000000000"));

            Assert.AreEqual(new QTimespan(long.MinValue), QTimespan.FromString(null));
            Assert.AreEqual(new QTimespan(long.MinValue), QTimespan.FromString(""));
            Assert.AreEqual(new QTimespan(long.MinValue), QTimespan.FromString("0Nn"));
        }

        [Test]
        public void TestQTimestamp()
        {
            Assert.AreEqual(-142079387000000000L, new QTimestamp(new DateTime(1995, 7, 1, 13, 30, 13)).Value);
            Assert.AreEqual(-31449601000000000L, new QTimestamp(new DateTime(1999, 1, 1, 23, 59, 59)).Value);
            Assert.AreEqual(0, new QTimestamp(new DateTime(2000, 1, 1)).Value);
            Assert.AreEqual(173498399000000000L, new QTimestamp(new DateTime(2005, 7, 1, 1, 59, 59)).Value);
            Assert.AreEqual(315671022000000000L, new QTimestamp(new DateTime(2010, 1, 1, 14, 23, 42)).Value);
        }

        [Test]
        public void testQTimestampToString()
        {
            Assert.AreEqual("1995.07.01D13:30:13.000000000", new QTimestamp(-142079387000000000L).ToString());
            Assert.AreEqual("1999.01.01D23:59:59.000000000", new QTimestamp(-31449601000000000L).ToString());
            Assert.AreEqual("2000.01.01D00:00:00.000000000", new QTimestamp(0L).ToString());
            Assert.AreEqual("2005.07.01D01:59:59.000000012", new QTimestamp(173498399000000012L).ToString());
            Assert.AreEqual("2010.01.01D14:23:42.000000066", new QTimestamp(315671022000000066L).ToString());

            Assert.AreEqual("0Np", new QTimestamp(long.MinValue).ToString());
        }

        [Test]
        public void testQTimestampFromString()
        {
            Assert.AreEqual(new QTimestamp(-142079387000000000L), QTimestamp.FromString("1995.07.01D13:30:13.000000000"));
            Assert.AreEqual(new QTimestamp(-31449601000000000L), QTimestamp.FromString("1999.01.01D23:59:59.000000000"));
            Assert.AreEqual(new QTimestamp(0L), QTimestamp.FromString("2000.01.01D00:00:00.000000000"));
            Assert.AreEqual(new QTimestamp(173498399000000012L), QTimestamp.FromString("2005.07.01D01:59:59.000000012"));
            Assert.AreEqual(new QTimestamp(315671022000000066L), QTimestamp.FromString("2010.01.01D14:23:42.000000066"));

            Assert.AreEqual(new QTimestamp(long.MinValue), QTimestamp.FromString(null));
            Assert.AreEqual(new QTimestamp(long.MinValue), QTimestamp.FromString(""));
            Assert.AreEqual(new QTimestamp(long.MinValue), QTimestamp.FromString("0Np"));
        }
    }
}