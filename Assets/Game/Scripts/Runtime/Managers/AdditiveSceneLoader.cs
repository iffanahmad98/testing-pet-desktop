using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages additive scene loading and switching between Pet and Farm scenes
/// This keeps managers persistent and avoids destroying/recreating components
/// </summary>
public class AdditiveSceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string petSceneName = "Pet";
    [SerializeField] private string farmSceneName = "FarmGame";

    [Header("Current State")]
    [SerializeField] private string currentActiveScene;

    private Dictionary<string, GameObject> loadedSceneRoots = new Dictionary<string, GameObject>();
    private Dictionary<string, Scene> loadedScenes = new Dictionary<string, Scene>();
    private bool isTransitioning = false;

    private static AdditiveSceneLoader _instance;
    public static AdditiveSceneLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AdditiveSceneLoader>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[AdditiveSceneLoader] Initialized and marked as DontDestroyOnLoad");
    }

    private void Start()
    {
        // Detect the currently opened scene as the initial active scene
        Scene currentScene = SceneManager.GetActiveScene();
        currentActiveScene = currentScene.name;

        Debug.Log($"[AdditiveSceneLoader] Initial scene detected: {currentActiveScene}");

        // Register the current scene's root objects
        RegisterCurrentSceneRoot(currentScene);
    }

    /// <summary>
    /// Register the currently opened scene's root objects
    /// </summary>
    private void RegisterCurrentSceneRoot(Scene scene)
    {
        if (loadedScenes.ContainsKey(scene.name))
        {
            Debug.LogWarning($"[AdditiveSceneLoader] Scene '{scene.name}' already registered!");
            return;
        }

        loadedScenes[scene.name] = scene;

        // Find root GameObjects in the current scene
        GameObject[] rootObjects = scene.GetRootGameObjects();

        // Create a parent to hold all root objects for easy show/hide
        GameObject sceneRoot = new GameObject($"[{scene.name}_Root]");
        SceneManager.MoveGameObjectToScene(sceneRoot, scene);

        foreach (GameObject rootObj in rootObjects)
        {
            if (rootObj != sceneRoot && rootObj != gameObject) // Don't parent AdditiveSceneLoader itself
            {
                rootObj.transform.SetParent(sceneRoot.transform, true);
            }
        }

        loadedSceneRoots[scene.name] = sceneRoot;

        Debug.Log($"[AdditiveSceneLoader] Registered current scene '{scene.name}' with {rootObjects.Length} root objects");
    }

    /// <summary>
    /// Load a scene additively
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    /// <param name="setActive">Whether to set this scene as active after loading</param>
    public IEnumerator LoadSceneAdditive(string sceneName, bool setActive = true)
    {
        if (loadedScenes.ContainsKey(sceneName))
        {
            Debug.LogWarning($"[AdditiveSceneLoader] Scene '{sceneName}' is already loaded!");
            if (setActive)
            {
                ShowScene(sceneName);
            }
            yield break;
        }

        Debug.Log($"[AdditiveSceneLoader] Loading scene '{sceneName}' additively...");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (asyncLoad == null)
        {
            Debug.LogError($"[AdditiveSceneLoader] Failed to load scene '{sceneName}'!");
            yield break;
        }

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Get the loaded scene
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        loadedScenes[sceneName] = loadedScene;

        // Find root GameObjects in the loaded scene
        GameObject[] rootObjects = loadedScene.GetRootGameObjects();

        // Create a parent to hold all root objects for easy show/hide
        GameObject sceneRoot = new GameObject($"[{sceneName}_Root]");
        SceneManager.MoveGameObjectToScene(sceneRoot, loadedScene);

        foreach (GameObject rootObj in rootObjects)
        {
            if (rootObj != sceneRoot) // Don't parent to itself
            {
                rootObj.transform.SetParent(sceneRoot.transform, true);
            }
        }

        loadedSceneRoots[sceneName] = sceneRoot;

        Debug.Log($"[AdditiveSceneLoader] Scene '{sceneName}' loaded successfully with {rootObjects.Length} root objects");

        if (setActive)
        {
            ShowScene(sceneName);
        }
        else
        {
            HideScene(sceneName);
        }
    }

    /// <summary>
    /// Unload a scene that was loaded additively
    /// </summary>
    public IEnumerator UnloadScene(string sceneName)
    {
        if (!loadedScenes.ContainsKey(sceneName))
        {
            Debug.LogWarning($"[AdditiveSceneLoader] Scene '{sceneName}' is not loaded!");
            yield break;
        }

        Debug.Log($"[AdditiveSceneLoader] Unloading scene '{sceneName}'...");

        // Save game state before unloading
        var monsterManager = ServiceLocator.Get<MonsterManager>();
        monsterManager?.SaveAllMonsters();

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        loadedScenes.Remove(sceneName);
        loadedSceneRoots.Remove(sceneName);

        Debug.Log($"[AdditiveSceneLoader] Scene '{sceneName}' unloaded successfully");
    }

    /// <summary>
    /// Show a loaded scene by activating its root GameObject
    /// </summary>
    public void ShowScene(string sceneName)
    {
        if (!loadedSceneRoots.ContainsKey(sceneName))
        {
            Debug.LogWarning($"[AdditiveSceneLoader] Cannot show scene '{sceneName}' - not loaded!");
            return;
        }

        GameObject sceneRoot = loadedSceneRoots[sceneName];
        if (sceneRoot != null)
        {
            sceneRoot.SetActive(true);
            currentActiveScene = sceneName;
            Debug.Log($"[AdditiveSceneLoader] Scene '{sceneName}' is now visible");

            Scene targetScene = sceneRoot.scene;
            if (targetScene.IsValid() && targetScene.isLoaded && SceneManager.GetActiveScene() != targetScene)
            {
                // MonsterManager.instance.audio.StopAllSFX();
                SceneManager.SetActiveScene(targetScene);
            }
        }
    }

    /// <summary>
    /// Hide a loaded scene by deactivating its root GameObject
    /// </summary>
    public void HideScene(string sceneName)
    {
        if (!loadedSceneRoots.ContainsKey(sceneName))
        {
            Debug.LogWarning($"[AdditiveSceneLoader] Cannot hide scene '{sceneName}' - not loaded!");
            return;
        }

        GameObject sceneRoot = loadedSceneRoots[sceneName];
        if (sceneRoot != null)
        {
            sceneRoot.SetActive(false);
            Debug.Log($"[AdditiveSceneLoader] Scene '{sceneName}' is now hidden");
        }
    }

    /// <summary>
    /// Switch from current scene to target scene
    /// Loads target scene if not loaded, then hides current and shows target
    /// </summary>
    public void SwitchToScene(string targetSceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[AdditiveSceneLoader] Scene transition already in progress!");
            return;
        }

        StartCoroutine(SwitchSceneCoroutine(targetSceneName));
    }

    private IEnumerator SwitchSceneCoroutine(string targetSceneName)
    {
        isTransitioning = true;

        Debug.Log($"[AdditiveSceneLoader] Switching to scene '{targetSceneName}'...");

        // Save current game state before switching
        var monsterManager = ServiceLocator.Get<MonsterManager>();
        monsterManager?.SaveAllMonsters();

        // Hide current active scene
        if (!string.IsNullOrEmpty(currentActiveScene) && currentActiveScene != targetSceneName)
        {
            HideScene(currentActiveScene);
        }

        // Load target scene if not already loaded
        if (!loadedScenes.ContainsKey(targetSceneName))
        {
            yield return StartCoroutine(LoadSceneAdditive(targetSceneName, true));
        }
        else
        {
            // Just show the target scene if already loaded
            ShowScene(targetSceneName);
        }

        isTransitioning = false;

        Debug.Log($"[AdditiveSceneLoader] Successfully switched to scene '{targetSceneName}'");
    }

    /// <summary>
    /// Public methods to switch to specific scenes
    /// </summary>
    public void SwitchToPetScene()
    {
        SwitchToScene(petSceneName);
    }

    public void SwitchToFarmScene()
    {
        SwitchToScene(farmSceneName);
    }

    /// <summary>
    /// Check if a scene is currently loaded
    /// </summary>
    public bool IsSceneLoaded(string sceneName)
    {
        return loadedScenes.ContainsKey(sceneName);
    }

    /// <summary>
    /// Check if a scene is currently visible
    /// </summary>
    public bool IsSceneVisible(string sceneName)
    {
        if (!loadedSceneRoots.ContainsKey(sceneName))
            return false;

        GameObject sceneRoot = loadedSceneRoots[sceneName];
        return sceneRoot != null && sceneRoot.activeSelf;
    }

    /// <summary>
    /// Get the currently active/visible scene name
    /// </summary>
    public string GetCurrentActiveScene()
    {
        return currentActiveScene;
    }
}
