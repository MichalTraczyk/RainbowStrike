using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyAudioContainer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this, GetComponent<AudioSource>().clip.length);
    }
}
