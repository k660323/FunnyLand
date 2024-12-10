using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreElement
{
    public GameObject go;
    public Image iconImage;
    public Text rank;
    public Text nickName;
    public Text score;
}

public class UI_ScoreBoard : UI_Base
{
    bool isActive;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            canvasGroup.alpha = value ? 1 : 0;
            isActive = value;
        }
    }

    bool isTeamMode;

    CanvasGroup canvasGroup;

    Dictionary<Player, ScoreElement> playerUIDic = new Dictionary<Player, ScoreElement>();

    enum Texts
    {
        ModText,
        RoundText,
        TotalRedScoreText,
        TotalBlueScoreText
    }

    enum GameObjects
    {
        SoloBoard,
        TeamBoard,
        PlayerScoreSGroup,
        PlayerScoreRGroup,
        PlayerScoreBGroup
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("CurRound"))
            Get<Text>((int)Texts.RoundText).text = $"¶ó¿îµå {propertiesThatChanged["CurRound"]} / {PhotonNetwork.CurrentRoom.CustomProperties["Round"]}";
    }

    public override void Init()
    {
        canvasGroup = Util.GetOrAddComponent<CanvasGroup>(gameObject);
        IsActive = false;
        isTeamMode = (bool)PhotonNetwork.CurrentRoom.CustomProperties["Team"];

        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        Get<Text>((int)Texts.ModText).text = (bool)PhotonNetwork.CurrentRoom.CustomProperties["Team"] == true ? "ÆÀÀü" : "°³ÀÎÀü";
        Get<Text>((int)Texts.RoundText).text = $"¶ó¿îµå {PhotonNetwork.CurrentRoom.CustomProperties["CurRound"]} / {PhotonNetwork.CurrentRoom.CustomProperties["Round"]}";

        ElementInit();
    }

    void ElementInit()
    {
        int i = 0;
     
        if (isTeamMode)
        {
            int redIndex = 0;
            int blueIndex = 0;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                GameObject root = null;
                int Index;
                if (player.CustomProperties["Team"].ToString() == "RedTeam")
                {
                    root = Get<GameObject>((int)GameObjects.PlayerScoreRGroup);
                    Index = ++redIndex;
                }
                else
                {
                    root = Get<GameObject>((int)GameObjects.PlayerScoreBGroup);
                    Index = ++blueIndex;
                }

                ScoreElement element = new ScoreElement();
                element.go = root.transform.GetChild(i).gameObject;
                element.rank = root.transform.GetChild(i).GetChild(0).GetComponent<Text>();
                element.rank.text = Index.ToString();
                element.iconImage = root.transform.GetChild(i).GetChild(1).GetComponent<Image>();
                element.nickName = root.transform.GetChild(i).GetChild(2).GetComponent<Text>();
                element.score = root.transform.GetChild(i).GetChild(3).GetComponent<Text>();
                element.go.SetActive(true);

                playerUIDic.Add(player, element);
                i++;
            }
            Get<GameObject>((int)GameObjects.SoloBoard).SetActive(false);
        }
        else
        {
            int index = 0;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                ScoreElement element = new ScoreElement();
                element.go = Get<GameObject>((int)GameObjects.PlayerScoreSGroup).transform.GetChild(i).gameObject;
                element.rank = Get<GameObject>((int)GameObjects.PlayerScoreSGroup).transform.GetChild(i).GetChild(0).GetComponent<Text>();
                element.rank.text = (++index).ToString();
                element.iconImage = Get<GameObject>((int)GameObjects.PlayerScoreSGroup).transform.GetChild(i).GetChild(1).GetComponent<Image>();
                element.nickName = Get<GameObject>((int)GameObjects.PlayerScoreSGroup).transform.GetChild(i).GetChild(2).GetComponent<Text>();
                element.score = Get<GameObject>((int)GameObjects.PlayerScoreSGroup).transform.GetChild(i).GetChild(3).GetComponent<Text>();
                element.go.SetActive(true);

                playerUIDic.Add(player, element);
                i++;
            }
            Get<GameObject>((int)GameObjects.TeamBoard).SetActive(false);
        }

        foreach (var playerUI in playerUIDic)
        {
            if (PhotonNetwork.LocalPlayer != playerUI.Key)
            {
                playerUI.Value.nickName.text = $"<color=white>{playerUI.Key.NickName}</color>";
            }
            else
            {
                playerUI.Value.rank.text = $"<color=lime>{playerUI.Value.rank.text}</color>";
                playerUI.Value.nickName.text = $"<color=lime>{playerUI.Key.NickName}</color>";
            }
            playerUI.Value.iconImage.sprite = Managers.Resource.ItemImageLoad(playerUI.Key.CustomProperties["IconImage"].ToString());
            playerUI.Value.score.text = $"<color=white>{playerUI.Key.CustomProperties["Score"]}</color>";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            IsActive = true;
        }
        else if(Input.GetKeyUp(KeyCode.Tab))
        {
            IsActive = false;
        }
    }

    public void UpdateScore(ref List<Player> pList)
    {
        int red = 0;
        int blue = 0;

        for (int i = 0; i < pList.Count(); i++)
        {
            int score = (int)pList[i].CustomProperties["Score"];
            string color = pList[i] == PhotonNetwork.LocalPlayer ? "lime" : "white";
            playerUIDic[pList[i]].rank.text = (i + 1).ToString();
            playerUIDic[pList[i]].score.text = $"<color={color}>{score}</color>";

            if (isTeamMode)
            {
                if (pList[i].CustomProperties["Team"].ToString() == "RedTeam")
                    playerUIDic[pList[i]].go.transform.SetSiblingIndex(red++);
                else if (pList[i].CustomProperties["Team"].ToString() == "BlueTeam")
                    playerUIDic[pList[i]].go.transform.SetSiblingIndex(blue++);
            }
            else
            {
                playerUIDic[pList[i]].go.transform.SetSiblingIndex(i);
            }
        }

        if (isTeamMode)
        {
            Get<Text>((int)Texts.TotalRedScoreText).text = $"ÃÑÇÕ : { Managers.Game.gameScene.totalScoreDic[Define.Team.RedTeam] }";
            Get<Text>((int)Texts.TotalBlueScoreText).text = $"ÃÑÇÕ : {  Managers.Game.gameScene.totalScoreDic[Define.Team.BlueTeam] }";
        }
    }

    public void UpdateLeavePlayer(Player p)
    {
        playerUIDic[p].nickName.text = $"<color=grey>{p.NickName}</color>";
        playerUIDic[p].go.transform.SetAsLastSibling();
    }
}
