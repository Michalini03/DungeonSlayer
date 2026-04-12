using UnityEngine;

public class SkeletonPlayFoot : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Transform playerTransform; // Drag your player here in the Inspector
    [SerializeField] private float maxDistance = 20f;   // Distance where volume reaches 0
    [SerializeField] private float maxVolume = 0.05f;   // The volume when right next to the player

    public void PlaySound()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform is not assigned on " + gameObject.name);
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float volumeDropoff = Mathf.Clamp01(1f - (distance / maxDistance));
        float finalVolume = maxVolume * volumeDropoff;

        if (finalVolume > 0f)
        {
            SoundManager.PlaySound(SoundType.SKELETON_FOOTSTEP, finalVolume);
        }
    }
}
