using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVector;
    public float speed;
    public Scanner scanner;
    Rigidbody2D rb;
    SpriteRenderer sr; 
    Animator anim; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
    }

    void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
    }
    void FixedUpdate()
    {
        Vector2 nextVector = speed * Time.fixedDeltaTime * inputVector;
        rb.MovePosition(rb.position + nextVector);
    }

    void LateUpdate()
    {
        anim.SetFloat("Speed", inputVector.magnitude);
        
        if (inputVector.x != 0)
        {
            // Handle horizontal input
            sr.flipX = inputVector.x < 0;
        }
    }
}
