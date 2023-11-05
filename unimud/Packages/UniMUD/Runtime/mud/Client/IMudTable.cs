#nullable enable
using System;

using Property = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;
using UniRx;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mud {
    
    public abstract class IMudTable {

        public string TableId { get { return GetTableId(); } }
        public RxTable Table { get { return NetworkManager.Datastore.store[GetTableId()]; } }

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
        
        public static RxRecord? GetRecord<T>(string key) where T : IMudTable, new() {
            T mudTable = (T)Activator.CreateInstance(typeof(T));
            RxTable rxTable = NetworkManager.Datastore.store[mudTable.GetTableId()];
            if(rxTable == null) {Debug.LogError($"{mudTable.GetTableId()}: RxTable not found"); return null;}
            rxTable.Entries.TryGetValue(key, out RxRecord record);
            return record;
        }

        public static T? MakeTable<T>(string key) where T : IMudTable, new() {
            var mudTable = new T();
            RxRecord? record = GetRecord<T>(key);
            if(record == null) {return null;}
            mudTable.PropertyToTable(record.RawValue);
            return mudTable;
        }

        public abstract void SetValues(params object[] values);

    }
}
