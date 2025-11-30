using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI component for displaying achievements and progress.
/// </summary>
public class AchievementUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent container for achievement entries")]
    public Transform achievementContainer;
    
    [Tooltip("Prefab for achievement entry")]
    public GameObject achievementEntryPrefab;
    
    [Tooltip("Panel that shows achievement unlock notification")]
    public GameObject unlockNotificationPanel;
    
    [Tooltip("Text for the unlock notification")]
    public Text unlockNotificationText;
    
    [Tooltip("Text for achievement name in notification")]
    public Text notificationNameText;
    
    [Tooltip("Text for achievement description in notification")]
    public Text notificationDescriptionText;
    
    [Tooltip("Text for experience reward in notification")]
    public Text notificationRewardText;
    
    [Tooltip("Overall progress text")]
    public Text overallProgressText;
    
    [Tooltip("Overall progress bar fill")]
    public Image overallProgressFill;
    
    [Header("Notification Settings")]
    [Tooltip("Duration to show unlock notification")]
    public float notificationDuration = 4f;
    
    [Tooltip("Animation speed for notification")]
    public float notificationAnimationSpeed = 2f;
    
    [Header("Colors")]
    public Color unlockedColor = new Color(1f, 0.85f, 0f);
    public Color lockedColor = Color.gray;
    public Color progressColor = new Color(0.5f, 0.7f, 1f);
    public Color hiddenColor = new Color(0.3f, 0.3f, 0.3f);
    
    private Queue<AchievementDefinition> notificationQueue = new Queue<AchievementDefinition>();
    private bool isShowingNotification = false;
    private float notificationTimer;
    private AchievementSystem achievementSystemRef;
    
    void Start()
    {
        // Subscribe to achievement events and store reference for cleanup
        achievementSystemRef = AchievementSystem.Instance;
        achievementSystemRef.OnAchievementUnlocked += OnAchievementUnlocked;
        achievementSystemRef.OnAchievementProgress += OnAchievementProgress;
        
        // Initialize notification panel
        if (unlockNotificationPanel != null)
        {
            unlockNotificationPanel.SetActive(false);
        }
        
        // Initial refresh
        RefreshAchievementList();
        UpdateOverallProgress();
    }
    
    void OnDestroy()
    {
        // Unsubscribe using stored reference
        if (achievementSystemRef != null)
        {
            achievementSystemRef.OnAchievementUnlocked -= OnAchievementUnlocked;
            achievementSystemRef.OnAchievementProgress -= OnAchievementProgress;
        }
    }
    
    void Update()
    {
        // Handle notification display timing
        if (isShowingNotification)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0)
            {
                HideNotification();
            }
        }
        
        // Process notification queue
        if (!isShowingNotification && notificationQueue.Count > 0)
        {
            ShowNotification(notificationQueue.Dequeue());
        }
    }
    
    /// <summary>
    /// Refreshes the achievement list display.
    /// </summary>
    public void RefreshAchievementList()
    {
        if (achievementContainer == null || achievementEntryPrefab == null)
        {
            return;
        }
        
        // Clear existing entries
        foreach (Transform child in achievementContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get all achievements
        List<AchievementDefinition> achievements = AchievementSystem.Instance.GetAllAchievements();
        
        // Sort: unlocked first, then by type
        achievements.Sort((a, b) =>
        {
            AchievementProgress progressA = AchievementSystem.Instance.GetProgress(a.id);
            AchievementProgress progressB = AchievementSystem.Instance.GetProgress(b.id);
            
            // Unlocked first
            if (progressA.isUnlocked != progressB.isUnlocked)
            {
                return progressA.isUnlocked ? -1 : 1;
            }
            
            // Then by type
            return a.type.CompareTo(b.type);
        });
        
        // Create entries
        foreach (var achievement in achievements)
        {
            CreateAchievementEntry(achievement);
        }
    }
    
    /// <summary>
    /// Creates a UI entry for an achievement.
    /// </summary>
    private void CreateAchievementEntry(AchievementDefinition achievement)
    {
        if (achievementEntryPrefab == null || achievementContainer == null)
        {
            return;
        }
        
        AchievementProgress progress = AchievementSystem.Instance.GetProgress(achievement.id);
        
        // Skip hidden achievements that aren't unlocked
        if (achievement.isHidden && !progress.isUnlocked)
        {
            CreateHiddenEntry();
            return;
        }
        
        GameObject entry = Instantiate(achievementEntryPrefab, achievementContainer);
        
        // Get UI components
        Text nameText = entry.transform.Find("NameText")?.GetComponent<Text>();
        Text descText = entry.transform.Find("DescriptionText")?.GetComponent<Text>();
        Image progressFill = entry.transform.Find("ProgressBar/Fill")?.GetComponent<Image>();
        Text progressText = entry.transform.Find("ProgressText")?.GetComponent<Text>();
        Image icon = entry.transform.Find("Icon")?.GetComponent<Image>();
        Image background = entry.GetComponent<Image>();
        
        // Set values
        if (nameText != null)
        {
            nameText.text = achievement.name;
            nameText.color = progress.isUnlocked ? unlockedColor : lockedColor;
        }
        
        if (descText != null)
        {
            descText.text = achievement.description;
        }
        
        if (progressFill != null)
        {
            progressFill.fillAmount = progress.progress;
            progressFill.color = progress.isUnlocked ? unlockedColor : progressColor;
        }
        
        if (progressText != null)
        {
            if (progress.isUnlocked)
            {
                progressText.text = "UNLOCKED";
                progressText.color = unlockedColor;
            }
            else
            {
                progressText.text = $"{Mathf.RoundToInt(progress.progress * 100)}%";
                progressText.color = progressColor;
            }
        }
        
        if (background != null)
        {
            background.color = progress.isUnlocked ? 
                new Color(0.2f, 0.2f, 0.1f) : 
                new Color(0.15f, 0.15f, 0.15f);
        }
    }
    
    /// <summary>
    /// Creates a placeholder for a hidden achievement.
    /// </summary>
    private void CreateHiddenEntry()
    {
        if (achievementEntryPrefab == null || achievementContainer == null)
        {
            return;
        }
        
        GameObject entry = Instantiate(achievementEntryPrefab, achievementContainer);
        
        Text nameText = entry.transform.Find("NameText")?.GetComponent<Text>();
        Text descText = entry.transform.Find("DescriptionText")?.GetComponent<Text>();
        Image progressFill = entry.transform.Find("ProgressBar/Fill")?.GetComponent<Image>();
        Text progressText = entry.transform.Find("ProgressText")?.GetComponent<Text>();
        Image background = entry.GetComponent<Image>();
        
        if (nameText != null)
        {
            nameText.text = "???";
            nameText.color = hiddenColor;
        }
        
        if (descText != null)
        {
            descText.text = "This achievement is hidden.";
            descText.color = hiddenColor;
        }
        
        if (progressFill != null)
        {
            progressFill.fillAmount = 0;
        }
        
        if (progressText != null)
        {
            progressText.text = "";
        }
        
        if (background != null)
        {
            background.color = new Color(0.1f, 0.1f, 0.1f);
        }
    }
    
    /// <summary>
    /// Updates the overall progress display.
    /// </summary>
    public void UpdateOverallProgress()
    {
        int unlocked = AchievementSystem.Instance.GetUnlockedCount();
        int total = AchievementSystem.Instance.GetTotalAchievementCount();
        float percentage = AchievementSystem.Instance.GetCompletionPercentage();
        
        if (overallProgressText != null)
        {
            overallProgressText.text = $"Achievements: {unlocked}/{total} ({percentage:F1}%)";
        }
        
        if (overallProgressFill != null)
        {
            overallProgressFill.fillAmount = percentage / 100f;
        }
    }
    
    /// <summary>
    /// Event handler for achievement unlock.
    /// </summary>
    private void OnAchievementUnlocked(AchievementDefinition achievement)
    {
        // Queue notification
        notificationQueue.Enqueue(achievement);
        
        // Refresh list
        RefreshAchievementList();
        UpdateOverallProgress();
    }
    
    /// <summary>
    /// Event handler for achievement progress.
    /// </summary>
    private void OnAchievementProgress(AchievementDefinition achievement, float progress)
    {
        // Could show a small progress update indicator
        // For now, just refresh the list
        RefreshAchievementList();
    }
    
    /// <summary>
    /// Shows an unlock notification.
    /// </summary>
    private void ShowNotification(AchievementDefinition achievement)
    {
        if (unlockNotificationPanel == null)
        {
            return;
        }
        
        isShowingNotification = true;
        notificationTimer = notificationDuration;
        
        unlockNotificationPanel.SetActive(true);
        
        if (unlockNotificationText != null)
        {
            unlockNotificationText.text = "ACHIEVEMENT UNLOCKED!";
        }
        
        if (notificationNameText != null)
        {
            notificationNameText.text = achievement.name;
            notificationNameText.color = unlockedColor;
        }
        
        if (notificationDescriptionText != null)
        {
            notificationDescriptionText.text = achievement.description;
        }
        
        if (notificationRewardText != null)
        {
            notificationRewardText.text = $"+{achievement.experienceReward} XP";
            if (!string.IsNullOrEmpty(achievement.unlockReward))
            {
                notificationRewardText.text += $"\nReward: {achievement.unlockReward}";
            }
        }
        
        // Start animation (could use DoTween or similar for better animations)
        StartCoroutine(NotificationAnimation());
    }
    
    /// <summary>
    /// Hides the unlock notification.
    /// </summary>
    private void HideNotification()
    {
        isShowingNotification = false;
        
        if (unlockNotificationPanel != null)
        {
            unlockNotificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Coroutine for notification animation.
    /// </summary>
    private System.Collections.IEnumerator NotificationAnimation()
    {
        if (unlockNotificationPanel == null)
        {
            yield break;
        }
        
        CanvasGroup canvasGroup = unlockNotificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = unlockNotificationPanel.AddComponent<CanvasGroup>();
        }
        
        // Fade in
        canvasGroup.alpha = 0f;
        float fadeInTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime * notificationAnimationSpeed;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        // Wait for notification to complete
        yield return new WaitForSeconds(notificationDuration - 1f);
        
        // Fade out
        elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime * notificationAnimationSpeed;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInTime);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Shows the full achievement panel.
    /// </summary>
    public void ShowAchievementPanel()
    {
        gameObject.SetActive(true);
        RefreshAchievementList();
        UpdateOverallProgress();
    }
    
    /// <summary>
    /// Hides the achievement panel.
    /// </summary>
    public void HideAchievementPanel()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Toggles the achievement panel visibility.
    /// </summary>
    public void ToggleAchievementPanel()
    {
        if (gameObject.activeSelf)
        {
            HideAchievementPanel();
        }
        else
        {
            ShowAchievementPanel();
        }
    }
}
