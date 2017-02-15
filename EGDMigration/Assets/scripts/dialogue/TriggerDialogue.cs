using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDialogue : MonoBehaviour {

    public string StateLabel;
    public int NumberOfConversations = -1;
    private DialogueInterface _dialogue;
    
	public void EnterDialogue()
    {
        Debug.Log("Trigger " + StateLabel);
        _dialogue.ShowDialogue(this);
    }

    private void Start()
    {
        _dialogue = FindObjectOfType<DialogueInterface>();
    }

}
