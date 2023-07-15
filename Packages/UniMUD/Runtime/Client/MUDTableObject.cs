using UnityEngine;
using mud.Client;
using mud.Unity;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mud.Client {

    public class MUDTableObject : ScriptableObject {
        public IMudTable Table { get { return table; } }
        public Type TableType { get { return tableType; } }
        public Type UpdateType { get { return updateType; } }
        public void SetTable(Type newTableType) {

            table = (IMudTable)System.Activator.CreateInstance(newTableType);

            tableType = table.GetType();
            updateType = table.TableUpdateType();

            tableName = tableType.ToString();
            tableUpdateName = updateType.ToString();

        }

        [Header("Table")]
        [SerializeField] string tableName;
        [SerializeField] string tableUpdateName;
        [SerializeField] IMudTable table;
        [SerializeField] Type tableType;
        [SerializeField] Type updateType;

    }

}
