using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private string firstLevelScene = "Level01";

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.volume = 1f;

        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSave;

        if (EventSystem.current != null && firstSelectedButton != null)
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void NewGame()
    {
        GameProgress.Reset();
        SaveSystem.Delete();
        Flow.Go(firstLevelScene);
    }

    public void Continue()
    {
        SaveData data = SaveSystem.Load();
        GameProgress.Apply(data);

        int maxBuildIndex = SceneManager.sceneCountInBuildSettings - 1;
        if (maxBuildIndex >= 1)
        {
            int targetIndex = Mathf.Clamp(data.levelIndex, 1, maxBuildIndex);
            Flow.Go(targetIndex);
        }
        else
        {
            Flow.Go(firstLevelScene);
        }
    }

    public void Quit() => Flow.Quit();
}
