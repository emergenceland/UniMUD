using UnityEngine;
using mud.Client;
using mud.Unity;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mud.Client {

    public class MUDTableObject : ScriptableObject {
        public string TableName { get { return tableName; } }
        public string TableUpdateName { get { return tableUpdateName; } }
        public Type Table { get { return tableType; } }
        public Type TableUpdate { get { return tableUpdate; } }
        public void SetTable(Type newtable) {

            IMudTable table = (IMudTable)System.Activator.CreateInstance(newtable);
            
            tableName = table.TableType().FullName;
            tableUpdateName = table.TableUpdateType().FullName;

            tableType = table.TableType();
            tableUpdate = table.TableUpdateType();

            Debug.Log(tableType.ToString());
            Debug.Log(tableUpdate.ToString());

        }

        [Header("Table")]
        [SerializeField] string tableName;
        [SerializeField] string tableUpdateName;
        [SerializeField] Type tableType;
        [SerializeField] Type tableUpdate;


    }

}
