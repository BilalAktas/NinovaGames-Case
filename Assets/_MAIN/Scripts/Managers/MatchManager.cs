using System;
using NinovaGamesCase.Core;
using NinovaGamesCase.Helpers;
using UnityEngine;

namespace NinovaGamesCase.Managers
{
    /// <summary>
    /// Manages the matching logic between blocks and doors, triggering events on matches and level completion.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static Action OnMatchedCompleted;
        public static event Action OnLevelCompleted;
        private int _matchedAmount;

        private MaterialPropertyBlock _mpb;
        
        private void Start()
        {
            Block.OnMatched += OnMatched;
            LevelManager.OnLevelLoaded += OnLevelLoaded;

            _mpb = new MaterialPropertyBlock();
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

        /// <summary>
        /// Called when a block matches a door; moves the block, plays particle effects, triggers animations and checks for level completion.
        /// </summary>
        /// <param name="door">The door involved in the match.</param>
        /// <param name="block">The block involved in the match.</param>
        private void OnMatched(Door door, Block block)
        {
            _matchedAmount++;
            if (door != null && block!=null)
            {
                block.OnMatchedMove(door.WorldPosition + Helpers.Helpers.LevelDoorDirectionToVector(door.LevelDoorData.Direction) * 5f);
                OnMatchedCompleted?.Invoke();
                var doorParticle = ObjectPool.Instance.GetFromPool("DoorParticle");
                doorParticle.transform.position = door.transform.position;
                doorParticle.transform.rotation = GetRotationForDirection(door.LevelDoorData.Direction);

                var p = doorParticle.GetComponent<ParticleSystemRenderer>();
                p.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", door.LevelDoorData.ColorData.ColorMaterial.GetColor("_Color"));
                p.SetPropertyBlock(_mpb);
            
            
                doorParticle.SetActive(true);
            
                door.OnMatchedAnim();
             
            }
            
            if (_matchedAmount>=LevelManager.CurrentConfig.LevelBlockData.Length)
            {
                _matchedAmount = 0;
                OnLevelCompleted?.Invoke();
                Debug.Log("LevelCompleted");
            }
        }
        
        private Quaternion GetRotationForDirection(LevelDoorDirection dir)
        {
            return dir switch
            {
                LevelDoorDirection.Up => Quaternion.identity,
                LevelDoorDirection.Down => Quaternion.Euler(0, 180, 0),
                LevelDoorDirection.Left => Quaternion.Euler(0, 270, 0),
                LevelDoorDirection.Right => Quaternion.Euler(0, 90, 0),
                _ => Quaternion.identity
            };
        }
    }   
}
