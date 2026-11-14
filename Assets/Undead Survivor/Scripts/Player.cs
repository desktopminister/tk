using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVector;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    Rigidbody2D rb;
    SpriteRenderer sr; 
    Animator anim; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
    }
    
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        Vector2 nextVector = speed * Time.fixedDeltaTime * inputVector;
        rb.MovePosition(rb.position + nextVector);
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        anim.SetFloat("Speed", inputVector.magnitude);
        
        if (inputVector.x != 0)
        {
            // Handle horizontal input
            sr.flipX = inputVector.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TakeDamage(Time.deltaTime * 10);
    }

    public void TakeDamage(float damage)
    {
        if (!GameManager.instance.isLive)
            return;
        
        GameManager.instance.health -= damage;

        if (GameManager.instance.health <= 0)
        {
            for (int i = 2; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }
}
