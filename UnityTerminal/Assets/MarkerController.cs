using System.Collections;
using System.Collections.Generic;
using ReacTiVisionHal.Model;
using UnityEngine;

public class MarkerController : MonoBehaviour {

    public float smoothing = .1f;

    private Vector3 _target;

    public Vector3 Target
    {
        get { return _target; }
        set
        {
            _target = value;

            StopCoroutine(nameof(Movement));
            StartCoroutine(Movement());
        }
    }

    private int count = 0;

    IEnumerator Movement()
    {
        while (Vector3.Distance(transform.position, _target) > .05f)
        {
            transform.position = Vector3.Lerp(transform.position, _target, smoothing * Time.deltaTime);
            yield return null;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //transform.Translate(0,0,.2f*Time.deltaTime);
    }
}
