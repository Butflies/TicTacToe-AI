using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Tooltip("�غ���Ϣ")]
    public Text TurnText;
    [Tooltip("����ʱ")]
    public Text TimerText;
    [Tooltip("�������")]
    public GameObject ResultGo;
    [Tooltip("����")]
    public Text ResultText;

    public void UpdateTurnInfo(bool isPlayer)
    {
        TurnText.text = isPlayer ? "���" : "����";
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
                resultStr = "����൱�Ķ��֡�";
                break;
            case EFaction.Player:
                resultStr = "�󼪴���������Լ���";
                resultStrColor = Color.yellow;
                break;
            case EFaction.AI:
                resultStr = "�ˣ�";
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
