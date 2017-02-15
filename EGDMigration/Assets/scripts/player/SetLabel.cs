using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLabel : MonoBehaviour {

    public string AssistLabel;
    private AssistantLabel _assist;
    private bool _has_player;

	// Use this for initialization
	void Start () {
        _assist = GameObject.FindGameObjectWithTag("AssistLabel").GetComponent<AssistantLabel>();
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerMove m = other.GetComponent<PlayerMove>();
        if ( m != null )
        {
            _assist.SetLabel(AssistLabel);
            _has_player = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerMove m = other.GetComponent<PlayerMove>();
        if (m != null)
        {
            _assist.SetLabel("");
            _has_player = false;
        }
    }
}
