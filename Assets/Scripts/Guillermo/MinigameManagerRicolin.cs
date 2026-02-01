using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManagerRicolin : MonoBehaviour
{
    public static MinigameManagerRicolin Instance;

    [Header("Scenes")]
    public List<string> minigameScenes;
    public string endScene;

    int currentMinigameIndex = -1;
    public bool sequenceRunning = false;

    NPC_Interaction npcInteraction;
    MeshRenderer meshRenderer;
    SphereCollider sphereCollider;

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

        // Components on SAME object (or children)
        npcInteraction = GetComponent<NPC_Interaction>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        sphereCollider = GetComponentInChildren<SphereCollider>();

        if (npcInteraction == null)
            Debug.LogWarning("NPC_Interaction missing on MinigameManagerRicolin object.");

        if (meshRenderer == null)
            Debug.LogWarning("MeshRenderer not found on MinigameManagerRicolin object.");

        if (sphereCollider == null)
            Debug.LogWarning("SphereCollider not found on MinigameManagerRicolin object.");
    }

    void Update()
    {
        if (sequenceRunning)
            return;

        if (Input.GetKeyDown(KeyCode.E) &&
            npcInteraction != null &&
            npcInteraction.playerIsInArea)
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
        SetNPCActive(false);

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
        SetNPCActive(true);

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

    // ---------------- HELPERS ----------------

    void SetNPCActive(bool active)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = active;

        if (sphereCollider != null)
            sphereCollider.enabled = active;
    }
}
