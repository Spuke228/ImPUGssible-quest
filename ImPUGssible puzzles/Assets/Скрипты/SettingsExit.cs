using UnityEngine;
using UnityEngine.SceneManagement; // если сцена будет меняться

public class SettingsExit : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu"; // имя сцены меню
    [SerializeField] private GameObject settingsPanel; // если настройки на панели, а не сцена

    private void Update()
    {
        // Выход по Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitSettings();
        }
    }

    // Привязать к кнопке через инспектор
    public void ExitSettings()
    {
        if (settingsPanel != null)
        {
            // скрываем панель настроек
            settingsPanel.SetActive(false);
        }
        else
        {
            // или загружаем сцену меню
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
