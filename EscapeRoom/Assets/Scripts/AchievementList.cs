using UnityEngine;

    [CreateAssetMenu(fileName = "AchievementList", menuName = "Achievements/Achievement List")]
    public class AchievementList : ScriptableObject
    {
        public Achievement[] achievements;
    }

    [System.Serializable]
    public class Achievement
    {
        public string AchievementID;
        public string Title;
        public string Description;
    }

