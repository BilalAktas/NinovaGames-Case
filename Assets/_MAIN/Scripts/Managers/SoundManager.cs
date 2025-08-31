using System;
using NinovaGamesCase.Core;
using UnityEngine;

namespace NinovaGamesCase.Managers
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _onMovedSource;
        [SerializeField] private AudioSource _onMoveFailedSource;
        [SerializeField] private AudioSource _onTimeFailedSource;
        [SerializeField] private AudioSource _onLevelCompletedSource;
        [SerializeField] private AudioSource _onMatchedSource;

        private void Start()
        {
            Block.OnMoved += OnMoved;
            Block.OnMoveFailed += OnMoveFailed;
            GameManager.OnTimeFailed += OnTimeFailed;
            MatchManager.OnLevelCompleted += OnLevelCompleted;
            MatchManager.OnMatchedCompleted += OnMatchedCompleted;
        }

        private void OnDestroy()
        {
            Block.OnMoved -= OnMoved;
            Block.OnMoveFailed -= OnMoveFailed;
            GameManager.OnTimeFailed -= OnTimeFailed;
            MatchManager.OnLevelCompleted -= OnLevelCompleted;
            MatchManager.OnMatchedCompleted -= OnMatchedCompleted;
        }

        private void OnMoved()
        {
            _onMovedSource.Play();
        }
        private void OnMoveFailed()
        {
            _onMoveFailedSource.Play();
        }
        private void OnTimeFailed()
        {
            _onTimeFailedSource.Play();
        }
        private void OnLevelCompleted()
        {
            _onLevelCompletedSource.Play();
        }
        private void OnMatchedCompleted()
        {
            _onMatchedSource.Play();
        }
    }   
}