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
using System.Threading;

namespace qSharp.Sample
{
    class AsynchQuery
    {
        static void Main(string[] args)
        {
            QCallbackConnection q = new QCallbackConnection(host: (args.Length >= 1) ? args[0] : "localhost",
                                                            port: (args.Length >= 2) ? Int32.Parse(args[1]) : 5000);
            try
            {
                q.DataReceived += OnData;
                q.Open();
                Console.WriteLine("conn: " + q + "  protocol: " + q.ProtocolVersion);
                Console.WriteLine("Press <ENTER> to close application");

                // definition of asynchronous multiply function
                // queryid - unique identifier of function call - used to identify
                // the result
                // a, b - actual parameters to the query
                q.Sync("asynchMult:{[queryid;a;b] res:a*b; (neg .z.w)(`queryid`result!(queryid;res)) }");
                q.StartListener();

                Random gen = new Random();
                // send asynchronous queries
                for (int i = 0; i < 10; i++)
                {
                    int a = gen.Next(20), b = gen.Next(20);
                    Console.WriteLine("Asynchronous call with queryid=" + i + " with arguments= " + a + ", " + b);
                    q.Async("asynchMult", i, a, b);
                }

                Thread.Sleep(2000);

                Console.ReadLine();
                q.Sync("value \"\\\\t 0\"");
                q.StopListener();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Console.ReadLine();
            }
            finally
            {
                q.Close();
            }
        }

        static void OnData(object sender, QMessageEvent message)
        {
            Console.WriteLine("Asynchronous message received.");
            Console.WriteLine("message type: " + message.Message.MessageType + " size: " + message.Message.MessageSize + " isCompressed: " + message.Message.Compressed + " endianess: " + message.Message.Endianess);
            PrintResult(message.Message.Data);
        }

        static void PrintResult(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("::");
            }
            else if (obj is QDictionary)
            {
                PrintResult(obj as QDictionary);
            }
            else
            {
                Console.WriteLine(obj);
            }
        }

        static void PrintResult(QDictionary d)
        {
            foreach (QDictionary.KeyValuePair e in d)
            {
                Console.WriteLine(e.Key + "| " + e.Value);
            }
        }

    }
}
