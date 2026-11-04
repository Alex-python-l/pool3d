using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Pool3D.Offline.Geometry
{
    [Serializable]
    public sealed class TableGeometryConfig
    {
        [SerializeField] private int schemaVersion;
        [SerializeField] private string geometryId;
        [SerializeField] private float longSide, shortSide, ballRadius, metresPerUnit;
        [SerializeField] private float cueStartX, rackX, headStringX, respottedObjectX;
        [SerializeField] private float[] outsideCorners, insideCorners, freeCorners, insideBorder, blockers, holes;
        public float LongSide { get { return longSide; } }
        public float ShortSide { get { return shortSide; } }
        public float BallRadius { get { return ballRadius; } }
        public float MetresPerUnit { get { return metresPerUnit; } }
        public float CueStartX { get { return cueStartX; } }
        public float RackX { get { return rackX; } }
        public float HeadStringX { get { return headStringX; } }
        public float RespottedObjectX { get { return respottedObjectX; } }
        public bool IsLegacy { get { return geometryId == "legacy"; } }
        public int BlockerCount { get { return blockers.Length/4; } }
        internal float[] Outside { get { return outsideCorners; } }
        internal float[] Inside { get { return insideCorners; } }
        internal float[] Free { get { return freeCorners; } }
        internal float[] Border { get { return insideBorder; } }
        internal float[] Blockers { get { return blockers; } }
        internal float[] Holes { get { return holes; } }

        public static TableGeometryConfig Load(string path)
        {
            var value=FromJson(File.ReadAllText(path));
            if(value.IsLegacy)throw new InvalidDataException("Integrated player requires the V3 geometry; legacy is a compatibility fixture only");
            return value;
        }
        public static TableGeometryConfig FromJson(string json)
        {
            var value = JsonUtility.FromJson<TableGeometryConfig>(json);
            if (value == null) throw new InvalidDataException("Table geometry is missing");
            value.Validate();
            return value;
        }
        public TableGeometryConfig Copy() { return FromJson(JsonUtility.ToJson(this)); }
        public void Validate()
        {
            if (schemaVersion != 1 || (geometryId != "legacy" && geometryId != "v3-poolset-2540")) throw new InvalidDataException("Unknown table geometry schema/id");
            var scalars = new[] { longSide, shortSide, ballRadius, metresPerUnit, cueStartX, rackX, headStringX, respottedObjectX };
            if (scalars.Any(v => float.IsNaN(v) || float.IsInfinity(v)) || longSide<=0 || shortSide<=0 || Mathf.Abs(ballRadius-28.575f)>.001f || Mathf.Abs(metresPerUnit-.001f)>.0000001f)
                throw new InvalidDataException("Invalid table units or dimensions");
            if (!IsLegacy && (Mathf.Abs(longSide-2540)>.001f || Mathf.Abs(shortSide-1270)>.001f)) throw new InvalidDataException("Geometry does not match the V3 asset");
            CheckArray(outsideCorners,4); CheckArray(insideCorners,4); CheckArray(freeCorners,4); CheckArray(insideBorder,6);
            if (blockers==null || blockers.Length<40 || blockers.Length%4!=0 || blockers.Length>4096) throw new InvalidDataException("Invalid cushion segments");
            CheckArray(blockers,blockers.Length); CheckArray(holes,18);
            if (Mathf.Abs(insideCorners[0]+longSide/2)>.01f || Mathf.Abs(insideCorners[1]-longSide/2)>.01f || Mathf.Abs(insideCorners[2]-shortSide/2)>.01f || Mathf.Abs(insideCorners[3]+shortSide/2)>.01f)
                throw new InvalidDataException("Inconsistent playing rectangle");
            if (!(outsideCorners[0]<insideCorners[0] && outsideCorners[1]>insideCorners[1] && outsideCorners[2]>insideCorners[2] && outsideCorners[3]<insideCorners[3])) throw new InvalidDataException("Invalid outer envelope");
            if (!(cueStartX>insideCorners[0]+ballRadius && cueStartX<headStringX-ballRadius && rackX>0 && rackX+ballRadius*9<insideCorners[1])) throw new InvalidDataException("Invalid rack/head positions");
            for (int i=0;i<blockers.Length;i+=4)
                if (Vector2.Distance(new Vector2(blockers[i],blockers[i+1]),new Vector2(blockers[i+2],blockers[i+3]))<.001f) throw new InvalidDataException("Zero-length cushion");
            for (int i=0;i<6;i++)
            {
                if (holes[i*3+2]<=ballRadius || holes[i*3+2]>150) throw new InvalidDataException("Invalid capture radius");
                if (Mathf.Abs(holes[i*3])>outsideCorners[1] || Mathf.Abs(holes[i*3+1])>outsideCorners[2]) throw new InvalidDataException("Pocket outside envelope");
                for (int j=0;j<i;j++) if (Vector2.Distance(Pocket(i),Pocket(j))<1) throw new InvalidDataException("Duplicate pocket");
                int mirror = i==0?5:i==1?4:i==2?3:i==3?2:i==4?1:0;
                if (Mathf.Abs(holes[i*3]-holes[mirror*3])>.01f || Mathf.Abs(holes[i*3+1]+holes[mirror*3+1])>.01f || Mathf.Abs(holes[i*3+2]-holes[mirror*3+2])>.01f) throw new InvalidDataException("Pocket ordering/symmetry mismatch");
            }
            if(IsLegacy) {
                var expected=PoolRule.LegacyGeometrySnapshot;
                if(longSide!=expected.longSide||shortSide!=expected.shortSide||cueStartX!=expected.cueStartX||rackX!=expected.rackX||headStringX!=expected.headStringX||respottedObjectX!=expected.respottedObjectX
                    ||!outsideCorners.SequenceEqual(expected.outsideCorners)||!insideCorners.SequenceEqual(expected.insideCorners)||!freeCorners.SequenceEqual(expected.freeCorners)||!insideBorder.SequenceEqual(expected.insideBorder)
                    ||!blockers.SequenceEqual(expected.blockers)||!holes.SequenceEqual(expected.holes))
                    throw new InvalidDataException("Legacy ID must contain the complete original geometry contract");
                return;
            }
            var expectedFree=new[]{insideCorners[0],headStringX,insideCorners[2],insideCorners[3]};
            if(freeCorners.Where((v,i)=>Mathf.Abs(v-expectedFree[i])>.01f).Any())throw new InvalidDataException("Head-string region does not match geometry");
            var expectedOuter=new[]{outsideCorners[0],outsideCorners[2],outsideCorners[1],outsideCorners[2],outsideCorners[1],outsideCorners[2],outsideCorners[1],outsideCorners[3],outsideCorners[1],outsideCorners[3],outsideCorners[0],outsideCorners[3],outsideCorners[0],outsideCorners[3],outsideCorners[0],outsideCorners[2]};
            if(expectedOuter.Where((v,i)=>Mathf.Abs(v-blockers[i])>.01f).Any())throw new InvalidDataException("Outer collision envelope does not match geometry");
            int[] sides=new int[4];float endX=0,mouthX=longSide,endY=0;
            for(int i=16;i<40;i+=4) {
                float x0=blockers[i],y0=blockers[i+1],x1=blockers[i+2],y1=blockers[i+3];
                if(Mathf.Abs(y0-y1)<.01f&&Mathf.Abs(Mathf.Abs(y0)-shortSide/2)<.01f) {
                    sides[y0>0?0:1]++;endX=Mathf.Max(endX,Mathf.Max(Mathf.Abs(x0),Mathf.Abs(x1)));mouthX=Mathf.Min(mouthX,Mathf.Min(Mathf.Abs(x0),Mathf.Abs(x1)));
                }else if(Mathf.Abs(x0-x1)<.01f&&Mathf.Abs(Mathf.Abs(x0)-longSide/2)<.01f) {
                    sides[x0>0?2:3]++;endY=Mathf.Max(endY,Mathf.Max(Mathf.Abs(y0),Mathf.Abs(y1)));
                }else throw new InvalidDataException("Main cushion positions do not match playing area");
            }
            if(!sides.SequenceEqual(new[]{2,2,1,1}))throw new InvalidDataException("Six nose rails must cover the four table sides");
            var expectedBorder=new[]{endY,-endY,-endX,-mouthX,mouthX,endX};
            if(expectedBorder.Where((v,i)=>Mathf.Abs(v-insideBorder[i])>.01f).Any())throw new InvalidDataException("AI pocket intervals do not match the cushion noses");
            for(int i=0;i<6;i++)if(Mathf.Abs(holes[i*3+1])<=shortSide/2||((i!=1&&i!=4)&&Mathf.Abs(holes[i*3])<=longSide/2))throw new InvalidDataException("Pocket centres do not match the playing area");
        }
        private static void CheckArray(float[] values,int length)
        {
            if(values==null || values.Length!=length || values.Any(v=>float.IsNaN(v)||float.IsInfinity(v))) throw new InvalidDataException("Invalid geometry array");
        }
        public Vector2 Pocket(int id) { if(id<0||id>=6)throw new ArgumentOutOfRangeException(nameof(id)); return new Vector2(holes[id*3],holes[id*3+1]); }
        public float PocketRadius(int id) { if(id<0||id>=6)throw new ArgumentOutOfRangeException(nameof(id)); return holes[id*3+2]; }
        public Vector2 ReflectCue(Vector2 cue,int side)
        {
            switch(side) {
                case 0:return new Vector2(-longSide+2*ballRadius-cue.x,cue.y);
                case 1:return new Vector2(cue.x,-shortSide+2*ballRadius-cue.y);
                case 2:return new Vector2(longSide-2*ballRadius-cue.x,cue.y);
                case 3:return new Vector2(cue.x,shortSide-2*ballRadius-cue.y);
                default:throw new ArgumentOutOfRangeException(nameof(side));
            }
        }
        public float[] BuildAiTargets()
        {
            var result=new float[36];
            for(int i=0;i<6;i++) {
                Vector2 centre=Pocket(i),tangent=new Vector2(-centre.y,centre.x).normalized*ballRadius*.5f;
                for(int j=0;j<3;j++){Vector2 target=centre+tangent*(j==0?0:j==1?-1:1);result[i*6+j*2]=target.x;result[i*6+j*2+1]=target.y;}
            }
            return result;
        }
        public float[] CopyBlockers(bool includeOuter=false) { return includeOuter?(float[])blockers.Clone():blockers.Skip(16).ToArray(); }
        public float[] BuildQuickBlockers()
        {
            float x=longSide/2,y=shortSide/2,cornerY=insideBorder[0],cornerX=insideBorder[5];
            return new[]{-x,-cornerY,-x,cornerY,-cornerX,y,cornerX,y,x,cornerY,x,-cornerY,cornerX,-y,-cornerX,-y};
        }
        private static Vector2 Nearest(Vector2 p,Vector2 a,Vector2 b) { Vector2 d=b-a;return a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude); }
        public bool CanPlace(Vector2 centre,float radius)
        {
            if(centre.x-radius<insideCorners[0]||centre.x+radius>insideCorners[1]||centre.y+radius>insideCorners[2]||centre.y-radius<insideCorners[3])return false;
            for(int i=0;i<6;i++)if(Vector2.Distance(centre,Pocket(i))<radius+PocketRadius(i))return false;
            for(int i=16;i<blockers.Length;i+=4)if(Vector2.Distance(centre,Nearest(centre,new Vector2(blockers[i],blockers[i+1]),new Vector2(blockers[i+2],blockers[i+3])))<radius-.001f)return false;
            return true;
        }
        public Vector2 Constrain(Vector2 centre,float radius)
        {
            // Six nose segments only: pocket throats and sinking balls remain open.
            for(int i=16;i<40;i+=4) {
                Vector2 a=new Vector2(blockers[i],blockers[i+1]),b=new Vector2(blockers[i+2],blockers[i+3]),d=b-a;
                float t=Vector2.Dot(centre-a,d)/d.sqrMagnitude;
                if(t<=0||t>=1)continue;
                Vector2 normal=new Vector2(-d.y,d.x).normalized;
                if(Vector2.Dot(normal,-(a+b))<0)normal=-normal;
                float gap=Vector2.Dot(centre-(a+d*t),normal);
                if(gap<radius && gap>-radius)centre+=normal*(radius-gap);
            }
            return centre;
        }
        internal static TableGeometryConfig Legacy(float[] originalBlockers,float[] originalHoles)
        {
            return new TableGeometryConfig {schemaVersion=1,geometryId="legacy",longSide=PoolRule.INNER_TABLE_WIDTH,shortSide=PoolRule.INNER_TABLE_HEIGHT,ballRadius=PoolRule.BALL_RADIUS,metresPerUnit=.001f,
                cueStartX=-705,rackX=505,headStringX=-495.3f,respottedObjectX=705,
                outsideCorners=(float[])PoolRule.outsideTableCorners.Clone(),insideCorners=(float[])PoolRule.insideTableCorners.Clone(),freeCorners=(float[])PoolRule.freeAreaCorners.Clone(),insideBorder=(float[])PoolRule.insideBorder.Clone(),
                blockers=(float[])originalBlockers.Clone(),holes=(float[])originalHoles.Clone()};
        }
    }
}
