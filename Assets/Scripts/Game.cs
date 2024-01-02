using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EFaction
{
    [HideInInspector]
    Empty,
    [InspectorName("���")]
    Player,
    [InspectorName("����")]
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
    /// ��ʼ������
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
    /// ����ʱ��ʱ������
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
    /// ����������Ϸ
    /// </summary>
    /// <param name="winFaction"></param>
    void EndGame(EFaction winFaction)
    {
        uiMgr.ShowResult(winFaction);
        Debug.Log($"<color=red>{winFaction} ʤ��</color>");
        PauseGame();
    }

    /// <summary>
    /// ��ͣ��Ϸ
    /// </summary>
    void PauseGame()
    {
        isGamePause = true;
    }

    /// <summary>
    /// ������Ϸ
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
    /// �˻�����
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
            // ����ʱ���һ��λ������
            if (remainGrids.Count == config.BoardSize.x * config.BoardSize.y)
                grid = new Grid(random.Next(0, config.BoardSize.x), random.Next(0, config.BoardSize.x));
            // ��֦�㷨ȡ����λ������
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
    /// ������ֻغ�
    /// </summary>
    void RandomTurn()
    {
        isPlayerTurn = random.Next(2) > 0;
    }

    /// <summary>
    /// �л��غϽ�ɫ
    /// </summary>
    void SwitchTurnOwner()
    {
        isPlayerTurn = !isPlayerTurn;
        uiMgr.UpdateTurnInfo(isPlayerTurn);

        remainTurnTime = config.MaxTurnTime;
    }

    /// <summary>
    /// ����
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
        Debug.Log($"{faction} ���� {position} ({i},{j})");

        var chess = new Chess(GameObject.Instantiate(chessPrefab), new Grid(i, j), faction);
        chess.go.transform.position = position + Vector3.up * config.GridSize;
        chess.go.transform.localScale *= config.GridSize / 2;

        chessList.Add(chess);

        Board[i][j] = (int)chess.faction;
        remainGrids.Remove(new Grid(i, j));
        return true;
    }

    /// <summary>
    /// �����Ƿ��ɽ�������
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
    /// �����Ƿ�����
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
    /// ����Ƿ�Ϊ��
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    bool IsEmpty(Grid grid) => Board[grid.x][grid.y] == 0;

    /// <summary>
    /// ��ǰ����ָ����Ӫ�Ƿ��ܴ��ʤ������
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
    /// ��С����㷨
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
    /// AlphaBeta��֦�㷨
    /// </summary>
    /// <param name="grid">����</param>
    /// <param name="Board">��ǰ����</param>
    /// <returns></returns>
    int AlphaBetaScore(int alpha, int beta, EFaction faction, int step = 0)
    {
        if (IsFull())
            return 0;
        // ���ݴ��ʤ���������貽������÷�
        else if (IsWin(faction))
            return faction == EFaction.Player ? 10 - step : step - 10;

        // ��������
        step++;

        // �������̵÷�
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
    /// AIģ������
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="isPlayer"></param>
    void SimulatePlaceChess(Grid grid, bool isPlayer)
    {
        EFaction faction = isPlayer ? EFaction.Player : EFaction.AI;
        SimulatePlaceChess(grid, faction);
    }
    /// <summary>
    /// AIģ������
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="isPlayer"></param>
    void SimulatePlaceChess(Grid grid, EFaction faction)
    {
        //Debug.Log($"{faction} ģ������ {grid}");
        Board[grid.x][grid.y] = (int)faction;
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="grid"></param>
    void ResetGrid(Grid grid)
    {
        // ��������
        Board[grid.x][grid.y] = 0;
        //Debug.Log($"�������{grid}");
    }
}
