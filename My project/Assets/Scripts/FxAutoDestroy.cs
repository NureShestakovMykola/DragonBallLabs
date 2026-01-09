using System.Collections;
using UnityEngine;

public class FxAutoDestroy : MonoBehaviour
{
    private ParticleSystem[] systems;
    private Coroutine routine;

    void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>(true);
    }

    void OnEnable()
    {
        if (systems == null || systems.Length == 0)
            systems = GetComponentsInChildren<ParticleSystem>(true);

        // На випадок повторного вмикання
        if (routine != null)
            StopCoroutine(routine);

        // Програти всі системи (якщо хтось вимкнений/зупинений)
        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i] != null)
                systems[i].Play(true);
        }

        routine = StartCoroutine(WaitAndDestroy());
    }

    private IEnumerator WaitAndDestroy()
    {
        // 1 кадр — щоб системи коректно стартували
        yield return null;

        while (true)
        {
            bool anyAlive = false;

            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i] == null) continue;

                if (systems[i].IsAlive(true))
                {
                    anyAlive = true;
                    break;
                }
            }

            if (!anyAlive)
                break;

            yield return null;
        }

        Destroy(gameObject);
    }
}
