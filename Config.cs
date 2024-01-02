using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    [Tooltip("AI")]
    public GameObject AI;
    [Tooltip("���")]
    public GameObject Player;
    [Tooltip("���")]
    public GameObject Grid;

    [Tooltip("���̹��")]
    public Grid BoardSize = new Grid(3, 3);
    [Tooltip("����С")]
    public float GridSize = 3;
    [Tooltip("�����")]
    public float GridGap = 0.1f;
    [Tooltip("ȡ��ʤ��������������")]
    public int winConnectChessCount = 3;

    [Tooltip("���غ�ʱ��")]
    public float MaxTurnTime = 10;
}
