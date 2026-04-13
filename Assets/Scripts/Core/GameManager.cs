using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float missionDuration = 180f; // 3 minutes

    [Header("References")]
    public Base playerBase;
    //public Base enemyHQ;
    public WaveManager waveManager;

    [Header("UI References")]
    public TextMeshProUGUI timerText;

    public GameObject victoryPanel;
    public GameObject defeatPanel;

    public float timeToWaitForChangeScreen = 5f;

    // State
    public bool IsGameOver { get; private set; }
    public float TimeRemaining { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        AudioManager.Instance.PlayBGM(BGMTrack.GamePlay);

        TimeRemaining = missionDuration;
        IsGameOver = false;
        Time.timeScale = 1f;

        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);

        UpdateUI();
        waveManager.StartWaves();
    }

    void Update()
    {
        if (IsGameOver) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            TriggerVictory("Survived! Mission Complete!");
        }

        UpdateUI();
    }

    void CheckResourceFailure()
    {
        // Only trigger failure if ALL units are out of resources
        Unit[] units = FindObjectsOfType<Unit>();
        bool allDepleted = true;
        foreach (var u in units)
        {
            if (!u.IsDead) { allDepleted = false; break; }
        }
        if (allDepleted) TriggerDefeat("All units lost!");
    }

    public void OnUnitDestroyed()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        int alive = 0;
        foreach (var u in units)
            if (!u.IsDead) alive++;

        if (alive == 0)
        {
            TriggerDefeat("All units destroyed!");
        }
    }

    public void OnBaseDestroyed()
    {
        TriggerDefeat("Base destroyed!");
    }

    public void OnEnemyHQDestroyed()
    {
        TriggerVictory("Enemy HQ Destroyed! Victory!");
    }

    public void OnWaveClear(string msg)
    {
        TriggerVictory(msg);
    }    

    void TriggerVictory(string msg)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        victoryPanel.SetActive(true);
        Time.timeScale = 0.3f;

        StartCoroutine(WaitForEnding());
    }

    void TriggerDefeat(string msg)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        defeatPanel.SetActive(true);
        Time.timeScale = 0.3f;

        StartCoroutine(WaitForEnding());
    }

    IEnumerator WaitForEnding()
    {
        yield return new WaitForSeconds(timeToWaitForChangeScreen);

        SceneManager.LoadSceneAsync("Start", LoadSceneMode.Single);
    }

    void UpdateUI()
    {
        int mins = Mathf.FloorToInt(TimeRemaining / 60);
        int secs = Mathf.FloorToInt(TimeRemaining % 60);
        timerText.text = $"{mins:00}:{secs:00}";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
