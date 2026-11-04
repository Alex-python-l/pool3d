// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Original IL2CPP state layout. Native wide stores are expressed by their actual fields.
using UnityEngine;
using Debug = UnityEngineInternalExport.Debug;

public partial class VSRule
{
    public class GameRoundState : RoundState
    {
        public int campstate; // 0x4C
        public bool fouled; // 0x50
        public int firsthit; // 0x54
        public int firstpot; // 0x58
        public int validPotCnt; // 0x5C
        public int potCnt; // 0x60
        public int blockcount; // 0x64
        public bool pooledball8; // 0x68
        public bool pooledball9; // 0x69
        public int pushOutState; // 0x6C
        public int[] foulCount; // 0x70
        public int rule; // 0x74
        public int callPocket_HoleId; // 0x78
        public int targetPocket_HoleId; // 0x7C
        public BallRoundState[] ballRoundStates; // 0x80

        // RVA: 0x9308D8
        public int FreeBallState { get { return isFreeBall ? (campstate == 0 ? 0 : 1) : -1; } }

        // RVA: 0x930B44
        public GameRoundState()
        {
            ballRoundStates = new BallRoundState[16];
        }

        // RVA: 0x9308F8
        public void Reset()
        {
            Debug.LogWarning("GameRoundState Reset");
            potCnt = 0;
            pushOutState = 0;
            fouled = false;
            campstate = 0;
            firsthit = -1;
            firstpot = -1;
            validPotCnt = 0;
            isSwitchPlayer = false;
            blockcount = 0;
            pooledball8 = false;
            pooledball9 = false;
            callPocket_HoleId = -1;
            targetPocket_HoleId = -1;
            for (int i = 0; i < foulCount.Length; i++) foulCount[i] = 0;
            ResetBallRoundStates();
        }

        // RVA: 0x9309DC
        public void ResetBallRoundStates()
        {
            for (int i = 0; i < ballRoundStates.Length; i++)
            {
                if (ballRoundStates[i] == null) ballRoundStates[i] = new BallRoundState();
                ballRoundStates[i].Reset();
            }
        }

        public class BallRoundState
        {
            public bool hitByCueball; // 0x8
            public bool blocked; // 0x9
            public bool potted; // 0xA

            // RVA: 0x930B2C (native constructor only calls System.Object constructor)
            public BallRoundState() { }

            // RVA: 0x930B34
            public void Reset()
            {
                potted = false;
                hitByCueball = false;
                blocked = false;
            }
        }
    }
}
