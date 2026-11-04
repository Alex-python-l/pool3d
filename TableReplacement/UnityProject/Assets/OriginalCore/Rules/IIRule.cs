// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Recovered from original IL2CPP metadata and ARM; see module_restore/rules/source_map.json.
using UnityEngine;

public interface IIRule
{
    void setLose(RoundState rs);
    void setWin(RoundState rs);
    float getTableWidth();
    float getTableHeight();
    float getBallRadius();
    float getForce();
    float getRollForce();
    float getHoleGravity();
    int getBallCount();
    Texture getBallTexture(int ballid);
    void resetBalls(PoolBall[] balls, PoolHole[] holes);
    Texture getShadowTexture();
    Texture getHighlightTexture();
    Texture getTargetTexture();
    int getBlockerCount();
    void resetBlockers(PoolBlocker[] blockers);
    int getHoleCount();
    void resetHoles(PoolHole[] holes);
    int getPlayerCount();
    int getPlayerType(int playerid);
    int getAimTarget(RoundState state);
    bool isAvailableTarget(RoundState state, PoolBall[] balls, int id);
    bool canPut(PoolBall[] balls, int ind, float x, float y);
    RoundState createRoundState();
    void resetRoundState(RoundState state);
    void resetRoundStateData(RoundState state);
    int getClickTarget(RoundState state);
    float getDefaultAimAngle();
    float getMinStrength();
    float getMaxStrength();
    void onClick(RoundState state, PoolBall[] balls);
    void onRoundOver(RoundState state, PoolBall[] balls, PoolHole[] holes);
    void onTimeOver(RoundState state, PoolBall[] balls);
    bool onPotBall(RoundState roundstate, int ballid, PoolBall[] balls, PoolHole hole);
    void newMatch(RoundState roundstate, PoolBall[] balls, PoolHole[] holes);
    void onHitBall(RoundState roundstate, int ballid1, int ballid2, PoolBall[] balls);
    void onHitBlocker(RoundState roundstate, int ballid, int blockerid, PoolBall[] balls, PoolBlocker[] blockers);
    void onPutFreeball(RoundState roundstate, PoolBall[] balls, int ballid);
    PoolAI getAI();
    bool checkFreeballArea(RoundState state, PoolBall ball0);
    bool showTargetSign(RoundState state);
    void checkBallPosition(PoolBall ball);
}
