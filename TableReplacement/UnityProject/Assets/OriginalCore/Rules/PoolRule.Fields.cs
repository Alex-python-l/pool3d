// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Recovered from original IL2CPP metadata and ARM; see module_restore/rules/source_map.json.
using System;
using UnityEngine;
using Random = System.Random;

public partial class PoolRule : IIRule
{
    protected RuleListener m_Listener; // 0x8
    public static Random rand; // 0x0
    public static float REAL_SIMULATOR_SCALE; // 0x4
    public static readonly float BALL_S; // 0x8
    public int difficulty; // 0xC
    public static bool isPractice; // 0xC
    public static float REAL_SCALE; // 0x10
    public static float TABLE_WIDTH; // 0x14
    public static float TABLE_HEIGHT; // 0x18
    public static float BALL_RADIUS; // 0x1C
    public static float INNER_TABLE_WIDTH; // 0x20
    public static float INNER_TABLE_HEIGHT; // 0x24
    public static float SLIDING_FORCE; // 0x28
    public static float ROLLING_FORCE; // 0x2C
    public static float BALL_TABLE_FORCE; // 0x30
    public static float SPIN_DECE_RATE; // 0x34
    public static float BALL_BALL_FORCE; // 0x38
    public static float V2W; // 0x3C
    public const int STATE_WAITING = 1;
    public const int STATE_RUNNING = 2;
    public const int STATE_FREEBALL = 3;
    public const int STATE_END = 4;
    public const int CAMP_STATE_BREAKOFF = 0;
    public const int CAMP_STATE_OPEN = 1;
    public const int CAMP_STATE_SOLIDE_COLOR = 2;
    public const int CAMP_STATE_STRIP_COLOR = 3;
    public const int PUSH_OUT_NONE = 0;
    public const int PUSH_OUT_START = 1;
    public const int PUSH_OUT_RUNNING = 2;
    public const int PUSH_OUT_GIVEUP = 3;
    public const int PUSH_OUT_END = 4;
    public const int PLAYER_TYPE_PLAYER = 0;
    public const int PLAYER_TYPE_AI = 1;
    public const int PLAYER_TYPE_OPPO = 2;
    public static float[] outsideTableCorners; // 0x40
    public static float[] insideTableCorners; // 0x44
    public static float[] freeAreaCorners; // 0x48
    public static float[] insideBorder; // 0x4C
    private static float[] blockersPosition_small; // 0x50
    private static float[] holePosition_small; // 0x54
    private static float[] blockersPosition_big; // 0x58
    private static float[] holePosition_big; // 0x5C
    private static float[] freeball_Aera; // 0x60
    public const int STANDARD = 0;
    public const int POCKET8BALL = 1;
    public const int ONLY_IN_3D = 2;
    public const int NOGUIDELINE = 4;
    private static float[] blockersPosition; // 0x64
    private static float[] holePosition; // 0x68
}
