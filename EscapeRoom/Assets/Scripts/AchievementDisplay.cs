using UnityEngine;
using TMPro;

public class AchievementDisplay : MonoBehaviour
{
    public GameObject achievementUIPrefab;
    public Transform achievementUIParent;

    public void DisplayAchievement(Achievement achievement)
    {
        GameObject achievementUI = Instantiate(achievementUIPrefab, achievementUIParent);
        achievementUI.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = achievement.Title;
        achievementUI.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = achievement.Description;

        Destroy(achievementUI, 2f);
    }
}