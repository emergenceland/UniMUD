using System;
using System.Collections.Generic;
using System.Text;

namespace mud.Network.schemas
{
    public static class EntityIdUtil
    {
        public static readonly string SingletonId = "0x60d";

        public static List<string> BytesToStringArray(List<byte[]> keyTuple)
        {
            List<string> hexStrings = new List<string>();

            StringBuilder hexBuilder = new StringBuilder();
            foreach (byte[] byteArray in keyTuple)
            {
                hexBuilder.Append("0x");
                hexBuilder.Append(BitConverter.ToString(byteArray).Replace("-", "").ToLower());
                hexStrings.Add(hexBuilder.ToString());
                hexBuilder.Clear();
            }

            return hexStrings;
        }

        public static string KeyTupleToEntityID(List<byte[]> keyTuple)
        {
            if (keyTuple.Count == 0)
            {
                return SingletonId;
            }

            var hexStrings = BytesToStringArray(keyTuple);

            return string.Join(":", hexStrings);
        }
    }
}
