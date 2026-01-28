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
    //public AudioClip footsteps1;
    //public AudioClip footsteps2;
    public AudioClip jump;
    public AudioClip dash;
    public AudioClip hound;
    public AudioClip mummy;
    public AudioClip sphinx;
    public AudioClip bow;
    public AudioClip sword;
    public AudioClip magic;
    public AudioClip boss;

    [Header("Soundtracks")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;
    public AudioClip bossMusic;


    private AudioClip currentMusic;
    private bool bossMusicPlaying = false;

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

    public void PlayMusicForScene(int buildIndex)
    {
        if (bossMusicPlaying) return;

        AudioClip target = null;

        // Menu scenes
        if (buildIndex == 0 || buildIndex == 1)
        {
            target = menuMusic;
        }
        // Gameplay scenes
        else if (buildIndex >= 2 && buildIndex <= 4)
        {
            target = levelMusic;
        }

        if (target == null || target == currentMusic) return;

        currentMusic = target;
        musicSource.clip = target;
        musicSource.volume = masterVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bossMusicPlaying = false;
        PlayMusicForScene(scene.buildIndex);

    }

    public void PlayBossMusic()
    {
        if (bossMusicPlaying || bossMusic == null) return;

        bossMusicPlaying = true;
        currentMusic = bossMusic;

        musicSource.Stop();
        musicSource.clip = bossMusic;
        musicSource.volume = masterVolume;
        musicSource.loop = true;
        musicSource.Play();
    }
}
