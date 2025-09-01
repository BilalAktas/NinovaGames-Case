using UnityEngine;

namespace NinovaGamesCase.Helpers
{
    /// <summary>
    /// Provides simple save and load functionality for player progress using PlayerPrefs.
    /// </summary>
    public class SaveLoadManager
    {
        private const string _LEVEL_KEY = "Level";
        
        /// <summary>
        /// Gets the current player level from PlayerPrefs. Defaults to 1 if not set.
        /// </summary>
        /// <returns>The current level number.</returns>
        public static int GetLevel() => PlayerPrefs.GetInt(_LEVEL_KEY, 1);
        
        /// <summary>
        /// Increases the stored player level by 1.
        /// </summary>
        public static void IncreaseLevel() => PlayerPrefs.SetInt(_LEVEL_KEY, GetLevel() + 1);
    }
}


