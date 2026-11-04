// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Dependency slice of original global Var. Unselected network/store/event fields are not restored here.
// Selected static initializers are proven assignments inside native .cctor RVA 0xDEEE78, not its full body.
public partial class Var
{
    public const int DEFAULT_CUE_ID = 0;
    public const int CUE_FRAGMENT_NUM = 4;
    public const int TOUR_PLAYER_STATE_NONE = -2;
    public const int TOUR_PLAYER_STATE_LEAVE = -1;
    public const int TOUR_PLAYE_STATE_PLAYING = 0;
    public const int TOUR_PLAYER_STATE_WIN = 1;
    public const int TOUR_PLAYER_STATE_LOSE = 2;
    public const int TOUR_PLAYER_STATE_WAITTING = 3;
    public const int TOUR_SLEF_STATE_NONE = -1;
    public const int TOUR_SELF_STATE_TURN1_PERPARE = 0;
    public const int TOUR_SELF_STATE_TURN1_END = 1;
    public const int TOUR_SELF_STATE_TURN2_PERPARE = 2;
    public const int TOUR_SELF_STATE_TURN2_END = 3;
    public const int TOUR_SELF_STATE_TURN3_PERPARE = 4;
    public const int TOUR_SELF_STATE_TURN3_END = 5;
    public const int SELF = 0;
    public const int OPPO = 1;
    public const int OTHER = 2;
    public const int TYPE_8BALL_NUM = 10;
    public const int TYPE_8BALL_START_ID = 0;
    public const int TYPE_9BALL_NUM = 6;
    public const int TYPE_9BALL_START_ID = 100;
    public const int TYPE_8BALL_TOUR_NUM = 3;
    public const int TYPE_8BALL_TOUR_START_ID = 200;
    public const int TYPE_9BALL_TOUR_NUM = 3;
    public const int TYPE_9BALL_TOUR_START_ID = 300;
    public const int TYPE_8BALL_1V1 = 0;
    public const int TYPE_8BALL_TOUR = 1;
    public const int TYPE_9BALL_1V1 = 2;
    public const int LEAVE_ME_ALONE = -1;
    public const int LEAVE_IN_GAME = 1;
    public const int LEAVE_AFTER_GAME = 2;
    public const int LEAVE_LOST_CONNECTION = 3;
    public const int TYPE_ADJUST_ANGLE = 1;
    public const int TYPE_ADJUST_SPIN = 2;
    public const int TYPE_ADJUST_FREEBALL = 3;
    public const int TYPE_ADJUST_STRENGTH = 4;
    public const int TYPE_SHOOT = 5;
    public const int TYPE_CHAT = 6;
    public const int TYPE_ADJUST_SPIN_CLOSE = 7;
    public const int TYPE_ADJUST_FREEBALL_PUT = 8;
    public const int TYPE_WINNER_CLAIM = 9;
    public const int TYPE_QUICK_ADJUST_ANGLE = 10;
    public const int TYPE_FINE_ADJUST_ANGLE = 11;
    public const int TYPE_ADJUST_SPIN_SHOW = 12;
    public const int TYPE_FREEBALL_SHOW = 20;
    public const int TYPE_FREEBALL_SELECTED = 21;
    public const int TYPE_FREEBALL_MOVE = 22;
    public const int TYPE_FREEBALL_SELECTED_CANCLE = 23;
    public const int TYPE_SHOWBREAKOFF_ANIMATION = 24;
    public const int TYPE_NEWROUND_START = 25;
    public const int TYPE_EXCHANGETURN_OK = 26;
    public const int TYPE_ADJUST_ANGLE_3D = 27;
    public const int TYPE_SWITCHVIEW_BTN = 29;
    public const int TYPE_CHANGESTICK = 30;
    public const int TYPE_PICKPOCKET_START = 31;
    public const int TYPE_PICKPOCKET_END = 32;
    public const int TYPE_PUSHOUT = 33;
    public const int TYPE_GIVEUPSTROKE = 34;
    public const int CUE_SENSITIVE_LEVEL_LOW = 0;
    public const int CUE_SENSITIVE_LEVEL_NORMAL = 1;
    public const int CUE_SENSITIVE_LEVEL_HIGH = 2;
    public const int PLAY_GAME_STATE_NONE = 0;
    public const int PLAY_GAME_STATE_PLAYING = 1;
    public const int PLAY_GAME_STATE_WIN = 2;
    public const int PLAY_GAME_STATE_LOSE = 3;
    public const int INSTEAD_CUE_ID = 24;
    public const int VIEW_STATE_2D = 0;
    public const int VIEW_STATE_3D = 1;
    public const int GAME_STATE_NONE = -1;
    public const int GAME_STATE_ONLINE = 0;
    public const int GAME_STATE_OFFLINE = 1;
    public const int GAME_STATE_INGAME = 2;
    public const int PLAYER_TYPE_SELF = 0;
    public const int PLAYER_TYPE_OPPO = 1;

