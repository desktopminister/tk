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
        GameObject enemy = GameManager.instance.poolManager.Get(0);
        enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
        enemy.GetComponent<Enemy>().Init(spawnDatas[level]);
    }
}

[System.Serializable]
public class SpawnData
{
    public int SpriteType;
    public int health;
    public float spawnRate;
    public float speed;
}