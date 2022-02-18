﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using RosNet.DataModel;
using RosNet.Field;
using RosNet.RosMessageParser;

namespace RosNet.RosReader
{
    /// <summary>
    /// Helper class for reading data in records
    /// </summary>
    public static class DataReader
    {
        /// <summary>
        /// Reads a connection and sets the data
        /// </summary>
        internal static void SetConnectionData(BinaryReader reader, ref Connection connection)
        {
            int dataLen = reader.ReadInt32();
            long endPos = reader.BaseStream.Position + dataLen;

            byte[] originalTopic = null;
            byte[] type = null;
            byte[] md5sum = null;
            List<FieldValue> messageDefinition = null;
            byte[] callerID = null;
            byte[] latching = null;
            while (reader.BaseStream.Position != endPos)
            {
                int fieldLen = reader.ReadInt32();
                string fieldName = Header.ReadName(reader);
                byte[] fieldValue = reader.ReadBytes(fieldLen - fieldName.Length - 1);

                switch (fieldName)
                {
                    case "topic":
                        originalTopic = fieldValue;
                        break;
                    case "type":
                        type = fieldValue;
                        break;
                    case "md5sum":
                        md5sum = fieldValue;
                        break;
                    case "message_definition":
                        messageDefinition = MessageDefinitionParser.ParseMessageDefinition(fieldValue);
                        break;
                    case "callerid":
                        callerID = fieldValue;
                        break;
                    case "latching":
                        latching = fieldValue;
                        break;
                }

            }
            connection.SetData(originalTopic, type, md5sum, messageDefinition, callerID, latching);
        }

        /// <summary>
        /// Reads message data
        /// </summary>
        /// <returns>message data in bytes</returns>
        internal static byte[] ReadMessageData(BinaryReader reader)
        {
            var dataLength = reader.ReadInt32();
            var data = reader.ReadBytes(dataLength);
            return data;
        }

        /// <summary>
        /// Reads chunk and adds all messages and message data to unParsedMessageHandler
        /// </summary>
        /// <returns>List of connections in chunk</returns>
        internal static List<Connection> ReadChunk(BinaryReader reader, ref UnParsedMessageHandler unParsedMessageHandler)
        {
            int dataLength = reader.ReadInt32();
            long endPos = reader.BaseStream.Position + dataLength;

            var connections = new List<Connection>();

            while (reader.BaseStream.Position != endPos)
            {
                Dictionary<string, FieldValue> header = Header.ReadHeader(reader);

                switch ((int) header["op"].Value.First())
                {
                    case 2:
                        var message = new Message(header["conn"], header["time"]);
                        var data = DataReader.ReadMessageData(reader);
                        unParsedMessageHandler.AddUnParsedMessage(message, data);
                        break;
                    case 7:
                        var connection = new Connection(header["conn"], header["topic"]);
                        DataReader.SetConnectionData(reader, ref connection);
                        connections.Add(connection);
                        break;
                    default:
                        var dataLen = reader.ReadInt32();
                        reader.ReadBytes(dataLen);
                        break;
                }
            }
            return connections;
        }
    }
}
