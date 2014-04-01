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
using System.Linq;
using System.Text;

namespace qSharp.Sample
{
    class Subscriber
    {
        static void Main(string[] args)
        {
            QCallbackConnection q = new QCallbackConnection(host: (args.Length >= 1) ? args[0] : "localhost",
                                                            port: (args.Length >= 2) ? Int32.Parse(args[1]) : 5000);
            try
            {
                q.DataReceived += OnData;
                q.ErrorOccured += OnError;
                q.Open();
                Console.WriteLine("conn: " + q + "  protocol: " + q.ProtocolVersion);
                Console.WriteLine("Press <ENTER> to close application");

                q.Sync("sub:{[x] .sub.h: .z.w }");
                q.Sync(".z.ts:{ (neg .sub.h) .z.p}");
                q.Sync("value \"\\\\t 100\"");
                q.StartListener();
                q.Async("sub", 0);

                Console.ReadLine();
                q.Sync("value \"\\\\t 0\"");
                q.StopListener();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error occured: " + e);
                Console.ReadLine();
            }
            finally
            {
                q.Close();
            }
        }

        static void OnData(object sender, QMessageEvent message)
        {
            PrintResult(message.Message.Data);
        }

        static void OnError(object sender, QErrorEvent error)
        {
            Console.Error.WriteLine("Error received via callback: " + error.Cause.Message);
        }

        static void PrintResult(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("::");
            }
            else if (obj is Array)
            {
                PrintResult(obj as Array);
            }
            else if (obj is QDictionary)
            {
                PrintResult(obj as QDictionary);
            }
            else if (obj is QTable)
            {
                PrintResult(obj as QTable);
            }
            else
            {
                Console.WriteLine(obj);
            }
        }

        static void PrintResult(Array a)
        {
            Console.WriteLine(Utils.ArrayToString(a));
        }

        static void PrintResult(QDictionary d)
        {
            foreach (QDictionary.KeyValuePair e in d)
            {
                Console.WriteLine(e.Key + "| " + e.Value);
            }
        }

        static void PrintResult(QTable t)
        {
            var rowsToShow = Math.Min(t.RowsCount, 20);
            var dataBuffer = new object[1 + rowsToShow][];
            var columnWidth = new int[t.ColumnsCount];

            dataBuffer[0] = new string[t.ColumnsCount];
            for (int j = 0; j < t.ColumnsCount; j++)
            {
                dataBuffer[0][j] = t.Columns[j];
                columnWidth[j] = t.Columns[j].Length + 1;
            }

            for (int i = 1; i < rowsToShow; i++)
            {
                dataBuffer[i] = new string[t.ColumnsCount];
                for (int j = 0; j < t.ColumnsCount; j++)
                {
                    var value = t[i - 1][j].ToString();
                    dataBuffer[i][j] = value;
                    columnWidth[j] = Math.Max(columnWidth[j], value.Length + 1);
                }
            }

            var formatting = "";
            for (int i = 0; i < columnWidth.Length; i++)
            {
                formatting += "{" + i + ",-" + columnWidth[i] + "}";
            }

            Console.WriteLine(formatting, dataBuffer[0]);
            Console.WriteLine(new string('-', columnWidth.Sum()));
            for (int i = 1; i < rowsToShow; i++)
            {
                Console.WriteLine(formatting, dataBuffer[i]);
            }
        }
    }
}