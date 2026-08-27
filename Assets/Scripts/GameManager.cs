using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public MazeGenerator mazeGenerator;
    public GameObject playerPrefab;
    public Text scoreText;

    [Header("Camera")]
    public Vector3 cameraOffset = new Vector3(0f, 6f, -6f);

    private GameObject playerInstance;
    private int score = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mazeGenerator == null)
            mazeGenerator = FindFirstObjectByType<MazeGenerator>();

        if (mazeGenerator != null)
            mazeGenerator.OnMazeGenerated += HandleMazeGenerated;

        SpawnPlayer();
        UpdateScoreUI();
    }

    void OnDestroy()
    {
        if (mazeGenerator != null)
            mazeGenerator.OnMazeGenerated -= HandleMazeGenerated;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null || mazeGenerator == null)
            return;

        Vector3 spawnPos = mazeGenerator.StartWorldPosition + Vector3.up * 0.5f;
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraFollow follow = cam.GetComponent<CameraFollow>();
            if (follow == null)
                follow = cam.gameObject.AddComponent<CameraFollow>();

            follow.offset = cameraOffset;
            follow.target = playerInstance.transform;
        }
    }

    // Called whenever the maze regenerates (including the very first time).
    void HandleMazeGenerated()
    {
        if (playerInstance == null || mazeGenerator == null)
            return;

        Vector3 spawnPos = mazeGenerator.StartWorldPosition + Vector3.up * 0.5f;

        Rigidbody rb = playerInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = spawnPos;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        playerInstance.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
    }

    public void PlayerReachedFinish()
    {
        score++;
        UpdateScoreUI();
        mazeGenerator.IncreaseMaze(2);
        mazeGenerator.GenerateAndDraw();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Simple always-on score readout so the score is visible even if no
    // UnityEngine.UI Text has been wired up in the scene. Safe to leave on;
    // remove this method if you hook up scoreText via a Canvas instead.
    //void OnGUI()
    //{
    //    int pad = 16;
    //    int w = 220;
    //    int h = 40;
    //    GUI.Box(new Rect(pad, pad, w, h), "");
    //    GUIStyle style = new GUIStyle(GUI.skin.label)
    //    {
    //        fontSize = 20,
    //        alignment = TextAnchor.MiddleCenter
    //    };
    //    GUI.Label(new Rect(pad, pad, w, h), "Score: " + score, style);
    //}
}
