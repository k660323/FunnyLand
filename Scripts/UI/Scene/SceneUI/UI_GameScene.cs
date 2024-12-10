using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    GameScene GS;
    public UI_ChoiceMap UCM { get; private set; }
    public UI_LoadMap ULM { get; private set; }
    public UI_ScoreBoard USB { get; private set; }

    enum CanvasGroups
    {
        FadeImage,
        StartImage,
        PauseImage,
        EndImage
    }

    public enum MessageQueues
    {
        SignQueue
    }

    public enum Texts
    {
        ShortcutKeysText,
        SpectatingText,
        CounterText
    }

    public override void Init()
    {
        base.Init();
        GS = FindObjectOfType<GameScene>();
        UCM = FindObjectOfType<UI_ChoiceMap>();
        UCM.InitV2();
        ULM = FindObjectOfType<UI_LoadMap>();
        ULM.InitV2();
        USB = FindObjectOfType<UI_ScoreBoard>();

        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<Text>(typeof(Texts));
        Bind<MessageQueue>(typeof(MessageQueues));
    }

    public void FadeOut(int index)
    {
        Get<CanvasGroup>(index).alpha = 1f;
        StartCoroutine(CorFadeOut(index));
    }

    IEnumerator CorFadeOut(int index)
    {
        float fadeDurtaion = 3f;
        float timer = 3f;

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            Get<CanvasGroup>(index).alpha = timer / fadeDurtaion;
            yield return null;
        }
    }

    public void FadeIn(int index)
    {
        Get<CanvasGroup>(index).alpha = 0f;
        StartCoroutine(CorFadeIn(index));
    }

    IEnumerator CorFadeIn(int index)
    {
        float fadeDurtaion = 3f;
        float timer = 0;

        while (timer < fadeDurtaion)
        {
            timer += Time.unscaledDeltaTime;
            Get<CanvasGroup>(index).alpha = timer / fadeDurtaion;
            yield return null;
        }
    }

    public void SetActiveUCM(bool isActive)
    {
        if (isActive)
            UCM.UIInit();
        else
            UCM.gameObject.SetActive(isActive);
    }

    public void SetActiveULM(bool isActive)
    {
        if (isActive)
            ULM.Loading();
        else
            ULM.gameObject.SetActive(isActive);
    }
}
