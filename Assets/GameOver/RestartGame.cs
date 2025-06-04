using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public void Restart()
    {
        // ������������� �������� ����� (�������� "MainScene" �� ��� ����� ������� �����)
        SceneManager.LoadScene("SampleScene");
    }

    public void ReturnToMainMenu()
    {

        // ��������� ����� � ������� ���� (���������, ��� ��� ��������� � Build Settings)
        SceneManager.LoadScene("MainMenu");

        // ���� � ��� ������� ���� ���������� �����, �������� "MainMenu" �� ��� ����� �����
    }
}
