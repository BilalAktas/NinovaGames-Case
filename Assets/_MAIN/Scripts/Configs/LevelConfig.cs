using System;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    public enum LevelDoorDirection {Up,Down,Right,Left}
    
    [System.Serializable]
    public struct LevelDoorData
    {
        public Vector2Int GridPosition;
        public int Length;
        public ColorData ColorData;
        public LevelDoorDirection Direction;
    }
    
    [System.Serializable]
    public struct LevelBlockData
    {
        public Vector2Int GridPosition;
        public Vector2Int[] MeshPositions;
        public BlockData BlockData;
    }

    [System.Serializable]
    public struct LevelWallData
    {
        public Vector2Int GridPosition;
        public int Length;
        public LevelDoorDirection Direction;
    }
    
    [CreateAssetMenu(menuName = "ScriptableObjects/LevelConfig", fileName = "LevelConfig")] 
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private Vector2Int _nodeSize;
        [SerializeField] private LevelBlockData[] _levelBlockData;
        [SerializeField] private LevelDoorData[] _levelDoorData;
        [SerializeField] private int _time;
        
        
        public Vector2Int GridSize => _gridSize;
        public Vector2Int NodeSize => _nodeSize;
        public LevelBlockData[] LevelBlockData => _levelBlockData;
        public LevelDoorData[] LevelDoorData => _levelDoorData;
        public int Time => _time;
        
    }   
}
