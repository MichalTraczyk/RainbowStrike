using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public float FixedSize = .005f;
    [HideInInspector]
    public Camera cam;

    private void Start()
    {
        cam = Camera.main;
        Destroy(this.gameObject, 5);
    }
    void AutoResize()
    {
        var distance = (cam.transform.position - transform.position).magnitude;
        var size = distance * FixedSize * cam.fieldOfView;
        transform.localScale = Vector3.one * size;
        transform.forward = transform.position - cam.transform.position;
    }
    void Update()
    {
        AutoResize();
    }
}
