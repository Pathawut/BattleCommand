using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ProductionEntry
{
    public UnitType unitType;
    public float buildTime = 5f;

    public bool isProducing = false;
    public float elapsed = 0f;

    public ProductionEntry(UnitType unitType, UnitData unitData)
    {
        this.unitType = unitType;
        buildTime = unitData.productionTime;

        isProducing = false;
        elapsed = 0f;
    }
}

public class ProductionSystem : MonoBehaviour
{
    public static ProductionSystem Instance { get; private set; }

    // ── INSPECTOR ─────────────────────────────────────────────────
    [Header("Buttons")]
    public Button[] createButtons;
    public TextMeshProUGUI[] creationTimes;

    [Header("Queue Display")]
    public GameObject queueObject1;
    public Image queueImage1;
    public TextMeshProUGUI queueTypeText1;
    public Slider creationTimeSlider;
    public TextMeshProUGUI creationTimeText;

    public GameObject queueObject2;
    public Image queueImage2;
    public TextMeshProUGUI queueTypeText2;

    [Header("Unit Prefabs")]
    public GameObject[] unitPrefabs;
    public UnitData[] unitData;
    public Sprite[] unitSprites;

    [Header("Spawn")]
    public Transform spawnPoint;            // Where finished units appear
    public float spawnRadius = 1.2f;

    // ── PRIVATE ─────────────────────────────────────────────────
    private Queue<ProductionEntry> productionQueue = new Queue<ProductionEntry>();
    private ProductionEntry currentEntry = null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for(int i = 0; i < creationTimes.Length; ++i)
        {
            creationTimes[i].text = $"({unitData[i].productionTime}s)";
        }

        createButtons[(int)UnitType.Heavy].onClick.AddListener(() => OrderProduction(UnitType.Heavy));
        createButtons[(int)UnitType.Scout].onClick.AddListener(() => OrderProduction(UnitType.Scout));
        createButtons[(int)UnitType.Sniper].onClick.AddListener(() => OrderProduction(UnitType.Sniper));

        RefreshQueueUI();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        HandleHotkeys();

        if (currentEntry == null && productionQueue.Count > 0)
        {
            currentEntry = productionQueue.Dequeue();
            StartCoroutine(ProduceUnit());
        }
    }

    void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            // Create Heavy Unity
            ProductionSystem.Instance.OrderProduction(UnitType.Heavy);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            // Create Scout Unity
            ProductionSystem.Instance.OrderProduction(UnitType.Scout);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            // Create Sniper Unity
            ProductionSystem.Instance.OrderProduction(UnitType.Sniper);
        }
    }


    public void OrderProduction(UnitType unitType)
    {
        bool exists = productionQueue.Any(p => p.unitType == unitType);
        if (exists)
        {
            Debug.Log($"Unity {unitType} in queue already.");
            return;
        }

        productionQueue.Enqueue(new ProductionEntry(unitType, unitData[(int)unitType]));
        SetButtonInteractable((int)unitType, false);

        RefreshQueueUI();
    }

    IEnumerator ProduceUnit()
    {
        if (currentEntry == null) yield break;
        if (currentEntry.isProducing) yield break;

        currentEntry.isProducing = true;
        currentEntry.elapsed = 0f;

        RefreshQueueUI();
        SetProgress(0, currentEntry.buildTime);

        // Count down
        while (currentEntry.elapsed < currentEntry.buildTime)
        {
            currentEntry.elapsed += Time.deltaTime;
            //float t = Mathf.Clamp01(currentEntry.elapsed / currentEntry.buildTime);
            SetProgress(currentEntry.elapsed, currentEntry.buildTime);

            yield return null;
        }

        // ── Spawn unit ──────────────────────────────────────────
        SpawnUnit(currentEntry.unitType);

        // ── Reset state ─────────────────────────────────────────

        SetButtonInteractable((int)currentEntry.unitType, true);
        currentEntry = null;
        RefreshQueueUI();
    }

    // ─────────────────────────────────────────────────────────────
    // SPAWN
    // ─────────────────────────────────────────────────────────────

    void SpawnUnit(UnitType unitType)
    {
        var prefab = unitPrefabs[(int)unitType];

        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = origin + new Vector3(offset.x, offset.y, 0f);

        Instantiate(prefab, pos, Quaternion.identity);
    }


    // ─────────────────────────────────────────────────────────────
    // UI HELPERS
    // ─────────────────────────────────────────────────────────────

    void SetButtonInteractable(int index, bool interactable)
    {
        createButtons[index].interactable = interactable;
    }

    void SetProgress(float dt, float buildTime)
    {
        var value = dt / buildTime;
        creationTimeSlider.value = value;

        creationTimeText.text = $"{value*100}%";
    }

    void RefreshQueueUI()
    {
        if (currentEntry == null)
        {
            queueObject1.SetActive(false);
            queueObject2.SetActive(false);
            return;
        }

        int index = (int)currentEntry.unitType;
        queueObject1.SetActive(true);
        queueImage1.sprite = unitSprites[index];
        queueTypeText1.text = unitData[index].unitName;

        //SetProgress(0, currentEntry.buildTime);

        if (productionQueue.Count == 0)
        {
            queueObject2.SetActive(false);
            return;
        }

        var nextEntry = productionQueue.Peek();
        int nextIndex = (int)nextEntry.unitType;

        queueObject2.SetActive(true);
        queueImage2.sprite = unitSprites[nextIndex];
        queueTypeText2.text = unitData[nextIndex].unitName;
    }
}
