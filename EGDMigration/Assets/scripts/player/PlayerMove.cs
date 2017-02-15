using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

    public Camera _view;

    private bool _movable = true;
    private AssistantLabel _assist;
    private Rigidbody _player;
    private DialogueInterface _interface;
    private Collider _other;
    private Vector3 _mouse;

    public void SetMovable(bool n)
    {
        _movable = n;
    }
	
	void Start ()
    {
        _player = GetComponent<Rigidbody>();
        _interface = FindObjectOfType<DialogueInterface>();
        _mouse = Input.mousePosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerDialogue trig = other.GetComponent<TriggerDialogue>();
        if ( trig != null )
        {
            _other = other;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == _other)
        {
            _other = null;
        }
    }

    void Update ()
    {
        if (!_movable)
            return;

        if (_interface != null && _interface.Active())
            return;

        if ( Input.GetKeyDown(KeyCode.Space) && _other != null )
        {
            TriggerDialogue trig = _other.GetComponent<TriggerDialogue>();
            _other = null;
            _player.velocity = new Vector3(0, 0, 0);
            trig.EnterDialogue();
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 rot = _view.transform.rotation.eulerAngles;
            Vector3 dif = Input.mousePosition - _mouse;

            rot.y = rot.y + dif.x;
            float delta = (rot.x - dif.y);
            while (delta > 180.0f)
                delta -= 360.0f;
            rot.x = Mathf.Min(45.0f, Mathf.Max(-45.0f, delta));

            _view.transform.rotation = Quaternion.Euler(rot);
        }
        _mouse = Input.mousePosition;
        Vector3 forward = Vector3.Normalize(new Vector3(_view.transform.forward.x, 0, _view.transform.forward.z));
        Vector3 right = Vector3.Normalize(new Vector3(_view.transform.right.x, 0, _view.transform.right.z));

        Vector3 nv = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W))
            nv += forward;
        if ( Input.GetKey(KeyCode.S) )
            nv -= forward;
        if (Input.GetKey(KeyCode.A))
            nv -= right;
        if (Input.GetKey(KeyCode.D))
            nv += right;

        _player.velocity = Vector3.Normalize(nv) * 25.0f;
    }
}
