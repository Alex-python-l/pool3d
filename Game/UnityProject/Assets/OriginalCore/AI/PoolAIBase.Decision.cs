// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using PoolCtr = Pool3D.Offline.OfflineMatchController;
// Original metadata fields and ARM decision branches; no replacement AI algorithm.
using UnityEngine;

public partial class PoolAIBase : PoolAI
{
    public AIStateMgr m_AIStateMgr;
    public PoolCtr m_PoolCtr;
    private float maxStrength;
    private float[] quickblockersPosition;
    private float halfR;
    public float[] quickholesPosition;
    private float[] blockersPosition;
    private Pool3D.Offline.Geometry.TableGeometryConfig Geometry {get{return m_PoolCtr.M_PoolRule.Geometry;}}
    private int blocker;
    private bool ignoreLog;
    private int level;
    private int target;
    private float cosda;
    public float angle;
    private float strength;
    private float score;
    private float tarx;
    private float tary;
    private int maxlevel;
    private float targetCosda;
    private float targetStrength;
    private float targetAngle;
    private float targetTarx;
    private float targetTary;
    private int difficulty;
    private int targetPocketId;
    protected bool pushout;
    protected bool giveUp;
    public float aimAngle;
    public float aiStrength;
    public float pitch;
    public float targetPitch;
    public int targetHoleId;

    // RVA: 0x91E280
    public PoolAIBase(PoolCtr poolCtr)
    {
        difficulty = 2;
        targetPocketId = -1;
        m_PoolCtr = poolCtr;
        m_AIStateMgr = new AIStateMgr();
        InitializeGeometry();
    }

    // RVA: 0x9223EC. Original InitializeArray bytes and inline assignments.
    private void InitializeGeometry()
    {
        quickblockersPosition = new float[] {
            -990.599976f, -432.200012f, -990.599976f, 432.200012f,
            -927.5f, 495.299988f, 927.5f, 495.299988f,
            990.599976f, 432.200012f, 990.599976f, -432.200012f,
            927.5f, -495.299988f, -927.5f, -495.299988f
        };
        halfR = 14.2875004f; // original bits 0x4164999A
        quickholesPosition = new float[36];
        quickholesPosition[0] = -990.6f;
        quickholesPosition[1] = 495.3f;
        quickholesPosition[2] = -990.6f - halfR;
        quickholesPosition[3] = 495.3f - (halfR + halfR);
        quickholesPosition[4] = halfR + halfR + -990.6f;
        quickholesPosition[5] = halfR + 495.3f;
        quickholesPosition[7] = 495.3f;
        quickholesPosition[8] = 0f - (halfR + halfR);
        quickholesPosition[9] = halfR + 495.3f;
        quickholesPosition[10] = halfR + halfR + 0f;
        quickholesPosition[11] = halfR + 495.3f;
        quickholesPosition[12] = 990.6f;
        quickholesPosition[13] = 495.3f;
        quickholesPosition[14] = halfR + halfR + 990.6f;
        quickholesPosition[15] = 495.3f - halfR;
        quickholesPosition[16] = 990.6f - halfR;
        quickholesPosition[17] = halfR + halfR + 495.3f;
        quickholesPosition[18] = 990.6f;
        quickholesPosition[19] = -495.3f;
        quickholesPosition[20] = halfR + halfR + 990.6f;
        quickholesPosition[21] = halfR + -495.3f;
        quickholesPosition[22] = 990.6f - halfR;
        quickholesPosition[23] = -495.3f - (halfR + halfR);
        quickholesPosition[25] = -495.3f;
        quickholesPosition[26] = halfR + halfR + 0f;
        quickholesPosition[27] = -495.3f - halfR;
        quickholesPosition[28] = 0f - (halfR + halfR);
        quickholesPosition[29] = -495.3f - halfR;
        quickholesPosition[30] = -990.6f;
        quickholesPosition[31] = -495.3f;
        quickholesPosition[32] = halfR + -990.6f;
        quickholesPosition[33] = -495.3f - (halfR + halfR);
        quickholesPosition[34] = -990.6f - (halfR + halfR);
        quickholesPosition[35] = halfR + -495.3f;
        blockersPosition = new float[] {
            -927.5f, 495.299988f, -75.0999985f, 495.299988f,
            75.0999985f, 495.299988f, 927.5f, 495.299988f,
            990.599976f, 432.200012f, 990.599976f, -432.200012f,
            927.5f, -495.299988f, 75.0999985f, -495.299988f,
            -75.0999985f, -495.299988f, -927.5f, -495.299988f,
            -990.599976f, -432.200012f, -990.599976f, 432.200012f
        };
        if(!Geometry.IsLegacy) {
            quickblockersPosition=Geometry.BuildQuickBlockers();
            quickholesPosition=Geometry.BuildAiTargets();
            blockersPosition=Geometry.CopyBlockers();
        }
    }

