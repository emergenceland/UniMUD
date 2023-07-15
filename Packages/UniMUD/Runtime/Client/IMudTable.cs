#nullable enable
using System;
using System.Collections.Generic;

using mud.Network.schemas;
using mud.Unity;
using Property = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mud.Client {
    
    [System.Serializable]
    public abstract class IMudTable {
        public TableId TableId { get { return GetTableId(); } }
        public abstract TableId GetTableId();


        public abstract Type TableType();
        public abstract Type TableUpdateType();
        public abstract IMudTable GetTableValue(string key);

        public static T? GetValueFromTable<T>(string key) where T : IMudTable, new() {
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
        public abstract RecordUpdate CreateTypedRecord(RecordUpdate newUpdate);
        public abstract IMudTable RecordUpdateToTable(RecordUpdate tableUpdate);



#if UNITY_EDITOR

        public bool SpawnTableType(string path, string assemblyName) {

            string fileName = path + this.GetType().ToString().Replace(assemblyName + ".", "") + ".asset";
            MUDTableObject typeFile = (MUDTableObject)AssetDatabase.LoadAssetAtPath(fileName, typeof(MUDTableObject));

            if (typeFile != null) {
                return false;
            }

            MUDTableObject asset = (MUDTableObject)ScriptableObject.CreateInstance(typeof(MUDTableObject));
            asset.SetTable(this.GetType());

            AssetDatabase.CreateAsset(asset, fileName);
            AssetDatabase.SaveAssets();

            return true;
        }

        public bool DeleteTableType(string path, string assemblyName) {

            string fileName = path + this.GetType().ToString().Replace(assemblyName + ".", "") + ".asset";

            MUDTableObject typeFile = (MUDTableObject)AssetDatabase.LoadAssetAtPath(fileName, this.GetType());

            if (typeFile == null) {
                return false;
            }

            AssetDatabase.DeleteAsset(fileName);
            AssetDatabase.SaveAssets();

            return true;
        }

#endif


    }
}
