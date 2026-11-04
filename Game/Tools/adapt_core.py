"""One-time patch of the isolated copy. Never targets recovered/source projects."""
from pathlib import Path

PROJECT = Path(__file__).resolve().parent.parent / 'UnityProject'
CORE = PROJECT / 'Assets/OriginalCore'


def edit(path, transform):
    text = path.read_text(encoding='utf-8')
    changed = transform(text)
    if changed == text: raise AssertionError(f'No changes: {path}')
    path.write_text(changed, encoding='utf-8')


def main():
    mappings = {'insideTableCorners':'GInside','outsideTableCorners':'GOutside','freeAreaCorners':'GFree','insideBorder':'GBorder','holePosition':'GHoles','blockersPosition':'GBlockers'}
    for name in ('PoolRule.Core.cs','VSRule.Bounds.cs','VSRule.Collisions.cs'):
        def rewrite(text):
            for old,new in mappings.items(): text=text.replace(old,new)
            if name=='PoolRule.Core.cs':
                text=text.replace('return TABLE_WIDTH;', 'return Geometry.IsLegacy?TABLE_WIDTH:Geometry.Outside[1]-Geometry.Outside[0];').replace('return TABLE_HEIGHT;', 'return Geometry.IsLegacy?TABLE_HEIGHT:Geometry.Outside[2]-Geometry.Outside[3];')
                text=text.replace('float radius = BALL_RADIUS;','float radius = BALL_RADIUS;\n        if(!Geometry.IsLegacy && !Geometry.CanPlace(new Vector2(x,y),radius)) return false;')
            if name=='VSRule.Bounds.cs':
                text=text.replace('float radius = BALL_RADIUS;', 'if(!Geometry.IsLegacy) { if(ball.inHole==null) { Vector2 p=Geometry.Constrain(new Vector2(ball.x,ball.y),ball.scale);ball.x=p.x;ball.y=p.y; } return; }\n        float radius = BALL_RADIUS;')
                text='using UnityEngine;\n'+text
            return text
        edit(CORE/'Rules'/name,rewrite)
    for name in ('EightBallVsRule.cs','NineBallVsRule.cs','VSRule.Core.cs'):
        edit(CORE/'Rules'/name,lambda t:t.replace('= -705f;', '= Geometry.CueStartX;').replace('baseX = 505f +','baseX = Geometry.RackX +').replace(', -705f, 0f, true)',', Geometry.CueStartX, 0f, true)').replace(', 705f, 0f, false)',', Geometry.RespottedObjectX, 0f, false)'))
    edit(CORE/'Rules/PoolRule.Placement.cs',lambda t:t.replace('candidate <= 1217f - BALL_RADIUS && ball.x >= BALL_RADIUS + -1225f', 'candidate <= (Geometry.IsLegacy?1217f:Geometry.Inside[1]) - BALL_RADIUS && ball.x >= BALL_RADIUS + (Geometry.IsLegacy?-1225f:Geometry.Inside[0])'))
    def table(text):
        text=text.replace('if (blockers == null)','if (blockers == null || blockers.Length != rule.getBlockerCount())').replace('if (holes == null)','if (holes == null || holes.Length != rule.getHoleCount())').replace('if (balls == null)','if (balls == null || balls.Length != rule.getBallCount())')
        return text.replace('rule.resetBalls(balls, holes);','EnsureCollisionCapacity();\n        rule.resetBalls(balls, holes);')
    edit(CORE/'Physics/PoolTable.cs',table)
    def ai(text):
        for old,new in (('private static float[]','private float[]'),('public static float[]','public float[]'),('private static float halfR','private float halfR'),('static PoolAIBase()','private void InitializeGeometry()')):text=text.replace(old,new)
        text=text.replace('m_AIStateMgr = new AIStateMgr();','m_AIStateMgr = new AIStateMgr();\n        InitializeGeometry();')
        marker='    // RVA: 0x921320'
        before,after=text.split(marker,1)
        pos=before.rfind('    }')
        before=before[:pos]+'''        if(!Geometry.IsLegacy) {
            quickblockersPosition=Geometry.BuildQuickBlockers();
            quickholesPosition=Geometry.BuildAiTargets();
            blockersPosition=Geometry.CopyBlockers();
        }
'''+before[pos:]
        text=before+marker+after
        return text.replace('    private int blocker;', '    private Pool3D.Offline.Geometry.TableGeometryConfig Geometry {get{return m_PoolCtr.M_PoolRule.Geometry;}}\n    private int blocker;')
    edit(CORE/'AI/PoolAIBase.Decision.cs',ai)
    def reflection(text):
        start='            float reflectedX = cueX, reflectedY = cueY;'
        for _ in range(2):
            index=text.index(start)
            end=text.index('\n            }',index)+len('\n            }')
            text=text[:index]+'''            Vector2 reflected=Geometry.ReflectCue(new Vector2(cueX,cueY),side);
            float reflectedX=reflected.x,reflectedY=reflected.y;'''+text[end:]
        return text
    edit(CORE/'AI/PoolAIBase.Geometry.cs',reflection)
    edit(CORE/'AI/PoolAIBase.FreeBall.cs',lambda t:t.replace('PoolRule.INNER_TABLE_WIDTH','Geometry.LongSide').replace('PoolRule.INNER_TABLE_HEIGHT','Geometry.ShortSide').replace('-(diameter + 495.3f)','Geometry.HeadStringX - diameter'))
    def host(text):
        text=text.replace('private int ruleBits;', 'private Pool3D.Offline.Geometry.TableGeometryConfig tableGeometry;\n        private int ruleBits;')
        text=text.replace('UseEnhancedSpin = spinSettings.enabledByDefault;', 'UseEnhancedSpin = spinSettings.enabledByDefault;\n                tableGeometry = Pool3D.Offline.Geometry.TableGeometryConfig.Load(System.IO.Path.Combine(Application.streamingAssetsPath,"TableGeometry.json"));')
        text=text.replace('M_PoolRule.SetListener(this);','M_PoolRule.ConfigureGeometry(tableGeometry);\n            M_PoolRule.SetListener(this);')
        return text
    edit(PROJECT/'Assets/OfflineRuntime/OfflineMatchController.cs',host)
    print('Isolated core geometry bindings applied; legacy globals retained')


if __name__=='__main__':main()
