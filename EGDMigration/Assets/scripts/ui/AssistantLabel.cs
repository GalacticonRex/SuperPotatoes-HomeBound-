using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssistantLabel : MonoBehaviour {

    public float Rate = 1.0f;

    private DialogueInterface _dialogue;
    private Text _assistant;
    private float _alpha;

    public void SetLabel(string t)
    {
        _assistant.text = t;
    }

	// Use this for initialization
	void Start () {
        _dialogue = FindObjectOfType<DialogueInterface>();
        _assistant = GetComponent<Text>();

        _alpha = 0.0f;
        Color c = _assistant.color;
        c.a = 0.0f;
        _assistant.color = c;
    }
	
	// Update is called once per frame
	void Update () {
        if (_dialogue.Active() || _assistant.text.Length == 0)
        {
            _alpha = 0.0f;
        }
        else
        {
            _alpha += Time.deltaTime / Rate;
            if (_alpha > 2.0f)
                _alpha -= 2.0f;
        }

        Color c = _assistant.color;
        c.a = (Mathf.Cos((_alpha+1.0f) * Mathf.PI)+1.0f)/2.0f;
        _assistant.color = c;
    }
}
