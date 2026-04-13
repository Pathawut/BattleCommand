// ─── Base.cs ─────────────────────────────────────────────────────
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Base : MonoBehaviour
{
    [Header("Config")]
    public bool isPlayerBase = true;
    public int maxHP = 500;
    public string baseName = "HQ";
    public Transform target;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Sprite destroySprite;

    [Header("UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    public int CurrentHP { get; private set; }
    public bool IsDestroyed { get; private set; }

    public Transform Target { get => target; }

    void Start()
    {
        CurrentHP = maxHP;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        if (IsDestroyed) return;
        CurrentHP -= Mathf.RoundToInt(amount);
        CurrentHP = Mathf.Max(0, CurrentHP);
        UpdateUI();

        if (CurrentHP <= 0) Destroy_Base();
    }

    void Destroy_Base()
    {
        IsDestroyed = true;
        if (spriteRenderer) spriteRenderer.sprite = destroySprite;

        if (isPlayerBase)
            GameManager.Instance.OnBaseDestroyed();
        else
            GameManager.Instance.OnEnemyHQDestroyed();
    }

    void UpdateUI()
    {
        if (hpSlider) hpSlider.value = (float)CurrentHP / maxHP;
        if (hpText)   hpText.text = $"{baseName}: {CurrentHP}/{maxHP}";
    }
}
