using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    public AnimationCurve curve;
    private void Start()
    {
        for (float i = 0; i < 1.7f; i += 0.2f)
        {
            Debug.Log(curve.Evaluate(i));
        }
    }

}
