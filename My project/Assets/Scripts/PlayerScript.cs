using UnityEngine;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
    [Header("Movement")]
    public float playerSpeed = 2.0f;

    [Tooltip("Коефіцієнт інерції при відпусканні клавіш (0..1). Менше = швидше зупинка.")]
    [Range(0f, 1f)]
    public float inertiaDamping = 0.9f;

    [Header("Keyboard")]
    public List<KeyCode> upButton = new List<KeyCode>();
    public List<KeyCode> downButton = new List<KeyCode>();
    public List<KeyCode> leftButton = new List<KeyCode>();
    public List<KeyCode> rightButton = new List<KeyCode>();

    [Header("Mouse look (head always up)")]
    [Tooltip("Якщо спрайт дивиться 'не в той бік', увімкніть інверсію.")]
    public bool invertFlip = false;

    [Tooltip("Якщо SpriteRenderer не на цьому об'єкті, а на дочірньому — перетягніть його сюди.")]
    public SpriteRenderer spriteRendererOverride;

    [Header("Shooting")]
    [Tooltip("Prefab снаряда (laser).")]
    public Transform laserPrefab;

    [Tooltip("Відстань від гравця, на якій з'являється снаряд.")]
    public float laserDistance = 0.6f;

    [Tooltip("Затримка між пострілами (сек).")]
    public float timeBetweenFires = 0.25f;

    [Tooltip("Кнопки пострілу, наприклад Mouse0 і Space.")]
    public List<KeyCode> shootButton = new List<KeyCode>();

    private float timeTilNextFire = 0f;

    private float currentSpeed = 0.0f;
    private Vector3 lastMovement = Vector3.zero;

    private SpriteRenderer sr;
    private Camera cam;

    void Awake()
    {
        sr = spriteRendererOverride != null ? spriteRendererOverride : GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        FaceMouseFlipOnly();
        Movement();

        timeTilNextFire -= Time.deltaTime;
        CheckShooting();
    }

    void FaceMouseFlipOnly()
    {
        if (cam == null || sr == null) return;

        Vector3 worldPos = GetMouseWorld2D();

        transform.rotation = Quaternion.identity;

        bool mouseLeft = worldPos.x < transform.position.x;
        sr.flipX = invertFlip ? !mouseLeft : mouseLeft;
    }

    Vector3 GetMouseWorld2D()
    {
        if (cam == null) return transform.position;

        Vector3 mouse = Input.mousePosition;
        mouse.z = -cam.transform.position.z;

        Vector3 worldPos = cam.ScreenToWorldPoint(mouse);
        worldPos.z = 0f;
        return worldPos;
    }

    void Movement()
    {
        Vector3 movement = Vector3.zero;

        movement += MoveIfPressed(upButton, Vector3.up);
        movement += MoveIfPressed(downButton, Vector3.down);
        movement += MoveIfPressed(leftButton, Vector3.left);
        movement += MoveIfPressed(rightButton, Vector3.right);

        movement.Normalize();

        if (movement.magnitude > 0f)
        {
            currentSpeed = playerSpeed;
            transform.Translate(movement * Time.deltaTime * playerSpeed, Space.World);
            lastMovement = movement;
        }
        else
        {
            transform.Translate(lastMovement * Time.deltaTime * currentSpeed, Space.World);
            currentSpeed *= inertiaDamping;
        }
    }

    Vector3 MoveIfPressed(List<KeyCode> keyList, Vector3 movement)
    {
        foreach (KeyCode key in keyList)
        {
            if (Input.GetKey(key))
                return movement;
        }
        return Vector3.zero;
    }

    void CheckShooting()
    {
        if (timeTilNextFire > 0f) return;
        if (laserPrefab == null) return;

        bool pressed = false;
        foreach (KeyCode key in shootButton)
        {
            if (Input.GetKey(key))
            {
                pressed = true;
                break;
            }
        }
        if (!pressed) return;

        Vector3 mouseWorld = GetMouseWorld2D();
        Vector2 dir = (Vector2)(mouseWorld - transform.position);

        if (dir.sqrMagnitude < 0.000001f) return;
        dir.Normalize();

        Vector3 spawnPos = transform.position + (Vector3)(dir * laserDistance);

        Transform laserT = Instantiate(laserPrefab, spawnPos, Quaternion.identity);

        // Ключове виправлення: задаємо напрям польоту без залежності від transform.up/right
        LaserScript laser = laserT.GetComponent<LaserScript>();
        if (laser != null)
            laser.SetDirection(dir);

        // ЗВУК ПОСТРІЛУ
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShoot();

        timeTilNextFire = timeBetweenFires;
    }
}
