// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using System.Collections.Generic;
using com.junerking.proto;
using UnityEngine;
using Debug = UnityEngineInternalExport.Debug;
using Random = UnityEngineInternalExport.Random;

public partial class VSRule
{
    // RVA: 0x928C38
    public VSRule() { foulTipId = -1; }

    // RVA: 0x930620
    static VSRule() { not_ai_player_count = 2; }

    // RVA: 0x92D9A8
    public void SetCCBalls(IList<CCBall> bs)
    {
        if (ccballs == null) ccballs = new Dictionary<int, CCBall>();
        ccballs.Clear();
        for (int i = 0; i < bs.Count; i++) ccballs.Add(bs[i].BallId, bs[i]);
    }

    // RVA: 0x92DC74
    public override int getPlayerCount() { return 2; }

    // RVA: 0x92DC7C
    public override int getPlayerType(int playerid) { return playerid >= not_ai_player_count ? 1 : 0; }

    // RVA: 0x92DCF0
    public override void setLose(RoundState rs)
    {
        int winId = 1 - rs.currentPlayer;
        rs.state = 4;
        rs.winner = winId;
        if (m_Listener != null) m_Listener.OnWin(winId);
    }

    // RVA: 0x92DDD0
    public override void setWin(RoundState rs)
    {
        int winId = rs.currentPlayer;
        rs.state = 4;
        rs.winner = winId;
        if (m_Listener != null) m_Listener.OnWin(winId);
    }

    // RVA: 0x92AF1C
    public void switchPlayer(RoundState rs, PoolBall[] balls)
    {
        if (m_Listener != null) m_Listener.SwitchPlayer();
        rs.isSwitchPlayer = true;
    }

    // RVA: 0x92DEAC
    public override bool showTargetSign(RoundState state) { return (uint)((GameRoundState)state).campstate > 1u; }

    // RVA: 0x92DF44
    public override int getBallCount() { return 16; }

    // RVA: 0x92DF4C (original single bx lr)
    public override void resetBalls(PoolBall[] balls, PoolHole[] holes) { }

    // RVA: 0x92DF50; indexed jump table separately resolved in original ARM evidence.
    public override bool isAvailableTarget(RoundState state, PoolBall[] balls, int id)
    {
        GameRoundState rs = (GameRoundState)state;
        int solidPlayer;
        switch (rs.campstate)
        {
            case 0:
                return id != 0;
            case 1:
                if (id != 8) return id != 0;
                // Original upper bound is 14, not 15; retained without a rule correction.
                for (int i = 1; i <= 14; i++)
                    if (i != 8 && balls[i].isAvailable) return false;
                return true;
            case 2:
                solidPlayer = 0;
                break;
            case 3:
                solidPlayer = 1;
                break;
            default:
                if (id != 8) return unchecked((uint)(id - 9)) < 7u;
                for (int i = 9; i <= 15; i++) if (balls[i].isAvailable) return false;
                return true;
        }
        bool solid = rs.currentPlayer == solidPlayer;
        if (id != 8) return unchecked((uint)(id - (solid ? 1 : 9))) < 7u;
        int start = solid ? 1 : 9;
        for (int i = 0; i < 7; i++) if (balls[start + i].isAvailable) return false;
        return true;
    }

    // RVA: 0x92E144
    public override RoundState createRoundState()
    {
        GameRoundState state = new GameRoundState();
        state.playerScore = new int[getPlayerCount()];
        state.foulCount = new int[getPlayerCount()];
        return state;
    }

    // RVA: 0x92E230
    public override void resetRoundState(RoundState state)
    {
        state.roundCount = 0;
        state.state = 1;
        state.currentPlayer = not_ai_player_count == 1 ? Random.Range(0, 2) : 0;
        state.combo = 0;
        state.winner = -1;
        // Native loop stores playerScore[0] on each iteration; the index is not repaired.
        for (int i = 0; i < state.playerScore.Length; i++) state.playerScore[0] = 0;
        state.combo = 0;
        state.totalGameTime = 0;
        state.roundTime = 0;
        state.counttime = false;
        state.potInOneBreak = 0;
        state.isFreeBall = false;
        state.lastFouledPlayer = -1;
        state.matchCount = 0;
        ((GameRoundState)state).Reset();
    }

