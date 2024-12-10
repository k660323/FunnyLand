using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardElement
{
    public GameObject go;
    public Text playerInfo;
    public int rewardScore;
}

public class UI_RoundResult : UI_Photon
{
    PhotonViewEx PV;
    object _lock = new object();
    bool isTeamMode;

    int redReward = 0;
    int blueReward = 0;

    Dictionary<Player, RewardElement> playerUIDic = new Dictionary<Player, RewardElement>();

    enum GameObjects
    {
        SoloResult,
        TeamResult,
        Solo,
        PlayerScoreRGroup,
        PlayerScoreBGroup
    }

    enum Texts
    {
        TotalRedScore,
        TotalBlueScore
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLeavePlayer(otherPlayer);
    }

    public override void Init()
    {
        base.Init();

        PV = GetComponent<PhotonViewEx>();
        isTeamMode = Managers.Game.isTeamMode;

        Bind<GameObject>(typeof(GameObjects));
        Bind<Text>(typeof(Texts));

        ElementInit();
        
        PV.RPC("SetPlayerInfo", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer, Managers.Game.rewardScore);
    }

    void ElementInit()
    {
        List<Player> pSList = Managers.Game.gameScene.pScoresList;
        int index = 0;

        if (isTeamMode)
        {
            int redIndex = 0;
            int blueIndex = 0;

            foreach (var player in pSList)
            {
                GameObject root = null;
                if (player.CustomProperties["Team"].ToString() == "RedTeam")
                {
                    root = Get<GameObject>((int)GameObjects.PlayerScoreRGroup);
                    index = redIndex++;
                }
                else
                {
                    root = Get<GameObject>((int)GameObjects.PlayerScoreBGroup);
                    index = blueIndex++;
                }

                RewardElement element = new RewardElement();
                element.go = root.transform.GetChild(index).gameObject;
                element.playerInfo = root.transform.GetChild(index).GetChild(0).GetComponent<Text>();
                element.go.SetActive(true);

                playerUIDic.Add(player, element);
            }

            Get<Text>((int)Texts.TotalRedScore).text = $"ÃÑÇÕ : " + Managers.Game.gameScene.totalScoreDic[Define.Team.RedTeam].ToString();
            Get<Text>((int)Texts.TotalBlueScore).text = $"ÃÑÇÕ : " + Managers.Game.gameScene.totalScoreDic[Define.Team.BlueTeam].ToString();
            Get<GameObject>((int)GameObjects.SoloResult).SetActive(false);
        }
        else
        {
            foreach (var player in pSList)
            {
                RewardElement element = new RewardElement();
                element.go = Get<GameObject>((int)GameObjects.Solo).transform.GetChild(index).gameObject;
                element.playerInfo = Get<GameObject>((int)GameObjects.Solo).transform.GetChild(index).GetChild(0).GetComponent<Text>();
                element.go.SetActive(true);

                playerUIDic.Add(player, element);
                index++;
            }
            Get<GameObject>((int)GameObjects.TeamResult).SetActive(false);
        }
    }

    [PunRPC]
    void SetPlayerInfo(Player player, int rewardScore)
    {
        lock (_lock)
        {
            string color = PhotonNetwork.LocalPlayer == player ? "lime" : "white";
            int score = int.Parse(player.CustomProperties["Score"].ToString());
            playerUIDic[player].playerInfo.text = $"<color={color}>{player.NickName}</color> {score}(+{rewardScore})";
            playerUIDic[player].rewardScore = rewardScore;

            if (isTeamMode)
            {
                if (player.CustomProperties["Team"].ToString() == "RedTeam")
                {
                    redReward += rewardScore;
                    Get<Text>((int)Texts.TotalRedScore).text += $"(+{redReward})";
                }
                else if (player.CustomProperties["Team"].ToString() == "BlueTeam")
                {
                    blueReward += rewardScore;
                    Get<Text>((int)Texts.TotalBlueScore).text = $"(+{blueReward})";
                }
            }
        }
    }

    void UpdateLeavePlayer(Player targetPlayer)
    {
        lock(_lock)
        {
            playerUIDic[targetPlayer].playerInfo.text = $"<color=grey>{targetPlayer.NickName}</color> (³ª°¨)";
            playerUIDic[targetPlayer].go.transform.SetAsLastSibling();
            if (isTeamMode)
            {
                int score = (int)targetPlayer.CustomProperties["Score"];
                if (targetPlayer.CustomProperties["Team"].ToString() == "RedTeam")
                {
                    redReward -= playerUIDic[targetPlayer].rewardScore;
                    Get<Text>((int)Texts.TotalRedScore).text += $"(+{redReward})";
                }
                else if (targetPlayer.CustomProperties["Team"].ToString() == "BlueTeam")
                {
                    blueReward -= playerUIDic[targetPlayer].rewardScore;
                    Get<Text>((int)Texts.TotalBlueScore).text += $"(+{blueReward})";
                }
            }
        }
    }

    private void OnDestroy()
    {
        Managers.Game.rewardScore = 0;
    }
}
