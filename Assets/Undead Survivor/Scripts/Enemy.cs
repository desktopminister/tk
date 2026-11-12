using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float speed;
    public Rigidbody2D target;
    Animator anim;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animatorController;
    public float deadDelay = 0.5f;
    public float flashDuration = 0.08f;
    public float knockbackForce = 3f;
    public float knockbackDuration = 0.15f;

    // GUI/Text Shader draws the sprite's alpha shape in a solid color
    static Material flashMaterial;

    bool isLive;
    public bool IsAlive => isLive;
    bool isHit;
    Rigidbody2D rb;
    Collider2D col;
    SpriteRenderer sr;
    Material defaultMaterial;
    Coroutine hitRoutine;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        wait = new WaitForFixedUpdate();
        defaultMaterial = sr.sharedMaterial;
        if (flashMaterial == null)
            flashMaterial = new Material(Shader.Find("GUI/Text Shader")) { color = Color.white };
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (!isLive || isHit || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rb.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return; 
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
        col.enabled = true;
        rb.simulated = true;
        isHit = false;
        sr.sharedMaterial = defaultMaterial;
        sr.sortingOrder = 2;
        anim.SetBool("Dead", false);
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
        if (!collision.CompareTag("Bullet") || !isLive)
            return;
        TakeDamage(collision.GetComponent<Bullet>().damage);
    }

    public void TakeDamage(float damage)
    {
        if (!isLive)
            return;

        health -= damage;
        if (hitRoutine != null)
            StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitRoutine());

        if (health > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            isLive = false;
            col.enabled = false;
            rb.simulated = false;
            sr.sortingOrder = 1;
            anim.SetBool("Dead", true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
            StartCoroutine(DeadRoutine());
        }
    }

    IEnumerator DeadRoutine()
    {
        // Wait for the animator to enter the dead state before reading its length
        yield return null;
        while (anim.IsInTransition(0))
            yield return null;

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + deadDelay);
        gameObject.SetActive(false);
    }

    IEnumerator HitRoutine()
    {
        isHit = true;
        rb.linearVelocity = Vector2.zero;

        sr.sharedMaterial = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        sr.sharedMaterial = defaultMaterial;

        if (isLive)
        {
            yield return wait;
            Vector3 dirVec = transform.position - GameManager.instance.player.transform.position;
            rb.AddForce(dirVec.normalized * knockbackForce, ForceMode2D.Impulse);
            yield return new WaitForSeconds(knockbackDuration);
            rb.linearVelocity = Vector2.zero;
        }

        isHit = false;
        hitRoutine = null;
    }
    // Animation event receiver kept for existing clips; despawn is handled by DeadRoutine
    void Dead()
    {
    }
}
