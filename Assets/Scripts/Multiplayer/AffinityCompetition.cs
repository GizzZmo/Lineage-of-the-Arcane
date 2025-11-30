using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages multiplayer affinity competition where players compete to build
/// the highest affinity with entities. This extends beyond the Custody Battle
/// to provide ongoing competition between players.
/// </summary>
public class AffinityCompetition : MonoBehaviour
{
    [Header("Competition Configuration")]
    [Tooltip("Duration of the competition in seconds")]
    public float competitionDuration = 300f; // 5 minutes
    
    [Tooltip("Entity ID to compete over (or empty for overall affinity)")]
    public string targetEntityId = "";
    
    [Tooltip("Minimum number of players to start")]
    public int minimumPlayers = 2;
    
    [Tooltip("Maximum number of players")]
    public int maximumPlayers = 4;
    
    [Header("Scoring Settings")]
    [Tooltip("Points for reaching Acquainted level")]
    public int acquaintedPoints = 10;
    
    [Tooltip("Points for reaching Bonded level")]
    public int bondedPoints = 25;
    
    [Tooltip("Points for reaching Devoted level")]
    public int devotedPoints = 50;
    
    [Tooltip("Points for reaching Ascended level")]
    public int ascendedPoints = 100;
    
    [Tooltip("Points per successful tether")]
    public int tetherPoints = 5;
    
    [Tooltip("Points lost per betrayal")]
    public int betrayalPenalty = 10;
    
    [Header("Competition State")]
    public bool isCompetitionActive = false;
    public float competitionStartTime;
    public List<CompetitorData> competitors = new List<CompetitorData>();
    
    [Header("Events")]
    public System.Action<CompetitorData> OnCompetitorJoined;
    public System.Action<CompetitorData> OnCompetitorLeft;
    public System.Action<CompetitorData, int> OnScoreChanged;
    public System.Action<CompetitorData> OnCompetitionWon;
    public System.Action OnCompetitionStarted;
    public System.Action OnCompetitionEnded;
    
