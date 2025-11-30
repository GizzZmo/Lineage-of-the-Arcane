using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the type of achievement.
/// </summary>
public enum AchievementType
{
    AffinityMilestone,      // Reaching specific affinity levels
    TetherDuration,         // Total time spent tethered
    SuccessfulTethers,      // Number of successful tethers
    EntityMastery,          // Reaching Ascended with a specific entity
    LineageMastery,         // Reaching Ascended with all entities in a lineage
    NoBetrayals,            // Completing tethers without betrayals
    CompetitionWin,         // Winning an affinity competition
    SpecialAbility,         // Using a special ability
    MultiEntity,            // Reaching milestones with multiple entities
    Challenge               // Special challenge achievements
}

/// <summary>
/// Represents a single achievement definition.
/// </summary>
[System.Serializable]
public class AchievementDefinition
{
    public string id;
    public string name;
    public string description;
    public AchievementType type;
    public string iconPath;
    
    // Requirements based on type
    public string targetEntityId;        // For entity-specific achievements
    public string targetLineage;         // For lineage achievements
    public AffinityLevel requiredLevel;  // For affinity milestones
    public int requiredCount;            // For count-based achievements
    public float requiredDuration;       // For time-based achievements
    
    // Rewards
    public int experienceReward;
    public string unlockReward;          // Cosmetic or gameplay unlock
    
    // Hidden achievements
    public bool isHidden;
    
    public AchievementDefinition(string id, string name, string description, AchievementType type)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
        this.requiredLevel = AffinityLevel.Stranger;
        this.requiredCount = 1;
        this.requiredDuration = 0f;
        this.experienceReward = 10;
        this.isHidden = false;
    }
}

/// <summary>
/// Represents a player's progress toward an achievement.
/// </summary>
[System.Serializable]
public class AchievementProgress
{
    public string achievementId;
    public bool isUnlocked;
    public float progress;              // 0-1 progress toward completion
    public int currentCount;            // For count-based achievements
    public float currentDuration;       // For time-based achievements
    public System.DateTime unlockedAt;
    
    public AchievementProgress(string achievementId)
    {
        this.achievementId = achievementId;
        this.isUnlocked = false;
        this.progress = 0f;
        this.currentCount = 0;
        this.currentDuration = 0f;
    }
}