    // RVA: 0x92E610
    protected void Restart(GameRoundState rs, PoolBall[] balls)
    {
        Debug.LogWarning("Restart");
        OnTip(3, rs.currentPlayer);
        resetBalls(balls, null);
        rs.potCnt = 0;
        rs.fouled = false;
        rs.pushOutState = 0;
        rs.combo = 0;
        rs.firsthit = -1;
        rs.firstpot = -1;
        rs.validPotCnt = 0;
        rs.isSwitchPlayer = false;
        rs.blockcount = 0;
        rs.pooledball8 = false;
        rs.pooledball9 = false;
        rs.callPocket_HoleId = -1;
        rs.targetPocket_HoleId = -1;
        rs.ResetBallRoundStates();
    }

    // RVA: 0x92E7FC
    private bool CheckOver(GameRoundState rs, PoolBall[] balls)
    {
        if (!rs.pooledball8)
        {
            if (balls[8].isAvailable) return false;
            rs.lastFouledPlayer = rs.currentPlayer;
            setLose(rs);
        }
        else if (rs.validPotCnt > 1 || rs.fouled)
        {
            rs.lastFouledPlayer = rs.currentPlayer;
            setLose(rs);
        }
        else if (!IsCallPocketBall(rs) || rs.targetPocket_HoleId == rs.callPocket_HoleId) setWin(rs);
        else setLose(rs);
        return true;
    }

    // RVA: 0x92E8D0
    private void CheckCamp(GameRoundState rs)
    {
        if (rs.validPotCnt > 0 && !rs.fouled)
        {
            int player = rs.currentPlayer;
            Debug.Log("Pool:rs.currentPlayer=" + rs.currentPlayer.ToString() + ", rs.firstpot=" + rs.firstpot.ToString());
            int camp = player == 0 ? 2 : 3;
            if (rs.firstpot > 8) camp = 5 - camp;
            Debug.Log("Pool:Camp decided, " + (camp == 2 ? "player" : "ai") + " pot color balls");
            rs.campstate = camp;
            if (m_Listener != null) m_Listener.ConfirmCamp();
        }
    }

    // RVA: 0x92EAE8
    private bool OnRoundOver_BreakOff(GameRoundState rs, PoolBall[] balls)
    {
        if (!balls[8].isAvailable)
        {
            Restart(rs, balls);
            switchPlayer(rs, balls);
            return true;
        }
        if (!balls[0].isAvailable || (rs.validPotCnt == 0 && rs.blockcount < 3))
        {
            OnTip(2, rs.currentPlayer);
            rs.fouled = true;
            putBall(null, balls, 0, Geometry.CueStartX, 0f, true);
        }
        else rs.campstate = 1;
        CheckSwitchPlayer(rs, balls);
        return false;
    }

    // RVA: 0x92EC54
    private bool OnRoundOver_OpenCamp(GameRoundState rs, PoolBall[] balls)
    {
        bool over = CheckOver(rs, balls);
        if (!over)
        {
            CheckFoul(rs, balls);
            CheckCamp(rs);
            CheckSwitchPlayer(rs, balls);
        }
        return over;
    }

    // RVA: 0x92EE14
    private bool OnRoundOver_SureCamp(GameRoundState rs, PoolBall[] balls)
    {
        bool over = CheckOver(rs, balls);
        if (!over)
        {
            CheckFoul(rs, balls);
            CheckSwitchPlayer(rs, balls);
        }
        return over;
    }

    // RVA: 0x92ECA4
    protected void CheckFoul(GameRoundState rs, PoolBall[] balls)
    {
        if (rs.validPotCnt == 0 && rs.blockcount == 0) { foulTipId = 8; rs.fouled = true; }
        if (rs.firsthit == -1) { foulTipId = 7; rs.fouled = true; }
        if (!balls[0].isAvailable)
        {
            putBall(rs, balls, 0, 0f, 0f, false);
            foulTipId = 6;
            rs.fouled = true;
        }
        if (!checkFreeballArea(rs, balls[0]) && rs.fouled) putBall(rs, balls, 0, 0f, 0f, false);
        OnTip(foulTipId, rs.currentPlayer);
        foulTipId = -1;
        rs.isFreeBall = rs.fouled;
        if (rs.isFreeBall) OnTip(0, 1 - rs.currentPlayer);
    }

