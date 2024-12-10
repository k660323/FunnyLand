using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enviroment : MonoBehaviour
{
    public Material skyBoxMaterial;
    public Light SunSource;
    public Vector3 LightRotation = new Vector3(50f, -180f, 0);
    public float LightIntensity = 1f;
    public Color LightColor = new Color(93f, 104f, 250f, 255f);
    public Color ShadowColor = new Color(107f, 122f, 160f, 255f);

    [Header("Enviroment Reflections")]
    public float reflectionIntensity = 1f;
    public int reflectionBounces = 1;

    [Header("Other Setting")]
    public bool isFog;
    public Color fogColor = new Color(128f, 128f, 128f, 255f);
    public FogMode fogMode = FogMode.ExponentialSquared;
    public float fogDensity = 0.01f;

    [Range(0f, 1f)]
    public float haloStrength = 1f;
    public float flareFadeSpeed = 3f;
    [Range(0f,1f)]
    public float flareStrength = 3f;
}
