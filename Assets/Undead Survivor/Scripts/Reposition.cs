using UnityEngine;

public class Reposition : MonoBehaviour
{
    Collider2D col;
    void Awake()
    {
        col = GetComponent<Collider2D>();
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;
        
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;
        float diffX = Mathf.Abs(playerPos.x - myPos.x);
        float diffY = Mathf.Abs(playerPos.y - myPos.y);

        Vector3 playerDir = GameManager.instance.player.inputVector;
        float dirX = playerDir.x < 0 ? -1 : (playerDir.x > 0 ? 1 : 0);
        float dirY = playerDir.y < 0 ? -1 : (playerDir.y > 0 ? 1 : 0);

        switch (transform.tag)
        {
            case "Ground":
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 40);
                }

                else if (diffY > diffX)
                {
                    transform.Translate(Vector3.up * dirY * 40);
                }
                break;

            case "Enemy":
                if (col.enabled)
                {
                    transform.Translate(playerDir * 20 + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f));
                }

                break;
        }
    }
}
