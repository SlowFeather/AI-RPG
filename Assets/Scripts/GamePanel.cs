using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public Text msgTxt;

    public Text select1Txt;
    public Text select2Txt;

    public Image bgImage;


    public Action select1BtnAction;
    public Action select2BtnAction;


    // Start is called before the first frame update
    
    public void OnSelect1BtnFunction()
    {
        GPTManager.Instance.AskChatGPT(select1Txt.text);
        select1BtnAction?.Invoke();
    }

    public void OnSelect2BtnFunction()
    {
        GPTManager.Instance.AskChatGPT(select2Txt.text);
        select2BtnAction?.Invoke();
    }

    public void SetMsg(string msg)
    {
        msgTxt.text = msg;
    }
}