    public static string INVALID_IP = "1234"; // +0x28; original sentinel, not an IP parser default.
    public static float FINAL_ROUNDTIME = 30f; // +0x44
    public static float ROUNDTIME = 30f; // +0x48
    public static float STANDARD_ROUNDTIME = 30f; // +0x4C
    public static float STANDARD_ROUNDTIME_LOC = 30f; // +0x50
    public static float MAXROUNDTIME = 60f; // +0x54
    public static float MINROUNDTIME = 1f; // +0x58
    public static float MAX_STRENGTH = 12000f; // +0x5C
    public static float MAX_SPIN = 1f; // +0x60
    public static float MAX_AIM_LENGTH = 6f; // +0x64
    public static float CUE_STRENGTH_UNIT_BASE = 4f; // +0x68
    public static float CUE_STRENTH_UNIT_MAX = 14f; // +0x6C
    public static float CUE_SPIN_UNIT_BASE = 5f; // +0x70
    public static float CUE_SPIN_UNIT_MAX = 15f; // +0x74
    public static float CUE_AIM_UNIT_BASE = 5f; // +0x78
    public static float CUE_AIM_UNIT_MAX = 15f; // +0x7C
    public static int STATUS_OK = 1; // +0x80
    public static int STATUS_ERROR = 0; // +0x84
    public static int REASON_CUE_BALL = 1; // +0x88
    public static int REASON_NO_BALL_HITTED = 2; // +0x8C
    public static int REASON_NO_BALL_HIT_BLOCK = 3; // +0x90
    public static int REASON_BREAKOFF = 4; // +0x94
    public static int HELP_FINEADJUST = 0; // +0xA0
    public static int HELP_SPIN = 1; // +0xA4
    public static int HELP_OPPOSTATE = 2; // +0xA8
    public static int HELP_CHAT = 3; // +0xAC
    public static int HELP_LEAVE = 4; // +0xB0
    public static float s = 0.5f; // +0xB4, literal0xDEF2F0
    public static float CUE_SENSITIVE_LOW = 0.3f; // +0xB8, literal0xDEF2F4
    public static float CUE_SENSITIVE_NORMAL = 0.5f; // +0xBC, literal0xDEF2F8
    public static float CUE_SENSITIVE_HIGH = 0.6f; // +0xC0, literal0xDEF2FC

    // RVA: 0xDEEA40. Original checks two sentinels only; it does not parse IPv4 or reject null.
    public static bool IsIPValid(string ip) { return ip != "" && ip != INVALID_IP; }

    // RVA: 0xDEEAF0
    public static int GetStrenthRangeId(float strength)
    {
        if (MAX_STRENGTH / 3f > strength) return 1;
        if ((MAX_STRENGTH + MAX_STRENGTH) / 3f > strength) return 2;
        return 3;
    }

    // RVA: 0xDEEBB4. Ratio is divided before multiplication by MAX_STRENGTH.
    public static float GetCueForce(float forceUnit, float strengthUnit)
    {
        return MAX_STRENGTH * ((CUE_STRENGTH_UNIT_BASE + forceUnit) / CUE_STRENTH_UNIT_MAX) * strengthUnit;
    }

    // RVA: 0xDEEC4C
    public static float GetCueSpin(float spinUnit, float sp)
    {
        return MAX_SPIN * ((CUE_SPIN_UNIT_BASE + spinUnit) / CUE_SPIN_UNIT_MAX) * sp;
    }

    // RVA: 0xDEECE4
    public static float GetCueAimLength(float aimUnit)
    {
        return MAX_AIM_LENGTH * ((CUE_AIM_UNIT_BASE + aimUnit) / CUE_AIM_UNIT_MAX);
    }

    // RVA: 0xDEED70. Original bitset0x00010003 and unsigned range type-200<3.
    public static bool IsPurchaseItem(int type)
    {
        return type == 0 || type == 1 || type == 16 || unchecked((uint)(type - 200)) < 3U || type == 100;
    }

    // RVA: 0xDEEDB0. Default1f is the original return for levels outside0..2.
    public static float GetCueSensitive(int level)
    {
        if (level == 2) return CUE_SENSITIVE_HIGH;
        if (level == 1) return CUE_SENSITIVE_NORMAL;
        if (level == 0) return CUE_SENSITIVE_LOW;
        return 1f;
    }

    // RVA: 0xDEEE6C, original tail branch to Object.ctor at0x19CC7CC.
    public Var() { }
}
