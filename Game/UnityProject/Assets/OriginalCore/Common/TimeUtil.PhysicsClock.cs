// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using System;

public partial class TimeUtil
{
    // Original static field +0x30, initialized at RVA 0xDE8C40.
    // IEEE754 low/high words 0xeb1c432d / 0x3f1a36e2.
    private static double Tick2Mills = 0.0001;

    // RVA 0xDE7FA0. The original is DateTime.Now.Ticks scaled to milliseconds,
    // not a replacement Unix/UTC timestamp. Conversion is the original unchecked cast.
    public static long CurrentTimeMills()
    {
        return unchecked((long)(Tick2Mills * (double)DateTime.Now.Ticks));
    }
}
