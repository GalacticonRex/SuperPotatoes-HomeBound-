using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueInterface : MonoBehaviour
{
    public TriggerDialogue Initial;
    public TextAsset[] Source;
    public GameObject ButtonPrefab;
    public Text Speaker;
    public Text Display;
    public InputField CommandInput;
    public float TextSpeed = 1.0f;

    private MetaLogic _logic;
    private ParseDialogue.DialogueDatabase _database;
    private ParseDialogue.DialogueNode _current = null;
    private bool _old_active = false;
    private bool _active = false;
    private int _index = 0;
    private bool _writing = false;
    private bool _choosing_option = false;
    private int _selection_made = -1;
    private int _prompt_number = 0;
    private GameObject[] _speaker;
    private GameObject[] _dialogue;
    private GameObject[] _input;

    private IEnumerator _coroutine = null;
    private List<GameObject> _buttons = new List<GameObject>();

    public bool OldActive()
    {
        return _old_active;
    }
    public bool Active()
    {
        return _active;
    }
    public void ShowDialogue(TriggerDialogue start)
    {
        _logic.StartTrigger(start);
        ShowDialogue(_database.start(start.StateLabel));
    }
    public void ShowDialogue(string start)
    {
        ShowDialogue(_database.start(start));
    }
    private void ShowDialogue(ParseDialogue.DialogueNode start)
    {
        _current = start;

        if (start == null)
        {
            _logic.FinishTrigger();
            PropagateInterface(false);
            return;
        }
        if (!_active)
        {
            PropagateInterface(true);
            HideButtons();
        }
        if (start != _current)
        {
            CommandInput.text = "";
            _prompt_number = 0;
        }

        bool input = _prompt_number < start.prompts;
        bool speaker = !input && (start.speaker != null && start.speaker.Length != 0);

        SetSpeakerBox(speaker);
        SetInputBox(input);
        if (input)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(CommandInput.gameObject, null);

        _index = 0;
        _writing = true;
        _selection_made = -1;
        
        Speaker.text = start.speaker;
        Display.text = "";
        StartCoroutine(_coroutine = NextCharacter());
    }

    private IEnumerator NextCharacter()
    {
        if (_index > _current.text.Length)
        {
            yield return null;

            _writing = false;
            if (_current.choices != 0)
                DisplayOptions(_current);
        }
        else
        {
            yield return new WaitForSeconds(TextSpeed / 30.0f);
            Display.text = _current.text.Substring(0, System.Math.Min(_index, _current.text.Length));
            _index++;
            StartCoroutine(_coroutine = NextCharacter());
        }
    }
    private void DisplayOptions(ParseDialogue.DialogueNode options)
    {
        RequestButtons(options.choices);
        for (int i = 0; i < _buttons.Count; i++)
        {
            Text txt = _buttons[i].GetComponentInChildren<Text>();
            txt.text = options.choice(i);
        }
        _choosing_option = true;
    }
    private void RequestButtons(int n)
    {
        if (_buttons.Count < n)
        {
            for (int i = _buttons.Count; i < n; i++)
            {
                GameObject go = Instantiate(ButtonPrefab);
                go.transform.SetParent(transform);
                _buttons.Add(go);
                go.SetActive(false);
            }
        }

        float buttonHeight = 62.0f;
        Vector3 offset = new Vector3(0, (_buttons.Count / 2.0f) * buttonHeight, 0);
        Vector3 increment = new Vector3(0, -buttonHeight, 0);
        for (int i = 0; i < n; i++)
        {
            _buttons[i].SetActive(true);

            Button but = _buttons[i].GetComponentInChildren<Button>();
            int tempInt = i;
            but.onClick.AddListener(() => ProcessClick(tempInt));

            _buttons[i].transform.position = transform.position + offset;
            offset += increment;
        }
    }
    private void HideButtons()
    {
        foreach (GameObject go in _buttons)
            go.SetActive(false);
    }
    private void PropagateInterface(bool value)
    {
        Stack<GameObject> stack = new Stack<GameObject>();
        stack.Push(gameObject);

        while ( stack.Count > 0 )
        {
            GameObject obj = stack.Pop();
            foreach( Transform t in obj.transform )
            {
                stack.Push(t.gameObject);
                t.gameObject.SetActive(value);
            }
        }
        _active = value;
    }
    private void ProcessClick(int i)
    {
        _selection_made = i;
    }

    private void SetSpeakerBox(bool x)
    {
        foreach (GameObject go in _speaker)
        {
            go.SetActive(x);
        }
    }
    private void SetDialogueBox(bool x)
    {
        foreach (GameObject go in _dialogue)
        {
            go.SetActive(x);
        }
    }
    private void SetInputBox(bool x)
    {
        foreach (GameObject go in _input)
        {
            go.SetActive(x);
        }
    }

    private void Start()
    {
        _logic = GetComponent<MetaLogic>();

        _speaker = GameObject.FindGameObjectsWithTag("SpeakerBox");
        _dialogue = GameObject.FindGameObjectsWithTag("DialogueBox");
        _input = GameObject.FindGameObjectsWithTag("InputBox");

        PropagateInterface(false);
        Database db = GetComponent<Database>();
        _database = new ParseDialogue.DialogueDatabase(db);
        foreach(TextAsset txt in Source)
        {
            ParseDialogue.Parse(txt, ref _database);
        }
        
        if( Initial != null)
            ShowDialogue(Initial);
    }
    private void Update()
    {
        if (!_active) return;

        if (_current != null)
        {
            if (_prompt_number < _current.prompts)
            {
                if (_old_active && Input.GetKeyDown(KeyCode.Return) )
                {
                    _prompt_number++;
                    if ( _current.Submit(CommandInput.text) )
                    {
                        StopCoroutine(_coroutine);
                        ShowDialogue(_current);
                    }
                    else
                    {
                        ShowDialogue(_database.next());
                    }
                }
            }
            else if (_old_active && Input.GetKeyDown(KeyCode.Space))
            {
                if (_writing)
                {
                    _index = _current.text.Length + 1;
                    Display.text = _current.text;
                    StopCoroutine(_coroutine);
                    StartCoroutine(_coroutine = NextCharacter());
                }
                else if (!_choosing_option)
                {
                    ParseDialogue.DialogueNode n = _database.next();
                    ShowDialogue(n);
                }
            }
        }

        if ( _choosing_option && _selection_made != -1 )
        {
            _choosing_option = false;
            HideButtons();
            ShowDialogue(_database.selectionMade(_selection_made));
        }

        _old_active = _active;
    }
}
