using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EFaction
{
    [HideInInspector]
    Empty,
    [InspectorName("玩家")]
    Player,
    [InspectorName("电脑")]
    AI,
}


public class Game : MonoBehaviour
{
    int[][] Board;
    
    public List<Chess> chessList;
    public List<Grid> remainGrids;

    System.Random random;

    bool isPlayerTurn;
    float remainTurnTime = int.MaxValue;
    bool isGamePause;

    Grid bestGrid = Grid.zero;

    UIManager uiMgr;
    CameraController camCtrl;
    Config config;

    // Start is called before the first frame update
    void Start()
    {
        chessList = new List<Chess>();
        random = new System.Random();
        remainGrids = new List<Grid>();

        uiMgr = GetComponent<UIManager>();
        camCtrl = GetComponent<CameraController>();
        config = GetComponent<Config>();

        InitBoard();

        RestartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(1))
            RestartGame();

        if (isGamePause)
            return;

        LogicControl();

        if (IsEnd(out EFaction winFaction))
            EndGame(winFaction);

        if (IsFull())
            EndGame(EFaction.Empty);

        TimerLogic();
    }

    /// <summary>
    /// 初始化棋盘
    /// </summary>
    void InitBoard()
    {
        Board = new int[config.BoardSize.x][];
        for (int i = 0; i < config.BoardSize.x; i++)
        {
            Board[i] = new int[config.BoardSize.y];
            for (int j = 0; j < config.BoardSize.y; j++)
            {
                Vector3 gridPosition = new Vector3(i * config.GridSize + i * config.GridGap, 0, j * config.GridSize + j * config.GridGap);
                GameObject grid = GameObject.Instantiate(config.Grid);
                grid.transform.position = gridPosition;
                grid.transform.localScale *= config.GridSize;
            }
        }
    }
    
    /// <summary>
    /// 倒计时定时器更新
    /// </summary>
    void TimerLogic()
    {
        if (remainTurnTime > 0)
        {
            if (isPlayerTurn)
                remainTurnTime -= Time.deltaTime;
        }
        else
        {
            remainTurnTime = 0;
            EndGame(EFaction.AI);
        }
        uiMgr.UpdateTimer(remainTurnTime);
    }

    /// <summary>
    /// 结束本次游戏
    /// </summary>
    /// <param name="winFaction"></param>
    void EndGame(EFaction winFaction)
    {
        uiMgr.ShowResult(winFaction);
        Debug.Log($"<color=red>{winFaction} 胜利</color>");
        PauseGame();
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    void PauseGame()
    {
        isGamePause = true;
    }

    /// <summary>
    /// 重启游戏
    /// </summary>
    void RestartGame()
    {
        isGamePause = false;

        uiMgr.HideResult();

        remainTurnTime = config.MaxTurnTime;

        foreach (var chess in chessList)
            Destroy(chess.go);
        chessList.Clear();

        remainGrids.Clear();

        for (int i = 0; i < config.BoardSize.x; i++)
        {
            for (int j = 0; j < config.BoardSize.y; j++)
            {
                ResetGrid(new Grid(i, j));
                remainGrids.Add(new Grid(i, j));
            }
        }

        RandomTurn();
    }

    /// <summary>
    /// 人机博弈
    /// </summary>
    void LogicControl()
    {
        if (isPlayerTurn)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = camCtrl.Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit castInfo;
                if (Physics.Raycast(ray, out castInfo))
                {
                    var go = castInfo.collider.gameObject;
                    if (PlaceChess(go.transform.position, config.Player, EFaction.Player))
                        SwitchTurnOwner();
                }
            }
        }
        else
        {
            Grid grid;
            // 先手时随机一个位置落子
            if (remainGrids.Count == config.BoardSize.x * config.BoardSize.y)
                grid = new Grid(random.Next(0, config.BoardSize.x), random.Next(0, config.BoardSize.x));
            // 剪枝算法取最优位置落子
            else
            {
                int score = AlphaBetaScore(0, 10, EFaction.AI);
                //int score = MiniMax(false);
                Debug.Log($"{bestGrid} : {score}");
                grid = bestGrid;
            }
            Vector3 gridPosition = new Vector3(grid.x * config.GridSize + grid.x * config.GridGap, 0, grid.y * config.GridSize + grid.y * config.GridGap);

            if (PlaceChess(gridPosition, config.AI, EFaction.AI))
                SwitchTurnOwner();
        }
    }

    /// <summary>
    /// 随机先手回合
    /// </summary>
    void RandomTurn()
    {
        isPlayerTurn = random.Next(2) > 0;
    }

    /// <summary>
    /// 切换回合角色
    /// </summary>
    void SwitchTurnOwner()
    {
        isPlayerTurn = !isPlayerTurn;
        uiMgr.UpdateTurnInfo(isPlayerTurn);

        remainTurnTime = config.MaxTurnTime;
    }

    /// <summary>
    /// 落子
    /// </summary>
    /// <param name="position"></param>
    /// <param name="chessPrefab"></param>
    /// <param name="faction"></param>
    bool PlaceChess(Vector3 position, GameObject chessPrefab, EFaction faction)
    {
        int i = (int)(position.x / (config.GridSize + config.GridGap));
        int j = (int)(position.z / (config.GridSize + config.GridGap));

        if (!IsEmpty(new Grid(i, j)))
            return false;
        Debug.Log($"{faction} 落子 {position} ({i},{j})");

        var chess = new Chess(GameObject.Instantiate(chessPrefab), new Grid(i, j), faction);
        chess.go.transform.position = position + Vector3.up * config.GridSize;
        chess.go.transform.localScale *= config.GridSize / 2;

        chessList.Add(chess);

        Board[i][j] = (int)chess.faction;
        remainGrids.Remove(new Grid(i, j));
        return true;
    }

    /// <summary>
    /// 棋盘是否达成结束条件
    /// </summary>
    /// <param name="winFaction"></param>
    /// <returns></returns>
    bool IsEnd(out EFaction winFaction)
    {
        winFaction = default;

        for (int i = 0; i < config.winConnectChessCount; i++)
        {
            if (Board[i][0] == Board[i][1] && Board[i][1] == Board[i][2] && Board[i][0] != 0)
            {
                winFaction = (EFaction)Board[i][0];
                return true;
            }
        }

        for (int i = 0; i < config.winConnectChessCount; i++)
        {
            if (Board[0][i] == Board[1][i] && Board[1][i] == Board[2][i] && Board[0][i] != 0)
            {
                winFaction = (EFaction)Board[0][i];
                return true;
            }
        }

        if (Board[0][0] == Board[1][1] && Board[1][1] == Board[2][2] && Board[0][0] != 0)
        {
            winFaction = (EFaction)Board[0][0];
            return true;
        }

        if (Board[0][2] == Board[1][1] && Board[1][1] == Board[2][0] && Board[0][2] != 0)
        {
            winFaction = (EFaction)Board[0][2];
            return true;
        }

        return false;
    }

    /// <summary>
    /// 棋盘是否已满
    /// </summary>
    /// <returns></returns>
    bool IsFull()
    {
        for (int i = 0; i < config.BoardSize.x; i++)
        {
            for (int j = 0; j < config.BoardSize.y; j++)
            {
                Grid grid = new Grid(i, j);
                if (IsEmpty(grid))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 棋格是否为空
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    bool IsEmpty(Grid grid) => Board[grid.x][grid.y] == 0;

    /// <summary>
    /// 当前棋盘指定阵营是否能达成胜利条件
    /// </summary>
    /// <param name="faction"></param>
    /// <returns></returns>
    private bool IsWin(EFaction faction)
    {
        for (int i = 0; i < config.BoardSize.x; i++)
        {
            for (int j = 0; j < config.BoardSize.y; j++)
            {
                Grid grid = new Grid(i, j);
                if (IsEmpty(grid))
                {
                    SimulatePlaceChess(grid, faction);
                    bool win = IsEnd(out EFaction winFaction) && winFaction == faction;
                    ResetGrid(grid);
                    if (win)
                    {
                        bestGrid = grid;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 最小最大化算法
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    int MiniMax(bool isPlayer, int step = 0)
    {
        if (IsFull())
            return 0;
        else if (IsWin(isPlayer ? EFaction.Player : EFaction.AI))
            return isPlayer ? 10 - step : step - 10;

        step++;
        int score = isPlayer ? -10 : 10;
        foreach (var grid in remainGrids)
        {
            if (IsEmpty(grid))
            {
                SimulatePlaceChess(grid, isPlayer);
                int curScore = MiniMax(!isPlayer, step);
                ResetGrid(grid);
                if (isPlayer)
                {
                    if (curScore > score)
                    {
                        score = curScore;
                        bestGrid = grid;
                    }
                }
                else
                {
                    if (curScore < score)
                    {
                        score = curScore;
                        bestGrid = grid;
                    }
                }
            }
        }

        return score;
    }

    /// <summary>
    /// AlphaBeta剪枝算法
    /// </summary>
    /// <param name="grid">落子</param>
    /// <param name="Board">当前棋盘</param>
    /// <returns></returns>
    int AlphaBetaScore(int alpha, int beta, EFaction faction, int step = 0)
    {
        if (IsFull())
            return 0;
        // 根据达成胜利条件所需步数计算得分
        else if (IsWin(faction))
            return faction == EFaction.Player ? 10 - step : step - 10;

        // 递增步数
        step++;

        // 计算棋盘得分
        switch (faction)
        {
            case EFaction.Player:
                foreach (var curGrid in remainGrids)
                {
                    if (!IsEmpty(curGrid))
                        continue;

                    SimulatePlaceChess(curGrid, faction);
                    int value = AlphaBetaScore(alpha, beta, EFaction.AI, step);
                    ResetGrid(curGrid);

                    if (value > alpha)
                    {
                        alpha = value;
                        bestGrid = curGrid;
                    }
                    if (alpha >= beta)
                        break;
                }
                return alpha;
            case EFaction.AI:
                foreach (var curGrid in remainGrids)
                {
                    if (!IsEmpty(curGrid))
                        continue;

                    SimulatePlaceChess(curGrid, faction);
                    int value = AlphaBetaScore(alpha, beta, EFaction.Player, step);
                    ResetGrid(curGrid);

                    if (value < beta)
                    {
                        beta = value;
                        bestGrid = curGrid;
                    }
                    
                    if (alpha >= beta)
                        break;
                }
                return beta;
            default:
                return 0;
        }

        
    }

    /// <summary>
    /// AI模拟落子
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="isPlayer"></param>
    void SimulatePlaceChess(Grid grid, bool isPlayer)
    {
        EFaction faction = isPlayer ? EFaction.Player : EFaction.AI;
        SimulatePlaceChess(grid, faction);
    }
    /// <summary>
    /// AI模拟落子
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="isPlayer"></param>
    void SimulatePlaceChess(Grid grid, EFaction faction)
    {
        //Debug.Log($"{faction} 模拟落子 {grid}");
        Board[grid.x][grid.y] = (int)faction;
    }

    /// <summary>
    /// 重置棋格
    /// </summary>
    /// <param name="grid"></param>
    void ResetGrid(Grid grid)
    {
        // 重置棋盘
        Board[grid.x][grid.y] = 0;
        //Debug.Log($"重置棋格{grid}");
    }
}
