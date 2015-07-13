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

namespace qSharp
{
    /// <summary>
    ///     Encapsulates the q message.
    /// </summary>
    public struct QMessage
    {
        /// <summary>
        ///     Creates new QMessage object.
        /// </summary>
        /// <param name="data"> data payload</param>
        /// <param name="messageType">type of the q message</param>
        /// <param name="endianess">endianess of the data</param>
        /// <param name="compressed">true if message was compressed, false otherwise</param>
        /// <param name="raw">true if  raw message was retrieved, false if message was parsed</param>
        /// <param name="messageSize">size of the message</param>
        /// <param name="dataSize">size of the data payload section</param>
        public QMessage(object data, MessageType messageType, Endianess endianess, bool compressed, bool raw,
            int messageSize, int dataSize)
            : this()
        {
            Data = data;
            MessageType = messageType;
            Endianess = endianess;
            Compressed = compressed;
            Raw = raw;
            MessageSize = messageSize;
            DataSize = dataSize;
        }

        /// <summary>
        ///     The data payload associated with the message.
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        ///     Indicates endianess of the last read message.
        /// </summary>
        public Endianess Endianess { get; private set; }

        /// <summary>
        ///     Indicates whether last read message was compressed.
        /// </summary>
        public bool Compressed { get; private set; }

        /// <summary>
        ///     Indicates whether last read message was parsed.
        /// </summary>
        public bool Raw { get; private set; }

        /// <summary>
        ///     Gets type of last read message.
        /// </summary>
        public MessageType MessageType { get; private set; }

        /// <summary>
        ///     Gets a total size of the last message.
        /// </summary>
        public int MessageSize { get; private set; }

        /// <summary>
        ///     Gets a size of data section in the last message.
        /// </summary>
        public int DataSize { get; private set; }
    }
}