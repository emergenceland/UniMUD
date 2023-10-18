#nullable enable
using System;

using Property = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;
using UniRx;

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
        public abstract void PropertyToTable(Property property);
        public abstract RecordUpdate RecordUpdateToTyped(RecordUpdate rxRecord);
        public static IObservable<RecordUpdate> GetUpdates<T>() where T : IMudTable, new() {return GetUpdates(typeof(T));}
        public static IObservable<RecordUpdate> GetUpdates(Type table) {

            if(table.IsSubclassOf(typeof(IMudTable)) == false) {Debug.LogError($"{table.Name} is not of type IMudTable"); return null;}

            IMudTable mudTable = (IMudTable)Activator.CreateInstance(table);

            return NetworkManager.Instance.sync.onUpdate
                .Where(update => update.Table.Name == mudTable.TableId)
                .Select(recordUpdate =>
                { 
                    return mudTable.RecordUpdateToTyped(recordUpdate); 
                });
        }
        
        public static T? GetValueFromTable<T>(string key) where T : IMudTable, new() {

            var table = new T();

            RxTable rxTable = NetworkManager.Datastore.store[table.GetTableId()];
            rxTable.Entries.TryGetValue(key, out RxRecord record);

            if(record == null) return null;
            table.PropertyToTable(record.RawValue);

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
