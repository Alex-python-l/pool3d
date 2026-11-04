// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Recovered from original IL2CPP metadata and ARM; see module_restore/rules/source_map.json.
using UnityEngine;

public interface PoolAI
{
    void think(RoundState roundstate, PoolBall[] balls, bool specTargetPocket);
    float getAngle();
    float getStrength();
    void thinkFreeball(RoundState roundstate, PoolBall[] balls, bool specTargetPocket);
    float getTarX();
    float getTarY();
}
