using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public void ExitGame()
    {
        // Выводим сообщение в консоль для отладки
        Debug.Log("Выход из игры...");

        // Для платформы WebGL показываем сообщение (так как Application.Quit() не работает)
        #if UNITY_WEBGL
                Debug.Log("В WebGL выход невозможен - показываем сообщение");
                // Можно добавить UI-сообщение для игрока
                FindObjectOfType<UIManager>().ShowMessage("В браузере игра не закрывается. Используйте Alt+F4");
        #else
                // В редакторе останавливаем Play Mode
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                // В собранной версии закрываем приложение
                Application.Quit();
        #endif
        #endif
    }
}
