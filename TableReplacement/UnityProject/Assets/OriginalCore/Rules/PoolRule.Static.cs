// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Original native static initialization; original arrays, not recomputed table geometry.
public partial class PoolRule
{
    // RVA: 0x92C158
    static PoolRule()
    {
        rand = new System.Random();
        REAL_SIMULATOR_SCALE = 0.1f;
        BALL_S = 1f;
        isPractice = false;
        V2W = 2.00510168f;
        BALL_BALL_FORCE = 500f;
        SPIN_DECE_RATE = 10f;
        BALL_TABLE_FORCE = 1000f;
        ROLLING_FORCE = 70f;
        SLIDING_FORCE = 1000f;
        INNER_TABLE_HEIGHT = 990.599976f;
        INNER_TABLE_WIDTH = 1981.19995f;
        BALL_RADIUS = 28.5750008f;
        TABLE_HEIGHT = 1530f;
        TABLE_WIDTH = 2810f;
        REAL_SCALE = 1000f;
        // outsideTableCorners: metadata field13400 @0x3f6f58 SHA256 68e98d51c3ba5222ab701df746f3fd81790b4eb93c8c26fb78134d076c133ae5
        outsideTableCorners = new float[] { -1085f, 1085f, 595f, -595f };
        // insideTableCorners: metadata field13437 @0x40dc08 SHA256 a250ed6eb4da0e5bbbeeadd468833b79c5ba303c5699f336b87696f3187303fc
        insideTableCorners = new float[] { -990.599976f, 990.599976f, 495.299988f, -495.299988f };
        // freeAreaCorners: metadata field13392 @0x3f6d48 SHA256 5a40e4461ac0e16d245800b7656e98ec44b3a0c0e2ff736a7bff9cffe79412c7
        freeAreaCorners = new float[] { -990.599976f, -495.299988f, 495.299988f, -495.299988f };
        // insideBorder: metadata field13420 @0x4021c8 SHA256 8a7166af4b7c6f6f7b0e83322b516b55d14be2fd72a0a23bc32a054072f91f31
        insideBorder = new float[] { 432.200012f, -432.200012f, -927.5f, -75.0999985f, 75.0999985f, 927.5f };
        // blockersPosition_small: metadata field13428 @0x40d940 SHA256 9500bbe1af3808f64b7d7274c5b75ea89315af3f5a4c3ad61919e434e73462a2
        blockersPosition_small = new float[] { -1085f, 595f, 1085f, 595f, 1085f, 595f, 1085f, -595f, 1085f, -595f, -1085f, -595f, -1085f, -595f, -1085f, 595f, -927.5f, 495.299988f, -75.0999985f, 495.299988f, 75.0999985f, 495.299988f, 927.5f, 495.299988f, 990.599976f, 432.200012f, 990.599976f, -432.200012f, 927.5f, -495.299988f, 75.0999985f, -495.299988f, -75.0999985f, -495.299988f, -927.5f, -495.299988f, -990.599976f, -432.200012f, -990.599976f, 432.200012f, -990.599976f, 432.200012f, -1041.09998f, 472.299988f, -927.5f, 495.299988f, -967.599976f, 545.799988f, -75.0999985f, 495.299988f, -42.5f, 547.900024f, 75.0999985f, 495.299988f, 42.5f, 547.900024f, 927.5f, 495.299988f, 967.599976f, 545.799988f, 990.599976f, 432.200012f, 1041.09998f, 472.299988f, 990.599976f, -432.200012f, 1041.09998f, -472.299988f, 927.5f, -495.299988f, 967.599976f, -545.799988f, 75.0999985f, -495.299988f, 42.5f, -547.900024f, -75.0999985f, -495.299988f, -42.5f, -547.900024f, -927.5f, -495.299988f, -967.599976f, -545.799988f, -990.599976f, -432.200012f, -1041.09998f, -472.299988f };
        // holePosition_small: metadata field13467 @0x40df98 SHA256 d33e43c7113604fb7d8c2d39ac920e3ad6f54b990b671cf47f2c60977dafa407
        holePosition_small = new float[] { -1025.59998f, 530.299988f, 60f, 0f, 537.799988f, 42.5f, 1025.59998f, 530.299988f, 60f, 1025.59998f, -530.299988f, 60f, 0f, -537.799988f, 42.5f, -1025.59998f, -530.299988f, 60f };
        // blockersPosition_big: metadata field13398 @0x3f6dd8 SHA256 68a83885a7345484c973e9d140de427104175605e7e3bad14bd41dc5cb028438
        blockersPosition_big = new float[] { -1085f, 595f, 1085f, 595f, 1085f, 595f, 1085f, -595f, 1085f, -595f, -1085f, -595f, -1085f, -595f, -1085f, 595f, -917.099976f, 495.299988f, -75.0999985f, 495.299988f, 75.0999985f, 495.299988f, 917.099976f, 495.299988f, 990.599976f, 421.799988f, 990.599976f, -421.799988f, 917.099976f, -495.299988f, 75.0999985f, -495.299988f, -75.0999985f, -495.299988f, -917.099976f, -495.299988f, -990.599976f, -421.799988f, -990.599976f, 421.799988f, -990.599976f, 421.799988f, -1041.09998f, 472.299988f, -917.099976f, 495.299988f, -967.599976f, 545.799988f, -75.0999985f, 495.299988f, -44.7000008f, 547.900024f, 75.0999985f, 495.299988f, 44.7000008f, 547.900024f, 917.099976f, 495.299988f, 967.599976f, 545.799988f, 990.599976f, 421.799988f, 1041.09998f, 472.299988f, 990.599976f, -421.799988f, 1041.09998f, -472.299988f, 917.099976f, -495.299988f, 967.599976f, -545.799988f, 75.0999985f, -495.299988f, 44.7000008f, -547.900024f, -75.0999985f, -495.299988f, -44.7000008f, -547.900024f, -917.099976f, -495.299988f, -967.599976f, -545.799988f, -990.599976f, -421.799988f, -1041.09998f, -472.299988f };
        // holePosition_big: metadata field13482 @0x40e210 SHA256 e7f84e866e4fdece5ec32d5e3e4cf5a2d7a610ae6b38a09b12866c082bd56816
        holePosition_big = new float[] { -1025.59998f, 530.299988f, 60f, 0f, 537.799988f, 44.7000008f, 1025.59998f, 530.299988f, 60f, 1025.59998f, -530.299988f, 60f, 0f, -537.799988f, 44.7000008f, -1025.59998f, -530.299988f, 60f };
        // freeball_Aera: metadata field13458 @0x40de70 SHA256 bfd101f9814973407b7d6c8236e402d38a9e2c472cbd0069c6d6b81f8cef3c22
        freeball_Aera = new float[] { -990.599976f, -432.200012f, -990.599976f, 432.200012f, -927.5f, 495.299988f, 927.5f, 495.299988f, 990.599976f, 432.200012f, 990.599976f, -432.200012f, 927.5f, -495.299988f, -927.5f, -495.299988f, -495.299988f, 495.299988f, -495.299988f, -495.299988f };
        blockersPosition = blockersPosition_big;
        holePosition = holePosition_big;
    }

    // RVA: 0x92C0A4
    public static void SetBlockerAndHolePosition(bool useSmall)
    {
        if (useSmall)
        {
            blockersPosition = blockersPosition_small;
            holePosition = holePosition_small;
        }
        else
        {
            blockersPosition = blockersPosition_big;
            holePosition = holePosition_big;
        }
    }
}
