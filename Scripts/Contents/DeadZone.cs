using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [SerializeField]
    string killMessage;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (other.gameObject == Managers.Game.myPlayer)
            {
                if (other.gameObject.TryGetComponent(out Stat stat))
                {
                    stat.OnAttacked(stat.MaxHp, killMessage);
                    StartCoroutine(Managers.Game.ChangeObserveMode());
                }
            }
        }
    }
}
