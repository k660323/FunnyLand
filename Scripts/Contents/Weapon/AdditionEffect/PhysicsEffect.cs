using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AddtionPhysicsEffect/Effect")]
public class PhysicsEffect : ScriptableObject
{
    public Define.PhysicsType type;
    public Define.PhysicsDir dir;
    public float power;
    public float duraiton;
}
