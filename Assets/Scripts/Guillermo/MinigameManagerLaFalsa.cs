using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManagerLaFalsa : MonoBehaviour
{
    public static MinigameManagerLaFalsa Instance;

    [Header("Scenes")]
    public List<string> minigameScenes;
    public string endScene;

    int currentMinigameIndex = -1;
    public bool sequenceRunning = false;

    NPC_Interaction npcInteraction;

    MeshRenderer[] meshRenderers;
    SkinnedMeshRenderer[] skinnedMeshRenderers;
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

        meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        sphereCollider = GetComponentInChildren<SphereCollider>(true);

        if (npcInteraction == null)
            Debug.LogWarning("NPC_Interaction missing on MinigameManagerLaFalsa object.");

        if (meshRenderers.Length == 0 && skinnedMeshRenderers.Length == 0)
            Debug.LogWarning("No MeshRenderer or SkinnedMeshRenderer found on NPC.");

        if (sphereCollider == null)
            Debug.LogWarning("SphereCollider not found on MinigameManagerLaFalsa object.");
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
        // Static meshes
        foreach (var r in meshRenderers)
            if (r != null)
                r.enabled = active;

        // Skinned meshes (characters)
        foreach (var r in skinnedMeshRenderers)
            if (r != null)
                r.enabled = active;

        // Interaction collider
        if (sphereCollider != null)
            sphereCollider.enabled = active;
    }
}
