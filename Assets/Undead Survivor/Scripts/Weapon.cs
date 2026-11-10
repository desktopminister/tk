using UnityEngine;
using UnityEngine.InputSystem;

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
        damage = data.baseDamage;
        count = data.baseCount;

        for (int i = 0; i < GameManager.instance.poolManager.prefabs.Length; i++)
        {
            if (data.projectile == GameManager.instance.poolManager.prefabs[i])
            {
                prefabId = i;
                break;
            }
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
        Hand hand = player.hands[(int)data.itemType];
        hand.sr.sprite = data.hand;
        hand.gameObject.SetActive(true);

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
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
            bullet.localPosition = Quaternion.Euler(0f, 0f, angle) * Vector3.up * 1.7f;
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
