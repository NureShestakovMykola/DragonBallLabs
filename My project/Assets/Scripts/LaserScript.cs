using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserScript : MonoBehaviour
{
    public float lifetime = 2.0f;
    public float speed = 8.0f;
    public int damage = 1;

    [Header("Direction")]
    [Tooltip("Напрям польоту в world-координатах. Якщо не задано ззовні, буде взято transform.right.")]
    public Vector2 moveDir = Vector2.right;

    [Tooltip("Якщо true — повертає спрайт лазера у напрямку польоту (візуально).")]
    public bool rotateToDirection = true;

    [Tooltip("Додатковий кут (градуси), якщо спрайт лазера «дивиться» не вправо. Напр., якщо спрайт дивиться вгору — поставте -90.")]
    public float visualAngleOffset = 0f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Якщо напрям не задавали — підстрахуємося від prefab-орієнтації
        if (moveDir.sqrMagnitude < 0.0001f)
            moveDir = transform.right;

        moveDir = moveDir.normalized;

        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg + visualAngleOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Викликається одразу після Instantiate для встановлення напрямку польоту.
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        moveDir = dir.normalized;

        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg + visualAngleOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;

        Vector2 nextPos = rb.position + moveDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }
}
