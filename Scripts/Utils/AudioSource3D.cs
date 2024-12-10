using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSource3D : MonoBehaviour
{
    public AudioSource audioSource;

    public void Play()
    {
        audioSource.Play();
        Invoke("ReturnStack", audioSource.clip.length);
    }

    void ReturnStack()
    {
        CancelInvoke("ReturnStack");
        Managers.Resource.Destroy(gameObject);
    }
}
