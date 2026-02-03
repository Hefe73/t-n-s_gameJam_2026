using System.Collections.Generic;
using UnityEngine;

public class BleedingGameManager : MonoBehaviour
{


    [SerializeField] private int woundID;

    public AudioSource bandageSound;
    public PlayUISound uiSoundPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        woundID = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // If it is not a wound, ignore
                if (!hit.collider.CompareTag("Wound"))
                {
                    return;
                }

                var gusher = hit.collider.gameObject.GetComponentInChildren<BloodGusher>();
                if (gusher != null && woundID == gusher.id_)
                {
                    Debug.Log("Correct wound!");
                    gusher.gameObject.SetActive(false);
                    woundID++;
                    bandageSound.volume = Random.Range(0.85f, 1f);
                    bandageSound.pitch = Random.Range(1f - 0.15f, 1f + 0.15f);
                    bandageSound.PlayOneShot(bandageSound.clip);
                    if (woundID >= 3)
                    {
                        uiSoundPlayer.PlaySoundWin();
                        MinigameManagerChoni.Instance.MinigameFinished(1.5f);
                        Debug.Log("Game finished");
                    }
                }
                else
                {
                    Debug.Log("Incorrect wound. Restart");
                    woundID = 0;
                    restartGame();
                }
            }
        }
    }

    void restartGame()
    {
        var gushers = Resources.FindObjectsOfTypeAll<BloodGusher>();
        foreach (var gusher in gushers)
        {
            gusher.gameObject.SetActive(true);
        }

        uiSoundPlayer.PlaySoundLoose();
    }
}