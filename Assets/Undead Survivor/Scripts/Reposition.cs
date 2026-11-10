using UnityEngine;
using UnityEngine.Tilemaps;

public class Reposition : MonoBehaviour
{
    Collider2D col;
    Vector3 groundStartPosition;
    Vector3 groundStartCenter;
    Vector3 groundSize;
    bool isGround;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (!CompareTag("Ground"))
            return;

        Tilemap tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Ground requires a Tilemap component.", this);
            return;
        }

        tilemap.CompressBounds();
        Bounds bounds = tilemap.localBounds;
        groundSize = transform.TransformVector(bounds.size);
        groundSize.x = Mathf.Abs(groundSize.x);
        groundSize.y = Mathf.Abs(groundSize.y);
        if (groundSize.x <= 0f || groundSize.y <= 0f)
        {
            Debug.LogError("Ground Tilemap must contain a non-empty rectangular tile area.", this);
            return;
        }

        groundStartPosition = transform.position;
        groundStartCenter = transform.TransformPoint(bounds.center);
        isGround = true;
    }

    void LateUpdate()
    {
        if (!isGround || !GameManager.instance.isLive)
            return;

        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 position = groundStartPosition;
        float width = groundSize.x * 2f;
        float height = groundSize.y * 2f;
        position.x += Mathf.Floor((playerPos.x - groundStartCenter.x + groundSize.x) / width) * width;
        position.y += Mathf.Floor((playerPos.y - groundStartCenter.y + groundSize.y) / height) * height;
        transform.position = position;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!CompareTag("Enemy") || !collision.CompareTag("Area") || !col.enabled)
            return;

        Vector3 playerDir = GameManager.instance.player.inputVector;
        transform.Translate(playerDir * 20 + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f));
    }
}
