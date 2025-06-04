using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public void ExitGame()
    {
        // ������� ��������� � ������� ��� �������
        Debug.Log("����� �� ����...");

        // ��� ��������� WebGL ���������� ��������� (��� ��� Application.Quit() �� ��������)
        #if UNITY_WEBGL
                Debug.Log("� WebGL ����� ���������� - ���������� ���������");
                // ����� �������� UI-��������� ��� ������
                FindObjectOfType<UIManager>().ShowMessage("� �������� ���� �� �����������. ����������� Alt+F4");
        #else
                // � ��������� ������������� Play Mode
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                // � ��������� ������ ��������� ����������
                Application.Quit();
        #endif
        #endif
    }
}
