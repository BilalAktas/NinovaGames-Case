using System;
using DG.Tweening;
using NinovaGamesCase.Helpers;
using TMPro;
using UnityEngine;

namespace NinovaGamesCase.Managers
{
    /// <summary>
    /// Enum representing the current game state.
    /// </summary>
    public enum GameState {Idle, Play}
    
    /// <summary>
    /// Manages the overall game flow, timer, and UI for level completion and failure.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public static event Action OnTimeFailed;
        
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private GameObject _levelCompletedUI;
        [SerializeField] private GameObject _levelFailedUI;

        
        /// <summary>
        /// Gets or sets the current game state.
        /// </summary>
        private GameState _gameState;
        public GameState GameState
        {
            get { return _gameState; }
            set { _gameState = value; }
        }
        
        private float currentTime;

        private void Awake()
        {
            //Application.targetFrameRate = 60; // build.
            
            MatchManager.OnLevelCompleted += OnLevelCompleted;
            LevelManager.OnLevelLoaded += OnLevelLoaded;
        }

        private void OnDestroy()
        {
            MatchManager.OnLevelCompleted -= OnLevelCompleted;
            LevelManager.OnLevelLoaded -= OnLevelLoaded;
        }

        /// <summary>
        /// Called when the level is completed. Switches to Idle state and shows the completion UI after a delay.
        /// </summary>
        private void OnLevelCompleted()
        {
            GameState = GameState.Idle;
            DOVirtual.DelayedCall(1f, () =>
            {
                _levelCompletedUI.SetActive(true);
            });
            
        }
        
        /// <summary>
        /// Called when a level is loaded. Resets UI, sets game state to Play, and resets the timer.
        /// </summary>
        private void OnLevelLoaded()
        {
            _levelCompletedUI.SetActive(false);
            _levelFailedUI.SetActive(false);
            GameState = GameState.Play;
            currentTime = LevelManager.CurrentConfig.Time;
            
        }

        /// <summary>
        /// Updates the game timer every frame and triggers failure if time runs out.
        /// </summary>
        private void Update()
        {
            if (GameState == GameState.Idle)
                return;
            
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(currentTime, 0f);
            UpdateTimerDisplay();

            if (currentTime<=0)
            {
                GameState = GameState.Idle;
                _levelFailedUI.SetActive(true);
                OnTimeFailed?.Invoke();
            }
        }

        /// <summary>
        /// Updates the timer UI display with the current time in minutes and seconds.
        /// </summary>
        private void UpdateTimerDisplay()
        {
            var minutes = Mathf.FloorToInt(currentTime / 60f);
            var seconds = Mathf.FloorToInt(currentTime % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}

