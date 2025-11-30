using UnityEngine;

/// <summary>
/// Manages all game audio including ambient sounds, effect sounds, and Parent-specific audio.
/// Uses a singleton pattern for easy access throughout the game.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambientSource;
    public AudioSource sfxSource;
    public AudioSource tetherSource;
    
    [Header("Volume Settings")]
    [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float musicVolume = 0.7f;
    [Range(0, 1)] public float ambientVolume = 0.8f;
    [Range(0, 1)] public float sfxVolume = 1f;
    [Range(0, 1)] public float tetherVolumeMultiplier = 0.5f;
    
    [Header("Default Sounds")]
    public AudioClip tetherFormSound;
    public AudioClip tetherBreakSound;
    public AudioClip tetherPulseSound;
    public AudioClip violationWarningSound;
    public AudioClip punishmentSound;
    public AudioClip playerDamageSound;
    public AudioClip playerDeathSound;
    public AudioClip rampantStartSound;
    public AudioClip rampantEndSound;
    
    [Header("Sanity Effects")]
    public AudioClip[] lowSanityWhispers;
    public float lowSanityThreshold = 30f;
    public float whisperInterval = 5f;
    
    private float lastWhisperTime;
    private bool isLowSanity = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeAudioSources();
    }
    
    void InitializeAudioSources()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            musicSource = CreateAudioSource("Music");
            musicSource.loop = true;
        }
        
        if (ambientSource == null)
        {
            ambientSource = CreateAudioSource("Ambient");
            ambientSource.loop = true;
        }
        
        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource("SFX");
        }
        
        if (tetherSource == null)
        {
            tetherSource = CreateAudioSource("Tether");
            tetherSource.loop = true;
        }
        
        UpdateVolumes();
    }
    
    AudioSource CreateAudioSource(string sourceName)
    {
        GameObject sourceObj = new GameObject($"AudioSource_{sourceName}");
        sourceObj.transform.SetParent(transform);
        return sourceObj.AddComponent<AudioSource>();
    }
    
    void Update()
    {
        // Handle low sanity whisper effects
        if (isLowSanity && Time.time - lastWhisperTime > whisperInterval)
        {
            PlayRandomWhisper();
        }
    }
    
    /// <summary>
    /// Updates all audio source volumes based on current settings.
    /// </summary>
    public void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        if (ambientSource != null)
            ambientSource.volume = ambientVolume * masterVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
        if (tetherSource != null)
            tetherSource.volume = sfxVolume * masterVolume * tetherVolumeMultiplier;
    }
    
    // ==================== Tether Sounds ====================
    
    /// <summary>
    /// Plays the sound when a tether is formed.
    /// </summary>
    public void PlayTetherForm()
    {
        if (tetherFormSound != null)
        {
            sfxSource.PlayOneShot(tetherFormSound);
        }
        Debug.Log("[Audio] Tether form sound played.");
    }
    
    /// <summary>
    /// Plays the sound when a tether breaks.
    /// </summary>
    public void PlayTetherBreak()
    {
        if (tetherBreakSound != null)
        {
            sfxSource.PlayOneShot(tetherBreakSound);
        }
        Debug.Log("[Audio] Tether break sound played.");
    }
    
    /// <summary>
    /// Starts the continuous tether pulse sound.
    /// </summary>
    public void StartTetherPulse()
    {
        if (tetherPulseSound != null && tetherSource != null)
        {
            tetherSource.clip = tetherPulseSound;
            tetherSource.Play();
        }
    }
    
    /// <summary>
    /// Stops the tether pulse sound.
    /// </summary>
    public void StopTetherPulse()
    {
        if (tetherSource != null)
        {
            tetherSource.Stop();
        }
    }
    
    // ==================== Parent/Combat Sounds ====================
    
    /// <summary>
    /// Plays a warning sound when the player violates a Parent's temperament.
    /// </summary>
    public void PlayViolationWarning()
    {
        if (violationWarningSound != null)
        {
            sfxSource.PlayOneShot(violationWarningSound);
        }
        Debug.Log("[Audio] Violation warning sound played.");
    }
    
    /// <summary>
    /// Plays the punishment sound when a Parent punishes the player.
    /// </summary>
    public void PlayPunishment()
    {
        if (punishmentSound != null)
        {
            sfxSource.PlayOneShot(punishmentSound);
        }
        Debug.Log("[Audio] Punishment sound played.");
    }
    
    /// <summary>
    /// Plays the rampant state start sound.
    /// </summary>
    public void PlayRampantStart()
    {
        if (rampantStartSound != null)
        {
            sfxSource.PlayOneShot(rampantStartSound);
        }
        Debug.Log("[Audio] Rampant start sound played.");
    }
    
    /// <summary>
    /// Plays the rampant state end sound.
    /// </summary>
    public void PlayRampantEnd()
    {
        if (rampantEndSound != null)
        {
            sfxSource.PlayOneShot(rampantEndSound);
        }
        Debug.Log("[Audio] Rampant end sound played.");
    }
    
    // ==================== Player Sounds ====================
    
    /// <summary>
    /// Plays the player damage sound.
    /// </summary>
    public void PlayPlayerDamage()
    {
        if (playerDamageSound != null)
        {
            sfxSource.PlayOneShot(playerDamageSound);
        }
    }
    
    /// <summary>
    /// Plays the player death sound.
    /// </summary>
    public void PlayPlayerDeath()
    {
        if (playerDeathSound != null)
        {
            sfxSource.PlayOneShot(playerDeathSound);
        }
    }
    
    // ==================== Ambient/Environment Sounds ====================
    
    /// <summary>
    /// Sets the ambient sound based on the active Parent.
    /// </summary>
    /// <param name="profile">The audio profile of the Parent.</param>
    public void SetParentAmbient(ParentAudioProfile profile)
    {
        if (profile != null && profile.ambientSound != null)
        {
            ambientSource.clip = profile.ambientSound;
            ambientSource.Play();
            Debug.Log($"[Audio] Set ambient sound to {profile.parentName}.");
        }
    }
    
    /// <summary>
    /// Stops the current ambient sound.
    /// </summary>
    public void StopAmbient()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }
    
    // ==================== Sanity Effects ====================
    
    /// <summary>
    /// Updates the sanity-based audio effects.
    /// </summary>
    /// <param name="sanityPercent">Current sanity as a percentage (0-100).</param>
    public void UpdateSanityAudio(float sanityPercent)
    {
        isLowSanity = sanityPercent < lowSanityThreshold;
        
        // Add distortion effects at low sanity
        if (isLowSanity)
        {
            // Could add audio filter effects here
            float distortionAmount = 1f - (sanityPercent / lowSanityThreshold);
            // ApplyDistortion(distortionAmount);
        }
    }
    
    void PlayRandomWhisper()
    {
        if (lowSanityWhispers != null && lowSanityWhispers.Length > 0)
        {
            int randomIndex = Random.Range(0, lowSanityWhispers.Length);
            AudioClip whisper = lowSanityWhispers[randomIndex];
            if (whisper != null)
            {
                sfxSource.PlayOneShot(whisper);
            }
        }
        lastWhisperTime = Time.time;
    }
    
    // ==================== Utility Methods ====================
    
    /// <summary>
    /// Plays a one-shot sound effect.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeMultiplier);
        }
    }
    
    /// <summary>
    /// Sets the master volume.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    /// <summary>
    /// Sets the music volume.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    /// <summary>
    /// Sets the SFX volume.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
}
