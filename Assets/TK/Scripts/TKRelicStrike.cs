using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TKRelicStrike : MonoBehaviour
{
    enum Pattern { Crescent, Thrust, Whirlwind }

    [SerializeField] Pattern pattern;
    [SerializeField] Sprite[] frames;
    [SerializeField] float framesPerSecond = 20f;
    [SerializeField] float[] hitTimes;

    SpriteRenderer image;
    LayerMask enemyLayer;
    float elapsed, damage, reach, radius;
    int nextHit;

    void Awake() => image = GetComponent<SpriteRenderer>();

    public void Cast(Player player, ItemData item, float power, int rangeBonus)
    {
        bool wind = pattern == Pattern.Whirlwind;
        float scale = 1f + rangeBonus * 0.01f;
        float reach = item.attackRange * scale;
        float search = wind ? player.scanner.scanRange : reach;
        var enemies = TKCombat.FindTargets(player.transform.position, search, player.scanner.targetLayer);
        if (enemies.Count == 0) return;

        Vector2 target = enemies[0].transform.position;
        player.PlayAttack(true, target);
        Vector2 origin = player.HandPosition;
        Vector2 aim = target - origin;
        Quaternion rotation = wind ? Quaternion.identity : Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
        var strike = TKPool.Spawn(this, wind ? target : origin, rotation);
        strike.transform.localScale = Vector3.one * (scale * item.effectScale);
        strike.enemyLayer = player.scanner.targetLayer;
        strike.damage = power;
        strike.reach = reach;
        strike.radius = item.areaRadius * scale;
        strike.elapsed = -0.08f; // Let the player's hand start its wind-up before the release.
        strike.nextHit = 0;
        strike.image.sprite = null;
        strike.image.color = Color.white;
        TKGameAudio.Play(wind ? TKGameAudio.Cue.Trigram : TKGameAudio.Cue.BladeSwish);
    }

    void Update()
    {
        if (!GameManager.instance.isLive) return;
        elapsed += Time.deltaTime;
        if (elapsed < 0f) return;

        int frame = (int)(elapsed * framesPerSecond);
        image.sprite = frames[Mathf.Min(frame, frames.Length - 1)];
        bool wind = pattern == Pattern.Whirlwind;
        if (wind)
        {
            float duration = frames.Length / framesPerSecond;
            image.color = new Color(1f, 1f, 1f, Mathf.Clamp01(Mathf.Min(elapsed / 0.1f, (duration - elapsed) / 0.23f)));
        }
        while (nextHit < hitTimes.Length && elapsed >= hitTimes[nextHit] && GameManager.instance.isLive)
        {
            nextHit++;
            int hits = TKCombat.DamageCircle(transform.position, wind ? radius : reach, damage, enemyLayer,
                filter: wind ? null : WithinBlade);
            if (hits > 0 && !wind) GameFeel.HitStop(0.035f);
        }
        if (frame >= frames.Length) TKPool.Release(gameObject);
    }

    bool WithinBlade(IDamageable target)
    {
        Vector2 offset = target.transform.position - transform.position;
        float forward = Vector2.Dot(offset, transform.right);
        if (forward < 0f || offset.sqrMagnitude > reach * reach) return false;
        if (pattern == Pattern.Thrust)
            return Mathf.Abs(Vector2.Dot(offset, transform.up)) <= radius;
        // A half-angle of 70 degrees gives the guandao a broad front edge.
        return forward >= offset.magnitude * 0.34202f;
    }
}
