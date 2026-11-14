using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class EnemyArrow : MonoBehaviour
{
    const float Lifetime = 5f;

    readonly RaycastHit2D[] hits = new RaycastHit2D[16];
    Rigidbody2D rb;
    CircleCollider2D col;
    ContactFilter2D playerFilter;
    Vector2 direction;
    float damage;
    float speed;
    float remainingLife;
    bool initialized;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
    }

    public void Init(Vector3 position, Vector2 direction, float damage, float speed)
    {
        this.direction = direction;
        this.damage = damage;
        this.speed = speed;
        remainingLife = Lifetime;
        transform.SetPositionAndRotation(position,
            Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 45f));
        rb.position = position;
        rb.rotation = transform.eulerAngles.z;
        playerFilter = new ContactFilter2D();
        playerFilter.SetLayerMask(1 << GameManager.instance.player.gameObject.layer);
        playerFilter.useTriggers = false;
        initialized = true;
    }

    void FixedUpdate()
    {
        if (!initialized || !GameManager.instance.isLive)
            return;

        remainingLife -= Time.fixedDeltaTime;
        if (remainingLife <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        float distance = speed * Time.fixedDeltaTime;
        // Sweep the arrow collider so fast projectiles cannot skip the player between physics steps.
        Vector3 scale = transform.lossyScale;
        float radius = col.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        int count = Physics2D.CircleCast(col.bounds.center, radius, direction, playerFilter, hits, distance);
        for (int i = 0; i < count; i++)
        {
            Player player = hits[i].collider.GetComponentInParent<Player>();
            if (player != null)
            {
                Hit(player);
                return;
            }
        }
        rb.MovePosition(rb.position + direction * distance);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized || !GameManager.instance.isLive)
            return;

        Player player = collision.GetComponentInParent<Player>();
        if (player != null && !collision.isTrigger)
            Hit(player);
    }

    void Hit(Player player)
    {
        initialized = false;
        player.TakeDamage(damage);
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        initialized = false;
        rb.linearVelocity = Vector2.zero;
    }
}
