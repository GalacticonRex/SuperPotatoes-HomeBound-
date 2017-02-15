using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

    public float Rate;

    private float _anchor;
    private float _value;

	// Use this for initialization
	void Start () {
        _value = 0.0f;
        _anchor = transform.position.y;

        Vector3 eul = transform.rotation.eulerAngles;
        eul.y = Random.Range(0.0f, 360.0f);
        transform.rotation = Quaternion.Euler(eul);
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 nv = transform.position;
        _value += Time.deltaTime / Rate;
        if (_value > 2.0f)
            _value -= 2.0f;
        nv.y = _anchor + (Mathf.Cos((_value + 1.0f) * Mathf.PI) + 1.0f) / 2.0f;
        transform.position = nv;

        Vector3 eul = transform.rotation.eulerAngles;
        eul.y += Time.deltaTime * 10.0f;
        transform.rotation = Quaternion.Euler(eul);
    }
}
