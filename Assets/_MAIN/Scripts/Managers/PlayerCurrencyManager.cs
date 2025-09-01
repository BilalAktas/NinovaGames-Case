using System;
using TMPro;
using UnityEngine;


namespace NinovaGamesCase.Managers
{
    /// <summary>
    /// Manages the player's coin currency, including adding, spending, and updating the UI display.
    /// </summary>
    public class PlayerCurrencyManager : MonoBehaviour
    {
        public static event Action OnCoinAmountChanged;
        [SerializeField] private TextMeshProUGUI _currencyCoinText;
        
        private void Start()
        {
            UpdateCoin();
            
            OnCoinAmountChanged += UpdateCoin;
        }
        
        private void OnDestroy()
        {
            OnCoinAmountChanged -= UpdateCoin;
        }
        
        /// <summary>
        /// Checks if the player has enough coins.
        /// </summary>
        /// <param name="amount">Amount to check.</param>
        /// <returns>True if enough coins, false otherwise.</returns>
        public static bool IsCoinEnough(int amount)
        {
            return GetCoin() >= amount;
        }

        /// <summary>
        /// Retrieves the current coin amount from PlayerPrefs.
        /// </summary>
        /// <returns>Current coin amount.</returns>
        private static int GetCoin() => PlayerPrefs.GetInt("PlayerCoin", 0);
    
        /// <summary>
        /// Adds coins to the player's total and triggers the update event.
        /// </summary>
        /// <param name="amount">Amount of coins to add.</param>
        public static void AddCoin(int amount)
        {
            PlayerPrefs.SetInt("PlayerCoin", GetCoin() + amount);
            OnCoinAmountChanged?.Invoke();
        }

        /// <summary>
        /// Sets the player's coin amount directly.
        /// </summary>
        /// <param name="amount">New coin amount.</param>
        private static void SetCoin(int amount) => PlayerPrefs.SetInt("PlayerCoin", amount);

        /// <summary>
        /// Spends coins from the player's total and triggers the update event.
        /// </summary>
        /// <param name="amount">Amount of coins to spend.</param>
        public static void SpendCoin(int amount)
        {
            SetCoin(GetCoin() - amount);
            OnCoinAmountChanged?.Invoke();
        }
    
        /// <summary>
        /// Updates the coin display UI text.
        /// </summary>
        private void UpdateCoin()
        {
            _currencyCoinText.text = GetCoin().ToString();
        }

    }   
}
