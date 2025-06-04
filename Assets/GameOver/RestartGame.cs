using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public void Restart()
    {
        // Перезагружаем основную сцену (замените "MainScene" на имя вашей игровой сцены)
        SceneManager.LoadScene("SampleScene");
    }

    public void ReturnToMainMenu()
    {

        // Загружаем сцену с главным меню (убедитесь, что она добавлена в Build Settings)
        SceneManager.LoadScene("MainMenu");

        // Если у вас главное меню называется иначе, замените "MainMenu" на имя вашей сцены
    }
}
