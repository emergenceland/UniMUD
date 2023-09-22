using v2;

namespace mud.Client
{
    public class Common
    {
        public static string GetTableKey(string address, string ns, string name)
        {
            return v2.Common.ConcatHex(new[]
            {
                address,
                v2.Common.StringToHex(ns, 16),
                v2.Common.StringToHex(name, 16),
            });
        }

        public static string GetTableKey(ProtocolParser.Table table)
        {
            return v2.Common.ConcatHex(new[]
            {
                table.Address,
                v2.Common.StringToHex(table.Namespace, 16),
                v2.Common.StringToHex(table.Name, 16)
            });
        }
    }
}
