using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The music track you want to play.")]
    [SerializeField] private AudioClip musicClip;

    [Tooltip("Adjust the volume of the background music.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Get the AudioSource component that is automatically attached
        audioSource = GetComponent<AudioSource>();

        // Configure the AudioSource
        audioSource.loop = true;          // Make it loop
        audioSource.playOnAwake = false;  // We will handle playing it manually
    }

    private void Start()
    {
        if (musicClip != null)
        {
            PlayMusic(musicClip, volume);
        }
        else
        {
            Debug.LogWarning("No AudioClip assigned to the BackgroundMusicManager!");
        }
    }

    /// <summary>
    /// Starts playing a specific track at a specific volume.
    /// </summary>
    public void PlayMusic(AudioClip clip, float startVolume)
    {
        audioSource.clip = clip;
        audioSource.volume = startVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Call this method from UI sliders or other scripts to change the volume dynamically.
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume); // Ensures volume stays between 0 and 1
        audioSource.volume = volume;
    }

#if UNITY_EDITOR
    // This allows you to drag the volume slider in the Unity Inspector 
    // during Play Mode and hear the volume change in real-time!
    private void OnValidate()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
#endif
}