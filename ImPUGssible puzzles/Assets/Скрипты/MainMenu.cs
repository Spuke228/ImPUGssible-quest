using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Запуск игры (смена сцены)
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");  // название сцены как в Build Settings
    }

    // Открыть настройки
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    // Выход из игры
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Игра закрыта"); // В редакторе Unity это просто сообщение, а в билде реально закроет игру
    }
}
