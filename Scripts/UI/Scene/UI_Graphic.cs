using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Graphic : UI_Base
{
    List<Resolution> resolutions = new List<Resolution>();

    UI_Preferences parent;

    enum Dropdowns
    {
        ResolutionDropdown,
        GraphicQualityDropdown
    }

    enum Toggles
    {
        FullScreenToggle,
        SyncToggle,
        FrameToggle
    }

    enum Sliders
    {
        TargetFrameSlider
    }

    enum InputFields
    {
        TargetFrameInputField
    }

    enum Buttons
    {
        ApplyButton,
        CloseButton
    }

    public override void Init()
    {
        Bind<Dropdown>(typeof(Dropdowns));
        Bind<Toggle>(typeof(Toggles));
        Bind<Slider>(typeof(Sliders));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        // �ػ󵵸� �־������..
        ResoultionInit();

        // ��ü ȭ��
        Get<Toggle>((int)Toggles.FullScreenToggle).isOn = Managers.Setting.gOption.isFullScreen;

        // ��ǥ ������
        Get<Slider>((int)Sliders.TargetFrameSlider).minValue = Managers.Setting.gOption.minRefreshRate;
        Get<Slider>((int)Sliders.TargetFrameSlider).maxValue = Managers.Setting.gOption.maxRefreshRate;
        Get<Slider>((int)Sliders.TargetFrameSlider).value = Managers.Setting.gOption.targetFrame;
        Get<Slider>((int)Sliders.TargetFrameSlider).onValueChanged.AddListener(value => { FrameSlideToInputField((int)value); });

        Get<InputField>((int)InputFields.TargetFrameInputField).text = Managers.Setting.gOption.targetFrame.ToString();
        Get<InputField>((int)InputFields.TargetFrameInputField).onEndEdit.AddListener(value => { InputFieldToFrameSlide(value); });
        
        // ���� ����ȭ
        Get<Toggle>((int)Toggles.SyncToggle).isOn = Managers.Setting.gOption.isSync;

        // ������ ī����
        Get<Toggle>((int)Toggles.FrameToggle).isOn = Managers.Setting.gOption.isFrame;

        // �׷��� ǰ��
        Get<Dropdown>((int)Dropdowns.GraphicQualityDropdown).value = Managers.Setting.gOption.graphicQualityIndex;

        // ����
        Get<Button>((int)Buttons.ApplyButton).gameObject.BindEvent((data) => SetOption());
        // ������
        parent = transform.parent.GetComponent<UI_Preferences>();
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click"); Managers.UI.ClosePopupUI(parent); });
    }

    void ResoultionInit()
    {
        var dropDown = Get<Dropdown>((int)Dropdowns.ResolutionDropdown);
        dropDown.options.Clear();

        resolutions.AddRange(Screen.resolutions);
        
        Resolution data = new Resolution();
        data.width = Managers.Setting.gOption.width;
        data.height = Managers.Setting.gOption.height;
        data.refreshRate = Managers.Setting.gOption.refreshRate;

        int screenIndex = 0;
        int value = 0;

        foreach (Resolution item in resolutions)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = item.width + " x " + item.height + " @ " + item.refreshRate + "hz";
            dropDown.options.Add(option);

            if (item.refreshRate == data.refreshRate && item.height == data.height && item.refreshRate == data.refreshRate)
            {
                screenIndex = value;
            }
            value++;
        }
        dropDown.RefreshShownValue();

        dropDown.value = screenIndex;
    }

    void FrameSlideToInputField(int value)
    {
        Get<InputField>((int)InputFields.TargetFrameInputField).text = value.ToString();
    }

    void InputFieldToFrameSlide(string value)
    {
        Slider frame = Get<Slider>((int)Sliders.TargetFrameSlider);
        if (int.TryParse(value, out int num))
        {
            frame.value = num;
        }
        else
        {
            Get<InputField>((int)InputFields.TargetFrameInputField).text = frame.value.ToString();
        }
    }

    void SetOption()
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        Resolution resolution = resolutions[Get<Dropdown>((int)Dropdowns.ResolutionDropdown).value];
        bool isFull = Get<Toggle>((int)Toggles.FullScreenToggle).isOn;
        bool isVsync = Get<Toggle>((int)Toggles.SyncToggle).isOn;
        bool isFrame = Get<Toggle>((int)Toggles.FrameToggle).isOn;
        int targetFrame = (int)Get<Slider>((int)Sliders.TargetFrameSlider).value;
        int qualityindex = Get<Dropdown>((int)Dropdowns.GraphicQualityDropdown).value;
        Managers.Setting.gOption.ApplyOption(resolution.width, resolution.height, isFull, resolution.refreshRate, isVsync, isFrame, targetFrame, qualityindex);
    }
}
