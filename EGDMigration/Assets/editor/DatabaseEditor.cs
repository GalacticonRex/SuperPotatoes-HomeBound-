using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Database))]
public class DatabaseEditor : Editor {
    private string NewEntry = "New Entry";

    public override void OnInspectorGUI()
    {
        Database db = (Database)target;

        EditorGUILayout.LabelField("Database Entries");
        Dictionary<string, string> updatedValues = new Dictionary<string, string>();
        List<string> toRemove = new List<string>();
        foreach (KeyValuePair<string, string> pair in db.Entries)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(pair.Key, GUILayout.Width(100));
            string strValue = GUILayout.TextField(pair.Value.ToString());
            if ( GUILayout.Button("x", GUILayout.Width(16)) )
            {
                toRemove.Add(pair.Key);
            }
            else
            {
                updatedValues[pair.Key] = strValue;
            }
            GUILayout.EndHorizontal();
        }
        foreach (KeyValuePair<string, string> pair in updatedValues)
        {
            db.Entries[pair.Key] = pair.Value;
        }
        foreach (string str in toRemove)
        {
            db.Entries.Remove(str);
        }

        GUILayout.BeginHorizontal();
        Color background = GUI.backgroundColor;
        bool valid = (NewEntry.Length != 0 && !db.Entries.ContainsKey(NewEntry));
        GUI.backgroundColor = (valid) ? background : Color.red;
        NewEntry = GUILayout.TextField(NewEntry);
        GUI.backgroundColor = background;
        if ( GUILayout.Button("Add New Entry", GUILayout.ExpandWidth(false)) && valid )
        {
            db.Entries[NewEntry] = "";
            NewEntry = "New Entry";
        }
        GUILayout.EndHorizontal();
    }
}