    // Singleton instance
    private static AffinityCompetition instance;
    public static AffinityCompetition Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AffinityCompetition>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AffinityCompetition");
                    instance = go.AddComponent<AffinityCompetition>();
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
        competitors = new List<CompetitorData>();
    }
    
    // Store reference to affinity system for cleanup
    private AffinitySystem affinitySystemRef;
    
    void Start()
    {
        // Subscribe to affinity events and store reference for cleanup
        affinitySystemRef = AffinitySystem.Instance;
        affinitySystemRef.OnAffinityLevelChanged += HandleAffinityLevelChanged;
        affinitySystemRef.OnAffinityGained += HandleAffinityGained;
        affinitySystemRef.OnAffinityLost += HandleAffinityLost;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events using stored reference
        if (affinitySystemRef != null)
        {
            affinitySystemRef.OnAffinityLevelChanged -= HandleAffinityLevelChanged;
            affinitySystemRef.OnAffinityGained -= HandleAffinityGained;
            affinitySystemRef.OnAffinityLost -= HandleAffinityLost;
        }
    }
    
    void Update()
    {
        if (isCompetitionActive)
        {
            // Check for time expiration
            if (Time.time - competitionStartTime >= competitionDuration)
            {
                EndCompetition();
            }
        }
    }
    
    /// <summary>
    /// Registers a player for the competition.
    /// </summary>
    /// <param name="player">The player to register.</param>
    /// <returns>True if registration was successful.</returns>
    public bool RegisterCompetitor(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogWarning("[COMPETITION] Cannot register null player.");
            return false;
        }
        
        if (competitors.Count >= maximumPlayers)
        {
            Debug.LogWarning("[COMPETITION] Competition is full.");
            return false;
        }
        
        // Check if player is already registered
        foreach (var competitor in competitors)
        {
            if (competitor.player == player)
            {
                Debug.LogWarning("[COMPETITION] Player already registered.");
                return false;
            }
        }
        
        CompetitorData newCompetitor = new CompetitorData
        {
            player = player,
            playerId = player.GetInstanceID().ToString(),
            playerName = player.name,
            score = 0,
            highestAffinityLevel = AffinityLevel.Stranger,
            successfulTethers = 0,
            betrayals = 0
        };
        
        competitors.Add(newCompetitor);
        OnCompetitorJoined?.Invoke(newCompetitor);
        
        Debug.Log($"[COMPETITION] {player.name} joined the competition.");
        return true;
    }
    
    /// <summary>
    /// Removes a player from the competition.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    public void UnregisterCompetitor(PlayerController player)
    {
        for (int i = competitors.Count - 1; i >= 0; i--)
        {
            if (competitors[i].player == player)
            {
                CompetitorData removed = competitors[i];
                competitors.RemoveAt(i);
                OnCompetitorLeft?.Invoke(removed);
                Debug.Log($"[COMPETITION] {player.name} left the competition.");
                return;
            }
        }
    }
    
    /// <summary>
    /// Starts the affinity competition.
    /// </summary>
    /// <param name="entityId">Optional entity ID to compete over (empty for overall).</param>
    public bool StartCompetition(string entityId = "")
    {
        if (competitors.Count < minimumPlayers)
        {
            Debug.LogWarning($"[COMPETITION] Not enough players. Need {minimumPlayers}, have {competitors.Count}.");
            return false;
        }
        
        if (isCompetitionActive)
        {
            Debug.LogWarning("[COMPETITION] Competition already in progress.");
            return false;
        }
        
        targetEntityId = entityId;
        competitionStartTime = Time.time;
        isCompetitionActive = true;
        
        // Reset all competitor scores
        foreach (var competitor in competitors)
        {
            competitor.score = 0;
            competitor.highestAffinityLevel = AffinityLevel.Stranger;
            competitor.successfulTethers = 0;
            competitor.betrayals = 0;
        }
        
        OnCompetitionStarted?.Invoke();
        
        string targetDesc = string.IsNullOrEmpty(targetEntityId) ? "all entities" : targetEntityId;
        Debug.Log($"[COMPETITION] Competition started! Target: {targetDesc}, Duration: {competitionDuration}s");
        
        return true;
    }
    
    /// <summary>
    /// Ends the competition and determines the winner.
    /// </summary>
    public void EndCompetition()
    {
        if (!isCompetitionActive) return;
        
        isCompetitionActive = false;
        
        // Determine winner
        CompetitorData winner = null;
        int highestScore = int.MinValue;
        
        foreach (var competitor in competitors)
        {
            if (competitor.score > highestScore)
            {
                highestScore = competitor.score;
                winner = competitor;
            }
        }
        
        if (winner != null)
        {
            OnCompetitionWon?.Invoke(winner);
            Debug.Log($"[COMPETITION] Competition ended! Winner: {winner.playerName} with {winner.score} points!");
        }
        else
        {
            Debug.Log("[COMPETITION] Competition ended with no winner.");
        }
        
        OnCompetitionEnded?.Invoke();
        
        // Log final standings
        LogStandings();
    }
    
    /// <summary>
    /// Reports a successful tether for a competitor.
    /// </summary>
    /// <param name="player">The player who completed the tether.</param>
    /// <param name="entityId">The entity that was tethered.</param>
    public void ReportSuccessfulTether(PlayerController player, string entityId)
    {
        if (!isCompetitionActive) return;
        
        // Check if this entity is relevant to the competition
        if (!string.IsNullOrEmpty(targetEntityId) && entityId != targetEntityId)
        {
            return;
        }
        
        CompetitorData competitor = FindCompetitor(player);
        if (competitor != null)
        {
            competitor.successfulTethers++;
            AddScore(competitor, tetherPoints);
            Debug.Log($"[COMPETITION] {competitor.playerName} completed a tether. +{tetherPoints} points.");
        }
    }
    
    /// <summary>
    /// Reports a betrayal (broken tether) for a competitor.
    /// </summary>
    /// <param name="player">The player who broke the tether.</param>
    /// <param name="entityId">The entity that was betrayed.</param>
    public void ReportBetrayal(PlayerController player, string entityId)
    {
        if (!isCompetitionActive) return;
        
        // Check if this entity is relevant to the competition
        if (!string.IsNullOrEmpty(targetEntityId) && entityId != targetEntityId)
        {
            return;
        }
        
        CompetitorData competitor = FindCompetitor(player);
        if (competitor != null)
        {
            competitor.betrayals++;
            AddScore(competitor, -betrayalPenalty);
            Debug.Log($"[COMPETITION] {competitor.playerName} betrayed an entity. -{betrayalPenalty} points.");
        }
    }
    
    /// <summary>
    /// Handles affinity level changes for scoring.
    /// </summary>
    private void HandleAffinityLevelChanged(string entityId, AffinityLevel newLevel)
    {
        if (!isCompetitionActive) return;
        
        // Check if this entity is relevant to the competition
        if (!string.IsNullOrEmpty(targetEntityId) && entityId != targetEntityId)
        {
            return;
        }
        
        // Award points for level milestones
        foreach (var competitor in competitors)
        {
            if (competitor.player != null && newLevel > competitor.highestAffinityLevel)
            {
                int points = GetPointsForLevel(newLevel);
                if (points > 0)
                {
                    competitor.highestAffinityLevel = newLevel;
                    AddScore(competitor, points);
                    Debug.Log($"[COMPETITION] {competitor.playerName} reached {newLevel}! +{points} points.");
                }
            }
        }
    }
    
    /// <summary>
    /// Handles affinity gained events.
    /// </summary>
    private void HandleAffinityGained(string entityId, float amount)
    {
        // Could add small continuous scoring based on affinity gains
    }
    
    /// <summary>
    /// Handles affinity lost events.
    /// </summary>
    private void HandleAffinityLost(string entityId, float amount)
    {
        // Could deduct points for significant affinity loss
    }
    
    /// <summary>
    /// Gets the points awarded for reaching a specific affinity level.
    /// </summary>
    private int GetPointsForLevel(AffinityLevel level)
    {
        switch (level)
        {
            case AffinityLevel.Acquainted:
                return acquaintedPoints;
            case AffinityLevel.Bonded:
                return bondedPoints;
            case AffinityLevel.Devoted:
                return devotedPoints;
            case AffinityLevel.Ascended:
                return ascendedPoints;
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// Adds score to a competitor.
    /// </summary>
    private void AddScore(CompetitorData competitor, int points)
    {
        competitor.score += points;
        competitor.score = Mathf.Max(0, competitor.score); // Don't go below 0
        OnScoreChanged?.Invoke(competitor, competitor.score);
    }
    
    /// <summary>
    /// Finds a competitor by player reference.
    /// </summary>
    private CompetitorData FindCompetitor(PlayerController player)
    {
        foreach (var competitor in competitors)
        {
            if (competitor.player == player)
            {
                return competitor;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Gets the current standings sorted by score.
    /// </summary>
    public List<CompetitorData> GetStandings()
    {
        List<CompetitorData> sorted = new List<CompetitorData>(competitors);
        sorted.Sort((a, b) => b.score.CompareTo(a.score));
        return sorted;
    }
    
    /// <summary>
    /// Gets the remaining time in the competition.
    /// </summary>
    public float GetRemainingTime()
    {
        if (!isCompetitionActive) return 0f;
        return Mathf.Max(0f, competitionDuration - (Time.time - competitionStartTime));
    }
    
    /// <summary>
    /// Logs the current standings to the console.
    /// </summary>
    public void LogStandings()
    {
        Debug.Log("[COMPETITION] === STANDINGS ===");
        List<CompetitorData> standings = GetStandings();
        for (int i = 0; i < standings.Count; i++)
        {
            CompetitorData c = standings[i];
            Debug.Log($"[COMPETITION] {i + 1}. {c.playerName}: {c.score} pts " +
                     $"(Tethers: {c.successfulTethers}, Betrayals: {c.betrayals}, " +
                     $"Best Level: {c.highestAffinityLevel})");
        }
    }
}

/// <summary>
/// Data class for tracking competitor information during affinity competition.
/// </summary>
[System.Serializable]
public class CompetitorData
{
    public PlayerController player;
    public string playerId;
    public string playerName;
    public int score;
    public AffinityLevel highestAffinityLevel;
    public int successfulTethers;
    public int betrayals;
}
