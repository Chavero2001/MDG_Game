using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI levelText;
    private Animation anim;
    public Sprite happySprite;
    public Sprite sadSprite;
    public Sprite angrySprite;

    private int points = 0;
    private int level = 1;
    // Called when the game starts
    void Start()
    {
        anim = pointsText.GetComponent<Animation>();
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        points += amount;
        AnimationState state = anim.PlayQueued(anim.clip.name, QueueMode.PlayNow);
        UpdateUI();
    }

    public void SetLevel(int newLevel)
    {
        level = newLevel;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (pointsText != null)
            pointsText.text = points.ToString("N0") + " pts"; // e.g. 1,000
        if (levelText != null)
            levelText.text = "Level " + level;
    }
}
