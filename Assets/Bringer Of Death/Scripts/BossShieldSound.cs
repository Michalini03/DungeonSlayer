using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BossShieldSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        // Grab the AudioSource component on this object
        audioSource = GetComponent<AudioSource>();
    }

    // Called every time the shield is SetActive(true)
    void OnEnable()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    // Called every time the shield is SetActive(false)
    void OnDisable()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}