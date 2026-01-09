using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text scoreText;
    public GameObject gameOverPanel;
    public TMP_Text gameOverTitle;
    public TMP_Text gameOverHint;
    public TMP_Text waveText;

    [Header("Start Screen")]
    [Tooltip("Панель стартового екрану (StartPanel). Має перекривати HUD.")]
    public GameObject startPanel;

    [Tooltip("Якщо true — на старті показується StartPanel і гра ставиться на паузу до натискання Start.")]
    public bool pauseGameUntilStart = true;

    [Tooltip("Додатково дозволити старт з клавіатури (Enter/Space). Можна вимкнути, якщо потрібна тільки кнопка.")]
    public bool allowKeyboardStart = true;

    [Header("Scoring")]
    public int scorePerKill = 1;

    public bool IsGameOver { get; private set; } = false;

    // Стан “гра запущена” (до цього моменту керування/постріл блокуються)
    public bool IsGameStarted { get; private set; } = false;

    private int score = 0;

    void Awake()
    {
        // Безпечне скидання timeScale (після перезапуску/переходів могло лишитися 0)
        Time.timeScale = 1f;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        UpdateScoreUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameOverTitle != null)
            gameOverTitle.text = "GAME OVER";

        if (gameOverHint != null)
            gameOverHint.text = "Press R to restart";

        // Стартовий екран
        if (pauseGameUntilStart)
        {
            ShowStartScreen();
        }
        else
        {
            // Якщо стартовий екран не потрібен
            IsGameStarted = true;
            if (startPanel != null) startPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void Update()
    {
        // Рестарт після Game Over
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
            return;
        }

        // Додатковий старт з клавіатури (за потреби)
        if (!IsGameOver && pauseGameUntilStart && !IsGameStarted && allowKeyboardStart)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
        }
    }

    public void AddKillScore()
    {
        // Очки не нараховуються, якщо гра не стартувала або вже game over
        if (IsGameOver) return;
        if (!IsGameStarted) return;

        score += scorePerKill;
        UpdateScoreUI();
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        // На Game Over стартовий екран не показується
        if (startPanel != null)
            startPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartScene()
    {
        // Перед перезавантаженням сцени timeScale має бути 1
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void SetWave(int wave)
    {
        // Хвиля може відображатися навіть до старту, але за потреби можна заблокувати:
        // if (!IsGameStarted) return;

        if (waveText != null)
            waveText.text = $"Wave: {wave}";
    }

    // ===== START SCREEN API =====

    private void ShowStartScreen()
    {
        IsGameOver = false;
        IsGameStarted = false;

        if (startPanel != null)
            startPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Викликається кнопкою Start (OnClick).
    /// </summary>
    public void StartGame()
    {
        if (IsGameOver) return;
        if (IsGameStarted) return;

        IsGameStarted = true;

        if (startPanel != null)
            startPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
