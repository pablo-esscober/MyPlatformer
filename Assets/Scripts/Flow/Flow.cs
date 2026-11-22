using UnityEngine;
using UnityEngine.SceneManagement;

public static class Flow
{
    public const string MainMenuScene = "MainMenu";

    public static void Go(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public static void Go(int buildIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    public static void Restart() => Go(SceneManager.GetActiveScene().name);

    public static void NextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        {
            GameProgress.LevelIndex = next;
            SaveSystem.Save(GameProgress.ToData());
            Go(next);
        }
        else
        {
            GameProgress.LevelIndex = 0;
            SaveSystem.Save(GameProgress.ToData());
            Go(MainMenuScene);
        }
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
