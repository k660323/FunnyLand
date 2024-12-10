using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class CRigidBody3D : MonoBehaviour
{
    [Min(0.0001f)]
    public float Mess = 1f;
    [Min(1f)]
    public float Drag = 1f;
    [Min(1f)]
    public float AngularDrag = 1f;
    public bool useGravity = true;
    public bool isKinematic = false;
    [Header("Constraints")]
    public bool[] FreezePosition = new bool[3];
    public bool[] FreezeRotation = new bool[3];

    ForceMode forceModeV;
    ForceMode forceModeR;

    [SerializeField]
    BaseController bc;

    [SerializeField]
    Vector3 velocity;

    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }

    //bool flag;
    [SerializeField]
    float resetGroundY = -0.082f;
    // -0.278528

    Ray ray;
    RaycastHit rayHit;
    [SerializeField]
    float DetectRay = 1f;

    float VelocityX
    {
        get
        {
            return velocity.x;
        }
        set
        {
            if ((velocity.x >= 0 && value <= 0) || (velocity.x <= 0 && value >= 0))
                value = 0;

            velocity.x = value;
        }
    }

    float VelocityY
    {
        get
        {
            return velocity.y;
        }
        set
        {
            if ((velocity.y >= 0 && value <= 0) || (velocity.y <= 0 && value >= 0))
                value = 0;

            velocity.y = value;
        }
    }

    float VelocityZ
    {
        get
        {
            return velocity.z;
        }
        set
        {
            if ((velocity.z >= 0 && value <= 0) || (velocity.z <= 0 && value >= 0))
                value = 0;

            velocity.z = value;
        }
    }

    [SerializeField]
    Vector3 anglerVelocity;

    float AnglerVX
    {
        get
        {
            return anglerVelocity.x;
        }
        set
        {
            if ((anglerVelocity.x > 0 && value < 0) || (anglerVelocity.x < 0 && value > 0))
                value = 0;

            anglerVelocity.x = value;
        }
    }

    float AnglerVY
    {
        get
        {
            return anglerVelocity.y;
        }
        set
        {
            if ((anglerVelocity.y > 0 && value < 0) || (anglerVelocity.y < 0 && value > 0))
                value = 0;

            anglerVelocity.y = value;
        }
    }

    float AnglerVZ
    {
        get
        {
            return anglerVelocity.z;
        }
        set
        {
            if ((anglerVelocity.z > 0 && value < 0) || (anglerVelocity.z < 0 && value > 0))
                value = 0;

            anglerVelocity.z = value;
        }
    }

    CharacterController target;

    private void OnEnable()
    {
        target = GetComponent<CharacterController>();
        bc = GetComponent<BaseController>();
    }
    
    private void FixedUpdate()
    {
        if (!target || !target.enabled)
            return;

        UpdatePosition();
        UpdatePhysic();
        UpdateGravity();
        UpdatePhysicRotaion();
    }

    protected virtual void UpdateFreezePosition()
    {
        if (FreezePosition[0])
            VelocityX = 0f;
        if (FreezePosition[1])
            VelocityY = 0f;
        if (FreezePosition[2])
            VelocityZ = 0f;
    }

    protected virtual void UpdatePosition()
    {
        if (bc.moveDir == Vector3.zero)
            return;

        target.Move(bc.moveDir * bc.nextFrameMSpeed * Time.deltaTime);
        bc.nextFrameMSpeed = 0f;
    }

    protected virtual void UpdatePhysic()
    {
        if (isKinematic)
            return;

        UpdateFreezePosition();
        
        if (velocity == Vector3.zero)
            return;

        // v = ma
        // f = ma
        // f/m = a

        Vector3 accel = (velocity / Mess) * Time.fixedDeltaTime;

        //Vector3 totalF;
        //Vector3 FN;
        //ray = new Ray(transform.position, Vector3.down);
        //if(Physics.Raycast(ray, out rayHit, DetectRay * transform.localScale.y))
        //{
        //    Vector3 downs = -rayHit.normal;
        //    float angle = Vector3.Angle(downs, Mess * Physics.gravity);
        //    FN = Mess * Physics.gravity * Mathf.Cos(angle);
        //}

        float deceleration = Mess * Drag * Time.fixedDeltaTime;

        if (velocity.x != 0)
            VelocityX -= (VelocityX > 0) ? deceleration : -deceleration;
        if (velocity.z != 0)
            VelocityZ -= (VelocityZ > 0) ? deceleration : -deceleration;

        target.Move(accel);
    }

    protected virtual void UpdateGravity()
    {
        if (!useGravity || isKinematic)
            return;

        if (!target.isGrounded)
        {
            // float d = -Drag > Physics.gravity.y ? Drag : -Drag;
            float d = Drag < -Physics.gravity.y ? Drag : -Drag;
           
            if(Physics.gravity.y < d) // d 양수
                velocity.y += (Physics.gravity.y / d) * Time.fixedDeltaTime;
            else if (Physics.gravity.y > d)// d 음수
                velocity.y += (d / Physics.gravity.y) * Time.fixedDeltaTime;
        }
        else
        {
            //velocity.y = resetGroundY;
            velocity.y = 0f;
        }
    }

    protected virtual void UpdateFreezeRotation()
    {
        if (FreezeRotation[0])
            AnglerVX = 0f;
        if (FreezeRotation[1])
            AnglerVY = 0f;
        if (FreezeRotation[2])
            AnglerVZ = 0f;
    }

    protected virtual void UpdatePhysicRotaion()
    {
        if (isKinematic)
            return;

        UpdateFreezeRotation();
        
        if (anglerVelocity == Vector3.zero)
            return;

        float deceleration = Mess * AngularDrag * Time.fixedDeltaTime;

        if (anglerVelocity.x != 0)
            AnglerVX -= (AnglerVX > 0) ? deceleration : -deceleration;
        if (anglerVelocity.y != 0)
            AnglerVY -= (AnglerVY > 0) ? deceleration : -deceleration;
        if (anglerVelocity.z != 0)
            AnglerVZ -= (AnglerVZ > 0) ? deceleration : -deceleration;
        
        target.transform.Rotate(anglerVelocity * Time.fixedDeltaTime);
    }

    public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
    {
        if (force.y > 0)
            force.y /= Mess / Drag;
        else if (force.y < 0)
            force.y *= Mess / Drag;

        velocity += force;
        forceModeV = forceMode;
    }

    public void AddRelativeForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
    {
        if (force.y > 0)
            force.y /= Mess / Drag;
        else if (force.y < 0)
            force.y *= Mess / Drag;

        velocity += transform.TransformVector(force);
        forceModeV = forceMode;
    }

    public void JumpTo(float jumpForce, bool reset = true)
    {
        if (jumpForce > 0)
            jumpForce /= Mess / Drag;
        else if (jumpForce < 0)
            jumpForce *= Mess / Drag;

        if (reset)
            velocity.y = jumpForce;
        else
            velocity.y += jumpForce;
    }

    public void AddTorque(Vector3 force, ForceMode forceMode = ForceMode.Force)
    {
        anglerVelocity += force;
        forceModeR = forceMode;
    }

    public void AddRelativeTorque(Vector3 force, ForceMode forceMode = ForceMode.Force)
    {
        anglerVelocity += transform.TransformVector(force);
        forceModeR = forceMode;
    }
}
