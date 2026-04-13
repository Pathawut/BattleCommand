using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Left panel UI:
///  - Shows selected unit's name, type, HP, status
///  - Shows command queue (up to 5 slots)
/// </summary>
public class UnitPanelUI : MonoBehaviour
{
    [Header("Unit Info")]
    public GameObject panel;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitTypeText;
    public Image unitTypeIcon;
    public Slider unitHPSlider;
    public TextMeshProUGUI unitHPText;
    public Sprite[] unityTypeSprites;

    [Header("Command Queue Display")]
    public Transform queueContainer;
    public GameObject queueSlotPrefab;
    public Sprite[] commandSprites;

    [Header("Command Buttons")]
    public Button moveButton;
    public Button attackButton;
    public Button defendButton;
    public Button clearButton;



    private Unit trackedUnit;
    private PlayerController controller;
    private List<GameObject> queueSlots = new List<GameObject>();

    void Awake()
    {
        controller = FindFirstObjectByType<PlayerController>();
        queueSlotPrefab?.SetActive(false);

        moveButton?.onClick.AddListener(()   => controller.SetCommandTypeMove());
        attackButton?.onClick.AddListener(() => controller.SetCommandTypeAttack());
        defendButton?.onClick.AddListener(() => controller.SetCommandTypeDefend());
        clearButton?.onClick.AddListener(()  => controller.ClearCommands());

        panel?.SetActive(false);
    }

    void Update()
    {
        if (trackedUnit == null || trackedUnit.IsDead)
        {
            HidePanel();
            return;
        }
        RefreshInfo(trackedUnit);
    }

    public void ShowUnit(Unit u)
    {
        trackedUnit = u;
        panel?.SetActive(true);
        RefreshInfo(u);
        RefreshQueue(u);
    }

    public void HidePanel()
    {
        trackedUnit = null;
        panel?.SetActive(false);
    }

    void RefreshInfo(Unit u)
    {
        if (unitNameText)   unitNameText.text   = u.unitName;
        if (unitTypeText)   unitTypeText.text   = u.unitType.ToString();
        if (unitTypeIcon)   unitTypeIcon.sprite = unityTypeSprites[(int)u.unitType];
        if (unitHPSlider)   unitHPSlider.value  = (float)u.CurrentHP / u.maxHP;
        if (unitHPText)     unitHPText.text     = $"{u.CurrentHP}/{u.maxHP}";
    }

    public void RefreshQueue(Unit u)
    {
        // Clear old slots
        foreach (var s in queueSlots) Destroy(s);
        queueSlots.Clear();

        if (u == null || queueContainer == null || queueSlotPrefab == null) return;

        // Use reflection-free approach: just show count
        var commands = u.GetCommandQueue();
        for (int i = 0; i < commands.Count; i++)
        {
            GameObject slot = Instantiate(queueSlotPrefab, queueContainer);
            slot.SetActive(true);

            //TextMeshProUGUI label = slot.GetComponentInChildren<TextMeshProUGUI>();
            //if (label) label.text = $"CMD {i + 1}";
            var image = slot.GetComponent<Image>();
            image.sprite = GetCommandSprite(commands[i]);
            queueSlots.Add(slot);
        }
    }

    private Sprite GetCommandSprite(Command cmd)
    {
        return commandSprites[(int)cmd.type];
    }
}