    // RVA: 0x92EC14
    private void CheckSwitchPlayer(GameRoundState rs, PoolBall[] balls)
    {
        if (!rs.fouled && rs.validPotCnt != 0) return;
        switchPlayer(rs, balls);
    }

    // RVA: 0x929F34
    public void SetStateToNextRound(GameRoundState rs)
    {
        Debug.LogWarning("SetStateToNextRound");
        rs.potCnt = 0;
        rs.fouled = false;
        rs.firsthit = -1;
        rs.firstpot = -1;
        rs.validPotCnt = 0;
        rs.isSwitchPlayer = false;
        rs.blockcount = 0;
        rs.pooledball8 = false;
        rs.pooledball9 = false;
        rs.ResetBallRoundStates();
    }

    // RVA: 0x92EE58
    public override void onRoundOver(RoundState state, PoolBall[] balls, PoolHole[] holes)
    {
        state.state = 1;
        HandDestage(balls);
        GameRoundState rs = (GameRoundState)state;
        bool over = rs.campstate == 0 ? OnRoundOver_BreakOff(rs, balls)
            : rs.campstate == 1 ? OnRoundOver_OpenCamp(rs, balls) : OnRoundOver_SureCamp(rs, balls);
        if (!over)
        {
            if (!rs.isSwitchPlayer && m_Listener != null) m_Listener.OnContinueTurn();
            if (!Pool3D.Offline.OfflineMatchController.Instance.IsNet) SetStateToNextRound(rs);
        }
    }

    // RVA: 0x92FD94
    public override void newMatch(RoundState state, PoolBall[] balls, PoolHole[] holes) { ((GameRoundState)state).Reset(); }

    // RVA: 0x930180
    public override void onClick(RoundState state, PoolBall[] balls)
    {
        state.isFreeBall = false;
        state.state = 2;
    }

    // RVA: 0x92B15C
    public override void onTimeOver(RoundState state, PoolBall[] balls)
    {
        GameRoundState rs = (GameRoundState)state;
        OnTip(9, state.currentPlayer);
        if (rs.campstate == 0) OnTip(1, 1 - state.currentPlayer);
        rs.isFreeBall = true;
        rs.fouled = true;
        switchPlayer(state, balls);
        if (!Pool3D.Offline.OfflineMatchController.Instance.IsNet) SetStateToNextRound(rs);
    }

    // RVA: 0x9301A8
    public void onPushOut(RoundState state)
    {
        ((GameRoundState)state).pushOutState = 2;
        OnTip(102, state.currentPlayer);
    }

    // RVA: 0x930280
    public virtual void giveUpStrick(RoundState state, PoolBall[] balls)
    {
        GameRoundState rs = (GameRoundState)state;
        OnTip(101, state.currentPlayer);
        rs.pushOutState = 4;
        switchPlayer(state, balls);
        if (!Pool3D.Offline.OfflineMatchController.Instance.IsNet) SetStateToNextRound(rs);
    }

    // RVA: 0x9303E8
    public bool IsHasGuidLine(RoundState state) { return (((GameRoundState)state).rule & 4) != 0; }

    // RVA: 0x9304A8
    public bool IsOnlyIn3dView(RoundState state) { return (((GameRoundState)state).rule & 2) != 0; }

    // RVA: 0x92AFE8
    public bool IsCallPocketBall(RoundState state) { return (((GameRoundState)state).rule & 1) != 0; }

    // RVA: 0x930568
    public static bool IsOnlyIn3dView(int rule) { return (rule & 2) != 0; }

    // RVA: 0x930570
    public static bool IsCallPocketBall(int rule) { return (rule & 1) != 0; }

    // RVA: 0x930578
    public static bool IsHasGuidLine(int rule) { return (rule & 4) != 0; }

    // RVA: 0x930580
    public static string Get_RuleDsp(int rule) { return (rule & 1) != 0 ? "Call pocket on 8-ball" : "Standard"; }
}
