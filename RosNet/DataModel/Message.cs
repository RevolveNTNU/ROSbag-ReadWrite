﻿using System;
using System.Collections.Generic;
using System.Linq;
using RosNet.Field;

namespace RosNet.DataModel
{
    /// <summary>
    /// Represents a ROSbag message
    /// </summary>
    public class Message
    {
        //Header fields of message record:
        public int Conn { get; }
        public (uint, uint) Time { get;  }

        //Data in message record:
        public Dictionary<string,FieldValue> Data { get; set; }

        /// <summary>
        /// Create a message with conn and time from message record header
        /// </summary>
        public Message(FieldValue conn, FieldValue time)
        {
            this.Conn = BitConverter.ToInt32(conn.Value);
            uint secs = BitConverter.ToUInt32(time.Value.Take(4).ToArray());
            uint nsecs = BitConverter.ToUInt32(time.Value.Skip(4).Take(4).ToArray());
            this.Time = (secs, nsecs);
        }

        public override string ToString()
        {
            var s = ($"Conn: {Conn} \n");
            s += ($"Time: {Time.Item1} : {Time.Item2} \n");
            s += "Data: \n";
            foreach (KeyValuePair<string, FieldValue> kvp in Data)
            {
                s += ($"{kvp.Value.ToString(true)} \n");
            }

            return s;
        }
    }
}