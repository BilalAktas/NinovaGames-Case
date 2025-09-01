using NinovaGamesCase.Core;
using UnityEngine;

namespace NinovaGamesCase.Helpers
{
    public class Helpers
    {
        public static Vector3 Vector2IntToVector3(Vector2Int pos) =>
            new Vector3(pos.x, 0, pos.y);

        public static Vector2Int Vector3ToVector2Int(Vector3 pos) =>
            new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));

        public static Vector3 Vector2ToVector3(Vector2 pos) =>
            new Vector3(pos.x, 0, pos.y);

        public static Vector3 LevelDoorDirectionToVector(LevelDoorDirection dir)
        {
            switch (dir)
            {
                case LevelDoorDirection.Up: return Vector3.forward; break;
                case LevelDoorDirection.Down: return Vector3.back; break;
                case LevelDoorDirection.Right: return Vector3.right; break;
                case LevelDoorDirection.Left: return Vector3.left; break;
            }

            return Vector3.forward;
        }

        public static (Vector3, Vector3) GetSetupPosAndScale(Vector3 dir, Vector3 pos, Vector3 scale, int length)
        {
            if (dir == Vector3.forward || dir == Vector3.back)
            {
                pos.x += .5f * (length - 1);
                scale = new Vector3(length, 1, 1);
            }
            else
            {
                pos.z += .5f * (length - 1);
                scale = new Vector3(1, 1, length);
            }
            
            return (pos,scale);
        }
        
        public static Vector3 GetBoundsEdgePoint(BoxCollider box, Vector3 direction)
        {
            direction.Normalize();

            var extents = box.bounds.extents;
            var absDir = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
            
            var scale = 1f / Mathf.Max(
                absDir.x / extents.x,
                absDir.y / extents.y,
                absDir.z / extents.z
            );

            var offset = direction * scale;
            return box.bounds.center + offset;
        }
        
        public static Color DarkenColor(Color color)
        {
            var darkenFactor = .1f;
            return new Color(
                Mathf.Clamp01(color.r * darkenFactor),
                Mathf.Clamp01(color.g * darkenFactor),
                Mathf.Clamp01(color.b * darkenFactor),
                color.a
            );
        }

    }    
}


