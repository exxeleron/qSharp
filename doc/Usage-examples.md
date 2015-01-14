- [Remote console](Usage-examples.md#remote-console)
- [Subscribing for asynchronous messages](Usage-examples.md#subscribing-for-asynchronous-messages)
- [Subscribing to tick service](Usage-examples.md#subscribing-to-tick-service)
- [Data publisher](Usage-examples.md#data-publisher)

### Remote console
This example shows how to create a simple interactive console for communication with a kdb+ process:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qSharp.Sample
{
    class RemoteConsole
    {
        static void Main(string[] args)
        {
            QConnection q = new QBasicConnection(host: (args.Length >= 1) ? args[0] : "localhost",
                                                 port: (args.Length >= 2) ? Int32.Parse(args[1]) : 5000);
            try
            {
                q.Open();
                Console.WriteLine("conn: " + q + "  protocol: " + q.ProtocolVersion);

                while (true)
                {
                    Console.Write("Q)");
                    var line = Console.ReadLine();

                    if (line.Equals("\\\\"))
                    {
                        break;
                    }
                    else
                    {
                        try
                        {
                            PrintResult(q.Sync(line));
                        }
                        catch (QException e)
                        {
                            Console.WriteLine("`" + e.Message);
                        }
                    }
                }
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
```

### Subscribing for asynchronous messages
This example shows how to create a simple subscription for asynchronous data using the Events mechanism:

```csharp
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
```

### Subscribing to tick service
This example depicts how to subscribe to standard kdb+ tickerplant service:

```csharp
using System;

namespace qSharp.Sample
{
    class Subscriber
    {
        static void Main(string[] args)
        {
            QCallbackConnection q = new QCallbackConnection(host: (args.Length >= 1) ? args[0] : "localhost",
                                                            port: (args.Length >= 2) ? Int32.Parse(args[1]) : 17010);
            try
            {
                q.DataReceived += OnData;
                q.ErrorOccured += OnError;
                q.Open();
                Console.WriteLine("conn: " + q + "  protocol: " + q.ProtocolVersion);
                Console.WriteLine("Press <ENTER> to close application");

                Object response = q.Sync(".u.sub", "trade", ""); // subscribe to tick
                QTable model = (QTable)((Object[])response)[1]; // get table model

                q.StartListener();
                Console.ReadLine();
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
            Object data = message.Message.Data;
            if (data is Object[])
            {
                // unpack upd message
                Object[] args = ((Object[])data);
                if (args.Length == 3 && args[0].Equals("upd") && args[2] is QTable)
                {
                    QTable table = (QTable)args[2];
                    foreach (QTable.Row row in table)
                    {
                        Console.WriteLine(row);
                    }
                }
            }
        }

        static void OnError(object sender, QErrorEvent error)
        {
            Console.Error.WriteLine("Error received via callback: " + error.Cause.Message);
        }

    }
}
```

### Data publisher
This example shows how to stream data to the kdb+ process using standard tickerplant API:
> :white_check_mark: Warning:
> this sample code overwrites: .u.upd function on q process

```csharp
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
```
