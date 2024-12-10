using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2DController : CameraController
{
    [SerializeField]
    BoxCollider2D mapCollider;

    protected override void Init()
    {
        base.Init();
        Managers.GameCursor.SettingGameCursor(DefaultCursorState);
        InitCameraMode();
        InitTargetOffsetPos();
    }

    protected override void InitCameraMode()
    {
        switch (cameraMode)
        {
            case Define.CameraMode.ThirdPerson:
                distanceToTarget = distanceToTarget != 0 ? Mathf.Clamp(distanceToTarget, minDistance, maxDistance) : Mathf.Clamp(-prevDistance, minDistance, maxDistance);
                prevDistance = -distanceToTarget;
                break;
        }

        if (viewPos == null)
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
            if (targetObject.TryGetComponent(out CapsuleCollider2D collider))
                offsetFromHead = new Vector3(0, collider.size.y * 0.85f);
            else
                offsetFromHead = Vector3.up;

            transform.position = targetObject.transform.position + offsetFromHead;
        }
    }

    public override void SetTarget(GameObject target, CinemachineVirtualCamera vCam)
    {
        if (Camera.main != null)
            Camera.main.GetComponent<AudioListener>().enabled = false;

        if (target == null)
        {
            gameObject.GetComponent<AudioListener>().enabled = true;
        }
        else
        {
            AudioListener targetAudioListener;

            if (targetObject != null && targetObject != target)
            {
                targetAudioListener = targetObject.GetComponent<AudioListener>();
                if (targetAudioListener != null)
                {
                    targetAudioListener.enabled = false;
                }
            }

            targetAudioListener = Util.GetOrAddComponent<AudioListener>(target);
            targetAudioListener.enabled = true;

            // vCam은 카메라 컨트롤러를 바라본다.
            // Update해야 카메라 컨트롤러는 플레이어를 바라본다.
            targetObject = target;
        }
        vCam.Follow = viewPos.transform;
        vCam.LookAt = transform;

        InitTargetOffsetPos();
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        TraceTarget(dt);
        CallMode(dt);
        SetValues(dt);
    }

    protected override void TraceTarget(float dt)
    {
        if (targetObject == null)
            return;

        deltaQuater = transform.rotation;
        deltaTargetPos = targetObject.transform.position + offsetFromHead;
        deltaCam = viewPos.transform.position;
        deltaLocalCam = viewPos.transform.localPosition;
        deltaDir = (deltaCam - deltaTargetPos).normalized;
        deltaDistance = (deltaTargetPos - deltaCam).magnitude;
    }

    void CallMode(float dt)
    {
        switch (cameraMode)
        {
            case Define.CameraMode.ThirdPerson:
                Zoom(dt);
                break;
        }
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

    protected override void SetValues(float dt)
    {
        if (targetObject == null)
            return;

        if(mapCollider != null)
        {
            float height = Camera.main.orthographicSize;
            float width = height * Screen.width / Screen.height;

            float lx = (mapCollider.size.x / 2) - width;
            float clampX = Mathf.Clamp(deltaTargetPos.x, -lx + mapCollider.offset.x, lx + mapCollider.offset.x);
            float ly = (mapCollider.size.y / 2) - height;
            float clampY = Mathf.Clamp(deltaTargetPos.y, -ly + mapCollider.offset.y, ly + mapCollider.offset.y);

            transform.position = new Vector2(clampX, clampY);
        }
        else
        {
            transform.position = deltaTargetPos;
        }

      
        transform.rotation = deltaQuater;
        viewPos.transform.localPosition = Vector3.Lerp(viewPos.transform.localPosition, deltaLocalCam, zoomInOutLerpSpeed * dt);
    }

    protected override void OnDestroy()
    {
        if (targetObject != null)
        {
            AudioListener targetAudioListener = targetObject.GetComponent<AudioListener>();
            if (targetAudioListener != null)
                targetAudioListener.enabled = false;
        }

        gameObject.GetComponent<AudioListener>().enabled = false;
       
        if (Camera.main != null)
            Camera.main.GetComponent<AudioListener>().enabled = true;
    }
}