    // RVA: 0x921320
    public void SetAILv(bool isNetVs, int playerLv, float maxStrength, int aiLv)
    {
        difficulty = playerLv < 2 ? 0 : playerLv < 4 ? 1 : playerLv < 8 ? 2 : playerLv < 16 ? 3 : 4;
        if (isNetVs) difficulty = aiLv;
        this.maxStrength = maxStrength;
    }

    // RVA: 0x921378
    public void Reset() { m_AIStateMgr.Reset(this); }
    // RVA: 0x9204BC
    public virtual float getAngle() { return angle; }
    // RVA: 0x9204C4. Clamping mutates the original field.
    public virtual float getStrength()
    {
        if (strength > maxStrength) strength = maxStrength;
        return strength;
    }
    // RVA: 0x921310
    public float getTarX() { return tarx; }
    // RVA: 0x921318
    public float getTarY() { return tary; }
    // RVA: 0x922120
    public int GetTargetPocketId() { return targetPocketId; }
    // RVA: 0x9204B8. Original single bx lr.
    public virtual void CheckPushOut(RoundState roundstate, PoolBall[] balls, bool isTargetPocketPicked) { }
    // RVA: 0x92211C. Original single bx lr.
    public void onClick(RoundState state, PoolBall[] balls) { }
    // RVA: 0x91E3B0
    public virtual void RandomShoot(RoundState roundstate, PoolBall[] balls, bool isTargetPocketPicked)
    {
        angle = UnityEngine.Random.value * 360f;
    }

    // RVA: 0x91EFA4. One shared random value selects both large-error branch and sign.
    private void adjustDifficulty()
    {
        float random = UnityEngine.Random.value;
        float error, sign;
        switch (difficulty)
        {
            case 0:
                if (!(random < 0.6f)) { angle += UnityEngine.Random.Range(-0.8f, 0.8f); break; }
                error = UnityEngine.Random.Range(0.4f, 0.8f);
                sign = 0.3f;
                angle = !(random < sign) ? angle - error : angle + error;
                break;
            case 1:
                if (!(random < 0.4f)) { angle += UnityEngine.Random.Range(-0.4f, 0.4f); break; }
                error = UnityEngine.Random.Range(0.4f, 0.8f);
                sign = 0.2f;
                angle = !(random < sign) ? angle - error : angle + error;
                break;
            case 2:
                if (!(random < 0.2f)) { angle += UnityEngine.Random.Range(-0.2f, 0.2f); break; }
                error = UnityEngine.Random.Range(0.4f, 0.8f);
                sign = 0.1f;
                angle = !(random < sign) ? angle - error : angle + error;
                break;
            case 3:
                if (!(random < 0.1f)) { angle += UnityEngine.Random.Range(-0.1f, 0.1f); break; }
                error = UnityEngine.Random.Range(0.4f, 0.6f);
                sign = 0.05f;
                angle = !(random < sign) ? angle - error : angle + error;
                break;
        }
        strength *= 1.1f;
    }

