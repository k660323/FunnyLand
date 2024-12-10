using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring2D : MonoBehaviour
{
    enum Dir
    {
        UP,
        RIGHT,
        DOWN,
        LEFT,
    }
    [SerializeField]
    Dir dir;

    [SerializeField]
    float pushPower;

    Vector2 DirToVector(Dir dir)
    {
        switch(dir)
        {
            case Dir.UP:
                return Vector2.up;
            case Dir.RIGHT:
                return Vector2.right;
            case Dir.DOWN:
                return Vector2.down;
            case Dir.LEFT:
                return Vector2.left;
            default:
                return Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Controller2D c2D))
        {
            if(c2D.PV.IsMine)
            {
                switch(dir)
                {
                    case Dir.UP:
                        if (c2D.rb.velocity.y < 0)
                        {
                            c2D.rb.velocity = Vector2.zero;
                            c2D.SetPhysics((int)Define.PhysicsType.Position, DirToVector(dir), pushPower, 1f);
                        }
                        break;
                    case Dir.DOWN:
                    case Dir.RIGHT:
                    case Dir.LEFT:
                        c2D.rb.velocity = Vector2.zero;
                        c2D.SetPhysics((int)Define.PhysicsType.Position, DirToVector(dir), pushPower, 1f);
                        break;
                }
            }
        }
    }
}
