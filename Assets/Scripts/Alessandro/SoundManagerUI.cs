using UnityEngine;
using UnityEngine.UI;

public class SoundManagerUI : MonoBehaviour
{
    [Header("Volume Slider")]
    public Slider MasterSlider;
    public Slider MusicSlider;
    public Slider SFXSlider;
    private void Start()
    {
        if (MasterSlider != null)
        {
            MasterSlider.value = SoundManager.Instance.masterVolume;
            MasterSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        if (MusicSlider != null)
        {
            MusicSlider.value = SoundManager.Instance.musicVolume;
            MusicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (SFXSlider != null)
        {
            SFXSlider.value = SoundManager.Instance.sfxVolume;
            SFXSlider.onValueChanged.AddListener(SetSfxVolume);
        }
    }
    
    public void SetMasterVolume(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }
    
    public void SetSfxVolume(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }
}