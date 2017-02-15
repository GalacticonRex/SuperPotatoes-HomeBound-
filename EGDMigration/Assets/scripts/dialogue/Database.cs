using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class Database : MonoBehaviour {

    public Dictionary<string, string> Entries = new Dictionary<string, string>();
    
    public string getEntry(string key)
    {
        string ret;
        if ( !Entries.TryGetValue(key, out ret))
        {
            Entries.Add(key, "");
            return "";
        }
        return ret;
    }
    public int getInteger(string key)
    {
        string ret;
        if (!Entries.TryGetValue(key, out ret))
        {
            Entries.Add(key, "0");
            return 0;
        }
        int val;
        if (!int.TryParse(ret, out val))
            return 0;
        else
            return val;
    }
    public void setEntry(string key, string value)
    {
        if (Entries.ContainsKey(key))
            Entries[key] = value;
        else
            Entries.Add(key, value);
    }
}
