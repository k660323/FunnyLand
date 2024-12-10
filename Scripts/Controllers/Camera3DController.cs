using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera3DController : CameraController
{
    //[SerializeField]
    //Transform targetTransform = null;
    public bool switchView = true;
    [Header("mouse X")]
    public bool updateX = true;
    [Header("mouse Y")]
    public bool updateY = true;
    [SerializeField]
    float initialY = 45f;  

    [Header("Collision")]
    public bool collisionDetection = true;
    public float backDetectionLength = 0.5f;

    Renderer[] bodyRenderers = null;
    FirstPresonRenderers firstPresonRenderers = null;
    RaycastHit frontHit,backHit;

    protected override void Init()
    {
        base.Init();
        Managers.GameCursor.SettingGameCursor(DefaultCursorState);
        transform.rotation = Quaternion.Euler(initialY, 0, 0);
        InitCameraMode();
        InitTargetOffsetPos();
    }

    protected override void InitCameraMode()
    {
        if (Camera.main != null)
            Camera.main.GetComponent<AudioListener>().enabled = false;
        gameObject.GetComponent<AudioListener>().enabled = true;

        switch (cameraMode)
        {
            case Define.CameraMode.FirstPerson:
                distanceToTarget = 0;
                break;
            case Define.CameraMode.ThirdPerson:
                distanceToTarget = distanceToTarget != 0 ? Mathf.Clamp(distanceToTarget, minDistance, maxDistance) : Mathf.Clamp(-prevDistance, minDistance, maxDistance);
                prevDistance = -distanceToTarget;
                break;
        }
      
        if(viewPos == null)
        {
            viewPos = Util.FindChild(gameObject, "viewPos");
            if (viewPos == null)
            {
                viewPos = new GameObject("viewPos");
                viewPos.transform.SetParent(transform);
            }
        }
        viewPos.transform.localPosition = new Vector3(0, 0, -distanceToTarget);
    }

    protected override void InitTargetOffsetPos()
    {
        if (targetObject != null)
        {
            if (bodyRenderers == null || (bodyRenderers.Length > 0 && !bodyRenderers[0].gameObject.activeSelf))
                bodyRenderers = targetObject.GetComponentsInChildren<Renderer>();

            if (firstPresonRenderers == null)
            {
                firstPresonRenderers = targetObject.GetComponentInChildren<FirstPresonRenderers>();
            }

            if (targetObject.TryGetComponent(out CapsuleCollider collider))
                offsetFromHead = new Vector3(0, collider.height * 0.85f);
            else if (targetObject.TryGetComponent(out CharacterController cController))
                offsetFromHead = new Vector3(0, cController.height * 0.85f);

            transform.position = targetObject.transform.position + offsetFromHead;
        }
    }

    public override void SetTarget(GameObject target, CinemachineVirtualCamera vCam)
    {
        // vCam은 카메라 컨트롤러를 바라본다.
        // Update해야 카메라 컨트롤러는 플레이어를 바라본다.
        targetObject = target;
        vCam.Follow = viewPos.transform;
        vCam.LookAt = transform;

        InitTargetOffsetPos();
        HandleBodyVisibility();
    }

    private void Update()
    {
        AltView();
    }

    private void LateUpdate()
    {
        float fdt = Time.fixedDeltaTime;
        ViewChangeInput();
        TraceTarget(fdt);
        CallMode(fdt);
        SetValues(fdt);
    }

    protected override void ViewChangeInput()
    {
        if(Input.GetKeyDown(KeyCode.V) && switchView)
        {
            switch(cameraMode)
            {
                case Define.CameraMode.FirstPerson:
                    cameraMode = Define.CameraMode.ThirdPerson;
                    break;
                case Define.CameraMode.ThirdPerson:
                    cameraMode = Define.CameraMode.FirstPerson;
                    break;
            }
            InitCameraMode();
            HandleBodyVisibility();
        }
    }

    void CallMode(float dt)
    {
        switch (cameraMode)
        {
            case Define.CameraMode.FirstPerson:

                break;
            case Define.CameraMode.ThirdPerson:
                Zoom(dt);
                CheckBlock(dt);
                break;
        }
    }

    void HandleBodyVisibility()
    {
        if (cameraMode == Define.CameraMode.FirstPerson)
        {
            InitTargetOffsetPos();
            if (bodyRenderers != null)
                for (int i = 0; i < bodyRenderers.Length; i++)
                {
                    if (bodyRenderers[i] == null)
                        continue;

                    if (firstPresonRenderers != null)
                    {
                        Renderer renderer;
                        renderer = firstPresonRenderers.ExceptionRenders.Find(x => x == bodyRenderers[i]);
                        if (renderer)
                        {
                            // 안 보임
                            SetRenderer(i, true);
                            continue;
                        }
                        renderer = firstPresonRenderers.FirstRenders.Find(x => x == bodyRenderers[i]);
                        if (renderer)
                        {
                            // 보임
                            SetRenderer(i, false);
                            //renderer.gameObject.SetActive(true);
                            continue;
                        } 
                    }

                    SetRenderer(i, true);
                }
        }
        else
        {
            if (bodyRenderers != null)
                for (int i = 0; i < bodyRenderers.Length; i++)
                {
                    if (bodyRenderers[i] == null)
                        continue;
                    if (firstPresonRenderers != null)
                    {
                        Renderer renderer;
                        renderer = firstPresonRenderers.FirstRenders.Find(x => x == bodyRenderers[i]);
                        if (renderer)
                        {
                            // 안보임
                            SetRenderer(i, true);
                            //renderer.gameObject.SetActive(false);
                            continue;
                        }
                    }
                    // 보임
                    SetRenderer(i, false);
                }
        }
    }

    void SetRenderer(int i, bool _hideBody)
    {
        if (bodyRenderers[i].GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
        {
            SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)bodyRenderers[i];
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.forceRenderingOff = _hideBody;
            }
        }
        else
        {
            bodyRenderers[i].enabled = !_hideBody;
        }
    }

    float CheckYaw(float x)
    {
        if (x < 180)
            return Mathf.Clamp(x, -1f, 80f);
        else
            return Mathf.Clamp(x, 280f, 360f);
    }

    protected override void TraceTarget(float dt)
    {
        if (targetObject == null)
            return;

        mouseDelta.x = updateX ? (Managers.Input.MouseX * Managers.Setting.gamePlayOption.mouseHorizontal) * dt : 0;
        mouseDelta.y = updateY ? (Managers.Input.MouseY * Managers.Setting.gamePlayOption.mouseVirtical) * dt : 0;

        Vector3 camAngle = transform.rotation.eulerAngles;

        float x = CheckYaw(camAngle.x - mouseDelta.y);
        float y = camAngle.y + mouseDelta.x;

        deltaQuater = Quaternion.Euler(new Vector3(x, y, 0));
        deltaTargetPos = targetObject.transform.position + offsetFromHead;
        deltaCam = viewPos.transform.position;
        deltaLocalCam = viewPos.transform.localPosition;
        deltaDir = (deltaCam - deltaTargetPos).normalized;
        deltaDistance = (deltaTargetPos - deltaCam).magnitude;
    }

    protected override void Zoom(float dt)
    {
        if (!updateZoom)
            return;

        float scroll = Managers.Input.MouseScrollWheel * Managers.Setting.gamePlayOption.wheel;
        if (scroll == 0)
            return;

        deltaDistance = Mathf.Clamp(deltaDistance + (scroll * -10), minDistance, maxDistance);
        deltaLocalCam = new Vector3(0, 0, -deltaDistance);
        prevDistance = deltaLocalCam.z;
    }

    void CheckBlock(float dt)
    {
        if (!collisionDetection)
            return;

        if (Physics.Raycast(deltaTargetPos, deltaDir, out frontHit, deltaDistance))
        {
            if(frontHit.transform.gameObject.tag != "Player" && frontHit.transform.gameObject.tag != "Bot")
            {
                deltaLocalCam = transform.InverseTransformPoint(frontHit.point);
            }
        }
        else
        {
            if (!Physics.Raycast(deltaCam, deltaDir, out backHit, backDetectionLength))
            {
                 deltaLocalCam = Vector3.Lerp(deltaLocalCam, new Vector3(0, 0, prevDistance), 20f * dt);
            }
        }
    }

    protected override void SetValues(float dt)
    {
        transform.position = deltaTargetPos;
        transform.rotation = deltaQuater;
        viewPos.transform.localPosition = Vector3.Lerp(viewPos.transform.localPosition, deltaLocalCam, zoomInOutLerpSpeed * dt);
    }

    void AltView()
    {
        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            prevRotation = transform.rotation;
            prevForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
            prevRight = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;
        }
        else if(Input.GetKeyUp(KeyCode.LeftAlt))
            transform.rotation = prevRotation;
    }
}
