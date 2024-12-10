using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_KillBoard : UI_Base
{
    public struct KillElement
    {
        public GameObject go;
        public Image image;
        public Text text;
    }

    bool isActive;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
        }
    }

    int alivePlayer;
    Dictionary<Player, KillElement> playerUIDic = new Dictionary<Player, KillElement>();

    public enum State
    {
        Update,
        Leave
    }

    enum Texts
    {
        PlayerCountText
    }

    enum GameObjects
    {
        PlayerList
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateKillList(otherPlayer, State.Leave);
    }

    public override void Init()
    {
        IsActive = false;

        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        alivePlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        Get<Text>((int)Texts.PlayerCountText).text = $"ÃÑ ÀÎ¿ø {alivePlayer} / {PhotonNetwork.CurrentRoom.PlayerCount}";

        int i = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            string color = (player == PhotonNetwork.LocalPlayer ? "lime" : "white");
            string kill = ((int)player.CustomProperties["Kill"]).ToString();
            KillElement element = new KillElement();
            if (Managers.Game.isTeamMode)
            {
                string team = player.CustomProperties["Team"].ToString();
                int teamIndex = team == "RedTeam" ? 0 : 5;
                element.go = Get<GameObject>((int)GameObjects.PlayerList).transform.GetChild(i + teamIndex).gameObject;
                element.image = element.go.GetComponent<Image>();
                element.image.color = (teamIndex < 5 ? Color.red : Color.blue);
            }
            else
            {
                element.go = Get<GameObject>((int)GameObjects.PlayerList).transform.GetChild(i).gameObject;
            }
           
            element.text = element.go.transform.GetChild(0).GetComponent<Text>();
            element.text.text = $"<color={color}>{player.NickName}</color> : 0Kill";

            element.go.SetActive(true);
            playerUIDic.Add(player, element);
          
            i++;
        }

        //foreach (var player in playerUIDic)
        //{
        //    string color = player.Key == PhotonNetwork.LocalPlayer ? "lime" : "white";
        //    string kill = ((int)player.Key.CustomProperties["Kill"]).ToString();
        //    player.Value.text.text = $"<color={color}>{player.Key.NickName}</color> : {kill}";
        //}

        //Managers.Game.ContentsScene.contentUIAction -= KeyInput;
        //Managers.Game.ContentsScene.contentUIAction += KeyInput;
    }

    private void KeyInput()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            IsActive = true;
        }
        else if (Input.GetKeyUp(KeyCode.BackQuote))
        {
            IsActive = false;
        }
    }

    public void UpdateKillList(Player player, State state)
    {
        switch (state)
        {
            case State.Update:
                string color = (player == PhotonNetwork.LocalPlayer ? "lime" : "white");
                string killCount = ((int)player.CustomProperties["Kill"]).ToString();
                playerUIDic[player].text.text = $"<color={color}>{player.NickName}</color> : {killCount}Kill";
                break;
            case State.Leave:
                Get<Text>((int)Texts.PlayerCountText).text = $"ÃÑ ÀÎ¿ø {--alivePlayer} / {PhotonNetwork.CurrentRoom.PlayerCount}";
                playerUIDic[player].text.text = $"<color=gray>{player.NickName}</color> : ³ª°¨";
                break;
        }
    }
}
