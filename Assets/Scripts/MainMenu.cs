using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private RectTransform menuContainer;
    [SerializeField] private GameObject confirmPanel;

    [Header("Buttons (можно оставить пустыми — скрипт найдёт сам)")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private void Start()
    {
        // ---- 1. Найти кнопки вручную, если они не назначены в инспекторе ----
        if (playButton == null) playButton = GameObject.Find("Button")?.GetComponent<Button>();
        if (settingsButton == null) settingsButton = GameObject.Find("Button (1)")?.GetComponent<Button>();
        if (exitButton == null) exitButton = GameObject.Find("Button (2)")?.GetComponent<Button>();
        // Если ваши кнопки называются иначе — измените имена выше на свои.
        // Например, если у вас "PlayButton", "SettingsButton", "ExitButton" — используйте их.

        // ---- 2. Логирование, чтобы видеть, что найдено ----
        Debug.Log($"playButton найден: {playButton != null}");
        Debug.Log($"settingsButton найден: {settingsButton != null}");
        Debug.Log($"exitButton найден: {exitButton != null}");

        // ---- 3. Принудительно показать меню (без анимации, для теста) ----
        if (menuGroup != null)
        {
            menuGroup.alpha = 1;
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("menuGroup не назначен! Добавьте CanvasGroup на MenuContainer и перетащите в поле.");
        }

        if (menuContainer != null)
            menuContainer.anchoredPosition = Vector2.zero;
        else
            Debug.LogError("menuContainer не назначен! Перетащите RectTransform MenuContainer.");

        // ---- 4. Привязка событий кнопок ----
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
        else
            Debug.LogWarning("Кнопка 'Играть' не найдена! Убедитесь, что в сцене есть объект с именем 'Button'.");

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        else
            Debug.LogWarning("Кнопка 'Настройки' не найдена! Убедитесь, что есть объект 'Button (1)'.");

        if (exitButton != null)
            exitButton.onClick.AddListener(ShowExitConfirm);
        else
            Debug.LogWarning("Кнопка 'Выход' не найдена! Убедитесь, что есть объект 'Button (2)'.");

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(QuitGame);
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(HideExitConfirm);

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // ---- 5. Можно запустить анимацию появления (закомментировано для надёжности) ----
        // StartCoroutine(AnimateMenuAppear());
        // Если хотите анимацию — раскомментируйте и закомментируйте строки с принудительным показом.
    }

    // ---- Методы действий (без изменений) ----
    public void PlayGame()
    {
        PlayClickSound();
        SceneManager.LoadScene("SampleScene"); // замените на имя вашей сцены
    }

    public void OpenSettings()
    {
        PlayClickSound();
        SceneManager.LoadScene("SettingsScene"); // или открыть панель настроек
    }

    public void ShowExitConfirm()
    {
        PlayClickSound();
        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    public void HideExitConfirm()
    {
        PlayClickSound();
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    public void QuitGame()
    {
        PlayClickSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    // ---- Анимация появления (опционально) ----
    private IEnumerator AnimateMenuAppear()
    {
        if (menuGroup == null) yield break;
        float duration = 0.8f;
        float elapsed = 0f;
        Vector2 startPos = menuContainer != null ? menuContainer.anchoredPosition : Vector2.zero;
        Vector2 targetPos = Vector2.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = 1 - (1 - t) * (1 - t);
            float moveEased = t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

            menuGroup.alpha = Mathf.Lerp(0, 1, eased);
            if (menuContainer != null)
                menuContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, moveEased);
            yield return null;
        }

        menuGroup.alpha = 1;
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;
        if (menuContainer != null)
            menuContainer.anchoredPosition = targetPos;
    }
}