using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamePlay : UI_Base
{
    UI_Preferences parent;

    enum Sliders
    {
        MouseVerticalSlider,
        MouseHorizontalSlider,
        WheelSlider
    }

    enum InputFields
    {
        MouseVerticalInputField,
        MouseHorizontalInputField,
        WheelInputField
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

        Get<Slider>((int)Sliders.MouseHorizontalSlider).minValue = Managers.Setting.gamePlayOption.mouseHVMin;
        Get<Slider>((int)Sliders.MouseHorizontalSlider).maxValue = Managers.Setting.gamePlayOption.mouseHVMax;
        Get<Slider>((int)Sliders.MouseVerticalSlider).minValue = Managers.Setting.gamePlayOption.mouseHVMin;
        Get<Slider>((int)Sliders.MouseVerticalSlider).maxValue = Managers.Setting.gamePlayOption.mouseHVMax;
        Get<Slider>((int)Sliders.WheelSlider).minValue = Managers.Setting.gamePlayOption.wheelMin;
        Get<Slider>((int)Sliders.WheelSlider).maxValue = Managers.Setting.gamePlayOption.wheelMax;

        Get<Slider>((int)Sliders.MouseHorizontalSlider).value = Managers.Setting.gamePlayOption.mouseHorizontal;
        Get<Slider>((int)Sliders.MouseVerticalSlider).value = Managers.Setting.gamePlayOption.mouseVirtical;
        Get<Slider>((int)Sliders.WheelSlider).value = Managers.Setting.gamePlayOption.wheel;

        Get<InputField>((int)InputFields.MouseHorizontalInputField).text = (Managers.Setting.gamePlayOption.mouseHorizontal).ToString();
        Get<InputField>((int)InputFields.MouseVerticalInputField).text = (Managers.Setting.gamePlayOption.mouseVirtical).ToString();
        Get<InputField>((int)InputFields.WheelInputField).text = (Managers.Setting.gamePlayOption.wheel).ToString();

        Get<Slider>((int)Sliders.MouseHorizontalSlider).onValueChanged.AddListener(value =>
        {
            int _value = Mathf.Clamp((int)value, Managers.Setting.gamePlayOption.mouseHVMin, Managers.Setting.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseHorizontalInputField).text = _value.ToString();
            Managers.Setting.gamePlayOption.ApplyOption(_value, Managers.Setting.gamePlayOption.mouseVirtical, Managers.Setting.gamePlayOption.wheel);
        });

        Get<Slider>((int)Sliders.MouseVerticalSlider).onValueChanged.AddListener(value =>
        {
            int _value = Mathf.Clamp((int)value, Managers.Setting.gamePlayOption.mouseHVMin, Managers.Setting.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseVerticalInputField).text = _value.ToString();
            Managers.Setting.gamePlayOption.ApplyOption(Managers.Setting.gamePlayOption.mouseHorizontal, _value, Managers.Setting.gamePlayOption.wheel);
        });

        Get<Slider>((int)Sliders.WheelSlider).onValueChanged.AddListener(value =>
        {
            int _value = Mathf.Clamp((int)value, Managers.Setting.gamePlayOption.wheelMin, Managers.Setting.gamePlayOption.wheelMax);
            Get<InputField>((int)InputFields.WheelInputField).text = _value.ToString();
            Managers.Setting.gamePlayOption.ApplyOption(Managers.Setting.gamePlayOption.mouseHorizontal, Managers.Setting.gamePlayOption.mouseVirtical, _value);
        });

        Get<InputField>((int)InputFields.MouseHorizontalInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), Managers.Setting.gamePlayOption.mouseHVMin, Managers.Setting.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseHorizontalInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.MouseHorizontalSlider).value = _value;
            Managers.Setting.gamePlayOption.ApplyOption(_value, Managers.Setting.gamePlayOption.mouseVirtical, Managers.Setting.gamePlayOption.wheel);
        });

        Get<InputField>((int)InputFields.MouseVerticalInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), Managers.Setting.gamePlayOption.mouseHVMin, Managers.Setting.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseVerticalInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.MouseVerticalSlider).value = _value;
            Managers.Setting.gamePlayOption.ApplyOption(Managers.Setting.gamePlayOption.mouseHorizontal, _value, Managers.Setting.gamePlayOption.wheel);
        });

        Get<InputField>((int)InputFields.WheelInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), Managers.Setting.gamePlayOption.wheelMin, Managers.Setting.gamePlayOption.wheelMax);
            Get<InputField>((int)InputFields.WheelInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.WheelSlider).value = _value;
            Managers.Setting.gamePlayOption.ApplyOption(Managers.Setting.gamePlayOption.mouseHorizontal, Managers.Setting.gamePlayOption.mouseVirtical, _value);
        });

        parent = transform.parent.GetComponent<UI_Preferences>();
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => { 
            Managers.Sound.Play2D("SFX/UI_Click"); 
            Managers.Setting.gamePlayOption.ApplyOption(); 
            Managers.UI.ClosePopupUI(parent); 
        });
    }
}