/// <summary>
/// Manages the achievement system tied to affinity milestones and gameplay events.
/// </summary>
public class AchievementSystem : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Whether achievements are enabled")]
    public bool achievementsEnabled = true;
    
    [Header("Runtime State")]
    [SerializeField] private List<AchievementDefinition> achievements = new List<AchievementDefinition>();
    [SerializeField] private Dictionary<string, AchievementProgress> playerProgress = new Dictionary<string, AchievementProgress>();
    
    // Events
    public System.Action<AchievementDefinition> OnAchievementUnlocked;
    public System.Action<AchievementDefinition, float> OnAchievementProgress;
    
    // Singleton instance
    private static AchievementSystem instance;
    public static AchievementSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AchievementSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AchievementSystem");
                    instance = go.AddComponent<AchievementSystem>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        playerProgress = new Dictionary<string, AchievementProgress>();
        InitializeAchievements();
    }
    
    // Store reference for cleanup
    private AffinitySystem affinitySystemRef;
    
    void Start()
    {
        // Subscribe to affinity events and store reference for cleanup
        affinitySystemRef = AffinitySystem.Instance;
        affinitySystemRef.OnAffinityLevelChanged += HandleAffinityLevelChanged;
        affinitySystemRef.OnAbilityUnlocked += HandleAbilityUnlocked;
    }
    
    void OnDestroy()
    {
        // Unsubscribe using stored reference
        if (affinitySystemRef != null)
        {
            affinitySystemRef.OnAffinityLevelChanged -= HandleAffinityLevelChanged;
            affinitySystemRef.OnAbilityUnlocked -= HandleAbilityUnlocked;
        }
    }
    
    /// <summary>
    /// Initializes the built-in achievements.
    /// </summary>
    private void InitializeAchievements()
    {
        achievements = new List<AchievementDefinition>();
        
        // === AFFINITY MILESTONE ACHIEVEMENTS ===
        
        // First steps achievements
        AddAchievement(new AchievementDefinition(
            "first_acquaintance",
            "First Acquaintance",
            "Reach Acquainted level with any entity",
            AchievementType.AffinityMilestone
        )
        {
            requiredLevel = AffinityLevel.Acquainted,
            experienceReward = 10
        });
        
        AddAchievement(new AchievementDefinition(
            "first_bond",
            "First Bond",
            "Reach Bonded level with any entity",
            AchievementType.AffinityMilestone
        )
        {
            requiredLevel = AffinityLevel.Bonded,
            experienceReward = 25
        });
        
        AddAchievement(new AchievementDefinition(
            "devoted_follower",
            "Devoted Follower",
            "Reach Devoted level with any entity",
            AchievementType.AffinityMilestone
        )
        {
            requiredLevel = AffinityLevel.Devoted,
            experienceReward = 50
        });
        
        AddAchievement(new AchievementDefinition(
            "first_ascension",
            "First Ascension",
            "Reach Ascended level with any entity for the first time",
            AchievementType.AffinityMilestone
        )
        {
            requiredLevel = AffinityLevel.Ascended,
            experienceReward = 100
        });
        
        // === ENTITY MASTERY ACHIEVEMENTS ===
        
        // Ignis lineage
        AddAchievement(new AchievementDefinition(
            "master_of_fire",
            "Master of Fire",
            "Reach Ascended level with Ignis Mater",
            AchievementType.EntityMastery
        )
        {
            targetEntityId = "IgnisMater",
            requiredLevel = AffinityLevel.Ascended,
            experienceReward = 150,
            unlockReward = "Fire Aura Effect"
        });
        
        // Aqua lineage
        AddAchievement(new AchievementDefinition(
            "master_of_water",
            "Master of Water",
            "Reach Ascended level with Aqua Pater",
            AchievementType.EntityMastery
        )
        {
            targetEntityId = "AquaPater",
            requiredLevel = AffinityLevel.Ascended,
            experienceReward = 150,
            unlockReward = "Water Aura Effect"
        });
        
        // Terra lineage
        AddAchievement(new AchievementDefinition(
            "master_of_earth",
            "Master of Earth",
            "Reach Ascended level with Terra Mater",
            AchievementType.EntityMastery
        )
        {
            targetEntityId = "TerraMater",
            requiredLevel = AffinityLevel.Ascended,
            experienceReward = 150,
            unlockReward = "Earth Aura Effect"
        });
        
        // Tempus lineage
        AddAchievement(new AchievementDefinition(
            "master_of_time",
            "Master of Time",
            "Reach Ascended level with Tempus Mater",
            AchievementType.EntityMastery
        )
        {
            targetEntityId = "TempusMater",
            requiredLevel = AffinityLevel.Ascended,
            experienceReward = 150,
            unlockReward = "Time Aura Effect"
        });
        
        // Dolor lineage
        AddAchievement(new AchievementDefinition(
            "master_of_pain",
            "Master of Pain",
            "Reach Ascended level with Dolor Mater",
            AchievementType.EntityMastery
        )
        {
            targetEntityId = "DolorMater",
            requiredLevel = AffinityLevel.Ascended,
            experienceReward = 150,
            unlockReward = "Pain Aura Effect"
        });
        
        // === TETHER ACHIEVEMENTS ===
        
        AddAchievement(new AchievementDefinition(
            "novice_tetherer",
            "Novice Tetherer",
            "Complete 10 successful tethers",
            AchievementType.SuccessfulTethers
        )
        {
            requiredCount = 10,
            experienceReward = 20
        });
        
        AddAchievement(new AchievementDefinition(
            "experienced_tetherer",
            "Experienced Tetherer",
            "Complete 50 successful tethers",
            AchievementType.SuccessfulTethers
        )
        {
            requiredCount = 50,
            experienceReward = 75
        });
        
        AddAchievement(new AchievementDefinition(
            "master_tetherer",
            "Master Tetherer",
            "Complete 100 successful tethers",
            AchievementType.SuccessfulTethers
        )
        {
            requiredCount = 100,
            experienceReward = 200
        });
        
        // === DURATION ACHIEVEMENTS ===
        
        AddAchievement(new AchievementDefinition(
            "patient_tether",
            "Patient Tether",
            "Maintain a single tether for 60 seconds",
            AchievementType.TetherDuration
        )
        {
            requiredDuration = 60f,
            experienceReward = 30
        });
        
        AddAchievement(new AchievementDefinition(
            "enduring_bond",
            "Enduring Bond",
            "Maintain a single tether for 120 seconds",
            AchievementType.TetherDuration
        )
        {
            requiredDuration = 120f,
            experienceReward = 75
        });
        
        AddAchievement(new AchievementDefinition(
            "eternal_connection",
            "Eternal Connection",
            "Maintain a single tether for 300 seconds",
            AchievementType.TetherDuration
        )
        {
            requiredDuration = 300f,
            experienceReward = 150
        });
        
        // === SPECIAL ABILITY ACHIEVEMENTS ===
        
        AddAchievement(new AchievementDefinition(
            "inferno_unleashed",
            "Inferno Unleashed",
            "Activate Ignis Mater's Inferno Embrace ability",
            AchievementType.SpecialAbility
        )
        {
            targetEntityId = "IgnisMater",
            experienceReward = 50
        });
        
        AddAchievement(new AchievementDefinition(
            "sanctuary_created",
            "Sanctuary Created",
            "Activate Aqua Pater's Tidal Sanctuary ability",
            AchievementType.SpecialAbility
        )
        {
            targetEntityId = "AquaPater",
            experienceReward = 50
        });
        
        AddAchievement(new AchievementDefinition(
            "bulwark_raised",
            "Bulwark Raised",
            "Activate Terra Mater's Earthen Bulwark ability",
            AchievementType.SpecialAbility
        )
        {
            targetEntityId = "TerraMater",
            experienceReward = 50
        });
        
        AddAchievement(new AchievementDefinition(
            "time_stopped",
            "Time Stopped",
            "Activate Tempus Mater's Temporal Stasis ability",
            AchievementType.SpecialAbility
        )
        {
            targetEntityId = "TempusMater",
            experienceReward = 50
        });
        
        AddAchievement(new AchievementDefinition(
            "martyrdom_complete",
            "Martyrdom Complete",
            "Activate Dolor Mater's Martyrdom ability",
            AchievementType.SpecialAbility
        )
        {
            targetEntityId = "DolorMater",
            experienceReward = 50
        });
        
        // === CHALLENGE ACHIEVEMENTS ===
        
        AddAchievement(new AchievementDefinition(
            "perfect_partner",
            "Perfect Partner",
            "Complete 10 tethers without any betrayals",
            AchievementType.NoBetrayals
        )
        {
            requiredCount = 10,
            experienceReward = 100
        });
        
        AddAchievement(new AchievementDefinition(
            "all_parents_acquainted",
            "Friend to All",
            "Reach Acquainted level with all 5 Parent entities",
            AchievementType.MultiEntity
        )
        {
            requiredLevel = AffinityLevel.Acquainted,
            requiredCount = 5,
            experienceReward = 100
        });
        
        AddAchievement(new AchievementDefinition(
            "all_parents_ascended",
            "The Arcane Master",
            "Reach Ascended level with all 5 Parent entities",
            AchievementType.MultiEntity
        )
        {
            requiredLevel = AffinityLevel.Ascended,
            requiredCount = 5,
            experienceReward = 500,
            unlockReward = "Master of the Arcane Title"
        });
        
        // === HIDDEN ACHIEVEMENTS ===
        
        AddAchievement(new AchievementDefinition(
            "from_ashes",
            "From the Ashes",
            "Reach Ascended level with an entity after being Hostile",
            AchievementType.Challenge
        )
        {
            requiredLevel = AffinityLevel.Ascended,
            experienceReward = 200,
            isHidden = true
        });
        
        Debug.Log($"[ACHIEVEMENTS] Initialized {achievements.Count} achievements.");
    }
    
    /// <summary>
    /// Adds an achievement to the system.
    /// </summary>
    private void AddAchievement(AchievementDefinition achievement)
    {
        achievements.Add(achievement);
        
        // Initialize progress tracking
        if (!playerProgress.ContainsKey(achievement.id))
        {
            playerProgress[achievement.id] = new AchievementProgress(achievement.id);
        }
    }
    
    /// <summary>
    /// Handles affinity level changes for achievement tracking.
    /// </summary>
    private void HandleAffinityLevelChanged(string entityId, AffinityLevel newLevel)
    {
        if (!achievementsEnabled) return;
        
        // Check affinity milestone achievements
        foreach (var achievement in achievements)
        {
            if (achievement.type == AchievementType.AffinityMilestone)
            {
                if (newLevel >= achievement.requiredLevel)
                {
                    TryUnlockAchievement(achievement.id);
                }
            }
            else if (achievement.type == AchievementType.EntityMastery)
            {
                if (entityId == achievement.targetEntityId && newLevel >= achievement.requiredLevel)
                {
                    TryUnlockAchievement(achievement.id);
                }
            }
        }
        
        // Check multi-entity achievements
        CheckMultiEntityAchievements();
    }
    
    /// <summary>
    /// Handles ability unlock events for achievement tracking.
    /// </summary>
    private void HandleAbilityUnlocked(string entityId)
    {
        if (!achievementsEnabled) return;
        
        // Check for special ability achievements when unlocked
        foreach (var achievement in achievements)
        {
            if (achievement.type == AchievementType.SpecialAbility &&
                achievement.targetEntityId == entityId)
            {
                // The achievement will be unlocked when the ability is actually used
                // This is tracked separately through ReportAbilityUsed
            }
        }
    }
    
    /// <summary>
    /// Reports that a special ability was used.
    /// </summary>
    public void ReportAbilityUsed(string entityId)
    {
        if (!achievementsEnabled) return;
        
        foreach (var achievement in achievements)
        {
            if (achievement.type == AchievementType.SpecialAbility &&
                achievement.targetEntityId == entityId)
            {
                TryUnlockAchievement(achievement.id);
            }
        }
    }
    
    /// <summary>
    /// Reports a successful tether completion.
    /// </summary>
    public void ReportSuccessfulTether(string entityId, float duration)
    {
        if (!achievementsEnabled) return;
        
        // Update tether count achievements
        foreach (var achievement in achievements)
        {
            if (achievement.type == AchievementType.SuccessfulTethers)
            {
                AchievementProgress progress = GetProgress(achievement.id);
                progress.currentCount++;
                progress.progress = (float)progress.currentCount / achievement.requiredCount;
                
                OnAchievementProgress?.Invoke(achievement, progress.progress);
                
                if (progress.currentCount >= achievement.requiredCount)
                {
                    TryUnlockAchievement(achievement.id);
                }
            }
            
            // Check duration achievements
            if (achievement.type == AchievementType.TetherDuration)
            {
                if (duration >= achievement.requiredDuration)
                {
                    TryUnlockAchievement(achievement.id);
                }
            }
        }
        
        // Update no-betrayal streak
        UpdateNoBetrayalProgress();
    }
    
    /// <summary>
    /// Reports a betrayal (broken tether).
    /// </summary>
    public void ReportBetrayal(string entityId)
    {
        if (!achievementsEnabled) return;
        
        // Reset no-betrayal progress
        foreach (var achievement in achievements)
        {
            if (achievement.type == AchievementType.NoBetrayals)
            {
                AchievementProgress progress = GetProgress(achievement.id);
                progress.currentCount = 0;
                progress.progress = 0f;
            }
        }
    }
    
    /// <summary>
    /// Updates progress for no-betrayal achievements.
    /// </summary>
    private void UpdateNoBetrayalProgress()
    {
        foreach (var achievement in achievements)
        {
            if (achievement.type == AchievementType.NoBetrayals)
            {
                AchievementProgress progress = GetProgress(achievement.id);
                progress.currentCount++;
                progress.progress = (float)progress.currentCount / achievement.requiredCount;
                
                OnAchievementProgress?.Invoke(achievement, progress.progress);
                
                if (progress.currentCount >= achievement.requiredCount)
                {
                    TryUnlockAchievement(achievement.id);
                }
            }
        }
    }
    
    /// <summary>
    /// Checks achievements that require progress with multiple entities.
    /// </summary>
    private void CheckMultiEntityAchievements()
    {
        string[] parentIds = { "IgnisMater", "AquaPater", "TerraMater", "TempusMater", "DolorMater" };
        
        foreach (var achievement in achievements)
        {
            if (achievement.type == AchievementType.MultiEntity)
            {
                int count = 0;
                foreach (string parentId in parentIds)
                {
                    AffinityLevel level = AffinitySystem.Instance.GetAffinityLevel(parentId);
                    if (level >= achievement.requiredLevel)
                    {
                        count++;
                    }
                }
                
                AchievementProgress progress = GetProgress(achievement.id);
                progress.currentCount = count;
                progress.progress = (float)count / achievement.requiredCount;
                
                OnAchievementProgress?.Invoke(achievement, progress.progress);
                
                if (count >= achievement.requiredCount)
                {
                    TryUnlockAchievement(achievement.id);
                }
            }
        }
    }
    
    /// <summary>
    /// Attempts to unlock an achievement.
    /// </summary>
    public bool TryUnlockAchievement(string achievementId)
    {
        AchievementProgress progress = GetProgress(achievementId);
        
        if (progress.isUnlocked)
        {
            return false; // Already unlocked
        }
        
        AchievementDefinition achievement = GetAchievement(achievementId);
        if (achievement == null)
        {
            return false;
        }
        
        progress.isUnlocked = true;
        progress.progress = 1f;
        progress.unlockedAt = System.DateTime.Now;
        
        Debug.Log($"[ACHIEVEMENT] Unlocked: {achievement.name}!");
        Debug.Log($"[ACHIEVEMENT] {achievement.description}");
        Debug.Log($"[ACHIEVEMENT] +{achievement.experienceReward} XP");
        
        if (!string.IsNullOrEmpty(achievement.unlockReward))
        {
            Debug.Log($"[ACHIEVEMENT] Reward: {achievement.unlockReward}");
        }
        
        OnAchievementUnlocked?.Invoke(achievement);
        
        return true;
    }
    
    /// <summary>
    /// Gets an achievement definition by ID.
    /// </summary>
    public AchievementDefinition GetAchievement(string achievementId)
    {
        foreach (var achievement in achievements)
        {
            if (achievement.id == achievementId)
            {
                return achievement;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Gets progress for an achievement.
    /// </summary>
    public AchievementProgress GetProgress(string achievementId)
    {
        if (!playerProgress.ContainsKey(achievementId))
        {
            playerProgress[achievementId] = new AchievementProgress(achievementId);
        }
        return playerProgress[achievementId];
    }
    
    /// <summary>
    /// Gets all achievements.
    /// </summary>
    public List<AchievementDefinition> GetAllAchievements()
    {
        return new List<AchievementDefinition>(achievements);
    }
    
    /// <summary>
    /// Gets all unlocked achievements.
    /// </summary>
    public List<AchievementDefinition> GetUnlockedAchievements()
    {
        List<AchievementDefinition> unlocked = new List<AchievementDefinition>();
        
        foreach (var achievement in achievements)
        {
            AchievementProgress progress = GetProgress(achievement.id);
            if (progress.isUnlocked)
            {
                unlocked.Add(achievement);
            }
        }
        
        return unlocked;
    }
    
    /// <summary>
    /// Gets the total number of achievements.
    /// </summary>
    public int GetTotalAchievementCount()
    {
        return achievements.Count;
    }
    
    /// <summary>
    /// Gets the number of unlocked achievements.
    /// </summary>
    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var achievement in achievements)
        {
            AchievementProgress progress = GetProgress(achievement.id);
            if (progress.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Gets the completion percentage.
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (achievements.Count == 0) return 0f;
        return (float)GetUnlockedCount() / achievements.Count * 100f;
    }
}
