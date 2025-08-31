using UnityEngine;

namespace NinovaGamesCase.Helpers
{
    public class SaveLoadManager
    {
        public static int GetLevel() => PlayerPrefs.GetInt("Level", 1);
        public static void IncreaseLevel() => PlayerPrefs.SetInt("Level", GetLevel() + 1);
    }
}


