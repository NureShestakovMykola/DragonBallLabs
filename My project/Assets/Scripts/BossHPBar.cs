using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("World Space Canvas або будь-€кий Transform, €кий м≥стить Slider HP (зазвичай BossHPWorldCanvas).")]
    public Transform barRoot;

    [Tooltip("Slider HP (BossHPSlider).")]
    public Slider hpSlider;

    [Tooltip("якщо SpriteRenderer не на цьому об'Їкт≥, перет€гн≥ть сюди SpriteRenderer боса (або залиште пустим Ч знайдетьс€ автоматично).")]
    public SpriteRenderer spriteRendererOverride;

    [Header("Position")]
    [Tooltip("ƒодаткова висота над головою (у world одиниц€х).")]
    public float extraHeight = 0.25f;

    [Tooltip("«сув по X (€кщо треба).")]
    public float xOffset = 0f;

    [Header("Behaviour")]
    [Tooltip("“римати HP-bar завжди р≥вним (не обертати разом ≥з босом).")]
    public bool keepUpright = true;

    private EnemyHealth enemyHealth;
    private SpriteRenderer sr;

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        sr = spriteRendererOverride != null ? spriteRendererOverride : GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);

        if (barRoot == null)
        {
            // шукаЇмо Canvas у д≥т€х
            Canvas c = GetComponentInChildren<Canvas>(true);
            if (c != null) barRoot = c.transform;
        }

        if (hpSlider == null && barRoot != null)
        {
            hpSlider = barRoot.GetComponentInChildren<Slider>(true);
        }
    }

    void Start()
    {
        SyncSlider();
        UpdateBarTransform();
    }

    void LateUpdate()
    {
        // €кщо боса вже майже знищено Ч просто не оновлюЇмо
        if (enemyHealth == null || barRoot == null || hpSlider == null) return;

        UpdateBarTransform();
        SyncSlider();
    }

    void SyncSlider()
    {
        int maxHp = Mathf.Max(1, enemyHealth.MaxHp);
        int curHp = Mathf.Clamp(enemyHealth.CurrentHp, 0, maxHp);

        hpSlider.minValue = 0;
        hpSlider.maxValue = maxHp;
        hpSlider.value = curHp;
    }

    void UpdateBarTransform()
    {
        // ¬изначаЇмо висоту "голови" по SpriteRenderer, €кщо в≥н Ї
        float y = extraHeight;

        if (sr != null)
        {
            // bounds.extents.y Ч половина висоти спрайта у world
            y += sr.bounds.extents.y;
        }

        Vector3 targetPos = transform.position + new Vector3(xOffset, y, 0f);
        barRoot.position = targetPos;

        if (keepUpright)
            barRoot.rotation = Quaternion.identity;
    }
}
