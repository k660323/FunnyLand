using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Sound : UI_Base
{
    UI_Preferences parent;

    enum Sliders
    {
        MasterVolSlider,
        BgVolSlider,
        EffectVolSlider
    }

    enum InputFields
    {
        MasterVolInputField,
        BgVolInputField,
        EffectVolInputField
    }

    enum Buttons
    {
        CloseButton
    }

    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        Get<Slider>((int)Sliders.MasterVolSlider).minValue = Managers.Setting.sOption.volMin;
        Get<Slider>((int)Sliders.MasterVolSlider).maxValue = Managers.Setting.sOption.volMax;
        Get<Slider>((int)Sliders.BgVolSlider).minValue = Managers.Setting.sOption.volMin;
        Get<Slider>((int)Sliders.BgVolSlider).maxValue = Managers.Setting.sOption.volMax;
        Get<Slider>((int)Sliders.EffectVolSlider).minValue = Managers.Setting.sOption.volMin;
        Get<Slider>((int)Sliders.EffectVolSlider).maxValue = Managers.Setting.sOption.volMax;

        Get<Slider>((int)Sliders.MasterVolSlider).value = Managers.Setting.sOption.masterVol;
        Get<Slider>((int)Sliders.BgVolSlider).value = Managers.Setting.sOption.backgroundVol;
        Get<Slider>((int)Sliders.EffectVolSlider).value = Managers.Setting.sOption.effectVol;

        Get<InputField>((int)InputFields.MasterVolInputField).text = (Managers.Setting.sOption.masterVol).ToString();
        Get<InputField>((int)InputFields.BgVolInputField).text = (Managers.Setting.sOption.backgroundVol).ToString();
        Get<InputField>((int)InputFields.EffectVolInputField).text = (Managers.Setting.sOption.effectVol).ToString();

        Get<Slider>((int)Sliders.MasterVolSlider).onValueChanged.AddListener(value =>
        {
            int _value = (int)value;
            Get<InputField>((int)InputFields.MasterVolInputField).text = _value.ToString();
            Managers.Setting.sOption.ApplyOption(_value, Managers.Setting.sOption.backgroundVol, Managers.Setting.sOption.effectVol);
        });

        Get<Slider>((int)Sliders.BgVolSlider).onValueChanged.AddListener(value =>
        {
            int _value = (int)value;
            Get<InputField>((int)InputFields.BgVolInputField).text = _value.ToString();
            Managers.Setting.sOption.ApplyOption(Managers.Setting.sOption.masterVol, _value, Managers.Setting.sOption.effectVol);
        });

        Get<Slider>((int)Sliders.EffectVolSlider).onValueChanged.AddListener(value =>
        {
            int _value = (int)value;
            Get<InputField>((int)InputFields.EffectVolInputField).text = _value.ToString();
            Managers.Setting.sOption.ApplyOption(Managers.Setting.sOption.masterVol, Managers.Setting.sOption.backgroundVol, _value);
        });

        Get<InputField>((int)InputFields.MasterVolInputField).onEndEdit.AddListener(value => 
        {
            int _value = Mathf.Clamp(int.Parse(value), 0, 100);
            Get<InputField>((int)InputFields.MasterVolInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.MasterVolSlider).value = _value;
            Managers.Setting.sOption.ApplyOption(_value, Managers.Setting.sOption.backgroundVol, Managers.Setting.sOption.effectVol);
        });

        Get<InputField>((int)InputFields.BgVolInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), 0, 100);
            Get<InputField>((int)InputFields.BgVolInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.BgVolSlider).value = _value;
            Managers.Setting.sOption.ApplyOption(Managers.Setting.sOption.masterVol, _value, Managers.Setting.sOption.effectVol);
        });

        Get<InputField>((int)InputFields.EffectVolInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), 0, 100);
            Get<InputField>((int)InputFields.EffectVolInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.EffectVolSlider).value = _value;
            Managers.Setting.sOption.ApplyOption(Managers.Setting.sOption.masterVol, Managers.Setting.sOption.backgroundVol, _value);
        });

        parent = transform.parent.GetComponent<UI_Preferences>();
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click"); Managers.Setting.sOption.ApplyOption(); Managers.UI.ClosePopupUI(parent); });
    }
}
