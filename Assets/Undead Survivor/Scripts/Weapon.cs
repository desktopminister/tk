using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;
    // Update is called once per frame
    float timer;
    Player player;
    ItemData data;

    public bool IsAreaWeapon => data != null &&
        (data.itemType == ItemData.ItemType.FireBreath || data.itemType == ItemData.ItemType.LightningStrike);

    void Awake()
    {
        player = GameManager.instance.player;
    }


    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).rotation = Quaternion.identity;
                }
                break;
            default:
                timer += Time.deltaTime;
                if (timer >= speed)
                {
                    timer = 0;
                    if (IsAreaWeapon)
                        FireAreaWeapon();
                    else
                        Fire();
                }
                break;
        }
    }
    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count += count;
        
        if (id ==0)
        {
            Batch();
        }
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void Init(ItemData data)
    {
        //basic set
        name = "Weapon_" + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;


        //property set
        id = data.itemId;
        this.data = data;
        damage = data.baseDamage;
        count = data.baseCount;

        bool foundPrefab = false;
        for (int i = 0; i < GameManager.instance.poolManager.prefabs.Length; i++)
        {
            if (data.projectile == GameManager.instance.poolManager.prefabs[i])
            {
                prefabId = i;
                foundPrefab = true;
                break;
            }
        }
        if (IsAreaWeapon && !foundPrefab)
        {
            Debug.LogError("Weapon projectile is not registered in PoolManager: " + data.name, this);
            enabled = false;
            return;
        }

        switch (id)
        {
            case 0:
                speed = 150;
                Batch();
                break;
            default:
                speed = 0.3f;
                break;

        }
        if (IsAreaWeapon)
            speed = data.attackInterval;
        else
        {
            Hand hand = player.hands[(int)data.itemType];
            hand.sr.sprite = data.hand;
            hand.gameObject.SetActive(true);
        }

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void SetAreaAttackRate(float rate)
    {
        speed = data.attackInterval * (1f - rate);
    }

    void FireAreaWeapon()
    {
        Vector2 origin = player.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, player.scanner.scanRange, player.scanner.targetLayer);
        List<Enemy> enemies = new List<Enemy>();
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null && enemy.IsAlive && !enemies.Contains(enemy))
                enemies.Add(enemy);
        }

        if (enemies.Count == 0)
            return;

        if (data.itemType == ItemData.ItemType.FireBreath)
        {
            Enemy nearest = enemies[0];
            float nearestDistance = ((Vector2)nearest.transform.position - origin).sqrMagnitude;
            foreach (Enemy enemy in enemies)
            {
                float distance = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = enemy;
                    nearestDistance = distance;
                }
            }

            Vector2 direction = ((Vector2)nearest.transform.position - origin).normalized;
            if (direction == Vector2.zero)
                direction = Vector2.right;
            float levelScale = 1f + count * 0.01f;
            // effectScale is visual only; damage range grows with level alone.
            WeaponEffect effect = GameManager.instance.poolManager.Get(prefabId).GetComponent<WeaponEffect>();
            effect.PlayFire(player.transform, direction, data.effectScale * levelScale, data.attackRange * levelScale, damage, player.scanner.targetLayer);
            return;
        }

        List<Vector2> positions = new List<Vector2>();
        while (enemies.Count > 0 && positions.Count < count)
        {
            int index = Random.Range(0, enemies.Count);
            Vector2 position = enemies[index].transform.position;
            enemies.RemoveAt(index);
            bool duplicate = false;
            foreach (Vector2 selected in positions)
            {
                if ((selected - position).sqrMagnitude < 0.0001f)
                {
                    duplicate = true;
                    break;
                }
            }
            if (duplicate)
                continue;
            positions.Add(position);
            WeaponEffect effect = GameManager.instance.poolManager.Get(prefabId).GetComponent<WeaponEffect>();
            effect.PlayLightning(position, data.effectScale, data.areaRadius, damage, player.scanner.targetLayer);
        }
    }
    void Batch()
    {
        for (int i = 0; i < count; i++)
        {
            Transform bullet;
            if (i < transform.childCount)
            {
                bullet = transform.GetChild(i);
            }
            else
            {
                bullet = GameManager.instance.poolManager.Get(prefabId).transform;
                bullet.parent = transform;
            }

            float angle = i * 360f / count;
            bullet.localPosition = Quaternion.Euler(0f, 0f, angle) * Vector3.up * 0.425f;
            bullet.rotation = Quaternion.identity;
            bullet.GetComponent<Bullet>().Init(damage, -1, bullet.up); // -1 is infinity pierce.
        }
    }
    void Fire()
    {
        if (!(player.scanner.nearestTarget != null))
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;
        Transform bullet = GameManager.instance.poolManager.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<Bullet>().Init(damage, count, dir);
    }
}
