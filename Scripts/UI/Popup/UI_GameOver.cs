using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : UI_Popup
{
    enum GameObjects
    {
        WinPanel,
        DrawPanel,
        LosePanel
    }

    enum Buttons
    {
        OutButton
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));

        GameResult();
        Get<Button>((int)Buttons.OutButton).gameObject.BindEvent((evt) => { PhotonNetwork.LeaveRoom(); });
    }

    void GameResult()
    {
        int maxScore = int.Parse(Managers.Game.gameScene.pScoresList[0].CustomProperties["Score"].ToString());
        int myScore = int.Parse(PhotonNetwork.LocalPlayer.CustomProperties["Score"].ToString());

        if(maxScore == myScore)
            Managers.Game.gameScene.mvpLocalPlayer = true;

        if (Managers.Game.isTeamMode)
        {
            string result = VictoryTeam();
            string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString();

            if ("Draw" == result)
            {
                Managers.Sound.Play2D("SFX/Win");
                Reward(Random.Range(5, 10), false, true, false);
                ShowPanel(false, true, false);
            }
            else if (myTeam == result)
            {
                Managers.Sound.Play2D("SFX/Win");
                Reward(Random.Range(50, 100), false, true, false);
                ShowPanel(true, false, false);
            }
            else
            {
                Managers.Sound.Play2D("SFX/Lose");
                Reward(Random.Range(5, 10), false, true, false);
                ShowPanel(false, false, true);
            }
        }
        else
        {
            if (myScore == maxScore)
            {
                Managers.Sound.Play2D("SFX/Win");
                Reward(Random.Range(50, 100), true, false, false);
                ShowPanel(true, false, false);
            }
            else
            {
                Managers.Sound.Play2D("SFX/Lose");
                Reward(Random.Range(5, 10), false, false, true);
                ShowPanel(false, false, true);
            }
        }
    }

    string VictoryTeam()
    {
        if (Managers.Game.gameScene.totalScoreDic[Define.Team.RedTeam] > Managers.Game.gameScene.totalScoreDic[Define.Team.BlueTeam])
        {
            return "RedTeam";
        }
        else if (Managers.Game.gameScene.totalScoreDic[Define.Team.RedTeam] < Managers.Game.gameScene.totalScoreDic[Define.Team.BlueTeam])
        {
            return "BlueTeam";
        }
        else
        {
            return "Draw";
        }
    }

    void Reward(int coin, bool win, bool draw, bool lose)
    {
        int curRound = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurRound"];
        int Round = (int)PhotonNetwork.CurrentRoom.CustomProperties["Round"];

        float roundProcess = (float)curRound / Round;
         coin = (int)(coin * (Managers.Game.gameScene.mvpLocalPlayer ? 2f : 1f) * roundProcess);

        Managers.Data.PlayerInfoData.coin += coin;

        if (win)
            Managers.Data.PlayerInfoData.win++;
        else if (draw)
            Managers.Data.PlayerInfoData.draw++;
        else if(lose)
            Managers.Data.PlayerInfoData.lose++;

        Managers.Data.SaveBackEndData(Managers.Data.PlayerInfoData, "PlayerInfo", "playerInfoToJson");
    }

    void ShowPanel(bool win, bool draw, bool lose)
    {
        Get<GameObject>((int)GameObjects.WinPanel).SetActive(win);
        Get<GameObject>((int)GameObjects.DrawPanel).SetActive(draw);
        Get<GameObject>((int)GameObjects.LosePanel).SetActive(lose);
    }

    public override void OnLeftRoom()
    {
        Managers.Scene.LeaveRoom();
    }
}
