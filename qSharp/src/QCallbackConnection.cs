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
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace qSharp
{

    /// <summary>
    /// Encapsulates a message received from kdb+.
    /// </summary>
    public class QMessageEvent : EventArgs
    {
        /// <summary>
        ///     The wrapped message.
        /// </summary>
        public readonly QMessage Message;

        /// <summary>
        /// Creates new QMessageEvent object.
        /// </summary>
        /// <param name="message">message received from kdb+</param>
        public QMessageEvent(QMessage message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Encapsulates an error encountered during receiving data from kdb+.
    /// </summary>
    public class QErrorEvent : EventArgs
    {
        /// <summary>
        ///     The source exception.
        /// </summary>
        public readonly Exception Cause;

        /// <summary>
        /// Creates new QErrorEvent object.
        /// </summary>
        /// <param name="cause">original cause</param>
        public QErrorEvent(Exception cause)
        {
            Cause = cause;
        }
    }

    /// <summary>
    ///     The QCallbackConnection, in addition to QBasicConnection, provides an internal thread-based mechanism
    ///     for asynchronous subscription.
    ///     Methods of QCallbackConnection are not thread safe.
    /// </summary>
    public class QCallbackConnection : QBasicConnection
    {

        internal QListener listener;
        internal Thread listenerThread;

        virtual public event EventHandler<QMessageEvent> DataReceived;
        virtual public event EventHandler<QErrorEvent> ErrorOccured;

        /// <summary>
        /// Initializes a new QCallbackConnection instance.
        /// </summary>
        /// <param name="host">Host of remote q service</param>
        /// <param name="port">Port of remote q service</param>
        /// <param name="username">Username for remote authorization</param>
        /// <param name="password">Password for remote authorization</param>
        /// <param name="encoding">Encoding used for serialization/deserialization of string objects</param>
        public QCallbackConnection(String host = "localhost", int port = 0, string username = null, string password = null,
                           Encoding encoding = null)
            : base(host, port, username, password, encoding)
        {
            // left empty
        }

        protected virtual void OnDataReceived(QMessageEvent e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, e);
            }
        }

        protected virtual void OnErrorOccured(QErrorEvent e)
        {
            if (ErrorOccured != null)
            {
                ErrorOccured(this, e);
            }
        }

        /// <summary>
        ///     Spawns a new thread which listens for asynchronous messages from the remote q host.
        ///     If a listener thread already exists, nothing happens.
        /// </summary>
        virtual public void StartListener()
        {
            lock (this)
            {
                if (listener == null)
                {
                    listener = new QListener(this);
                    listenerThread = new Thread(listener.Run);
                    listenerThread.Start();
                }
            }
        }

        /// <summary>
        ///     Indicates that a listener thread should stop. The listener thread is stopped after receiving next message from the remote q host.
        ///     If a listener doesn't exists, nothing happens.
        /// </summary>
        virtual public void StopListener()
        {
            lock (this)
            {
                if (listener != null)
                {
                    listener.Running = false;
                    listenerThread.Join(500);
                    listener = null;
                }
            }
        }

        protected internal class QListener
        {
            private readonly QCallbackConnection connection;
            internal volatile bool Running;

            internal QListener(QCallbackConnection connection)
            {
                this.connection = connection;
                Running = true;
            }

            internal void Run()
            {
                while (Running && connection.IsConnected())
                {
                    try
                    {
                        object data = connection.Receive(dataOnly: false);
                        connection.OnDataReceived(new QMessageEvent((QMessage) data));
                    }
                    catch (QException e)
                    {
                        connection.OnErrorOccured(new QErrorEvent(e));
                    }
                    catch (Exception e)
                    {
                        connection.OnErrorOccured(new QErrorEvent(e));
                        Running = false;
                        break;
                    }
                }
            }
        }
    }
}