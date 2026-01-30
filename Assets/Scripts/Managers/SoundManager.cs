using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
   public static SoundManager Instance;

    [Header("Audio Sources")] public AudioSource musicSource;

    [Header("Volume Settings")] [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgSfxVolume = 1f;
    
    [Header("Music Library")]
    public AudioClip[] musicLibrary; 
    
    // 0 -> Menus
    // 1 -> Combat uwu robocatgirl
    // 2 -> Combat mexican
    // 3 -> Combat fighter
    // 4 -> Celebration

    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";
    private const string BGSFXKey = "BackgroundEffectsVolume";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();
        ApplyMusicVolume();
        
        PlayMusic(musicLibrary[0]);
    }

    private void Update()
    {
        ApplyMusicVolume();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
        musicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFXKey, 1f);
        bgSfxVolume = PlayerPrefs.GetFloat(BGSFXKey, 1f);
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MasterKey, masterVolume);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.SetFloat(SFXKey, sfxVolume);
        PlayerPrefs.SetFloat(BGSFXKey, bgSfxVolume);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        ApplyMusicVolume();
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        ApplyMusicVolume();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        SaveVolumeSettings();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;
    }

    public void PlayMusic(AudioClip newClip, float fadeDuration = 1f)
    {
        StartCoroutine(FadeInNewMusic(newClip, fadeDuration));
    }

    private IEnumerator FadeInNewMusic(AudioClip newClip, float duration)
    {
        if (musicSource.isPlaying)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(masterVolume * musicVolume, 0, t / duration);
                yield return null;
            }

            musicSource.Stop();
        }

        musicSource.clip = newClip;
        musicSource.Play();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, masterVolume * musicVolume, t / duration);
            yield return null;
        }

        ApplyMusicVolume();
    }
}