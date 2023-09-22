#nullable enable
using System;
using System.Collections.Generic;

using mud.Network.schemas;
using Property = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;
using v2;
using NetworkManager = mud.Unity.NetworkManager;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mud.Client {
    
    [System.Serializable]
    public abstract class IMudTable {
        public ProtocolParser.Table TableId { get { return GetTableId(); } }
        public abstract ProtocolParser.Table GetTableId();


        public abstract Type TableType();
        public abstract Type TableUpdateType();
        public abstract void RecordToTable(RxRecord rxRecord);

        public static T? GetValueFromTable<T>(string key) where T : IMudTable, new() {
            var table = new T();
            var record = NetworkManager.Instance.ds.GetValue(table.TableId, key);

            if (record == null) { return null; }
            else { table.RecordToTable(record); return table; }
        }

        public abstract void SetValues(params object[] values);
        public abstract RecordUpdate CreateTypedRecord(RecordUpdate newUpdate);
        public abstract IMudTable RecordUpdateToTable(RecordUpdate recordUpdate);



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
