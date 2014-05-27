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
            Console.WriteLine("Asynchronous message received: " + message.Message.Data);
            Console.WriteLine("message type: " + message.Message.MessageType + " size: " + message.Message.MessageSize + " isCompressed: " + message.Message.Compressed + " endianess: " + message.Message.Endianess);
        }

        static void OnError(object sender, QErrorEvent error)
        {
            Console.Error.WriteLine("Error received via callback: " + error.Cause.Message);
        }

    }
}