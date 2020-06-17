using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using VoiceChat;
using UnityEngine.UI;

public class VoiceChatIndicatorContoller : MonoBehaviour
{
    [HideInInspector]
    public VoiceChatPlayer voicePlayer;

    [HideInInspector]
    public bool muted;

    private int delayIndicateTalking = 0;

    public Image backgroundImage;

    private Color defaultColor;


    private void Awake()
    {
        defaultColor = backgroundImage.color;
    }


    void FixedUpdate()
    {
        if (delayIndicateTalking > 0)
        {
            delayIndicateTalking--;
            if (delayIndicateTalking < 5)
            {
                backgroundImage.color = defaultColor;
            }
            else
                backgroundImage.color = Color.red;           
        }
    }

    public void OnPlayerTalk()
    {
        delayIndicateTalking = 60;
    }

    public void OnPressMuteButton()
    {
        muted = !GetComponent<Toggle>().isOn;
    }
 
}
