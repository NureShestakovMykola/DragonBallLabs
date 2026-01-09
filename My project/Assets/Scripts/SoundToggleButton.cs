using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundToggleButton : MonoBehaviour
{
    [Header("UI")]
    public Button button;

    [Tooltip("Текст на кнопці, якщо використовується TextMeshPro.")]
    public TMP_Text labelTMP;

    [Tooltip("Текст на кнопці, якщо використовується звичайний UI Text.")]
    public Text labelUI;

    [Header("Captions")]
    public string captionOn = "Sound: ON";
    public string captionOff = "Sound: OFF";

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        // Автопідписка на клік (щоб не залежати від On Click() в інспекторі)
        if (button != null)
        {
            button.onClick.RemoveListener(ToggleSound);
            button.onClick.AddListener(ToggleSound);
        }
    }

    void Start()
    {
        RefreshLabel();
    }

    public void ToggleSound()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.ToggleMuted();
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        bool muted = (AudioManager.Instance != null) && AudioManager.Instance.IsMuted;
        string text = muted ? captionOff : captionOn;

        if (labelTMP != null) labelTMP.text = text;
        if (labelUI != null) labelUI.text = text;
    }
}
