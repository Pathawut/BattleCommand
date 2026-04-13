using UnityEngine;

/// <summary>
/// Sets up the map at runtime.
/// Attach to an empty GameObject in scene. Drag in prefabs via Inspector.
/// 
/// Map Layout:
///   Player Base  (bottom-left)
///   Enemy HQ     (top-right)
///   Spawn Point 1 (top-left)  ← basic + heavy waves
///   Spawn Point 2 (top-right) ← flanker waves
///   Chokepoint   (center)
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public float mapWidth  = 20f;
    public float mapHeight = 14f;

    [Header("Prefabs")]
    public GameObject playerBasePrefab;
    public GameObject enemyHQPrefab;
    public GameObject wallPrefab;        // Simple obstacle sprite
    public GameObject groundPrefab;      // Background tile
    public GameObject chopointMarkerPrefab;

    [Header("Unit Spawn (Player)")]
    public GameObject heavyUnitPrefab;
    public GameObject scoutUnitPrefab;
    public GameObject sniperUnitPrefab;

    void Start()
    {
        BuildMap();
        SpawnPlayerUnits();
    }

    void BuildMap()
    {
        float hw = mapWidth  / 2f;
        float hh = mapHeight / 2f;

        // Background
        if (groundPrefab)
        {
            GameObject g = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity);
            g.transform.localScale = new Vector3(mapWidth, mapHeight, 1);
        }

        // Border walls
        CreateWall(new Vector3(0,  hh, 0), new Vector3(mapWidth, 0.5f, 1));  // top
        CreateWall(new Vector3(0, -hh, 0), new Vector3(mapWidth, 0.5f, 1));  // bottom
        CreateWall(new Vector3(-hw, 0, 0), new Vector3(0.5f, mapHeight, 1)); // left
        CreateWall(new Vector3( hw, 0, 0), new Vector3(0.5f, mapHeight, 1)); // right

        // Chokepoint walls (center funnel)
        CreateWall(new Vector3(-2, 2,  0), new Vector3(0.5f, 3f, 1));
        CreateWall(new Vector3( 2, -2, 0), new Vector3(0.5f, 3f, 1));

        // Player base
        if (playerBasePrefab)
            Instantiate(playerBasePrefab, new Vector3(-hw + 2, -hh + 2, 0), Quaternion.identity);

        // Enemy HQ
        if (enemyHQPrefab)
            Instantiate(enemyHQPrefab, new Vector3(hw - 2, hh - 2, 0), Quaternion.identity);

        // Chokepoint marker
        if (chopointMarkerPrefab)
            Instantiate(chopointMarkerPrefab, Vector3.zero, Quaternion.identity);
    }

    void CreateWall(Vector3 pos, Vector3 scale)
    {
        if (wallPrefab == null) return;
        GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity);
        w.transform.localScale = scale;
    }

    void SpawnPlayerUnits()
    {
        float hw = mapWidth  / 2f;
        float hh = mapHeight / 2f;
        Vector3 basePos = new Vector3(-hw + 2, -hh + 2, 0);

        if (heavyUnitPrefab)  Instantiate(heavyUnitPrefab,  basePos + new Vector3(1.5f, 0, 0),    Quaternion.identity);
        if (scoutUnitPrefab)  Instantiate(scoutUnitPrefab,  basePos + new Vector3(0, 1.5f, 0),    Quaternion.identity);
        if (sniperUnitPrefab) Instantiate(sniperUnitPrefab, basePos + new Vector3(-1.5f, 0.5f, 0), Quaternion.identity);
    }
}
