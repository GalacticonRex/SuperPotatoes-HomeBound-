using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TextInterface : MonoBehaviour {

    public GameObject ButtonPrefab;
    public Text Display;

    private Node _current = null;
    private int _index = 0;
    private bool _writing = false;
    private bool _choosing_option = false;
    private IEnumerator _coroutine = null;
    private List<GameObject> _buttons = new List<GameObject>();

    public void SendMessage(Node start)
    {
        _current = start;
        _index = 0;
        _writing = true;
        StartCoroutine(_coroutine = NextCharacter());
    }

    IEnumerator NextCharacter()
    {
        if (_index > _current.Text.Length)
        {
            yield return null;
            Transition[] trans = _current.GetTransitions();
            if ( (trans.Length == 1 && trans[0].Option.Length != 0) || trans.Length > 1 )
            {
                DisplayOptions(trans);
            }
            else
            {
                _writing = false;
            }
        }
        else
        {
            yield return new WaitForSeconds(_current.TestSpeed / 30.0f);
            Display.text = _current.Text.Substring(0, _index);
            _index++;
            StartCoroutine(_coroutine = NextCharacter());
        }
    }
    void DisplayOptions(Transition[] options)
    {
        RequestButtons(options.Length);
        for ( int i=0;i<_buttons.Count;i++ )
        {
            Text txt = _buttons[i].GetComponentInChildren<Text>();
            txt.text = options[i].Option;
        }
    }
    void RequestButtons(int n)
    {
        if (_buttons.Count < n)
        {
            for ( int i=_buttons.Count;i<n;i++ )
            {
                GameObject go = Instantiate(ButtonPrefab);
                go.transform.SetParent(transform);
                _buttons.Add(go);
                go.SetActive(false);
            }
        }

        float buttonHeight = 32.0f;
        Vector3 offset = new Vector3(0, (_buttons.Count / 2.0f) * buttonHeight, 0);
        Vector3 increment = new Vector3(0, -buttonHeight, 0);
        for (int i=0;i<n;i++)
        {
            _buttons[i].SetActive(true);

            Button but = _buttons[i].GetComponentInChildren<Button>();
            int tempInt = i;
            but.onClick.AddListener(() => ProcessClick(tempInt));

            _buttons[i].transform.position = transform.position + offset;
            offset += increment;
        }
    }
    void HideButtons()
    {
        foreach( GameObject go in _buttons )
            go.SetActive(false);
    }
    void ProcessClick(int i)
    {
        Debug.Log("Selected option " + i.ToString());
        HideButtons();
        Transition[] tlist = _current.GetTransitions();
        SendMessage(tlist[i].Traverse());
    }

    void Update()
    {
        if (_current != null && Input.GetKeyDown(KeyCode.Space))
        {
            if (_writing)
            {
                _index = _current.Text.Length + 1;
                StopCoroutine(_coroutine);
                StartCoroutine(_coroutine = NextCharacter());
            }
            else if (!_choosing_option)
            {
                Node next = _current.GetTransitions()[0].Traverse();
                SendMessage(next);
            }
        }
    }
}
