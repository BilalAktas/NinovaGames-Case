using System;
using UnityEngine;
using UnityEngine.UI;

namespace NinovaGamesCase.Managers
{
    /// <summary>
    /// Manages the UI related to power-ups and handles user interactions with power-up buttons.
    /// </summary>
    public class PowerUpUIManager : MonoBehaviour
    {
        public static event Action<PowerUp> OnUsedPowerUp;

        [SerializeField] private GameObject _powerUpsParent;
        
        [Header("Vacuum")] 
        [SerializeField] private Button _vacuumPowerUpButton;
        
        private void Start()
        {
            _vacuumPowerUpButton.onClick.AddListener(UseVacuum);
            PowerUpActionManager.OnPowerUpCompleted += OnPowerUpCompleted;
        }

        private void OnDestroy()
        {
            _vacuumPowerUpButton.onClick.RemoveAllListeners();
            PowerUpActionManager.OnPowerUpCompleted -= OnPowerUpCompleted;
        }

        /// <summary>
        /// Called when the vacuum power-up button is clicked.
        /// Disables the power-ups UI and invokes the power-up usage event.
        /// </summary>
        private void UseVacuum()
        {
            _powerUpsParent.SetActive(false);
            OnUsedPowerUp?.Invoke(PowerUp.Vacuum);
        }

        /// <summary>
        /// Called when a power-up action is completed.
        /// Re-enables the power-ups UI.
        /// </summary>
        private void OnPowerUpCompleted()
        {
            _powerUpsParent.SetActive(true);
        }
    }   
}
