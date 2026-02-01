using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("Scenes")] public List<string> minigameScenes;
    public string endScene;

    int currentMinigameIndex = -1;
    public bool sequenceRunning = false;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!sequenceRunning && Input.GetKeyDown(KeyCode.F))
        {
            StartMinigames();
        }
    }

    public void StartMinigames()
    {
        if (minigameScenes.Count == 0)
        {
            Debug.LogWarning("No minigames assigned!");
            return;
        }

        sequenceRunning = true;
        currentMinigameIndex = -1;
        LoadNextMinigame();
    }

    void LoadNextMinigame()
    {
        currentMinigameIndex++;

        if (currentMinigameIndex >= minigameScenes.Count)
        {
            EndSequence();
            return;
        }

        string sceneName = minigameScenes[currentMinigameIndex];
        Debug.Log("Loading minigame: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    void EndSequence()
    {
        Debug.Log("All minigames completed!");
        sequenceRunning = false;

        if (!string.IsNullOrEmpty(endScene))
            SceneManager.LoadScene(endScene);
    }

    // CALLED BY MINIGAMES
    public void MinigameFinished(float delay)
    {
        Debug.Log($"Minigame finished! Loading next in {delay} seconds.");
        StartCoroutine(LoadNextWithDelay(delay));
    }

    System.Collections.IEnumerator LoadNextWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextMinigame();
    }
}