using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPresonRenderers : MonoBehaviour
{
    [SerializeField]
    List<Renderer> excetpionRenders;

    [SerializeField]
    List<Renderer> firstRenders;

    public List<Renderer> ExceptionRenders
    {
        get
        {
            return excetpionRenders;
        }
    }

    public List<Renderer> FirstRenders
    {
        get
        {
            return firstRenders;
        }
    }

    public void Awake()
    {
        foreach(var r in firstRenders)
        {
            if (r.GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
            {
                SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)r;
                if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.forceRenderingOff = true;
                }
            }
            else
            {
                r.enabled = false;
            }
        }
    }
}
