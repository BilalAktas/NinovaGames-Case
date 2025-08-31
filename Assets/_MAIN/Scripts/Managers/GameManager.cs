using DG.Tweening;
using TMPro;
using UnityEngine;

namespace NinovaGamesCase.Managers
{
    public enum GameState {Idle, Play}
    
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private float _totalTime;
        [SerializeField] private GameObject _levelCompletedUI;
        
        private GameState _gameState;
        
        private float currentTime;

        private void Start()
        {
            _gameState = GameState.Play;
            
            currentTime = _totalTime;

            MatchManager.OnLevelCompleted += OnLevelCompleted;
        }

        private void OnDestroy()
        {
            MatchManager.OnLevelCompleted -= OnLevelCompleted;
        }

        private void OnLevelCompleted()
        {
            _gameState = GameState.Idle;
            DOVirtual.DelayedCall(1f, () =>
            {
                _levelCompletedUI.SetActive(true);
            });
            
        }

        private void Update()
        {
            if (_gameState == GameState.Idle)
                return;
            
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(currentTime, 0f);
            UpdateTimerDisplay();

            if (currentTime<=0)
            {
                //fail.
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

