using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCamera : MonoBehaviour
{
    [SerializeField] Camera parentCamera;
    Camera thisCamera;

    public bool aiming;

    private void Start()
    {
        thisCamera = GetComponent<Camera>();
    }
    void Update()
    {
        float fov = aiming ? 35 : 60;
        thisCamera.fieldOfView = Mathf.Lerp(thisCamera.fieldOfView, fov, Time.deltaTime * 15);
        //thisCamera.fieldOfView = parentCamera.fieldOfView;
    }
}
