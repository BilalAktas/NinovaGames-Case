using NinovaGamesCase.Core;
using UnityEngine;

namespace NinovaGamesCase.Helpers
{
    /// <summary>
    /// Provides utility functions for vector conversions, positioning, scaling, and color manipulation.
    /// </summary>
    public class Helpers
    {
        /// <summary>
        /// Converts a Vector2Int to a Vector3 with y set to zero.
        /// </summary>
        /// <param name="pos">Input Vector2Int.</param>
        /// <returns>Converted Vector3.</returns>
        public static Vector3 Vector2IntToVector3(Vector2Int pos) =>
            new Vector3(pos.x, 0, pos.y);

        /// <summary>
        /// Converts a Vector3 to a Vector2Int by flooring x and z components.
        /// </summary>
        /// <param name="pos">Input Vector3.</param>
        /// <returns>Converted Vector2Int.</returns>
        public static Vector2Int Vector3ToVector2Int(Vector3 pos) =>
            new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));

        /// <summary>
        /// Converts a Vector2 to a Vector3 with y set to zero.
        /// </summary>
        /// <param name="pos">Input Vector2.</param>
        /// <returns>Converted Vector3.</returns>
        public static Vector3 Vector2ToVector3(Vector2 pos) =>
            new Vector3(pos.x, 0, pos.y);

        /// <summary>
        /// Converts a LevelDoorDirection enum value to a normalized Vector3 direction.
        /// </summary>
        /// <param name="dir">The door direction enum.</param>
        /// <returns>Corresponding Vector3 direction.</returns>
        public static Vector3 LevelDoorDirectionToVector(LevelDoorDirection dir) => dir switch
        {
            LevelDoorDirection.Up => Vector3.forward,
            LevelDoorDirection.Down => Vector3.back,
            LevelDoorDirection.Right => Vector3.right,
            LevelDoorDirection.Left => Vector3.left,
            _ => Vector3.forward
        };

        /// <summary>
        /// Calculates the position offset and scale based on direction and length for setup.
        /// </summary>
        /// <param name="dir">Direction vector.</param>
        /// <param name="pos">Base position.</param>
        /// <param name="scale">Original scale.</param>
        /// <param name="length">Length along the direction.</param>
        /// <returns>Tuple of adjusted position and scale.</returns>
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

            return (pos, scale);
        }

        /// <summary>
        /// Calculates the point on the bounds edge of a BoxCollider in a given direction.
        /// </summary>
        /// <param name="box">BoxCollider to calculate from.</param>
        /// <param name="direction">Direction vector.</param>
        /// <returns>Point on the bounds edge in the given direction.</returns>
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

        /// <summary>
        /// Returns a darker version of the input color by applying a fixed darkening factor.
        /// </summary>
        /// <param name="color">Original color.</param>
        /// <returns>Darkened color.</returns>
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