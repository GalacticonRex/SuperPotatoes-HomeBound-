using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WriteText : MonoBehaviour {

    public Text Textbox;
    public string Value;
    public float SecondsPerLetter = 1.0f;
    public float InitialDelay = 0.0f;
    private string _string;
    private string _written;
    private int _next;
    private int _used;
    private bool _show;
    private float[] _values;

	// Use this for initialization
	void Start () {
        _string = Value;
        _values = new float[Value.Length];
        for (int i = 0; i < _values.Length; i++)
            _values[i] = 0.0f;
        _next = Random.Range(0, _values.Length);
        _used = 0;
        _show = true;

        Textbox.text = GenerateText();
    }

    string GenerateText()
    {
        string buffer = "";
        for ( int i=0;i<_values.Length;i++ )
        {
            int val = (int)(_values[i] * 255.0f);
            buffer += "<color=#ffffff" + val.ToString("X2") + ">" + _string[i] + "</color>";
        }
        return buffer;
    }
	
	// Update is called once per frame
	void Update () {
        if (_next >= Value.Length)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _written = Value;
            _next = Value.Length;
            Textbox.text = _written;
        }

        if (InitialDelay > 0)
            InitialDelay -= Time.deltaTime;
        else if (!_show)
        {

        }
        else
        {
            _values[_next] += Time.deltaTime / SecondsPerLetter;

            if (_values[_next] >= 1.0f)
            {
                _values[_next] = 1.0f;
                _used++;
                if (_used < _values.Length)
                {
                    while (_values[_next] != 0)
                        _next = Random.Range(0, _values.Length);
                }
            }

            Textbox.text = GenerateText();
        }
    }
}
