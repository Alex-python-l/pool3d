// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Recovered from original IL2CPP metadata and ARM; see module_restore/rules/source_map.json.
using System.Collections.Generic;
using com.junerking.proto;

public partial class VSRule : PoolRule
{
    public bool isBallPosFromNet; // 0x10
    public Dictionary<int, CCBall> ccballs; // 0x14
    public static int not_ai_player_count; // 0x0
    protected int foulTipId; // 0x18
    public const int TIP_FREEBALL = 0;
    public const int TIP_BREAKING = 1;
    public const int TIP_BREAKING_ILLEGEN = 2;
    public const int TIP_BREAKING_RESTART = 3;
    public const int TIP_FOUL_BADHIT_STRIP = 4;
    public const int TIP_FOUL_BADHIT_SLOID = 5;
    public const int TIP_FOUL_CUE_POT = 6;
    public const int TIP_FOUL_CUE_NOHIT = 7;
    public const int TIP_FOUL_BADSTROCKE = 8;
    public const int TIP_FOUL_TIMEOVER = 9;
    public const int TIP_TURN = 100;
    public const int TIP_GIVEUP_STRICK = 101;
    public const int TIP_PUSHOUT = 102;
}
