using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class docomoTextToTalk_GUI : MonoBehaviour
{
    [SerializeField]
    private docomoTextToTalk_Create d;

    [SerializeField]
    private InputField wordField;

    [SerializeField]
    private Toggle speakerID;
    [SerializeField]
    private Toggle styleID;

    [SerializeField]
    private Slider voiceTypeSlider;

    [SerializeField]
    private Text voiceTypeText;


    [SerializeField]
    private Slider speechRateSlider;

    [SerializeField]
    private Text speechRateText;


    public void setWord()
    {
        if (d == null) return;
        if (wordField == null) return;
        d.setText(wordField.text);
    }

    public void setSpeakerID()
    {
        if (d == null) return;
        if (speakerID == null) return;
        if (speakerID.isOn)
        {
            d.setSpeakerID("14");
        }
        else
        {
            d.setSpeakerID("1");
        }
    }

    public void setStyleID()
    {
        if (d == null) return;
        if (styleID == null) return;
        if (styleID.isOn)
        {
            d.setStyleID("14");
        }
        else
        {
            d.setStyleID("1");
        }
    }

    public void setVoiceType()
    {
        if (d == null) return;
        if (voiceTypeSlider == null) return;
        if (voiceTypeText == null) return;
        voiceTypeText.text = voiceTypeSlider.value.ToString("0.00");
        d.setVoiceType(voiceTypeSlider.value);
    }

    public void setSpeechRate()
    {
        if (d == null) return;
        if (speechRateSlider == null) return;
        if (speechRateText == null) return;
        speechRateText.text = speechRateSlider.value.ToString("0.00");
        d.setSpeechRate(speechRateSlider.value);
    }

    public void speech()
    {
        if (d == null) return;
        d.Play();
    }

    private void Start()
    {
        if (wordField != null)
        {
            wordField.text = "おめでとう";
            setWord();
        }
    }

}