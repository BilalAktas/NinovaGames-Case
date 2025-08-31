using UnityEngine;

namespace NinovaGamesCase.Core
{
    public class Wall : MonoBehaviour
    {
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


