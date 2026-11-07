using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public Rigidbody2D target;
    Animator anim;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animatorController;

    bool isLive;
    Rigidbody2D rb;
    SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!isLive)
            return;

        Vector2 dirVec = target.position - rb.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive)
            return;

        if (target.position.x < rb.position.x)
            sr.flipX = true;
        else
            sr.flipX = false;
    }
    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        health = maxHealth;
    }

    public void Init(SpawnData spawnData)
    {
        anim.runtimeAnimatorController = animatorController[spawnData.SpriteType];
        maxHealth = spawnData.health;
        speed = spawnData.speed;
        health = maxHealth;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
            return;
        health -=collision.GetComponent<Bullet>().damage;

        if (health > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            Dead();
        }
    }
    void Dead()
    {
        anim.SetTrigger("Dead");
        isLive = false;
        gameObject.SetActive(false);
    }
}
