using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation2D : MonoBehaviour {


    public Vector2[] Position;
    public float[] Rotation;
    public float[] Scale;
    public float[] Duration;

    private int _index;
    private Vector2 _current_position;
    private float _current_rotation;
    private float _current_scale;
    private float _current_time;

    // Use this for initialization
    void Start () {
        _current_position = transform.position;
        _current_rotation = transform.rotation.eulerAngles.y;
        _current_scale = transform.localScale.x;
        _index = 0;
        _current_time = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
        if (_index >= Duration.Length)
            return;

        if (Duration[_index] == -1)
        {
            Vector2 dpos = Position[_index] * Time.deltaTime;
            float drot = Rotation[_index] * Time.deltaTime;
            float dscl = Scale[_index] * Time.deltaTime;

            transform.position = transform.position + new Vector3(dpos.x, dpos.y, 0);
            Vector3 eul = transform.rotation.eulerAngles;
            eul.z += drot;
            transform.rotation = Quaternion.Euler(eul);
            transform.localScale = transform.localScale + new Vector3(dscl, dscl, 1);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _current_time = Duration[_index];
            }
            float d = _current_time / Duration[_index];

            Vector2 pos = _current_position * (1.0f - d) + Position[_index] * d;
            float rot = _current_rotation * (1.0f - d) + Rotation[_index] * d;
            float scl = _current_scale * (1.0f - d) + Scale[_index] * d;

            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
            Vector3 eul = transform.rotation.eulerAngles;
            eul.z = rot;
            transform.rotation = Quaternion.Euler(eul);
            transform.localScale = new Vector3(scl, scl, 1);

            _current_time += Time.deltaTime;

            if (_current_time >= Duration[_index])
            {
                _current_position = Position[_index];
                _current_rotation = Rotation[_index];
                _current_scale = Scale[_index];
                _current_time = 0.0f;
                _index++;
            }
        }
    }
}
