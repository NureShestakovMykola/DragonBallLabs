using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    [Tooltip("—к≥льки попадань потр≥бно, щоб вбити (за замовчуванн€м 1).")]
    public int maxHp = 1;

    [Tooltip("—к≥льки очок даЇ ворог за вбивство.")]
    public int scoreValue = 1;

    [Header("Death FX (Normal)")]
    [Tooltip("ѕрефаб ефекту смерт≥ звичайного ворога (FX_EnemyDeath). якщо null Ч ефект не спавнитьс€.")]
    public GameObject deathFxPrefab;

    [Tooltip("«сув позиц≥њ ефекту в≥дносно transform.position (у world одиниц€х).")]
    public Vector3 deathFxOffset = Vector3.zero;

    [Header("Death FX (Boss)")]
    [Tooltip("ѕрефаб ефекту смерт≥ боса (FX_BossDeath). якщо null Ч використовуЇтьс€ deathFxPrefab.")]
    public GameObject bossDeathFxPrefab;

    [Tooltip("«сув позиц≥њ ефекту боса в≥дносно transform.position (у world одиниц€х).")]
    public Vector3 bossDeathFxOffset = Vector3.zero;

    [Tooltip("ћножник масштабу ефекту смерт≥ дл€ боса (1 = без зм≥н).")]
    public float bossDeathFxScaleMultiplier = 1.8f;

    [Tooltip("јвтовизначенн€ боса: €кщо на обТЇкт≥ (або в д≥т€х) Ї BossHPBar Ч вважати босом.")]
    public bool autoDetectBossByHpBar = true;

    private int hp;
    private bool isDead = false;

    public int CurrentHp => hp;
    public int MaxHp => maxHp;

    void Awake()
    {
        hp = Mathf.Max(1, maxHp);
    }

    public void SetHp(int newHp)
    {
        maxHp = Mathf.Max(1, newHp);
        hp = maxHp;
    }

    public void SetScoreValue(int v)
    {
        scoreValue = Mathf.Max(0, v);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHitByLaser(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitByLaser(collision.gameObject);
    }

    void TryHitByLaser(GameObject other)
    {
        if (isDead) return;

        LaserScript laser = other.GetComponent<LaserScript>();
        if (laser == null) return;

        Destroy(other);

        int dmg = Mathf.Max(1, laser.damage);
        hp -= dmg;

        if (hp > 0) return;

        Kill(awardScore: true);
    }

    /// <summary>
    /// ”н≥ф≥кований метод знищенн€ ворога.
    /// awardScore=true використовуЇтьс€, коли ворога вбив гравець (лазером тощо).
    /// awardScore=false використовуЇтьс€, коли ворог знищуЇтьс€ не €к "kill" (наприклад, самознищенн€ при контакт≥).
    /// </summary>
    public void Kill(bool awardScore)
    {
        if (isDead) return;
        isDead = true;

        SpawnDeathFx();

        // «¬”  —ћ≈–“≤ (через Уцентр звукуФ в сцен≥)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        if (awardScore && GameManager.Instance != null)
        {
            for (int i = 0; i < scoreValue; i++)
                GameManager.Instance.AddKillScore();
        }

        Destroy(gameObject);
    }

    private bool IsBoss()
    {
        if (!autoDetectBossByHpBar) return false;

        if (GetComponent<BossHPBar>() != null) return true;
        if (GetComponentInChildren<BossHPBar>(true) != null) return true;

        return false;
    }

    private void SpawnDeathFx()
    {
        bool isBoss = IsBoss();

        GameObject prefabToSpawn = null;
        Vector3 offsetToUse = Vector3.zero;

        if (isBoss)
        {
            prefabToSpawn = (bossDeathFxPrefab != null) ? bossDeathFxPrefab : deathFxPrefab;
            offsetToUse = (bossDeathFxPrefab != null) ? bossDeathFxOffset : deathFxOffset;
        }
        else
        {
            prefabToSpawn = deathFxPrefab;
            offsetToUse = deathFxOffset;
        }

        if (prefabToSpawn == null) return;

        Vector3 pos = transform.position + offsetToUse;

        GameObject fx = Instantiate(prefabToSpawn, pos, Quaternion.identity);

        if (isBoss && fx != null)
        {
            float mult = Mathf.Max(0.01f, bossDeathFxScaleMultiplier);
            fx.transform.localScale = fx.transform.localScale * mult;
        }
    }
}