    // RVA: 0x91E43C
    public virtual void think(RoundState roundstate, PoolBall[] balls, bool isTargetPocketPicked)
    {
        strength = Var.MAX_STRENGTH;
        float bestDistance = 0f;
        bool found = false;
        int chosen = 0;
        for (int i = 1; i < balls.Length; ++i)
        {
            if (!balls[i].isAvailable || !m_PoolCtr.M_PoolRule.isAvailableTarget(roundstate, balls, i)) continue;
            for (int h = 0; h < quickholesPosition.Length / 2; ++h)
            {
                if (isTargetPocketPicked && targetPocketId != h / 3) continue;
                float candidate = checkStraightPot(balls, i, h);
                if (!(candidate >= 0f) || (found && !(cosda < targetCosda))) continue;
                found = true;
                cosda = targetCosda;
                angle = targetAngle;
                strength = targetStrength;
                bestDistance = candidate;
                chosen = i;
                if (!isTargetPocketPicked) targetPocketId = h / 3;
            }
        }
        if (found)
        {
            level = 3;
            target = chosen;
            adjustDifficulty();
            return;
        }
        if (maxlevel >= 3) { level = -1; return; }
        if (difficulty >= 2)
        {
            for (int i = 1; i < balls.Length; ++i)
            {
                if (!balls[i].isAvailable || !m_PoolCtr.M_PoolRule.isAvailableTarget(roundstate, balls, i)) continue;
                for (int h = 0; h < quickholesPosition.Length / 2; ++h)
                {
                    if (isTargetPocketPicked && targetPocketId != h / 3) continue;
                    float candidate = checkPotByBlocker(balls, i, h);
                    if (!(candidate >= 0f) || (found && !(cosda < targetCosda))) continue;
                    found = true;
                    cosda = targetCosda;
                    angle = targetAngle;
                    strength = targetStrength;
                    bestDistance = candidate;
                    chosen = i;
                    if (!isTargetPocketPicked) targetPocketId = h / 3;
                }
            }
            if (found)
            {
                level = 2;
                target = chosen;
                adjustDifficulty();
                return;
            }
        }
        if (maxlevel >= 2) { level = -1; return; }
        CheckPushOut(roundstate, balls, isTargetPocketPicked);
        for (int i = 1; i < balls.Length; ++i)
        {
            if (!balls[i].isAvailable || !m_PoolCtr.M_PoolRule.isAvailableTarget(roundstate, balls, i)) continue;
            float candidate = checkStraightShot(balls, i, true);
            if (!(candidate > 0f) || (found && !(candidate < bestDistance))) continue;
            found = true;
            angle = targetAngle;
            chosen = i;
            bestDistance = candidate;
        }
        if (!found && difficulty >= 2)
        {
            for (int i = 1; i < balls.Length; ++i)
            {
                if (!balls[i].isAvailable || !m_PoolCtr.M_PoolRule.isAvailableTarget(roundstate, balls, i)) continue;
                float candidate = checkShotByBlocker(balls, i);
                if (!(candidate > 0f) || (found && !(candidate < bestDistance))) continue;
                found = true;
                angle = targetAngle;
                chosen = i;
                bestDistance = candidate;
            }
        }
        if (found)
        {
            score = bestDistance;
            level = 1;
            target = chosen;
            adjustDifficulty();
            return;
        }
        if (maxlevel >= 1) { level = -1; return; }
        level = 0;
        for (int i = 1; i < balls.Length; ++i)
        {
            if (!balls[i].isAvailable || !m_PoolCtr.M_PoolRule.isAvailableTarget(roundstate, balls, i)) continue;
            float candidate = checkStraightShot(balls, i, false);
            if (!(candidate > 0f) || (found && !(candidate < bestDistance))) continue;
            found = true;
            angle = targetAngle;
            bestDistance = candidate;
        }
        if (found) return;
        level = -1;
        RandomShoot(roundstate, balls, isTargetPocketPicked);
    }
}

public class PoolAI8Ball : PoolAIBase
{
    // RVA: 0x91E214
    public PoolAI8Ball(PoolCtr poolCtr) : base(poolCtr) { }
}

public class PoolAI9Ball : PoolAIBase
{
    // RVA: 0x91E31C
    public PoolAI9Ball(PoolCtr poolCtr) : base(poolCtr) { }
    // RVA: 0x91E388
    public override void RandomShoot(RoundState roundstate, PoolBall[] balls, bool isTargetPocketPicked)
    {
        angle = UnityEngine.Random.value * 360f;
    }
    // RVA: 0x91E3D8
    public override void CheckPushOut(RoundState roundstate, PoolBall[] balls, bool isTargetPocketPicked)
    {
        if (m_PoolCtr.IsCanPushOut()) { pushout = true; return; }
        if (m_PoolCtr.IsCanGiveUp()) giveUp = true;
    }
}
