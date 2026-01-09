using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHp = 100;
    public int currentHp = 100;

    [Header("UI (World HP bar)")]
    [Tooltip("Slider над гравцем (World Space).")]
    public Slider hpSlider;

    [Tooltip("TMP текст (необовТ€зково). якщо в≥н Ї, по ньому легко перев≥рити: HP реально зменшуЇтьс€ чи н≥.")]
    public TMP_Text hpText;

    [Tooltip(" ор≥нь UI (Canvas/Panel), €кий треба позиц≥онувати над гравцем. якщо null Ч буде вз€то transform Slider-а.")]
    public Transform uiRoot;

    [Tooltip("якщо SpriteRenderer не на цьому об'Їкт≥ Ч призначити вручну.")]
    public SpriteRenderer spriteRendererOverride;

    [Header("UI Follow")]
    [Tooltip("„и позиц≥онувати HP-bar над гравцем автоматично.")]
    public bool followPlayer = true;

    [Tooltip("ƒодаткова висота над головою (world одиниц≥).")]
    public float extraHeight = 0.25f;

    [Tooltip("«сув по X (€кщо треба).")]
    public float xOffset = 0f;

    [Tooltip("Ќе обертати UI разом з гравцем.")]
    public bool keepUpright = true;

    [Tooltip("ѕримусово робити scale додатн≥м, щоб Slider не ≥нвертувавс€ при в≥ддзеркаленн≥ (Scale X = -1).")]
    public bool keepScalePositive = true;

    [Header("Damage cooldown")]
    [Tooltip("«ахист в≥д багаторазового в≥дн≥манн€ HP за один момент.")]
    public float damageCooldown = 0.35f;

    [Header("Debug")]
    [Tooltip("Ћогувати зм≥ни HP у Console (допомагаЇ знайти, хто додаЇ HP).")]
    public bool logHpChanges = false;

    private float dmgTimer = 0f;
    private SpriteRenderer sr;

    void Awake()
    {
        if (hpSlider == null)
            hpSlider = GetComponentInChildren<Slider>(true);

        if (hpText == null)
            hpText = GetComponentInChildren<TMP_Text>(true);

        // якщо у вас Slider лежить всередин≥ Canvas/Panel ≥ треба рухати весь блок Ч призначте uiRoot вручну.
        if (uiRoot == null && hpSlider != null)
            uiRoot = hpSlider.transform;

        sr = spriteRendererOverride != null ? spriteRendererOverride : GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);

        maxHp = Mathf.Max(1, maxHp);

        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        // якщо в ≥нспектор≥ випадково стоњть 0 Ч стартувати з повного HP
        if (currentHp <= 0) currentHp = maxHp;
    }

    void Start()
    {
        SyncUI();
        UpdateUITransform();
    }

    void Update()
    {
        if (dmgTimer > 0f)
            dmgTimer -= Time.deltaTime;
    }

    void LateUpdate()
    {
        if (followPlayer)
            UpdateUITransform();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (dmgTimer > 0f) return;
        dmgTimer = damageCooldown;

        int before = currentHp;

        currentHp -= amount;
        if (currentHp < 0) currentHp = 0;

        if (logHpChanges)
            Debug.Log($"[PlayerHealth] TakeDamage({amount}) {before} -> {currentHp}", this);

        SyncUI();

        if (currentHp <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        int before = currentHp;

        currentHp += amount;
        if (currentHp > maxHp) currentHp = maxHp;

        if (logHpChanges)
            Debug.Log($"[PlayerHealth] Heal({amount}) {before} -> {currentHp}", this);

        SyncUI();
    }

    private void SyncUI()
    {
        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHp;
            hpSlider.value = Mathf.Clamp(currentHp, 0, maxHp);
        }

        if (hpText != null)
            hpText.text = $"HP: {currentHp}/{maxHp}";
    }

    private void UpdateUITransform()
    {
        if (uiRoot == null) return;

        float y = extraHeight;

        if (sr != null)
            y += sr.bounds.extents.y;

        uiRoot.position = transform.position + new Vector3(xOffset, y, 0f);

        if (keepUpright)
            uiRoot.rotation = Quaternion.identity;

        if (keepScalePositive)
        {
            Vector3 s = uiRoot.localScale;
            uiRoot.localScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        }
    }
}
