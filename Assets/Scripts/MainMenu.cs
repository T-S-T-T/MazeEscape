using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Name of the gameplay scene to load when Play is pressed.")]
    public string gameSceneName = "MainGame";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    // Draws the two menu buttons (Play / Quit) using IMGUI so the menu works
    // out of the box with no extra Canvas/EventSystem setup required.
    // Feel free to replace this with a styled UnityEngine.UI Canvas later —
    // just call PlayGame()/QuitGame() from your Button OnClick events instead.
    void OnGUI()
    {
        int buttonWidth = 220;
        int buttonHeight = 60;
        int spacing = 20;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 22
        };

        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        Rect titleRect = new Rect(centerX - 200, centerY - 160, 400, 60);
        GUI.Label(titleRect, "Maze Escape", titleStyle);

        Rect playRect = new Rect(centerX - buttonWidth / 2f, centerY - buttonHeight / 2f, buttonWidth, buttonHeight);
        if (GUI.Button(playRect, "Play", buttonStyle))
        {
            PlayGame();
        }

        Rect quitRect = new Rect(centerX - buttonWidth / 2f, playRect.yMax + spacing, buttonWidth, buttonHeight);
        if (GUI.Button(quitRect, "Quit", buttonStyle))
        {
            QuitGame();
        }
    }
}
