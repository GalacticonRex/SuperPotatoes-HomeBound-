using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour {

    private AudioSource _audio;

	// Use this for initialization
	void Start () {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            Destroy(this.gameObject);
        StartCoroutine(AudioInitial());
    }

    public void Play()
    {
        Vector3 rand = Random.onUnitSphere;
        transform.position = new Vector3(rand.x * 100, rand.y * 50, rand.z * 100);
        _audio.Play();
        StartCoroutine(AudioCallback(_audio.clip.length));
    }
    IEnumerator AudioInitial()
    {
        float waitFor = Random.Range(1.0f, 40.0f);
        yield return new WaitForSeconds(waitFor);
        Play();
    }
    IEnumerator AudioCallback(float len)
    {
        yield return new WaitForSeconds(len);
        StartCoroutine(AudioInitial());
    }
}
