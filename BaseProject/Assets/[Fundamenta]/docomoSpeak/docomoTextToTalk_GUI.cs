using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class docomoTextToTalk_GUI : MonoBehaviour
{
    [SerializeField]
    private docomoTextToTalk d;

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
        if (d.httpReq == null) return;
        if (wordField == null) return;
        d.httpReq.setText(wordField.text);
    }

    public void setSpeakerID()
    {
        if (d.httpReq == null) return;
        if (speakerID == null) return;
        if (speakerID.isOn)
        {
            d.httpReq.setSpeakerID("14");
        }
        else
        {
            d.httpReq.setSpeakerID("1");
        }
    }

    public void setStyleID()
    {
        if (d.httpReq == null) return;
        if (styleID == null) return;
        if (styleID.isOn)
        {
            d.httpReq.setStyleID("14");
        }
        else
        {
            d.httpReq.setStyleID("1");
        }
    }

    public void setVoiceType()
    {
        if (d.httpReq == null) return;
        if (voiceTypeSlider == null) return;
        if (voiceTypeText == null) return;
        voiceTypeText.text = voiceTypeSlider.value.ToString("0.00");
        d.httpReq.setVoiceType(voiceTypeSlider.value);
    }

    public void setSpeechRate()
    {
        if (d.httpReq == null) return;
        if (speechRateSlider == null) return;
        if (speechRateText == null) return;
        speechRateText.text = speechRateSlider.value.ToString("0.00");
        d.httpReq.setSpeechRate(speechRateSlider.value);
    }

    public void speech()
    {
        if (d == null) return;
        StartCoroutine(d.PlayCoroutine());
    }

    private void Start()
    {
        if (wordField != null)
        {
            wordField.text = "おめでとう";
            //フォーカスをあてないとtextがhttprequestに反映されない
            wordField.ActivateInputField();
        }
    }

}