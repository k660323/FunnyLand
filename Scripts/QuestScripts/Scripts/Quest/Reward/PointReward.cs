using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Reward/Point", fileName ="PointReward_")]
public class PointReward : Reward
{
    public override void Give(Quest quest)
    {
        Managers.Game.rewardScore += (Managers.Game.isTeamMode ? TeamQuantity : Quantity);
    }
}
