// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public partial class PoolRule
{
    // RVA: 0x92C150 (native constructor only calls System.Object constructor)
    public PoolRule() { }
    // RVA: 0x92B3B4
    public void SetListener(RuleListener listener) { m_Listener = listener; }
    // RVA: 0x92B3BC (original bx lr)
    public virtual void setLose(RoundState rs) { }
    // RVA: 0x92B3C0 (original bx lr)
    public virtual void setWin(RoundState rs) { }
    // RVA: 0x92B3C4
    public virtual float getTableWidth() { return Geometry.IsLegacy?TABLE_WIDTH:Geometry.Outside[1]-Geometry.Outside[0]; }
    // RVA: 0x92B428
    public virtual float getTableHeight() { return Geometry.IsLegacy?TABLE_HEIGHT:Geometry.Outside[2]-Geometry.Outside[3]; }
    // RVA: 0x92B48C
    public virtual float getBallRadius() { return BALL_RADIUS; }
    // RVA: 0x92B4F0 (original literal1500)
    public virtual float getForce() { return 1500f; }
    // RVA: 0x92B4FC (original literal100)
    public virtual float getRollForce() { return 100f; }
    // RVA: 0x92B508 (original literal3600)
    public virtual float getHoleGravity() { return 3600f; }
    // RVA: 0x92B514
    public virtual Vector3 getHolePos(int id)
    {
        if (id >= 0 && id < GHoles.Length / 3)
            return new Vector3(GHoles[id * 3], GHoles[id * 3 + 1], GHoles[id * 3 + 2]);
        return Vector3.zero;
    }
    // RVA: 0x92B6CC (original mov r0,#0)
    public virtual int getBallCount() { return 0; }
    // RVA: 0x92B6D4 (original mov r0,#0)
    public virtual Texture getBallTexture(int ballid) { return null; }
    // RVA: 0x92B6DC (original bx lr)
    public virtual void resetBalls(PoolBall[] balls, PoolHole[] holes) { }
    // RVA: 0x92B6E0 (original mov r0,#0)
    public virtual Texture getShadowTexture() { return null; }
    // RVA: 0x92B6E8 (original mov r0,#0)
    public virtual Texture getHighlightTexture() { return null; }
    // RVA: 0x92B6F0 (original mov r0,#0)
    public virtual Texture getTargetTexture() { return null; }
    // RVA: 0x92B6F8
    public virtual int getBlockerCount() { return GBlockers.Length / 4; }
    // RVA: 0x92B778; original inlined PoolBlocker.setLinePosition is re-expressed as its public API.
    public virtual void resetBlockers(PoolBlocker[] blockers)
    {
        for (int i = 0; i < blockers.Length; i++)
        {
            blockers[i].setLinePosition(GBlockers[i * 4], GBlockers[i * 4 + 1],
                GBlockers[i * 4 + 2], GBlockers[i * 4 + 3]);
            blockers[i].force = 1200f;
        }
    }
    // RVA: 0x92B950
    public virtual int getHoleCount() { return GHoles.Length / 3; }
    // RVA: 0x92B9D4
    public virtual void resetHoles(PoolHole[] holes)
    {
        for (int i = 0; i < holes.Length; i++)
        {
            holes[i].x = GHoles[i * 3];
            holes[i].y = GHoles[i * 3 + 1];
            float diameter = GHoles[i * 3 + 2] + GHoles[i * 3 + 2];
            holes[i].width = diameter;
            holes[i].height = diameter;
            holes[i].id = i;
        }
    }
    // RVA: 0x92BB84 (original mov r0,#0)
    public virtual int getPlayerCount() { return 0; }
    // RVA: 0x92BB8C (original mov r0,#0)
    public virtual int getPlayerType(int playerid) { return 0; }
    // RVA: 0x92BB94 (original mvn r0,#0)
    public virtual int getAimTarget(RoundState state) { return -1; }
    // RVA: 0x92BB9C
    public virtual bool isAvailableTarget(RoundState state, PoolBall[] balls, int id) { return id != 0; }
    // RVA: 0x92BBAC
    public virtual bool canPut(PoolBall[] balls, int ind, float x, float y)
    {
        float radius = BALL_RADIUS;
        if(!Geometry.IsLegacy && !Geometry.CanPlace(new Vector2(x,y),radius)) return false;
        if (x - radius < GInside[0]) return false;
        if (x + radius > GInside[1]) return false;
        if (y + radius > GInside[2]) return false;
        if (y - radius < GInside[3]) return false;
        float minDistanceSquared = radius * radius * 4f;
        for (int i = 0; i < balls.Length; i++)
        {
            if (ind != i && balls[i].isAvailable)
            {
                float dx = balls[i].x - x;
                float dy = balls[i].y - y;
                if (minDistanceSquared > dy * dy + dx * dx) return false;
            }
        }
        for (int i = 0; i < GHoles.Length / 3; i++)
        {
            float dx = x - GHoles[i * 3];
            float dy = y - GHoles[i * 3 + 1];
            // Original compares scale+hole value to squared distance; no extra squaring is inserted.
            if (!(balls[ind].scale + GHoles[i * 3 + 2] <= dy * dy + dx * dx)) return false;
        }
        return true;
    }
    // RVA: 0x92BFB8
    public virtual RoundState createRoundState() { return new RoundState(); }
    // RVA: 0x92C038 (original bx lr)
    public virtual void resetRoundState(RoundState state) { }
    // RVA: 0x92C03C (original bx lr)
    public virtual void resetRoundStateData(RoundState state) { }
    // RVA: 0x92C040 (original mov r0,#0)
    public virtual int getClickTarget(RoundState state) { return 0; }
    // RVA: 0x92C048 (original zero float)
    public virtual float getDefaultAimAngle() { return 0f; }
    // RVA: 0x92C050 (original literal200)
    public virtual float getMinStrength() { return 200f; }
    // RVA: 0x92C05C (original zero float)
    public virtual float getMaxStrength() { return 0f; }
    // RVA: 0x92C064 (original bx lr)
    public virtual void onClick(RoundState state, PoolBall[] balls) { }
    // RVA: 0x92C068 (original bx lr)
    public virtual void onRoundOver(RoundState state, PoolBall[] balls, PoolHole[] holes) { }
    // RVA: 0x92C06C (original bx lr)
    public virtual void onTimeOver(RoundState state, PoolBall[] balls) { }
    // RVA: 0x92C070 (original mov r0,#0)
    public virtual bool onPotBall(RoundState roundstate, int ballid, PoolBall[] balls, PoolHole hole) { return false; }
    // RVA: 0x92C078 (original bx lr)
    public virtual void newMatch(RoundState roundstate, PoolBall[] balls, PoolHole[] holes) { }
    // RVA: 0x92C07C (original bx lr)
    public virtual void onHitBall(RoundState roundstate, int ballid1, int ballid2, PoolBall[] balls) { }
    // RVA: 0x92C080 (original bx lr)
    public virtual void onHitBlocker(RoundState roundstate, int ballid, int blockerid, PoolBall[] balls, PoolBlocker[] blockers) { }
    // RVA: 0x92C084 (original bx lr)
    public virtual void onPutFreeball(RoundState roundstate, PoolBall[] balls, int ballid) { }
    // RVA: 0x92C088 (original mov r0,#0)
    public virtual PoolAI getAI() { return null; }
    // RVA: 0x92C090 (original mov r0,#0)
    public virtual bool checkFreeballArea(RoundState state, PoolBall ball0) { return false; }
    // RVA: 0x92C098 (original mov r0,#0)
    public virtual bool showTargetSign(RoundState state) { return false; }
    // RVA: 0x92C0A0 (original bx lr)
    public virtual void checkBallPosition(PoolBall ball) { }
}
