using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEffect : MonoBehaviour
{
    [SerializeField]
    MagneticField magnetic;
    [SerializeField]
    CameraController controller;
    [SerializeField]
    GameObject blueZoneState;

    private void Start()
    {
        magnetic = GameObject.FindGameObjectWithTag("MagneticField").GetComponent<MagneticField>();
        controller = Managers.Game.camController;
        blueZoneState = Managers.Resource.Instantiate("UI/Scene/Contents/BlueZoneState");
        blueZoneState.transform.SetParent(Managers.UI.ContentsRoot.transform);
    }

    private void LateUpdate()
    {
        if (controller == null || controller.targetObject == null)
            return;
        if (magnetic == null)
            return;

        float pDistacneToTarget = (new Vector3(controller.targetObject.transform.position.x, 0f, controller.targetObject.transform.position.z) - magnetic.transform.position).sqrMagnitude;
        if (pDistacneToTarget > magnetic.pRadius)
            blueZoneState.SetActive(true);
        else
            blueZoneState.SetActive(false);
    }
}
