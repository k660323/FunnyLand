using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    float m_time;
    [SerializeField]
    float disableTime;

    DelayActive[] nextActive;

    private void Awake()
    {
        nextActive= transform.GetComponentsInChildren<DelayActive>();
    }

    public void Play()
    {
        m_time = Time.time;
        foreach(DelayActive dA in nextActive)
            dA.Play(m_time);

        StartCoroutine(DestoryTimer());
    }

    IEnumerator DestoryTimer()
    {
        while (Time.time <= m_time + disableTime)
            yield return null;

        Managers.Resource.Destroy(gameObject);
        Stop();
    }

    private void Stop()
    {
        StopAllCoroutines();
        foreach (DelayActive dA in nextActive)
            dA.Stop();
    }
}