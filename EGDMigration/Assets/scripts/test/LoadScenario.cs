using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LoadScenario : MonoBehaviour {

    public TriggerDialogue Initial;

    void Start()
    {
        Debug.Log("TRIGGER INITIAL");
        DialogueInterface d = FindObjectOfType<DialogueInterface>();
        d.ShowDialogue(Initial.StateLabel);
    }

}
