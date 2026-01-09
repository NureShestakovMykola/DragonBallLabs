using UnityEngine;

public class EnemyTouchPlayer : MonoBehaviour
{
    [Header("Contact damage")]
    [Tooltip("—к≥льки HP зн≥маЇ контакт ≥з ворогом.")]
    public int touchDamage = 10;

    [Tooltip("якщо true Ч ворог знищуЇтьс€ п≥сл€ удару об гравц€.")]
    public bool destroySelfOnHit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    void HandleHit(Collider2D col)
    {
        PlayerHealth ph = col.GetComponent<PlayerHealth>();
        if (ph == null) ph = col.GetComponentInParent<PlayerHealth>();
        if (ph == null) return;

        ph.TakeDamage(touchDamage);

        if (destroySelfOnHit)
        {
            // —амознищенн€ Ќ≈ повинно нараховувати очки €к "kill"
            EnemyHealth eh = GetComponent<EnemyHealth>();
            if (eh == null) eh = GetComponentInParent<EnemyHealth>();

            if (eh != null)
                eh.Kill(awardScore: false);
            else
                Destroy(gameObject);
        }
    }
}
