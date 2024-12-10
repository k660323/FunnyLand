using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Dimension")]
    public Define.DimensionMode dimensionMode = Define.DimensionMode._3D;
    [Header("View")]
    public Define.CameraMode cameraMode = Define.CameraMode.ThirdPerson;

    [SerializeField]
    protected bool DefaultCursorState = true;

    [Header("ViewPos")]
    public GameObject viewPos;

    public GameObject targetObject { get; protected set; } = null;
    //[SerializeField]
    //Transform targetTransform = null;

    [Header("Zoom (Third person)")]
    public bool updateZoom = true;

    // 콜라이더 없으면 직접 수정
    [SerializeField]
    protected Vector3 offsetFromHead = Vector3.up;
    [Min(0f)]
    [SerializeField]
    protected float distanceToTarget = 5f;
    //[Min(0f)]
    //public float zoomInOutSpeed = 40f;
    [Min(0f)]
    public float zoomInOutLerpSpeed = 10f;
    [Min(0f)]
    public float minDistance = 2f;
    [Min(0.001f)]
    public float maxDistance = 8f;

    protected Vector2 mouseDelta;
    protected Vector3 deltaTargetPos;
    protected Vector3 deltaCam;
    protected Vector3 deltaLocalCam;
    protected Quaternion deltaQuater;
    protected Vector3 deltaDir;
    protected Quaternion prevRotation;

    protected float deltaDistance;
    protected float prevDistance;

    public Vector3 prevForward { get; protected set; }
    public Vector3 prevRight { get; protected set; }

    private void Start()
    {
        Init();
    }

    protected virtual void Init() 
    {
        switch(dimensionMode)
        {
            case Define.DimensionMode._2D:
                Camera.main.orthographic = true;
                Camera.main.usePhysicalProperties = false;
                break;
            case Define.DimensionMode._3D:
                Camera.main.orthographic = false;
                Camera.main.usePhysicalProperties = true;
                break;
        }    
    }

    protected virtual void InitCameraMode() { }

    protected virtual void InitTargetOffsetPos() { }

    public virtual void SetTarget(GameObject target, CinemachineVirtualCamera vCam)
    {
        // vCam은 카메라 컨트롤러를 바라본다.
        // Update해야 카메라 컨트롤러는 플레이어를 바라본다.
        targetObject = target;
        vCam.Follow = viewPos.transform;
        vCam.LookAt = transform;

        InitTargetOffsetPos();
    }

    protected virtual void ViewChangeInput() { }

    protected virtual void TraceTarget(float dt) { }

    protected virtual void Zoom(float dt) { }

    protected virtual void SetValues(float dt) { }

    protected virtual void OnDestroy()
    {
        gameObject.GetComponent<AudioListener>().enabled = false;
        if (Camera.main != null)
            Camera.main.GetComponent<AudioListener>().enabled = true;
    }
}
