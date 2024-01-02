using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    [Tooltip("AI")]
    public GameObject AI;
    [Tooltip("玩家")]
    public GameObject Player;
    [Tooltip("棋格")]
    public GameObject Grid;

    [Tooltip("棋盘规格")]
    public Grid BoardSize = new Grid(3, 3);
    [Tooltip("棋格大小")]
    public float GridSize = 3;
    [Tooltip("棋格间隔")]
    public float GridGap = 0.1f;
    [Tooltip("取得胜利的连接棋子数")]
    public int winConnectChessCount = 3;

    [Tooltip("最大回合时间")]
    public float MaxTurnTime = 10;
}
