using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBootstrap : MonoBehaviour
{
    [SerializeField] private IntEventSO coinsChanged;

    private void Start()
    {
        GameProgress.LevelIndex = SceneManager.GetActiveScene().buildIndex;
        if (coinsChanged != null)
            coinsChanged.Raise(GameProgress.Coins);
    }
}
