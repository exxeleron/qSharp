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
using System.Threading;

namespace qSharp.Sample
{
    public class Publisher
    {

        public static void Main(String[] Args)
        {
            QConnection q = new QBasicConnection(Args.Length >= 1 ? Args[0] : "localhost",
                                                 Args.Length >= 2 ? int.Parse(Args[1]) : 5001, null, null);
            try
            {
                q.Open();
                Console.WriteLine("conn: " + q + "  protocol: " + q.ProtocolVersion);
                Console.WriteLine("WARNING: this application overwrites: .u.upd function on q process");
                Console.WriteLine("Press <ENTER> to close application");
                q.Sync(".u.upd:{[x;y] show (x;y)};");

                PublisherTask pt = new PublisherTask(q);
                Thread workerThread = new Thread(pt.Run);
                workerThread.Start();

                Console.ReadLine();
                pt.Stop();
                workerThread.Join();
            }
            catch (Exception e)
            {
                Console.WriteLine("`" + e.Message);
            }
            finally
            {
                q.Close();
            }
        }

    }

    class PublisherTask
    {
        private QConnection q;
        private Random r;
        private Boolean running = true;

        public PublisherTask(QConnection q)
        {
            this.q = q;
            this.r = new Random((int)DateTime.Now.Millisecond);
        }

        public void Stop()
        {
            running = false;
        }

        public void Run()
        {
            while (running)
            {
                try
                {
                    // publish data to tick
                    // function: .u.upd
                    // table: ask
                    q.Sync(".u.upd", "ask", GetAskData());
                    Console.Out.Write(".");
                    System.Threading.Thread.Sleep(500);
                }
                catch (QException e1)
                {
                    // q error
                    Console.WriteLine("`" + e1.Message);
                }

            }
        }

        private Object[] GetAskData()
        {
            int rows = r.Next(10) + 1;
            Object[] data = new Object[] { new QTime[rows], new String[rows], new String[rows], new float[rows] };
            for (int i = 0; i < rows; i++)
            {
                ((QTime[])data[0])[i] = new QTime(DateTime.Now);
                ((String[])data[1])[i] = "INSTR_" + r.Next(100);
                ((String[])data[2])[i] = "qSharp";
                ((float[])data[3])[i] = (float)r.NextDouble() * r.Next(100);
            }

            return data;
        }
    }
}