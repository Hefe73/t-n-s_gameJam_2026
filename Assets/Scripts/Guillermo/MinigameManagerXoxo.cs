using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManagerXoxo : MonoBehaviour
{
    public static MinigameManagerXoxo Instance;
    public bool isHealed = false;
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
        // Singleton per NPC instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;

        npcInteraction = GetComponent<NPC_Interaction>();

        meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        sphereCollider = GetComponentInChildren<SphereCollider>(true);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
            Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 🔥 Kill AFTER sequence ends AND a new scene loads
        if (!sequenceRunning)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E) &&
            npcInteraction != null &&
            npcInteraction.playerIsInArea && !isHealed)
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

        // 🔑 NOW it becomes persistent
        DontDestroyOnLoad(gameObject);

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

        SceneManager.LoadScene(minigameScenes[currentMinigameIndex]);
    }

    void EndSequence()
    {
        Debug.Log("All minigames completed!");

        sequenceRunning = false;
        SetNPCActive(true);
        isHealed = true;
        GameLoopManager.Instance.patientsHealed++;

        if (!string.IsNullOrEmpty(endScene))
            SceneManager.LoadScene(endScene);
    }

    public void MinigameFinished(float delay)
    {
        StartCoroutine(LoadNextWithDelay(delay));
    }

    System.Collections.IEnumerator LoadNextWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextMinigame();
    }

    void SetNPCActive(bool active)
    {
        foreach (var r in meshRenderers)
            if (r) r.enabled = active;

        foreach (var r in skinnedMeshRenderers)
            if (r) r.enabled = active;

        if (sphereCollider)
            sphereCollider.enabled = active;
    }
}
