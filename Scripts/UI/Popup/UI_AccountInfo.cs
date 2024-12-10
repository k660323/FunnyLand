using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AccountInfo : UI_Popup
{
    enum Images
    {
        PlayerImage
    }

    enum Texts
    {
        TitleText,
        WinRateText,
        IntroduceText
    }

    enum Buttons
    {
        CloseButton
    }

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            ClosePopupUI(); 
        });
    }

    public void SetAccountInfo(int imageIndex, string nickName, int total, int win, int lose, int draw, int winRate, string introduce)
    {
        if (Managers.Data.ItemDict.TryGetValue(imageIndex, out Data.ItemData value))
            Get<Image>((int)Images.PlayerImage).sprite = Managers.Resource.ItemImageLoad(value.Image);
        Get<Text>((int)Texts.TitleText).text = $"{nickName}´ÔÀÇ Á¤º¸";
        Get<Text>((int)Texts.WinRateText).text = $"{total}Àü\n{win}½Â {lose}ÆÐ {draw}¹«\n½Â·ü : {winRate}%";
        Get<Text>((int)Texts.IntroduceText).text = introduce;
    }
}
