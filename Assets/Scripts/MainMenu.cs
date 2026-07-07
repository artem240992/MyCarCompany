using UnityEngine;
using UnityEngine.SceneManagement; // для работы со сценами

public class MainMenu : MonoBehaviour
{
    // Метод для кнопки "Играть" — загружает игровую сцену
    public void PlayGame()
    {
        // Предположим, игровая сцена называется "GameScene"
        // Вы можете изменить на точное имя вашей сцены
        SceneManager.LoadScene("SampleScene");
    }

    // Метод для кнопки "Настройки" — загружает сцену настроек (опционально)
    public void OpenSettings()
    {
        // Если у вас есть сцена настроек
        SceneManager.LoadScene("SettingsScene");
    }

    // Метод для кнопки "Выход" — закрывает приложение
    public void QuitGame()
    {
        // В редакторе это не работает, но в собранной игре — работает
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // для остановки в редакторе
        #endif
    }
}