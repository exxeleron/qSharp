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

using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace qSharp.test
{
    [TestFixture]
    internal class QWriterTest
    {
        [Test]
        public void TestObjectSerializationQ2()
        {
            var expressions = new QExpressions(new Dictionary<string, string> {{"q2", "..\\..\\test\\QExpressions.out"}});
            foreach (string expr in expressions.GetExpressions("q2"))
            {
                var stream = new MemoryStream();
                var reader = new BinaryReader(stream);
                var writer = new QWriter(stream, Encoding.ASCII, 1);
                int dataSize = writer.Write(expressions.GetReferenceObject("q2", expr), MessageType.Sync);
                stream.Seek(8, SeekOrigin.Begin);

                Assert.AreEqual(expressions.GetBinaryExpression("q2", expr), reader.ReadBytes(dataSize),
                                "Serialization failed for q expression: " + expr);

                stream.Close();
            }
        }

        [Test]
        public void TestObjectSerializationQ3()
        {
            var expressions =
                new QExpressions(new Dictionary<string, string> {{"q3", "..\\..\\test\\QExpressions.3.out"}});
            foreach (string expr in expressions.GetExpressions("q3"))
            {
                var stream = new MemoryStream();
                var reader = new BinaryReader(stream);
                var writer = new QWriter(stream, Encoding.ASCII, 3);
                int dataSize = writer.Write(expressions.GetReferenceObject("q3", expr), MessageType.Sync);
                stream.Seek(8, SeekOrigin.Begin);

                Assert.AreEqual(expressions.GetBinaryExpression("q3", expr), reader.ReadBytes(dataSize),
                                "Serialization failed for q expression: " + expr);

                stream.Close();
            }
        }
    }
}