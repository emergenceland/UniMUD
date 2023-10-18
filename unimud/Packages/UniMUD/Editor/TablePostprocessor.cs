using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Object = UnityEngine.Object;

namespace mud {

    public class TablePostprocessor : AssetPostprocessor {

        
        //TODO, file that defines these
        static string Namespace = "DefaultNamespace";
        static string CodePath = "Assets/Scripts/codegen/";
        static string TablePath = "Assets/Resources/MUD/Tables/";

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

            //DISABLE
            return;

            int added = 0;
            int deleted = 0;

            List<string> addedTables = new List<string>();
            List<string> deletedTables = new List<string>();

            // Debug.Log("Preprocessing");

            //does not create recursively..
            Directory.CreateDirectory(TablePath);

            var newTables = importedAssets.Where(asset => Path.GetExtension(asset) == ".cs").ToList();

            foreach (string str in newTables) {
                if (ToggleTable(true, str)) {
                    added++;
                    addedTables.Add(Path.GetFileNameWithoutExtension(str));
                }
            }

            var oldTables = deletedAssets.Where(asset => Path.GetExtension(asset) == ".cs").ToList();

            foreach (string str in oldTables) {
                if (ToggleTable(false, str)) {
                    deleted++;
                    deletedTables.Add(Path.GetFileNameWithoutExtension(str));
                }
            }

            if (added != 0) {
                Debug.Log("MUD: Added " + added + " tables");
                for(int i = 0; i < addedTables.Count; i++) {
                    Debug.Log("MUD: " + addedTables[i]);
                }
            }

            if (deleted != 0) {
                Debug.Log("MUD: Removed " + deleted + " tables");
                for(int i = 0; i < deletedTables.Count; i++) {
                    Debug.Log("MUD: " + deletedTables[i]);
                }
            }

            Object [] tableScripts = AssetDatabase.LoadAllAssetsAtPath(CodePath);
 
            for(int i = 0; i < tableScripts.Length; i++) {

            }

        }

        static bool ToggleTable(bool toggle, string path) {

            string filename = Path.GetFileNameWithoutExtension(path);
            string namespaceFile =  Namespace + "." + filename;

            //find the mudnamespace
            System.Reflection.Assembly a = System.Reflection.Assembly.Load(Namespace);
            Type t = a.GetType(namespaceFile);

            if (t == null) {
                // Debug.LogError("Could not get type: " + namespaceFile);
                return false;
            }

            IMudTable mudTable = (IMudTable)Activator.CreateInstance(t);

            if (mudTable == null) {
                // Debug.LogError("Not a table class: " + filename);
                return false;
            }

            //the parent class IMudTable is being imported, ignore, dont process
            if (mudTable.GetType() == typeof(IMudTable)) {
                return false;
            }

            //we have imported a new mud table script
            //niice!            

            string tableName = filename;

            if (toggle) {
                return mudTable.SpawnTableType(TablePath, Namespace);
            } else {
                return mudTable.DeleteTableType(TablePath, Namespace);
            }
        }
    }


}