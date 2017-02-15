using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoToSleep : MonoBehaviour {

    public Image Black;
    private DialogueInterface _dint;
    private AssistantLabel _label;
    private MetaLogic _logic;
    private PlayerMove _has_player;

    private void Start()
    {
        _dint = FindObjectOfType<DialogueInterface>();
        _label = FindObjectOfType<AssistantLabel>();
        _logic = FindObjectOfType<MetaLogic>();
    }
    private void OnTriggerEnter(Collider other)
    {
        _has_player = other.GetComponent<PlayerMove>();
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerMove mov = other.GetComponent<PlayerMove>();
        if (mov != null)
        {
            _has_player = null;
        }
    }
    private void Update()
    {
        if ( _has_player != null && !_dint.OldActive() && Input.GetKeyDown(KeyCode.Space) )
        {
            StartCoroutine(FadeIn());
        }
    }
    IEnumerator FadeIn()
    {
        _label.SetLabel("Sleeping");
        _has_player.SetMovable(false);
        int pts = 25;
        for (int i = 0; i < pts; i++)
        {
            yield return new WaitForSeconds(0.05f);
            Color c = Black.color;
            c.a = (float)i / (float)pts;
            Black.color = c;
        }
        StartCoroutine(FadeOut());
    }
    IEnumerator FadeOut()
    {
        int pts = 25;
        for (int i = 0; i < pts; i++)
        {
            yield return new WaitForSeconds(0.05f);
            Color c = Black.color;
            c.a = 1.0f - (float)i / (float)pts;
            Black.color = c;
        }
        _has_player.SetMovable(true);
        _label.SetLabel(GetComponent<SetLabel>().AssistLabel);
        _logic.NewDay();
    }
}
