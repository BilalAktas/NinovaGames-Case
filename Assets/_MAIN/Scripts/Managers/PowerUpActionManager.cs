using System;
using DG.Tweening;
using NinovaGamesCase.Core;
using NinovaGamesCase.Helpers;
using UnityEngine;
using Grid = NinovaGamesCase.Core.Grid;

namespace NinovaGamesCase.Managers
{
    /// <summary>
    /// Enum representing available power-up types.
    /// </summary>
    public enum PowerUp {None, Vacuum}
    
    /// <summary>
    /// Manages power-up usage and their corresponding actions in the game.
    /// </summary>
    public class PowerUpActionManager : Singleton<PowerUpActionManager>
    {
        public static event Action OnPowerUpCompleted;
        
        public PowerUp PowerUp { get; private set; }
        public bool CanUse { get; private set; }
        
        [Header("Vacuum")] 
        [SerializeField] private GameObject _vacuum;
        [SerializeField] private Transform _vacuumBlockPlace;
        private static readonly int _ON = Animator.StringToHash("On");
        private static readonly int _OFF = Animator.StringToHash("Off");
        private Animator _vacuumAnim;
        
        private void Start()
        {
            CanUse = true;
            _vacuumAnim = _vacuum.GetComponent<Animator>();
            
            PowerUpUIManager.OnUsedPowerUp += OnUsedPowerUp;
            InputDragger.OnPowerUpUsed += PowerUpAction;
        }

        private void OnDestroy()
        {
            PowerUpUIManager.OnUsedPowerUp -= OnUsedPowerUp;
            InputDragger.OnPowerUpUsed -= PowerUpAction;
        }

        /// <summary>
        /// Called when a power-up is selected from the UI.
        /// Triggers related animations.
        /// </summary>
        /// <param name="powerUp">Selected power-up type.</param>
        private void OnUsedPowerUp(PowerUp powerUp)
        {
            PowerUp = powerUp;
            switch (PowerUp)
            {
                case PowerUp.Vacuum:
                    _vacuumAnim.SetTrigger(_ON);
                    break;
            }
        }

        /// <summary>
        /// Executes the action associated with the currently selected power-up.
        /// </summary>
        /// <param name="selectedBlock">The block targeted by the power-up.</param>
        private void PowerUpAction(Block selectedBlock)
        {
            Debug.Log(selectedBlock.BlockData.ColorData.MatchColor);
            switch (PowerUp)
            {
                case PowerUp.Vacuum:

                    CanUse = false;
                    
                    var allBlockByColor = Grid.Instance.GetAllBlockByColor(selectedBlock.BlockData.ColorData.MatchColor);
                    foreach (var block in allBlockByColor)
                    {
                        block.OnVacuumPowerUp(_vacuumBlockPlace.position);
                    }

                    DOVirtual.DelayedCall(.6f, () =>
                    {
                        MatchManager.OnMatchedCompleted?.Invoke();
                        _vacuumAnim.SetTrigger(_OFF);

                        DOVirtual.DelayedCall(.6f, () =>
                        {
                            PowerUp = PowerUp.None;
                            
                            CanUse = true;
                            OnPowerUpCompleted?.Invoke();
                        });
                   
                    });
                    
                    break;
            }
        }
    } 
}


