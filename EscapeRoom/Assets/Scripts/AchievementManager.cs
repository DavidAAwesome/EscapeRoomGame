using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public AchievementDisplay achievementDisplay;

    private AchievementList achievementList;

    private void Start()
    {
        achievementList = Resources.Load<AchievementList>("AchievementList");
        LoadAchievements();
    }

    public void UnlockAchievement(string achievementID)
    {
        foreach (var achievement in achievementList.achievements)
        {
            if (achievement.AchievementID == achievementID)
            {
                // Logic for unlocking the achievement
                achievementDisplay.DisplayAchievement(achievement);
                SaveAchievement(achievementID);
                break;
            }
        }
    }

    private void SaveAchievement(string achievementID)
    {
        PlayerPrefs.SetInt(achievementID, 1); // Save achievement as unlocked
    }

    private void LoadAchievements()
    {
        foreach (var achievement in achievementList.achievements)
        {
            if (PlayerPrefs.GetInt(achievement.AchievementID, 0) == 1)
            {
                // Achievement already unlocked, handle accordingly
            }
        }
    }
}