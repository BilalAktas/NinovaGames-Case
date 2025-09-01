using System;
using UnityEngine;
using UnityEngine.UI;

namespace NinovaGamesCase.Managers
{

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

        private void UseVacuum()
        {
            _powerUpsParent.SetActive(false);
            OnUsedPowerUp?.Invoke(PowerUp.Vacuum);
        }

        private void OnPowerUpCompleted()
        {
            _powerUpsParent.SetActive(true);
        }
    }   
}
