using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentController : MonoBehaviour
{
    [SerializeField]
    bool isRandomWeather;
    [Min(0)]
    [SerializeField]
    int targetWeatherNum;

    [SerializeField]
    List<Enviroment> enviroments = new List<Enviroment>();


    PhotonViewEx PV;

    private void Start()
    {
        PV = GetComponent<PhotonViewEx>();

        if (PhotonNetwork.IsMasterClient)
        {
            if (isRandomWeather)
                targetWeatherNum = Random.Range(0, enviroments.Count);
            PV.RPC("ApplyWeather", RpcTarget.AllBufferedViaServer, targetWeatherNum);
        }
    }

    [PunRPC]
    public void ApplyWeather(int index)
    {
        if (index >= enviroments.Count)
            return;

        targetWeatherNum = index;
        Enviroment enviroment = enviroments[index];

        RenderSettings.skybox = enviroment.skyBoxMaterial;
        enviroment.SunSource.transform.rotation = Quaternion.Euler(enviroment.LightRotation);
        enviroment.SunSource.intensity = enviroment.LightIntensity;
        enviroment.SunSource.color = enviroment.LightColor;
        RenderSettings.sun = enviroment.SunSource;
        RenderSettings.subtractiveShadowColor = enviroment.ShadowColor;

        RenderSettings.reflectionIntensity = enviroment.reflectionIntensity;
        RenderSettings.reflectionBounces = enviroment.reflectionBounces;

        RenderSettings.fog = enviroment.isFog;
        if (enviroment.isFog)
        {
            RenderSettings.fogColor = enviroment.fogColor;
            RenderSettings.fogMode = enviroment.fogMode;
            RenderSettings.fogDensity = enviroment.fogDensity;
        }

        RenderSettings.haloStrength = enviroment.haloStrength;
        RenderSettings.flareFadeSpeed = enviroment.flareFadeSpeed;
        RenderSettings.flareStrength = enviroment.flareStrength;
    }
}
