using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoadingScene : UI_Scene
{
    public enum Texts
    {
        IsTeamText,
        LoadingCompletePlayerText,
        TipText,
        LoadingPercentText,
        LoadingText
    }

    public override void Init()
    {
        base.Init();
        Bind<Text>(typeof(Texts));

        bool isTeam = (bool)PhotonNetwork.CurrentRoom.CustomProperties["Team"];
        Get<Text>((int)Texts.IsTeamText).text = isTeam ? "팀전" : "개인전";
        Get<Text>((int)Texts.LoadingCompletePlayerText).text = $"로딩 완료된 플레이어 0/{PhotonNetwork.CurrentRoom.PlayerCount}";

        StartCoroutine(CorLoadingText());
    }

    IEnumerator CorLoadingText()
    {
        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.5f);
                Get<Text>((int)Texts.LoadingText).text += ".";
            }
            yield return new WaitForSeconds(0.5f);
            Get<Text>((int)Texts.LoadingText).text = "Now Loading";
        }
    }
}
