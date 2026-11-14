using UnityEngine;

public class Spawner : MonoBehaviour
{
    int level;
    public Transform[] spawnPoints;
    public SpawnData[] spawnDatas;
    float timer;
    void Awake()
    {
        spawnPoints = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 10f), spawnDatas.Length - 1);
        if (timer > spawnDatas[level].spawnRate)
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        SpawnData spawnData = SelectSpawnData();
        GameObject enemy = GameManager.instance.poolManager.Get(0);
        Vector3 position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
        // Spawn points only give a direction; the distance follows the current screen size
        if (CameraView.TryGetWorldRect(position.z, CameraView.EnemySpawnMargin, out Rect spawnArea))
        {
            float z = position.z;
            position = CameraView.PointOnEdge(spawnArea, position - transform.position);
            position.z = z;
        }
        enemy.transform.position = position;
        enemy.GetComponent<Enemy>().Init(spawnData);
    }

    SpawnData SelectSpawnData()
    {
        float totalWeight = 0f;
        for (int i = 0; i <= level; i++)
            totalWeight += spawnDatas[i].spawnWeight;

        float choice = Random.Range(0f, totalWeight);
        for (int i = 0; i < level; i++)
        {
            if (choice < spawnDatas[i].spawnWeight)
                return spawnDatas[i];
            choice -= spawnDatas[i].spawnWeight;
        }
        return spawnDatas[level];
    }
}

[System.Serializable]
public class SpawnData
{
    public int SpriteType;
    public int health;
    [Tooltip("Seconds between spawns at this difficulty stage. Earlier entries remain eligible.")]
    public float spawnRate;
    public float speed;
    [Tooltip("Relative frequency among unlocked entries. Weights 3 and 1 give a 75% / 25% mix.")]
    [Min(0.01f)] public float spawnWeight = 1f;
    [Tooltip("Enables ranged attacks independently of the animator SpriteType index.")]
    public bool usesRangedAttack;
    [Min(0.1f)] public float attackRange = 2f;
    [Min(0.1f)] public float attackInterval = 3f;
    [Min(0.1f)] public float arrowDamage = 5f;
    [Min(0.1f)] public float arrowSpeed = 1.75f;
}