using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InputManager
{
    public UnityAction axisAction = null; // 초기화 x
    public UnityAction keyAction = null;
    public UnityAction uIAction = null; // 초기화 x
    public UnityAction<Define.MouseEvent> MouseAction = null;

    bool isCameraMove = true;
    public bool IsCameraMove
    {
        get
        {
            return isCameraMove;
        }
        set
        {
            isCameraMove = value;
        }
    }
    public float MouseX;
    public float MouseY;
    public float MouseScrollWheel;

    bool isControll = true;
    public bool IsControll
    {
        get
        {
            return isControll;
        }
        set
        {
            isControll = value;
        }
    }
    public float Horizontal;
    public float Vertical;
    public bool LeftShift;
    public bool LeftAlert;
    public bool IsMouseButtonDownLeft;
    public bool SpaceBar;

    bool isUIAction = true;

    public bool IsUIActive
    {
        get
        {
            return isUIAction;
        }
        set

        {
            isUIAction = value;
        }
    }

    bool _lPressed = false;
    bool _rPressed = false;

    float _pressedTime = 0f;

    public void Init()
    {
        axisAction -= AxisInput;
        axisAction += AxisInput;

        uIAction -= Managers.UI.CommandSetting;
        uIAction += Managers.UI.CommandSetting;
    }
    
    void AxisInput()
    {
        if (Managers.Game.isGameStart)
        {
            MouseX = Input.GetAxis("Mouse X") * (IsCameraMove ? 1 : 0);
            MouseY = Input.GetAxis("Mouse Y") * (IsCameraMove ? 1 : 0);
            MouseScrollWheel = Input.GetAxisRaw("Mouse ScrollWheel") * (IsCameraMove ? 1 : 0);
            Horizontal = Input.GetAxis("Horizontal") * (IsControll ? 1 : 0);
            Vertical = Input.GetAxis("Vertical") * (IsControll ? 1 : 0);
            LeftShift = IsControll ? Input.GetKey(KeyCode.LeftShift) : false;
            LeftAlert = IsControll ? Input.GetKey(KeyCode.LeftAlt) : false;
            IsMouseButtonDownLeft = IsControll ? Input.GetMouseButtonDown(0) : false;
            SpaceBar = IsControll ? Input.GetKeyDown(KeyCode.Space) : false;
        }
        else
        {
            MouseX = 0f;
            MouseY = 0f;
            MouseScrollWheel = 0f;

            Horizontal = 0f;
            Vertical = 0f;
            LeftShift = false;
            LeftAlert = false;
            IsMouseButtonDownLeft = false;
            SpaceBar = false;
        }
    }

    // Update is called once per frame
    public void OnUpdate()
    {
        if (axisAction != null)
            axisAction?.Invoke();

        if (keyAction != null)
            keyAction?.Invoke();

        if (uIAction != null && IsUIActive)
            uIAction?.Invoke();

        // UI 영역에 있을시 false 반환
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (MouseAction != null)
        {
            if(Input.GetMouseButton(0))
            {
                if(!_lPressed)
                {
                    MouseAction.Invoke(Define.MouseEvent.LPointerDown);
                    _pressedTime = Time.time;
                }
                MouseAction.Invoke(Define.MouseEvent.LPress);
                _lPressed = true;
            }
            else if(Input.GetMouseButton(1))
            {
                if (!_rPressed)
                {
                    MouseAction.Invoke(Define.MouseEvent.RPointerDown);
                    _pressedTime = Time.time;
                }
                MouseAction.Invoke(Define.MouseEvent.RPress);
                _rPressed = true;
            }
            else
            {
                if (_lPressed)
                {
                    if (Time.time < _pressedTime + 0.2f)
                        MouseAction.Invoke(Define.MouseEvent.LClick);
                    MouseAction.Invoke(Define.MouseEvent.LPointerUp);
                }
                else if (_rPressed)
                {
                    if (Time.time < _pressedTime + 0.2f)
                        MouseAction.Invoke(Define.MouseEvent.RClick);
                    MouseAction.Invoke(Define.MouseEvent.RPointerUp);
                }
                
                _lPressed = false;
                _rPressed = false;

                _pressedTime = 0f;
            }
        }
    }

    public void Clear()
    {
        keyAction = null;
        MouseAction = null;
        IsUIActive = true;
        IsControll = true;
        IsCameraMove = true;
    }
}
