using System;
using NinovaGamesCase.Core;
using NinovaGamesCase.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Grid = NinovaGamesCase.Core.Grid;
using Random = UnityEngine.Random;

namespace NinovaGamesCase.Managers
{
    /// <summary>
    /// Manages level loading, progression, and UI updates related to levels.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static event Action OnLevelLoaded;
        
        [SerializeField] private LevelConfig[] _allConfigs;
        public static LevelConfig CurrentConfig;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _failLevelButton;
        [SerializeField] private Button _retryButton;

        private void Awake()
        {
            MatchManager.OnLevelCompleted += OnLevelCompleted;
            _nextLevelButton.onClick.AddListener(NextLevel);
            _failLevelButton.onClick.AddListener(NextLevel);
            _retryButton.onClick.AddListener(NextLevel);
        }

        private void Start()
        {
            LoadLevel();
        }

        private void OnDestroy()
        {
            MatchManager.OnLevelCompleted -= OnLevelCompleted;
            _nextLevelButton.onClick.RemoveAllListeners();
            _failLevelButton.onClick.RemoveAllListeners();
            _retryButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Loads the next level unless a power-up is active. Clears the current grid before loading.
        /// </summary>
        private void NextLevel()
        {
            if (PowerUpActionManager.Instance.PowerUp != PowerUp.None)
                return;
            
            Grid.Instance.ClearGrid();
            LoadLevel();
        }

        /// <summary>
        /// Called when a level is completed. Advances the saved level and awards coins.
        /// </summary>
        private void OnLevelCompleted()
        {
            SaveLoadManager.IncreaseLevel();
            PlayerCurrencyManager.AddCoin(Random.Range(10,100));
        }

        /// <summary>
        /// Loads the current level configuration and initializes the grid and UI.
        /// </summary>
        private void LoadLevel()
        {
            var currentLevel = SaveLoadManager.GetLevel();
            var id = (currentLevel - 1) % _allConfigs.Length;

            CurrentConfig = _allConfigs[id];

            _levelText.text = currentLevel.ToString();
            
            Grid.Instance.CreateGrid();
            
            OnLevelLoaded?.Invoke();
        }
    }   
}
