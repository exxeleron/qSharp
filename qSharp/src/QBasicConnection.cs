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
using System.Text;


namespace qSharp
{


    /// <summary>
    ///     Connector class for interfacing with the kdb+ service.
    ///     Provides methods for synchronous and asynchronous interaction.
    ///     Methods of QBasicConnection are not thread safe.
    /// </summary>
    public class QBasicConnection : QConnection
    {
        public const int DefaultMaxReadingChunk = 65536;

        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public System.Text.Encoding Encoding { get; private set; }
        public int MaxReadingChunk { get; private set; }

        public int ProtocolVersion { get; protected set; }

        protected internal TcpClient connection;
        internal Stream stream;
        protected QReader Reader { get; set; }
        protected QWriter Writer { get; set; }


        /// <summary>
        /// Initializes a new QBasicConnection instance.
        /// </summary>
        /// <param name="host">Host of remote q service</param>
        /// <param name="port">Port of remote q service</param>
        /// <param name="username">Username for remote authorization</param>
        /// <param name="password">Password for remote authorization</param>
        /// <param name="encoding">Encoding used for serialization/deserialization of string objects. Default: Encoding.ASCII</param>
        /// <param name="maxReadingChunk">Maxium number of bytes read in a single chunk from stream</param>
        public QBasicConnection(String host = "localhost", int port = 0, string username = null, string password = null, Encoding encoding = null, int maxReadingChunk = DefaultMaxReadingChunk)
        {
            Encoding = encoding ?? Encoding.ASCII;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            MaxReadingChunk = maxReadingChunk;
        }

        /// <summary>
        ///     Initializes connection with the remote q service.
        /// </summary>
        virtual public void Open()
        {
            if (!IsConnected())
            {
                if (Host != null)
                {
                    InitSocket();
                    Initialize();

                    Reader = new QReader(stream, Encoding, MaxReadingChunk);
                    Writer = new QWriter(stream, Encoding, ProtocolVersion);
                }
                else
                {
                    throw new QConnectionException("Host cannot be null");
                }
            }
        }

        private void InitSocket()
        {
            connection = new TcpClient(Host, Port);
            stream = connection.GetStream();
        }

        private void Initialize()
        {
            string credentials = Password != null ? string.Format("{0}:{1}", Username, Password) : Username;
            byte[] request = Encoding.GetBytes(credentials + "\x3\x0");
            byte[] response = new byte[2];

            stream.Write(request, 0, request.Length);
            if (stream.Read(response, 0, 1) != 1)
            {
                Close();
                InitSocket();

                request = Encoding.GetBytes(credentials + "\x0");
                stream.Write(request, 0, request.Length);
                if (stream.Read(response, 0, 1) != 1)
                {
                    throw new QConnectionException("Connection denied.");
                }
            }

            ProtocolVersion = Math.Min(response[0], (byte)3);
        }

        /// <summary>
        ///     Closes connection with the remote q service.
        /// </summary>
        virtual public void Close()
        {
            if (IsConnected())
            {
                connection.Close();
                connection = null;
            }
        }

        /// <summary>
        ///     Reinitializes connection with the remote q service.
        /// </summary>
        virtual public void Reset()
        {
            if (connection != null)
            {
                connection.Close();
            }
            connection = null;
            Open();
        }

        /// <summary>
        ///     Check whether connection with the remote q host has been established and is active.
        /// </summary>
        /// <returns>true if connection with remote host is established and is active, false otherwise</returns>
        public bool IsConnected()
        {
            return connection != null && connection.Connected && connection.Client.Connected
                   && !(connection.Client.Poll(1000, SelectMode.SelectRead) & (connection.Client.Available == 0));
        }

        /// <summary>
        ///     Executes a synchronous query against the remote q service.
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>deserialized response from the remote q service</returns>
        virtual public object Sync(string query, params object[] parameters)
        {
            Query(MessageType.Sync, query, parameters);
            QMessage response = Reader.Read();

            if (response.MessageType == MessageType.Response)
            {
                return response.Data;
            }
            else
            {
                Writer.Write(new QException("nyi: qSharp expected response message"), response.MessageType == MessageType.Async ? MessageType.Async : MessageType.Response);
                throw new QReaderException("Received an " + response.MessageType + " message where response where expected");
            }
        }

        /// <summary>
        ///     Executes an asynchronous query against the remote q service.
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <param name="parameters">Additional parameters</param>
        virtual public void Async(string query, params object[] parameters)
        {
            Query(MessageType.Async, query, parameters);
        }

        /// <summary>
        ///     Executes a query against the remote q service.
        ///     Result of the query has to be retrieved by calling a Receive method.
        /// </summary>
        /// <param name="msgType">Indicates whether message should be synchronous or asynchronous</param>
        /// <param name="query">Query to be executed</param>
        /// <param name="parameters">Additional parameters</param>
        virtual public int Query(MessageType msgType, string query, params object[] parameters)
        {
            if (stream == null)
            {
                throw new QConnectionException("Connection has not been initalized.");
            }

            if (parameters.Length > 8)
            {
                throw new QWriterException("Too many parameters.");
            }

            if (parameters.Length == 0) // simple string query
            {
                return Writer.Write(query.ToCharArray(), msgType);
            }
            else
            {
                var request = new object[parameters.Length + 1];
                request[0] = query.ToCharArray();

                int i = 1;
                foreach (object param in parameters)
                {
                    request[i++] = param;
                }

                return Writer.Write(request, msgType);
            }
        }

        /// <summary>
        ///     Reads next message from the remote q service.
        /// </summary>
        /// <param name="dataOnly">if true returns only data part of the message, if false retuns data and message meta-information encapsulated in QMessage</param>
        /// <param name="raw">indicates whether message should be parsed to C# object or returned as an array of bytes</param>
        /// <returns>deserialized response from the remote q service</returns>
        virtual public object Receive(bool dataOnly = true, bool raw = false)
        {
            return dataOnly ? Reader.Read(raw).Data : Reader.Read(raw);
        }

        /// <summary>
        ///     Returns a System.String that represents the current QConnection.
        /// </summary>
        /// <returns>A System.String that represents the current QConnection</returns>
        public override string ToString()
        {
            return string.Format(":{0}:{1}", Host, Port);
        }

    }
}