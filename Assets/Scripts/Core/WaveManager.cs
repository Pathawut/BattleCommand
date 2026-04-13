using UnityEngine;
using System.Collections;

[System.Serializable]
public class WaveConfig
{
    public string waveName;
    public float startDelay;       // seconds after previous wave ends
    public int scoutCount;
    public int heavyCount;
    public int sniperCount;
    public float spawnInterval = 1.5f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configs")]
    public WaveConfig[] waves;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;   // 2 spawn points from map

    [Header("Enemy Prefabs")]
    public GameObject scoutEnemyPrefab;
    public GameObject heavyEnemyPrefab;
    public GameObject sniperEnemyPrefab;

    [Header("UI")]
    public TMPro.TextMeshProUGUI waveText;

    private int currentWave = 0;

    public void StartWaves()
    {
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        foreach (var wave in waves)
        {
            if (GameManager.Instance.IsGameOver) yield break;

            currentWave++;
            if (waveText) waveText.text = $"Wave {currentWave}";

            yield return new WaitForSeconds(wave.startDelay);

            if (GameManager.Instance.IsGameOver) yield break;

            yield return StartCoroutine(SpawnWave(wave));

            // Wait until all enemies from this wave are dead
            yield return StartCoroutine(WaitForWaveClear());
        }

        // All waves done — if player survived, they win
        if (!GameManager.Instance.IsGameOver)
        {
            // Survival victory is handled by timer; HQ destruction is separate
            var msg = "All waves cleared!";
            if (waveText) waveText.text = msg;

            GameManager.Instance.OnWaveClear(msg);
        }
    }

    IEnumerator SpawnWave(WaveConfig wave)
    {
        for (int i = 0; i < wave.scoutCount; i++)
        {
            SpawnEnemy(scoutEnemyPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
        for (int i = 0; i < wave.heavyCount; i++)
        {
            SpawnEnemy(heavyEnemyPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
        for (int i = 0; i < wave.sniperCount; i++)
        {
            // Flankers use a different spawn point
            SpawnEnemy(sniperEnemyPrefab, flanker: true);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    void SpawnEnemy(GameObject prefab, bool flanker = false)
    {
        if (prefab == null || spawnPoints.Length == 0) return;

        int idx = flanker && spawnPoints.Length > 1
            ? Random.Range(1, spawnPoints.Length)
            : 0;

        Vector3 pos = spawnPoints[idx].position + (Vector3)Random.insideUnitCircle * 0.5f;
        Instantiate(prefab, pos, Quaternion.identity);
    }

    IEnumerator WaitForWaveClear()
    {
        yield return new WaitForSeconds(3f); // Give enemies time to spawn before checking

        while (true)
        {
            if (GameManager.Instance.IsGameOver) yield break;

            EnemyUnit[] enemies = FindObjectsOfType<EnemyUnit>();
            bool allDead = true;
            foreach (var e in enemies)
                if (!e.IsDead) { allDead = false; break; }

            if (allDead) yield break;
            yield return new WaitForSeconds(1f);
        }
    }
}
