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


namespace qSharp
{
    /// <summary>
    ///     Represents q message type.
    /// </summary>
    public enum MessageType
    {
        Async,
        Sync,
        Response
    }

    /// <summary>
    ///     Represents endianess of the message.
    /// </summary>
    public enum Endianess
    {
        BigEndian,
        LittleEndian
    }

    /// <summary>
    ///     Interface for the q connector.
    ///     Defines methods for synchronous and asynchronous interaction.
    /// </summary>
    public interface QConnection
    {
        string Host { get; }
        int Port { get; }
        string Username { get; }
        string Password { get; }
        System.Text.Encoding Encoding { get; }
        int ProtocolVersion { get; }

        /// <summary>
        ///     Initializes connection with the remote q service.
        /// </summary>
        void Open();

        /// <summary>
        ///     Reinitializes connection with the remote q service.
        /// </summary>
        void Reset();

        /// <summary>
        ///     Closes connection with the remote q service.
        /// </summary>
        void Close();

        /// <summary>
        ///     Check whether connection with the remote q host has been established. Note that this function doesn't check whether the connection is still active.
        /// </summary>
        /// <returns>true if connection with remote host is established, false otherwise</returns>
        bool IsConnected();

        /// <summary>
        ///     Executes a synchronous query against the remote q service.
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>deserialized response from the remote q service</returns>
        object Sync(string query, params object[] parameters);

        /// <summary>
        ///     Executes an asynchronous query against the remote q service.
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <param name="parameters">Additional parameters</param>
        void Async(string query, params object[] parameters);

        /// <summary>
        ///     Executes a query against the remote q service.
        ///     Result of the query has to be retrieved by calling a Receive method.
        /// </summary>
        /// <param name="msgType">Indicates whether message should be synchronous or asynchronous</param>
        /// <param name="query">Query to be executed</param>
        /// <param name="parameters">Additional parameters</param>
        /// <returns>number of written bytes</returns>
        int Query(MessageType msgType, string query, params object[] parameters);

        /// <summary>
        ///     Reads next message from the remote q service.
        /// </summary>
        /// <param name="dataOnly">if true returns only data part of the message, if false retuns data and message meta-information encapsulated in QMessage</param>
        /// <param name="raw">indicates whether message should be parsed to C# object or returned as an array of bytes</param>
        /// <returns>deserialized response from the remote q service</returns>
        object Receive(bool dataOnly = true, bool raw = false);
    }
}