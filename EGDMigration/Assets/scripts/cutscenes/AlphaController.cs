using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaController : MonoBehaviour {

    public float TargetAlpha = 1.0f;
    public float TimeToRun = 1.0f;
    public float TimeToWait = 0.0f;
    public KeyCode KeyToPress = KeyCode.None;
    public ICutsceneAction Action = null;

    private UnityEngine.UI.MaskableGraphic _sprite;
    private float _timer = 0.0f;
    private float _orig = 0.0f;
    private bool _running = false;

    private void Start()
    {
        _sprite = GetComponent<UnityEngine.UI.MaskableGraphic>();
        _orig = _sprite.color.a;
    }
	
	private void Update ()
    {
        if ( _running )
        {
            if (_timer >= TimeToRun)
                return;

            _timer += Time.deltaTime;
            if (_timer > TimeToRun)
            {
                if (Action != null )
                    Action.PerformAction(); 
                _timer = TimeToRun;
            }

            Color col = _sprite.color;
            float d = _timer / TimeToRun;
            col.a = _orig * (1.0f - d) + TargetAlpha * d    ;
            _sprite.color = col;
        }
        else
        {
            if (KeyToPress != KeyCode.None && Input.GetKeyDown(KeyToPress))
            {
                _running = true;
                return;
            }

            if( TimeToWait > 0 )
            {
                _timer += Time.deltaTime;

                if ( _timer >= TimeToWait)
                {
                    _timer = 0.0f;
                    _running = true;
                }
            }
        }
	}
}
