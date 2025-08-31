using System;
using NinovaGamesCase.Core;
using NinovaGamesCase.Helpers;
using UnityEngine;

namespace NinovaGamesCase.Managers
{
    public class MatchManager : MonoBehaviour
    {
        public static event Action OnMatchedCompleted;
        public static event Action OnLevelCompleted;
        private int _matchedAmount;
        
        private void Start()
        {
            Block.OnMatched += OnMatched;
            LevelManager.OnLevelLoaded += OnLevelLoaded;
        }

        private void OnDestroy()
        {
            Block.OnMatched -= OnMatched;
            LevelManager.OnLevelLoaded -= OnLevelLoaded;
        }
        
        private void OnLevelLoaded()
        {
            _matchedAmount = 0;
        }

        private void OnMatched(Door door, Block block)
        {
            _matchedAmount++;
            block.OnMatchedMove(door.WorldPosition + Helpers.Helpers.LevelDoorDirectionToVector(door.LevelDoorData.Direction) * 5f);
            OnMatchedCompleted?.Invoke();
            var doorParticle = ObjectPool.Instance.GetFromPool("DoorParticle");
            doorParticle.transform.position = door.transform.position;
            switch (door.LevelDoorData.Direction)
            {
                case LevelDoorDirection.Up:
                    doorParticle.transform.rotation = Quaternion.identity;
                    break;
                case LevelDoorDirection.Down:
                    doorParticle.transform.rotation = Quaternion.Euler(0,180,0);
                    break;
                case LevelDoorDirection.Left:
                    doorParticle.transform.rotation = Quaternion.Euler(0,270,0);
                    break;
                case LevelDoorDirection.Right:
                    doorParticle.transform.rotation = Quaternion.Euler(0,90,0);
                    break;
            }

            var p = doorParticle.GetComponent<ParticleSystemRenderer>();
            p.material.SetColor("_BaseColor", door.LevelDoorData.ColorData.ColorMaterial.GetColor("_Color"));
            
            
            doorParticle.SetActive(true);
            
            door.OnMatchedAnim();
            if (_matchedAmount>=LevelManager.CurrentConfig.LevelBlockData.Length)
            {
                _matchedAmount = 0;
                OnLevelCompleted?.Invoke();
                Debug.Log("LevelCompleted");
            }
        }
    }   
}
