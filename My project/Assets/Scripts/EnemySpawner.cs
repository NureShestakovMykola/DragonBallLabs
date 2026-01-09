using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy skins (10 prefabs)")]
    [Tooltip("Wave 1 -> [0], Wave 2 -> [1], ... Wave 10 -> [9], Wave 11 -> [0] (по колу).")]
    public Transform[] enemyPrefabs;

    [Header("Boss skins (multiple prefabs)")]
    [Tooltip("EnemyBoss, EnemyBoss_B, EnemyBoss_C, EnemyBoss_D ...")]
    public Transform[] bossPrefabs;

    [Tooltip("false = по пор€дку (A,B,C,D...), true = випадково.")]
    public bool randomBossSkin = false;

    [Header("Boss")]
    [Tooltip(" ожн≥ N хвиль буде бос. 3 => хвил≥ 3,6,9,...")]
    public int bossEveryNWaves = 3;

    [Tooltip("HP першого боса (при laser.damage=1).")]
    public int bossBaseHp = 30;

    [Tooltip("«б≥льшенн€ HP кожного наступного боса.")]
    public int bossHpIncreasePerBoss = 10;

    [Tooltip("ќчки за боса (через EnemyHealth.scoreValue).")]
    public int bossScoreValue = 25;

    [Tooltip("ƒодатковий множник швидкост≥ боса.")]
    public float bossMoveSpeedMultiplier = 0.85f;

    [Header("Boss Death FX override (recommended)")]
    [Tooltip("якщо задано Ч буде примусово встановлено у EnemyHealth.bossDeathFxPrefab дл€ кожного заспавненого боса.")]
    public GameObject bossDeathFxPrefab;

    [Tooltip("«сув ефекту смерт≥ боса (world). «азвичай 0,0,0 або невеликий +Y.")]
    public Vector3 bossDeathFxOffset = Vector3.zero;

    [Tooltip("ћножник масштабу FX смерт≥ боса (1 = без зм≥н).")]
    public float bossDeathFxScaleMultiplier = 1.8f;

    [Tooltip("якщо true Ч спавнер встановлюЇ пол€ boss FX у EnemyHealth боса.")]
    public bool applyBossDeathFxOverride = true;

    [Header("References")]
    [Tooltip("ѕерет€гн≥ть Player (Transform) сюди.")]
    public Transform player;

    [Header("Wave settings")]
    public float waveDuration = 20f;

    [Header("Spawn timing (Wave 1)")]
    public float spawnMinStart = 1.6f;
    public float spawnMaxStart = 2.4f;

    [Header("Spawn timing limits")]
    public float spawnMinLimit = 0.35f;
    public float spawnMaxLimit = 0.60f;

    [Range(0.5f, 1f)]
    public float spawnIntervalFactorPerWave = 0.90f;

    [Header("Max enemies on screen")]
    public int maxAliveStart = 10;
    public int maxAliveIncreasePerWave = 5;

    [Header("Spawn burst")]
    public int spawnPerTickStart = 2;
    public int spawnPerTickEveryNWaves = 1;
    public int spawnPerTickMax = 8;

    [Header("Enemy scaling per wave")]
    [Range(1f, 2f)]
    [Tooltip("ћножник швидкост≥ ворог≥в по хвил€х ƒќ 5-њ хвил≥.")]
    public float enemySpeedFactorPerWave = 1.10f;

    [Tooltip("ѕочинаючи з ц≥Їњ хвил≥ множник швидкост≥ перестаЇ зростати (або росте слабше).")]
    public int speedCapFromWave = 6; // тобто п≥сл€ 5-њ

    [Tooltip("якщо true Ч п≥сл€ 5-њ хвил≥ швидк≥сть Ќ≈ росте (капитьс€). якщо false Ч росте, але пов≥льн≥ше.")]
    public bool hardCapEnemySpeed = true;

    [Range(1f, 1.2f)]
    [Tooltip("якщо hardCapEnemySpeed=false Ч п≥сл€ 5-њ хвил≥ використовуЇтьс€ цей слабший множник.")]
    public float enemySpeedFactorAfterCap = 1.03f;

    [Header("Spawn area")]
    public float spawnMargin = 0.5f;

    [Header("No spawn near player (anti-instant-death)")]
    [Tooltip("–ад≥ус, у €кому вороги/бос Ќ≈ можуть спавнитись б≥л€ гравц€.")]
    public float noSpawnRadius = 3.0f;

    [Tooltip("—к≥льки раз≥в пробувати знайти позиц≥ю спавну, що не близько до гравц€.")]
    public int maxSpawnAttempts = 40;

    [Tooltip("ƒодатковий safety-буфер до noSpawnRadius (щоб точно не поруч).")]
    public float extraSafeDistance = 0.25f;

    [Header("Boss wave behaviour")]
    public bool spawnMinionsDuringBossWave = false;
    public float bossWaveMinionSpawnSlowdown = 2.5f;

    private Camera cam;

    private int lastWaveReported = -1;

    private int lastBossWaveSpawned = -1;
    private int bossesSpawned = 0;
    private Transform bossInstance;

    void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        int wave = GetCurrentWave();

        if (wave != lastWaveReported)
        {
            lastWaveReported = wave;
            if (GameManager.Instance != null)
                GameManager.Instance.SetWave(wave);
        }

        if (IsBossWave(wave))
        {
            SpawnBossIfNeeded(wave);
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield return null;
                continue;
            }

            if (cam == null) cam = Camera.main;
            if (cam == null)
            {
                yield return null;
                continue;
            }

            int waveNow = GetCurrentWave();
            bool bossWave = IsBossWave(waveNow);

            if (bossWave && !spawnMinionsDuringBossWave)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                yield return null;
                continue;
            }

            int waveForWait = GetCurrentWave();

            float spawnMin = Mathf.Max(spawnMinLimit,
                spawnMinStart * Mathf.Pow(spawnIntervalFactorPerWave, waveForWait - 1));

            float spawnMax = Mathf.Max(spawnMaxLimit,
                spawnMaxStart * Mathf.Pow(spawnIntervalFactorPerWave, waveForWait - 1));

            if (IsBossWave(waveForWait) && spawnMinionsDuringBossWave)
            {
                spawnMin *= bossWaveMinionSpawnSlowdown;
                spawnMax *= bossWaveMinionSpawnSlowdown;
            }

            float wait = Random.Range(spawnMin, spawnMax);
            yield return new WaitForSeconds(wait);

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                continue;

            int wave = GetCurrentWave();

            if (IsBossWave(wave) && !spawnMinionsDuringBossWave)
                continue;

            int maxAlive = maxAliveStart + (wave - 1) * maxAliveIncreasePerWave;

            int alive = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (alive >= maxAlive) continue;

            int spawnPerTick = spawnPerTickStart;
            if (spawnPerTickEveryNWaves > 0)
                spawnPerTick += (wave - 1) / spawnPerTickEveryNWaves;

            spawnPerTick = Mathf.Clamp(spawnPerTick, 1, spawnPerTickMax);

            int allowed = maxAlive - alive;
            int toSpawn = Mathf.Min(spawnPerTick, allowed);

            Transform prefab = GetPrefabForWave(wave);
            if (prefab == null) continue;

            float speedMultiplier = GetEnemySpeedMultiplier(wave);

            for (int i = 0; i < toSpawn; i++)
            {
                if (!TryGetSpawnPosition(out Vector3 pos))
                    break; // €кщо не знайшли безпечну точку Ч не спавнимо

                Transform enemy = Instantiate(prefab, pos, Quaternion.identity);

                var move = enemy.GetComponent<EnemyRandomMove>();
                if (move != null)
                    move.moveSpeed *= speedMultiplier;
            }
        }
    }

    float GetEnemySpeedMultiplier(int wave)
    {
        if (wave < speedCapFromWave)
        {
            return Mathf.Pow(enemySpeedFactorPerWave, wave - 1);
        }

        int capWave = Mathf.Max(1, speedCapFromWave - 1);
        float atCap = Mathf.Pow(enemySpeedFactorPerWave, capWave - 1);

        if (hardCapEnemySpeed)
        {
            return atCap;
        }
        else
        {
            int extraWaves = wave - capWave;
            float after = Mathf.Pow(enemySpeedFactorAfterCap, extraWaves);
            return atCap * after;
        }
    }

    void SpawnBossIfNeeded(int wave)
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0) return;
        if (lastBossWaveSpawned == wave) return;
        if (bossInstance != null) return;

        Transform chosenBossPrefab = ChooseBossPrefab();
        if (chosenBossPrefab == null) return;

        if (!TryGetSpawnPosition(out Vector3 pos))
            return;

        bossInstance = Instantiate(chosenBossPrefab, pos, Quaternion.identity);
        lastBossWaveSpawned = wave;

        bossesSpawned += 1;
        int bossIndex = bossesSpawned;

        int bossHp = Mathf.Max(1, bossBaseHp + (bossIndex - 1) * bossHpIncreasePerBoss);

        float waveSpeedMultiplier = GetEnemySpeedMultiplier(wave);

        var move = bossInstance.GetComponent<EnemyRandomMove>();
        if (move != null)
            move.moveSpeed *= (waveSpeedMultiplier * bossMoveSpeedMultiplier);

        var hp = bossInstance.GetComponent<EnemyHealth>();
        if (hp != null)
        {
            hp.SetHp(bossHp);
            hp.SetScoreValue(bossScoreValue);

            // ¬ј∆Ћ»¬ќ: централ≥зовано задаЇмо FX дл€ боса (щоб не правити кожен boss prefab вручну)
            if (applyBossDeathFxOverride)
            {
                hp.bossDeathFxPrefab = bossDeathFxPrefab;
                hp.bossDeathFxOffset = bossDeathFxOffset;
                hp.bossDeathFxScaleMultiplier = bossDeathFxScaleMultiplier;
            }
        }

        bossInstance.name = $"BOSS_{bossIndex}_HP_{bossHp}";
    }

    Transform ChooseBossPrefab()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0) return null;

        int nextBossNumber = bossesSpawned + 1;

        if (randomBossSkin)
        {
            int rnd = Random.Range(0, bossPrefabs.Length);
            return bossPrefabs[rnd];
        }
        else
        {
            int index = (nextBossNumber - 1) % bossPrefabs.Length;
            return bossPrefabs[index];
        }
    }

    Transform GetPrefabForWave(int wave)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;
        int index = (wave - 1) % enemyPrefabs.Length;
        return enemyPrefabs[index];
    }

    bool IsBossWave(int wave)
    {
        if (bossEveryNWaves <= 0) return false;
        return (wave % bossEveryNWaves) == 0;
    }

    int GetCurrentWave()
    {
        return 1 + Mathf.FloorToInt(Time.timeSinceLevelLoad / Mathf.Max(1f, waveDuration));
    }

    bool TryGetSpawnPosition(out Vector3 result)
    {
        result = Vector3.zero;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 candidate = GetRandomInsideCamera();

            if (player == null)
            {
                result = candidate;
                return true;
            }

            float safeRadius = Mathf.Max(0.1f, noSpawnRadius + extraSafeDistance);
            float dist = Vector2.Distance(candidate, player.position);

            if (dist >= safeRadius)
            {
                result = candidate;
                return true;
            }
        }

        return false;
    }

    Vector3 GetRandomInsideCamera()
    {
        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        float x = Random.Range(cam.transform.position.x - w + spawnMargin,
                               cam.transform.position.x + w - spawnMargin);

        float y = Random.Range(cam.transform.position.y - h + spawnMargin,
                               cam.transform.position.y + h - spawnMargin);

        return new Vector3(x, y, 0f);
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(player.position, noSpawnRadius);

        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(player.position, noSpawnRadius + extraSafeDistance);
    }
}
