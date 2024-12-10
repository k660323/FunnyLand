using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Reward/Kill", fileName = "KillCountReward_")]
public class KillCountReward : Reward
{
    [SerializeField]
    [Min(1)]
    int mutiple;
    public override void Give(Quest quest)
    {
        int killScore = Mathf.Clamp(quest.CurrentTaskGroup.Tasks[0].CurrentSuccess, 0, Quantity);
        Managers.Game.rewardScore += (killScore * mutiple);
    }
}
