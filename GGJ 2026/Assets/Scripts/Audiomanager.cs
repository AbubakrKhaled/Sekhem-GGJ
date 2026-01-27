using UnityEngine;
using UnityEngine.SceneManagement;

public class Audiomanager : MonoBehaviour
{
    public static Audiomanager Instance;

    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;

    [Header("Audio Clips")]
    public AudioClip footsteps1;
    public AudioClip footsteps2;
    public AudioClip jump;
    public AudioClip dash;
    public AudioClip playerhurt1;
    public AudioClip playerhurt2;
    public AudioClip enemyhurt1;
    public AudioClip enemyhurt2;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, masterVolume);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.volume = masterVolume;
        musicSource.loop = true;
        musicSource.Play();
    }
}
