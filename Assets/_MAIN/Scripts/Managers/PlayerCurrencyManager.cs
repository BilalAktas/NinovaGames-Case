using System;
using TMPro;
using UnityEngine;


namespace NinovaGamesCase.Managers
{
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

        
        public static bool IsCoinEnough(int amount)
        {
            return GetCoin() >= amount;
        }

        private static int GetCoin() => PlayerPrefs.GetInt("PlayerCoin", 0);
    
        public static void AddCoin(int amount)
        {
            PlayerPrefs.SetInt("PlayerCoin", GetCoin() + amount);
            OnCoinAmountChanged?.Invoke();
        }

        private static void SetCoin(int amount) => PlayerPrefs.SetInt("PlayerCoin", amount);

        public static void SpendCoin(int amount)
        {
            SetCoin(GetCoin() - amount);
            OnCoinAmountChanged?.Invoke();
        }
    
        private void UpdateCoin()
        {
            _currencyCoinText.text = GetCoin().ToString();
        }

    }   
}
