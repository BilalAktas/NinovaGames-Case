using System;
using DG.Tweening;
using NinovaGamesCase.Helpers;
using TMPro;
using UnityEngine;

namespace NinovaGamesCase.Managers
{
    public enum GameState {Idle, Play}
    
    public class GameManager : Singleton<GameManager>
    {
        public static event Action OnTimeFailed;
        
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private GameObject _levelCompletedUI;
        [SerializeField] private GameObject _levelFailedUI;

        private GameState _gameState;
        public GameState GameState
        {
            get { return _gameState; }
            set { _gameState = value; }
        }
        
        private float currentTime;

        private void Awake()
        {
            MatchManager.OnLevelCompleted += OnLevelCompleted;
            LevelManager.OnLevelLoaded += OnLevelLoaded;
        }

        private void OnDestroy()
        {
            MatchManager.OnLevelCompleted -= OnLevelCompleted;
            LevelManager.OnLevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelCompleted()
        {
            GameState = GameState.Idle;
            DOVirtual.DelayedCall(1f, () =>
            {
                _levelCompletedUI.SetActive(true);
            });
            
        }
        
        private void OnLevelLoaded()
        {
            _levelCompletedUI.SetActive(false);
            _levelFailedUI.SetActive(false);
            GameState = GameState.Play;
            currentTime = LevelManager.CurrentConfig.Time;
            
        }

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


        private void UpdateTimerDisplay()
        {
            var minutes = Mathf.FloorToInt(currentTime / 60f);
            var seconds = Mathf.FloorToInt(currentTime % 60f);
            _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}

