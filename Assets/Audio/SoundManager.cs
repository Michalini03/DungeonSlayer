using UnityEngine;
using System;

public enum SoundType
{
    PLAYER_SWORD,
    PLAYER_JUMP,
    PLAYER_DEATH,
    PLAYER_FOOTSTEP,
    PLAYER_HIT,
    PLAYER_DASH,
    SEEKER_FLYING,
    SEEKER_DEATH,
    SEEKER_ATTACK,
    SEEKER_HIT,
    SKELETON_FOOTSTEP,
    SKELETON_DEATH,
    SKELETON_HIT,
    RAT_HIT,
    RAT_DEATH,
    GOBLIN_HIT,
    GOBLIN_DEATH,
    RAT_ATTACK,
    BOSS_HIT,
    BOSS_DEATH,
    BOSS_TELEPORT,
    BOSS_SPELL,
    BOSS_ATTACK,
    BOSS_SPAWN,
    BOSS_SHIELD
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]

public class SoundManager : MonoBehaviour {


    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(clip, volume);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds {   get => sounds;    }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}
