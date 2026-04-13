using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Handles all player mouse input:
///  - Left click  → Select unit
///  - Right click → Move command (if unit selected)
///  - UI buttons  → Attack / Defend commands
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Layer Masks")]
    public LayerMask unitLayer;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("Selection")]
    public GameObject selectionRingPrefab; // Optional circle under selected unit

    private Unit selectedUnit;
    private Camera mainCam;
    private CommandType pendingCommandType = CommandType.Move;
    private bool awaitingCommandPlacement = false;

    // UI panel reference
    public UnitPanelUI unitPanel;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (EventSystem.current.IsPointerOverGameObject()) return; // Don't click through UI

        HandleLeftClick();
        HandleRightClick();
        HandleHotkeys();
    }

    // ── INPUT HANDLERS ────────────────────────────────────────────

    void HandleLeftClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Try to select a player unit
        Collider2D hit = Physics2D.OverlapPoint(worldPos, unitLayer);
        if (hit != null)
        {
            Unit u = hit.GetComponent<Unit>();
            if (u != null && !(u is EnemyUnit) && !u.IsDead)
            {
                SelectUnit(u);
                return;
            }
        }

        // If in command placement mode and clicked ground
        if (awaitingCommandPlacement && selectedUnit != null)
        {
            // Check if clicked enemy for attack
            Collider2D enemyHit = Physics2D.OverlapPoint(worldPos, enemyLayer);
            if (enemyHit != null && pendingCommandType == CommandType.Attack)
            {
                EnemyUnit target = enemyHit.GetComponent<EnemyUnit>();
                if (target != null && !target.IsDead)
                {
                    IssueCommand(pendingCommandType, target.transform.position, target);
                    awaitingCommandPlacement = false;
                    return;
                }
            }

            IssueCommand(pendingCommandType, worldPos);
            awaitingCommandPlacement = false;
        }
        else
        {
            // Deselect
            if (selectedUnit != null) selectedUnit.Deselect();
            selectedUnit = null;
            unitPanel?.HidePanel();
        }
    }

    void HandleRightClick()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (selectedUnit == null) return;

        Vector2 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Check if right-clicked on enemy → attack command
        Collider2D enemyHit = Physics2D.OverlapPoint(worldPos, enemyLayer);
        if (enemyHit != null)
        {
            EnemyUnit target = enemyHit.GetComponent<EnemyUnit>();
            if (target != null && !target.IsDead)
            {
                IssueCommand(CommandType.Attack, target.transform.position, target);
                return;
            }
        }

        // Right click on ground → Move command
        IssueCommand(CommandType.Move, worldPos);
    }

    void HandleHotkeys()
    {
        if (selectedUnit == null) return;

        if (Input.GetKeyDown(KeyCode.A)) PrepareCommand(CommandType.Attack);
        if (Input.GetKeyDown(KeyCode.D)) PrepareCommand(CommandType.Defend);
        if (Input.GetKeyDown(KeyCode.Escape)) ClearCommands();

    }

    // ── COMMAND INTERFACE ─────────────────────────────────────────

    public void PrepareCommand(CommandType type)
    {
        if (selectedUnit == null) return;
        pendingCommandType = type;
        awaitingCommandPlacement = true;
        // Cursor hint could go here
    }

    public void SetCommandTypeMove()    => PrepareCommand(CommandType.Move);
    public void SetCommandTypeAttack()  => PrepareCommand(CommandType.Attack);
    public void SetCommandTypeDefend()  => PrepareCommand(CommandType.Defend);

    void IssueCommand(CommandType type, Vector3 targetPos, EnemyUnit target = null)
    {
        if (selectedUnit == null || selectedUnit.IsDead) return;
        CommandSystem.Instance.IssueCommand(selectedUnit, type, targetPos, target);
        unitPanel?.RefreshQueue(selectedUnit);
    }

    public void ClearCommands()
    {
        selectedUnit?.ClearCommands();
        unitPanel?.RefreshQueue(selectedUnit);
    }

    // ── SELECTION ─────────────────────────────────────────────────

    void SelectUnit(Unit u)
    {
        if (selectedUnit != null) selectedUnit.Deselect();
        selectedUnit = u;
        selectedUnit.Select();
        unitPanel?.ShowUnit(selectedUnit);
    }

    public Unit GetSelectedUnit() => selectedUnit;

    // ── UNITY PRODUCTION ─────────────────────────────────────────────────

    public void PrepareToCreate(UnitType unitType)
    {

    }
}
