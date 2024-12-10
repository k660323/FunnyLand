using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SurvivalList : UI_Base
{
    public struct SurvivalElement
    {
        public GameObject go;
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
    Dictionary<Player, SurvivalElement> playerUIDic = new Dictionary<Player, SurvivalElement>();

    enum State
    {
        Update,
        Leave
    }

    enum Texts
    {
        AliveText
    }

    enum GameObjects
    {
        PlayerList
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(changedProps.ContainsKey("Die"))
        {
            UpdateSurvivalList(targetPlayer, State.Update);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateSurvivalList(otherPlayer, State.Leave);
    }

    public override void Init()
    {
        IsActive = false;

        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        alivePlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        Get<Text>((int)Texts.AliveText).text = $"생존 인원 {alivePlayer} / {PhotonNetwork.CurrentRoom.PlayerCount}";

        int i = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            SurvivalElement element = new SurvivalElement();
            element.go = Get<GameObject>((int)GameObjects.PlayerList).transform.GetChild(i).gameObject;
            element.text = Get<GameObject>((int)GameObjects.PlayerList).transform.GetChild(i).GetChild(0).GetComponent<Text>();
            element.go.SetActive(true);

            playerUIDic.Add(player, element);
            i++;
        }

        foreach (var player in playerUIDic)
        {
            string color = player.Key == PhotonNetwork.LocalPlayer ? "lime" : "white";
            string alive = (bool)player.Key.CustomProperties["Die"] == true ? "사망" : "생존";
            player.Value.text.text = $"<color={color}>{player.Key.NickName}</color> : {alive}";
        }

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

    void UpdateSurvivalList(Player player,State state)
    {
        switch (state)
        {
            case State.Update:
                string color = player == PhotonNetwork.LocalPlayer ? "lime" : "white";
                string alive = null;

                if ((bool)player.CustomProperties["Die"])
                {
                    Get<Text>((int)Texts.AliveText).text = $"생존 인원 {--alivePlayer} / {PhotonNetwork.CurrentRoom.PlayerCount}";
                    alive = "사망";
                }
                else
                {
                    Get<Text>((int)Texts.AliveText).text = $"생존 인원 {++alivePlayer} / {PhotonNetwork.CurrentRoom.PlayerCount}";
                    alive = "생존";
                }

                playerUIDic[player].text.text = $"<color={color}>{player.NickName}</color> : {alive}";
                break;
            case State.Leave:
                if (!(bool)player.CustomProperties["Die"])
                    Get<Text>((int)Texts.AliveText).text = $"생존 인원 {--alivePlayer} / {PhotonNetwork.CurrentRoom.PlayerCount}";
                playerUIDic[player].text.text = $"<color=gray>{player.NickName}</color> : 나감";
                break;
        }
    }
}
