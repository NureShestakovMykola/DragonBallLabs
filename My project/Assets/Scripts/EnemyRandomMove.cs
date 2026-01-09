using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRandomMove : MonoBehaviour
{
    [Header("Speed")]
    public float moveSpeed = 1.6f;

    [Header("Chase behaviour")]
    [Tooltip("Якщо true — ворог переслідує гравця.")]
    public bool chasePlayer = true;

    [Tooltip("Сила руху до гравця (чим більше — тим агресивніше).")]
    public float chaseStrength = 1.0f;

    [Tooltip("Сила випадкового відхилення (щоб рух був 'живим').")]
    public float wanderStrength = 0.18f;

    [Tooltip("Як часто оновлювати цільовий напрям (сек).")]
    public float retargetMin = 0.25f;
    public float retargetMax = 0.55f;

    [Tooltip("Згладжування повороту напрямку. Більше = швидше реагує.")]
    public float directionSmooth = 10f;

    [Header("Target finding")]
    [Tooltip("Шукати гравця по Tag=Player, якщо target не заданий.")]
    public bool autoFindPlayerByTag = true;

    [Tooltip("Якщо потрібно — можна призначити ціль вручну.")]
    public Transform target;

    [Tooltip("Передбачення руху гравця (якщо у гравця є Rigidbody2D).")]
    public bool leadTarget = true;

    [Tooltip("Час випередження (сек).")]
    public float leadTime = 0.15f;

    [Header("Enemy separation")]
    [Tooltip("Радіус, у якому ворог відштовхується від інших ворогів.")]
    public float separationRadius = 0.55f;

    [Tooltip("Сила відштовхування від інших ворогів.")]
    public float separationStrength = 0.65f;

    [Header("Screen bounds bounce")]
    [Tooltip("Відступ від країв екрана всередину (world units).")]
    public float screenPadding = 0.30f;

    public bool useMainCamera = true;
    public Camera cameraOverride;

    private Rigidbody2D rb;
    private Vector2 desiredDir = Vector2.right;
    private Vector2 currentDir = Vector2.right;

    private float timer;
    private Camera cam;

    private Rigidbody2D targetRb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Критично: щоб не обертало від колізій
        rb.freezeRotation = true;
        rb.gravityScale = 0f;

        cam = useMainCamera ? Camera.main : cameraOverride;

        AcquireTarget();
        PickNewDirection();
    }

    void AcquireTarget()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            return;
        }

        if (!autoFindPlayerByTag) return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            target = p.transform;
            targetRb = p.GetComponent<Rigidbody2D>();
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;

        if (cam == null)
            cam = useMainCamera ? Camera.main : cameraOverride;

        if (chasePlayer && (target == null) && autoFindPlayerByTag)
            AcquireTarget();

        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
            PickNewDirection();

        // Плавно наближуємо поточний напрям до бажаного
        float t = 1f - Mathf.Exp(-directionSmooth * Time.fixedDeltaTime);
        currentDir = Vector2.Lerp(currentDir, desiredDir, t);

        if (currentDir.sqrMagnitude < 0.0001f)
            currentDir = Vector2.right;

        currentDir.Normalize();

        rb.linearVelocity = currentDir * moveSpeed;

        BounceFromScreenEdges();
    }

    void PickNewDirection()
    {
        timer = Random.Range(retargetMin, retargetMax);

        Vector2 dir = Vector2.zero;

        // 1) Напрям до гравця
        if (chasePlayer && target != null)
        {
            Vector2 toTarget = (Vector2)target.position - rb.position;

            // випередження (якщо у гравця є Rigidbody2D)
            if (leadTarget && targetRb != null)
                toTarget += targetRb.linearVelocity * leadTime;

            if (toTarget.sqrMagnitude > 0.0001f)
                dir += toTarget.normalized * chaseStrength;
        }

        // 2) Трохи випадковості
        Vector2 wander = Random.insideUnitCircle.normalized;
        dir += wander * wanderStrength;

        // 3) Розштовхування від інших ворогів
        if (separationRadius > 0.01f && separationStrength > 0.01f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, separationRadius);
            Vector2 away = Vector2.zero;
            int count = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;
                if (hits[i].attachedRigidbody == rb) continue; // це ми

                // Відштовхуємось лише від об’єктів з Tag "Enemy"
                if (!hits[i].CompareTag("Enemy")) continue;

                Vector2 diff = rb.position - (Vector2)hits[i].transform.position;
                float d = diff.magnitude;
                if (d < 0.0001f) continue;

                away += diff / d; // нормалізований вклад
                count++;
            }

            if (count > 0)
            {
                away /= count;
                if (away.sqrMagnitude > 0.0001f)
                    dir += away.normalized * separationStrength;
            }
        }

        // 4) Якщо чомусь dir порожній — fallback
        if (dir.sqrMagnitude < 0.0001f)
            dir = Random.insideUnitCircle.normalized;

        desiredDir = dir.normalized;
    }

    void BounceFromScreenEdges()
    {
        if (cam == null) return;

        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        float left = cam.transform.position.x - w + screenPadding;
        float right = cam.transform.position.x + w - screenPadding;
        float bottom = cam.transform.position.y - h + screenPadding;
        float top = cam.transform.position.y + h - screenPadding;

        Vector2 pos = rb.position;
        bool bounced = false;

        if (pos.x < left)
        {
            pos.x = left;
            currentDir.x = Mathf.Abs(currentDir.x);
            desiredDir.x = Mathf.Abs(desiredDir.x);
            bounced = true;
        }
        else if (pos.x > right)
        {
            pos.x = right;
            currentDir.x = -Mathf.Abs(currentDir.x);
            desiredDir.x = -Mathf.Abs(desiredDir.x);
            bounced = true;
        }

        if (pos.y < bottom)
        {
            pos.y = bottom;
            currentDir.y = Mathf.Abs(currentDir.y);
            desiredDir.y = Mathf.Abs(desiredDir.y);
            bounced = true;
        }
        else if (pos.y > top)
        {
            pos.y = top;
            currentDir.y = -Mathf.Abs(currentDir.y);
            desiredDir.y = -Mathf.Abs(desiredDir.y);
            bounced = true;
        }

        if (bounced)
        {
            rb.position = pos;
            currentDir = currentDir.normalized;
            desiredDir = desiredDir.normalized;
            rb.linearVelocity = currentDir * moveSpeed;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (separationRadius > 0.01f)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, separationRadius);
        }
    }
}
