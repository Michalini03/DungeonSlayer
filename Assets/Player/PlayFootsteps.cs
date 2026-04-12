using UnityEngine;

public class PlayFootsteps : MonoBehaviour
{
    public void PlaySound()
    {
        SoundManager.PlaySound(SoundType.PLAYER_FOOTSTEP, 0.3f);
    }
}
