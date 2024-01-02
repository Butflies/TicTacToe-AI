using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Tooltip("回合信息")]
    public Text TurnText;
    [Tooltip("倒计时")]
    public Text TimerText;
    [Tooltip("结算界面")]
    public GameObject ResultGo;
    [Tooltip("结算")]
    public Text ResultText;

    public void UpdateTurnInfo(bool isPlayer)
    {
        TurnText.text = isPlayer ? "玩家" : "电脑";
    }

    public void UpdateTimer(float time)
    {
        TimerText.text = $"{Mathf.Floor(time)}";
    }

    public void ShowResult(EFaction winFaction)
    {
        string resultStr = null;
        Color resultStrColor = Color.white;
        switch (winFaction)
        {
            case EFaction.Empty:
                resultStr = "旗鼓相当的对手。";
                break;
            case EFaction.Player:
                resultStr = "大吉大利，今晚吃鸡！";
                resultStrColor = Color.yellow;
                break;
            case EFaction.AI:
                resultStr = "菜！";
                resultStrColor = Color.red;
                break;
        }

        ResultText.text = resultStr;
        ResultText.color = resultStrColor;
        ResultGo.SetActive(true);
    }

    public void HideResult()
    {
        ResultGo.SetActive(false);
    }
}
