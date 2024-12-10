using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_LedgeBlockerScene : UI_ContentsScene
{
    LedgeBlockerScene LBS;
    GaugeUI gUI;

    public enum CanvasGroups
    {
        SelectGroup
    }

    public enum SelectUIs
    {
        Select0,
        Select1,
        Select2
    }

    public enum Texts
    {
        Score,
        Combo
    }

    public override void Init()
    {
        base.Init();

        LBS = GameObject.FindGameObjectWithTag("ContentsScene").GetComponent<LedgeBlockerScene>();
        gUI = FindObjectOfType<GaugeUI>();
        Bind<Text>(typeof(Texts));
        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<SelectUI>(typeof(SelectUIs));
    }

    private void LateUpdate()
    {
        LBPlayerInfo lbpc = LBS.LBPList[(int)Define.PlayerList.ForcePlayer];
        if (lbpc)
        {
            Get<Text>((int)Texts.Score).text = LBS.LBPList[(int)Define.PlayerList.ForcePlayer].score.ToString();
            Get<Text>((int)Texts.Combo).text = LBS.LBPList[(int)Define.PlayerList.ForcePlayer].combo.ToString();

            float rate = LBS.LBPList[(int)Define.PlayerList.ForcePlayer].lifeGauge / LBS.LBPList[(int)Define.PlayerList.ForcePlayer].maxLifeGauge;
            gUI.GaugeUpdate(rate);
        }
    }

    public void SetBtnAction(bool flag)
    {
        Get<CanvasGroup>((int)CanvasGroups.SelectGroup).interactable = flag;
        Get<SelectUI>((int)SelectUIs.Select0).enabled = flag;
        Get<SelectUI>((int)SelectUIs.Select1).enabled = flag;
        Get<SelectUI>((int)SelectUIs.Select2).enabled = flag;
    }
}
