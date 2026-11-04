// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Recovered from original IL2CPP metadata and ARM; see module_restore/rules/source_map.json.
public class RoundState
{
    public int state; // 0x8
    public int currentPlayer; // 0xC
    public int winner; // 0x10
    public int[] playerScore; // 0x14
    public int restTime; // 0x18
    public bool counttime; // 0x1C
    public int totalGameTime; // 0x20
    public int roundTime; // 0x24
    public int roundCount; // 0x28
    public int matchCount; // 0x2C
    public int potInOneBreak; // 0x30
    public int lastFouledPlayer; // 0x34
    public int lastFouledReason; // 0x38
    public bool isFreeBall; // 0x3C
    public bool isSwitchPlayer; // 0x3D
    private bool isSelfRound; // 0x3E
    public bool needResetBalls; // 0x3F
    public int combo; // 0x40
    public int nInOne; // 0x44
    public bool combineShot; // 0x48
    public bool bankShot; // 0x49

    // RVA: 0x92C584
    public bool IsSelfRound { get { return currentPlayer == 0; } }

    // RVA: 0x92C024
    public RoundState()
    {
        lastFouledPlayer = -1;
        lastFouledReason = -1;
    }
}
