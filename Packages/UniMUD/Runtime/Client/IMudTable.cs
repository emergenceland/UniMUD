#nullable enable
using mud.Network.schemas;
using mud.Unity;
using Property = System.Collections.Generic.Dictionary<string, object>;
using System.Collections.Generic;

namespace mud.Client
{
    public abstract class IMudTable {
        public IMudTable(){}
        public abstract object[] GetValues();
        public abstract IMudTable? GetTableValue(string key);
        public TableId TableId {get{return GetTableId();}}
        public abstract TableId GetTableId();

        public static IMudTable? GetValue<T>(string key) where T : IMudTable, new()
        {
            var table = new T();
            var query = new Query()
                .Find("?value", "?attribute")
                .Where(table.TableId.ToString(), key, "?attribute", "?value");
            var result = NetworkManager.Instance.ds.Query(query);
            var hasValues = table.SetValues(result);
            return hasValues ? table : null;

        }

        public abstract void SetValues(params object[] values);
        public abstract bool SetValues(IEnumerable<Property> result);
        
        // {

        //     var hasValues = false;

        //     foreach (var record in result)
        //     {
        //         var attribute = record["attribute"].ToString();
        //         var value = record["value"];

        //         switch (attribute)
        //         {
        //             case "x":
        //                 var xValue = (long)value;
        //                 table.x = xValue;
        //                 hasValues = true;
        //                 break;
        //             case "y":
        //                 var yValue = (long)value;
        //                 table.y = yValue;
        //                 hasValues = true;
        //                 break;
        //         }
        //     }

        // }

    }
}
