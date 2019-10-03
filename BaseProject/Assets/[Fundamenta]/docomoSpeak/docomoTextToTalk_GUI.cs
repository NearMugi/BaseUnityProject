using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class docomoTextToTalk_GUI : MonoBehaviour
{
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
        if (docomoTextToTalk.Instance.httpReq == null) return;
        if (wordField == null) return;
        docomoTextToTalk.Instance.httpReq.setText(wordField.text);
    }

    public void setSpeakerID()
    {
        if (docomoTextToTalk.Instance.httpReq == null) return;
        if (speakerID == null) return;
        if (speakerID.isOn)
        {
            docomoTextToTalk.Instance.httpReq.setSpeakerID("14");
        }
        else
        {
            docomoTextToTalk.Instance.httpReq.setSpeakerID("1");
        }
    }

    public void setStyleID()
    {
        if (docomoTextToTalk.Instance.httpReq == null) return;
        if (styleID == null) return;
        if (styleID.isOn)
        {
            docomoTextToTalk.Instance.httpReq.setStyleID("14");
        }
        else
        {
            docomoTextToTalk.Instance.httpReq.setStyleID("1");
        }
    }

    public void setVoiceType()
    {
        if (docomoTextToTalk.Instance.httpReq == null) return;
        if (voiceTypeSlider == null) return;
        if (voiceTypeText == null) return;
        voiceTypeText.text = voiceTypeSlider.value.ToString("0.00");
        docomoTextToTalk.Instance.httpReq.setVoiceType(voiceTypeSlider.value);
    }

    public void setSpeechRate()
    {
        if (docomoTextToTalk.Instance.httpReq == null) return;
        if (speechRateSlider == null) return;
        if (speechRateText == null) return;
        speechRateText.text = speechRateSlider.value.ToString("0.00");
        docomoTextToTalk.Instance.httpReq.setSpeechRate(speechRateSlider.value);
    }

    public void speech()
    {
        if (docomoTextToTalk.Instance == null) return;
        StartCoroutine(docomoTextToTalk.Instance.PlayCoroutine());
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