using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaLogic : MonoBehaviour {

    private int _day_number;
    private DialogueInterface _dint;
    private Database _db;
    private AssistantLabel _label;
    private Dictionary<TriggerDialogue, GameObject> _triggers;
    private TriggerDialogue _go;

    public void StartTrigger(TriggerDialogue diag)
    {
        Debug.Log("Hello");
        _go = diag;
    }
    public void FinishTrigger()
    {
        if ( _go != null && _go.StateLabel != "C4 start" )
        {
            Debug.Log("Goodbye");
            if (_db.getEntry("AnyWin") == "true")
            {
                Debug.Log("YOU WIN MOTHERF*****");
                UnityEngine.SceneManagement.SceneManager.LoadScene("ending");
            }
            else
            {
                _label.SetLabel("");
                _go.gameObject.SetActive(false);
                _go = null;
            }
        }
    }
    public void NewDay()
    {
        _day_number++;
        foreach ( KeyValuePair<TriggerDialogue, GameObject> kvp in _triggers )
        {
            if ( kvp.Key.StateLabel != "C1 conv1 start" )
                kvp.Value.SetActive(true);
        }
        switch (_day_number)
        {
            case 4:
                _dint.ShowDialogue("landing");
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        _label = FindObjectOfType<AssistantLabel>();
        _dint = GetComponent<DialogueInterface>();
        _db = GetComponent<Database>();
        _triggers = new Dictionary<TriggerDialogue, GameObject>();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("DialogueTriggerTag");
        foreach( GameObject go in gos )
        {
            TriggerDialogue diag = go.GetComponent<TriggerDialogue>();
            if ( diag != null )
                _triggers.Add(diag, go);
        }
    }

}
