#nullable enable
using System;

using Property = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mud {
    
    [System.Serializable]
    public abstract class IMudTable {

        public string TableId { get { return GetTableId(); } }
        public RxTable Table { get { return NetworkManager.Instance.ds.store[GetTableId()]; } }

        public abstract string GetTableId();
        public abstract Type TableType();
        public abstract Type TableUpdateType();
        public abstract void RecordToTable(RxRecord rxRecord);

        public static T? GetValueFromTable<T>(string key) where T : IMudTable, new() {
            var table = new T();
            // var record = NetworkManager.Instance.ds.GetValue(Table, key);
            //
            // if (record == null) { return null; }
            // else { table.RecordToTable(record); return table; }
            return table;
        }

        public abstract void SetValues(params object[] values);



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
