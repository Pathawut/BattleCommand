using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        AudioManager.Instance.PlayBGM(BGMTrack.Title);
    }

    private void OnStartClicked()
    {
        startButton.interactable = false;
        AudioManager.Instance.PlaySFX(SFX.PressStart);

        SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Single);
    }
}
