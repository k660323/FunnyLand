using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager
{ 

    public bool ParticleAwakePlay(string path, Vector3 pos, Transform parent)
    {
        Particle particle = Managers.Resource.Instantiate("Effects/" + path).GetComponent<Particle>();
        particle.transform.position = pos;
        particle.transform.SetParent(parent);
        particle.Play();

        return true;
    }

    public Particle ParticleManualPlay(string path, Vector3 pos, Transform parent)
    {
        Particle particle = Managers.Resource.Instantiate("Effects/" + path).GetComponent<Particle>();
        particle.transform.position = pos;
        particle.transform.SetParent(parent);

        return particle;
    }
}
