#nullable enable
using System;

using Property = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;
using UniRx;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mud {
    
    public abstract class MUDTable {

        public string TableId { get { return GetTableId(); } }
        public RxTable Table { get { return NetworkManager.Datastore.store[GetTableId()]; } }
        public Property? RawValue {get{return update == null ? null : (Property)(update.CurrentRecordValue);}}
        public RecordUpdate? update = null;
        public abstract string GetTableId();
        public abstract Type TableType();
        public abstract Type TableUpdateType();
        public abstract void PropertyToTable(Property property);
        public abstract RecordUpdate RecordUpdateToTyped(RecordUpdate rxRecord);
        
        public static IObservable<RecordUpdate> GetUpdates<T>() where T : MUDTable, new() {return GetUpdates(typeof(T));}
        public static IObservable<RecordUpdate> GetUpdates(Type table) {

            if(table.IsSubclassOf(typeof(MUDTable)) == false) {Debug.LogError($"{table.Name} is not of type IMudTable"); return null;}

            MUDTable mudTable = (MUDTable)Activator.CreateInstance(table);

            return NetworkManager.Instance.sync.onUpdate
                .Where(update => update.Table.Name == mudTable.TableId)
                .Select(recordUpdate =>
                { 
                    return mudTable.RecordUpdateToTyped(recordUpdate); 
                });
        }
        
        public static RxRecord? GetRecord<T>(string key) where T : MUDTable, new() {
            return GetRecord(key, typeof(T));
        }

        public static RxRecord? GetRecord(string key, Type tableType) {
            MUDTable mudTable = (MUDTable)Activator.CreateInstance(tableType);
            RxTable rxTable = NetworkManager.Datastore.store[mudTable.GetTableId()];
            if(rxTable == null) {Debug.Log($"{mudTable.GetTableId()}: RxTable not found"); return null;}
            rxTable.Entries.TryGetValue(key, out RxRecord record);
            return record;
        }

        public static T? GetTable<T>(string key) where T : MUDTable, new() {
            var mudTable = new T();
            RxRecord? record = GetRecord<T>(key);
            if(record == null) {return null;}
            mudTable.PropertyToTable(record.RawValue);
            return mudTable;
        }

        public abstract void SetValues(params object[] values);

    }
}
