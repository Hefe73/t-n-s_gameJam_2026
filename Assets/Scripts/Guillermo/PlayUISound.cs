using UnityEngine;

public class PlayUISound : MonoBehaviour
{
    public AudioSource uiSound1;
    public AudioSource uiSound2;
    public AudioSource winMinigame;
    public AudioSource looseMinigame;
    public AudioSource microgameCleared;
    
    private float pitchRange = 0.05f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySound1()
    {
        uiSound1.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
        uiSound1.PlayOneShot(uiSound1.clip);
    }
    public void PlaySound2()
    {
        uiSound2.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
        uiSound2.PlayOneShot(uiSound2.clip);
    }
    
    public void PlaySoundWin()
    {
        //winMinigame.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
        winMinigame.PlayOneShot(winMinigame.clip);
    }
    public void PlaySoundLoose()
    {
        //looseMinigame.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
        looseMinigame.PlayOneShot(looseMinigame.clip);
    }

    public void PlaySoundMicrogamecleared()
    {
        microgameCleared.PlayOneShot(microgameCleared.clip);
    }
}
