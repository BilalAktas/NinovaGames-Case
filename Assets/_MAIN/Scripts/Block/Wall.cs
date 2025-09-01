using UnityEngine;

namespace NinovaGamesCase.Core
{
    /// <summary>
    /// Represents a static wall element placed on the grid, defined by its direction, position, and length.
    /// </summary>
    public class Wall : MonoBehaviour
    {
        /// <summary>
        /// Initializes the wall's position and scale based on grid data and direction.
        /// </summary>
        /// <param name="data">The wall configuration data including direction, grid position, and length.</param>
        public void Initialize(LevelWallData data)
        {
            var dir = Helpers.Helpers.LevelDoorDirectionToVector(data.Direction);
            var position = Grid.Instance.GetGrid()[data.GridPosition.x, data.GridPosition.y].WorldPosition + dir * 1f;
            position.y = 0;

            var mesh = transform.GetChild(0);
            var s = Helpers.Helpers.GetSetupPosAndScale(dir, position, mesh.localScale, data.Length);

            transform.position = s.Item1;
            mesh.localScale = s.Item2;
        }
    }
}