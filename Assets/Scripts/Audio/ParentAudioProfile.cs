using UnityEngine;

/// <summary>
/// Defines the audio profile for a specific Parent entity.
/// Each Parent has unique ambient sounds, warning sounds, and temperament-specific audio.
/// </summary>
[CreateAssetMenu(fileName = "NewParentAudio", menuName = "Lineage of Arcane/Parent Audio Profile")]
public class ParentAudioProfile : ScriptableObject
{
    [Header("Parent Identification")]
    public string parentName;
    public string description;
    
    [Header("Ambient Sounds")]
    [Tooltip("The ambient sound that plays while tethered to this Parent.")]
    public AudioClip ambientSound;
    [Range(0, 1)] public float ambientVolume = 0.8f;
    
    [Header("Summon Sounds")]
    [Tooltip("Sound played when this Parent is summoned.")]
    public AudioClip summonSound;
    [Tooltip("Sound played when this Parent's tether breaks.")]
    public AudioClip tetherBreakSound;
    [Tooltip("Sound played when this Parent goes rampant.")]
    public AudioClip rampantSound;
    
    [Header("Temperament Sounds")]
    [Tooltip("Warning sound when player is about to violate temperament.")]
    public AudioClip temperamentWarningSound;
    [Tooltip("Sound played when player is punished for violating temperament.")]
    public AudioClip punishmentSound;
    [Tooltip("Sound played when player pleases the Parent's temperament.")]
    public AudioClip pleasedSound;
    
    [Header("Attack Sounds")]
    [Tooltip("Sound played when this Parent attacks while rampant.")]
    public AudioClip attackSound;
    [Tooltip("Sound played for area attacks.")]
    public AudioClip areaAttackSound;
    
    [Header("Environmental Sounds")]
    [Tooltip("Sound played when environmental shift activates.")]
    public AudioClip environmentActivateSound;
    [Tooltip("Looping environmental effect sound.")]
    public AudioClip environmentLoopSound;
    [Tooltip("Sound played when environmental shift deactivates.")]
    public AudioClip environmentDeactivateSound;
    
    [Header("Audio Settings")]
    [Range(0.5f, 1.5f)] public float pitchVariation = 0.1f;
    public bool spatialize = true;
    
    /// <summary>
    /// Gets a slightly randomized pitch for variation.
    /// </summary>
    public float GetRandomizedPitch()
    {
        return 1f + Random.Range(-pitchVariation, pitchVariation);
    }
}
