using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public float stepDuration = 1f;

    void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(stepDuration);

        countdownText.text = "2";
        yield return new WaitForSeconds(stepDuration);

        countdownText.text = "1";
        yield return new WaitForSeconds(stepDuration);

        countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        countdownText.gameObject.SetActive(false);

        StartMinigame();
    }

    void StartMinigame()
    {
        // 🔎 Find ANY minigame in this scene
        var startable = FindFirstObjectByType<MonoBehaviour>() as IMinigameStartable;

        if (startable != null)
            startable.StartMinigame();
        else
            Debug.LogWarning("No IMinigameStartable found in scene!");
    }
}
