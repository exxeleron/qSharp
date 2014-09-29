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
using System.Text;
using NUnit.Framework;

namespace qSharp.test
{
    internal class FunctionMock : QFunction
    {
        internal FunctionMock()
            : base(0)
        { }

        public bool Equals(FunctionMock f)
        {
            return f is QFunction;
        }

        public override bool Equals(object o)
        {
            return o is QFunction;
        }
    }

    [TestFixture]
    internal class QReaderTest
    {
        [Test]
        public void TestCompressedObjectDeserializationQ2()
        {
            var expressions =
                new QExpressions(new Dictionary<string, string> {{"q2", "..\\..\\test\\QCompressedExpressions.out"}});
            var reference = new Dictionary<string, object>();

            var q1000 = new string[1000];
            var q200 = new object[] {new int[200], new int[200], new string[200]};
            for (int i = 0; i < q1000.Length; i++)
            {
                q1000[i] = "q";
            }
            for (int i = 0; i < 200; i++)
            {
                ((int[]) q200[0])[i] = i;
                ((int[]) q200[1])[i] = i + 25;
                ((string[]) q200[2])[i] = "a";
            }

            reference["1000#`q"] = q1000;
            reference["([] q:1000#`q)"] = new QTable(new[] {"q"}, new object[] {q1000});
            reference["([] a:til 200;b:25+til 200;c:200#`a)"] = new QTable(new[] {"a", "b", "c"}, q200);

            foreach (string expr in expressions.GetExpressions("q2"))
            {
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                var reader = new QReader(stream, Encoding.ASCII, QBasicConnection.DefaultMaxReadingChunk);
                byte[] binaryExpr = expressions.GetBinaryExpression("q2", expr);
                writer.Write((byte) 1); // little endian
                writer.Write((byte) 0);
                writer.Write((byte) 1); // compressed
                writer.Write((byte) 0);
                writer.Write(binaryExpr.Length + 8);
                writer.Write(binaryExpr);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                try
                {
                    object obj = reader.Read().Data;
                    if (obj is QDictionary || obj is QTable || obj is QLambda)
                    {
                        // force usage of Equals method
                        Assert.IsTrue(reference[expr].Equals(obj), "Deserialization failed for q expression: " + expr);
                    }
                    else
                    {
                        Assert.AreEqual(reference[expr], obj, "Deserialization failed for q expression: " + expr);
                    }
                }
                catch (System.Exception e)
                {
                    Assert.Fail("Deserialization failed for q expression: " + expr + " caused by: " + e);
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        [Test]
        public void TestObjectDeserializationQ2()
        {
            var expressions = new QExpressions(new Dictionary<string, string> {{"q2", "..\\..\\test\\QExpressions.out"}});
            foreach (string expr in expressions.GetExpressions("q2"))
            {
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                var reader = new QReader(stream, Encoding.ASCII, QBasicConnection.DefaultMaxReadingChunk);
                byte[] binaryExpr = expressions.GetBinaryExpression("q2", expr);
                writer.Write((byte) 1); // little endian
                writer.Write((byte) 0);
                writer.Write((byte) 0);
                writer.Write((byte) 0);
                writer.Write(binaryExpr.Length + 8);
                writer.Write(binaryExpr);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                try
                {
                    object obj = reader.Read().Data;
                    if (obj is QDictionary || obj is QTable || obj is QLambda)
                    {
                        // force usage of Equals method
                        Assert.IsTrue(expressions.GetReferenceObject("q2", expr).Equals(obj),
                                      "Deserialization failed for q expression: " + expr);
                    }
                    else
                    {
                        Assert.AreEqual(expressions.GetReferenceObject("q2", expr), obj,
                                        "Deserialization failed for q expression: " + expr);
                    }
                }
                catch (QException e)
                {
                    Assert.AreEqual(((QException) expressions.GetReferenceObject("q2", expr)).Message, e.Message,
                                    "Deserialization failed for q error: " + e.Message);
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        [Test]
        public void TestObjectDeserializationQ3()
        {
            var expressions =
                new QExpressions(new Dictionary<string, string> {{"q3", "..\\..\\test\\QExpressions.3.out"}});
            foreach (string expr in expressions.GetExpressions("q3"))
            {
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                var reader = new QReader(stream, Encoding.ASCII, QBasicConnection.DefaultMaxReadingChunk);
                byte[] binaryExpr = expressions.GetBinaryExpression("q3", expr);
                writer.Write((byte) 1); // little endian
                writer.Write((byte) 0);
                writer.Write((byte) 0);
                writer.Write((byte) 0);
                writer.Write(binaryExpr.Length + 8);
                writer.Write(binaryExpr);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                try
                {
                    object obj = reader.Read().Data;
                    if (obj is QDictionary || obj is QTable || obj is QLambda)
                    {
                        // force usage of Equals method
                        Assert.IsTrue(expressions.GetReferenceObject("q3", expr).Equals(obj),
                                      "Deserialization failed for q expression: " + expr);
                    }
                    else
                    {
                        Assert.AreEqual(expressions.GetReferenceObject("q3", expr), obj,
                                        "Deserialization failed for q expression: " + expr);
                    }
                }
                catch (QException e)
                {
                    System.Console.WriteLine(e);
                    Assert.AreEqual(((QException) expressions.GetReferenceObject("q3", expr)).Message, e.Message,
                                    "Deserialization failed for q error: " + e.Message);
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        [Test]
        public void TestFunctionsDeserialization()
        {
            var expressions =
                new QExpressions(new Dictionary<string, string> { { "q3", "..\\..\\test\\QExpressionsFunctions.out" } });
            var reference = new Dictionary<string, object>
            {
                {"{x+y}[3]", new QProjection(new object[] { new QLambda("{x+y}"), 3L })},
                {"insert [1]", new QProjection(new object[] { new FunctionMock(), 1L })},
                {"xbar", new QLambda("k){x*y div x:$[16h=abs[@x];\"j\"$x;x]}")},
                {"not", new FunctionMock()},
                {"and", new FunctionMock()},
                {"md5", new QProjection(new object[] { new FunctionMock(), -15L })},
                {"any", new FunctionMock()},
                {"save", new FunctionMock()},
                {"raze", new FunctionMock()},
                {"sums", new FunctionMock()},
                {"prev", new FunctionMock()},
            };
           

            foreach (string expr in expressions.GetExpressions("q3"))
            {
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                var reader = new QReader(stream, Encoding.ASCII, QBasicConnection.DefaultMaxReadingChunk);

                byte[] binaryExpr = expressions.GetBinaryExpression("q3", expr);
                writer.Write((byte)1); // little endian
                writer.Write((byte)0);
                writer.Write((byte)0); // uncompressed
                writer.Write((byte)0);
                writer.Write(binaryExpr.Length + 8);
                writer.Write(binaryExpr);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                try
                {
                    object obj = reader.Read().Data;
                    if (obj is QDictionary || obj is QTable || obj is QLambda || obj is QProjection || obj is QFunction)
                    {
                        // force usage of Equals method
                        Assert.IsTrue(reference[expr].Equals(obj), "Deserialization failed for q expression: " + expr);
                    }
                    else
                    {
                        Assert.AreEqual(reference[expr], obj, "Deserialization failed for q expression: " + expr);
                    }
                }
                catch (System.Exception e)
                {
                    Assert.Fail("Deserialization failed for q expression: " + expr + " caused by: " + e);
                }
                finally
                {
                    stream.Close();
                }
            }
        }
    }
}