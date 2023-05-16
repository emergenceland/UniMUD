using System;
using System.Linq;
using static mud.Network.schemas.Common;

namespace mud.Network.schemas
{
    public class TableId
    {
        private readonly string _ns;
        public readonly string name;

        public TableId(string ns, string name)
        {
            _ns = ns;
            this.name = name;
        }

        public override string ToString()
        {
            return $"TableId<{(_ns ?? "[empty]")}:{(name ?? "[empty]")}>";
        }

        public string ToHexString()
        {
            return ToHexString(_ns, name);
        }

        public byte[] ToBytes()
        {
            return ToBytes(_ns, name);
        }

        private static string ToHexString(string ns, string name)
        {
            var tableId = ToBytes(ns, name);
            return ByteArrayToHexString(tableId);
        }

        private static byte[] ToBytes(string ns, string name)
        {
            var tableId = new byte[32];
            Buffer.BlockCopy(StringToBytes16(ns), 0, tableId, 0, 16);
            Buffer.BlockCopy(StringToBytes16(name), 0, tableId, 16, 16);
            return tableId;
        }

        public static TableId FromBytes32(byte[] tableId)
        {
            var tableIdBytes = new byte[32];
            Array.Copy(tableId, 0, tableIdBytes, 0, tableId.Length);
            var namespaceParam = BytesToString(tableIdBytes.Take(16).ToArray()).TrimEnd('\0');
            var name = BytesToString(tableIdBytes.Skip(16).Take(16).ToArray()).TrimEnd('\0');
            return new TableId(namespaceParam, name);
        }

        public static TableId FromHexString(string str) {
            var tableIdBytes = HexStringToByteArray(str);
            return FromBytes32(tableIdBytes);
        }
    }
}
