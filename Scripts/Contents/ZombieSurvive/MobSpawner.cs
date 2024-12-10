using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField]
    bool isSpawn = false;
    [SerializeField]
    float spawnDelay = 1f;

    public void SpawnMob(int index, int spawnCount)
    {
        if (isSpawn)
            StartCoroutine(Spawn(index, spawnCount));
    }

    IEnumerator Spawn(int index, int spawnCount)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Managers.Game.ContentsScene.SpawnBot_M(transform.position, Vector3.one, index);
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
