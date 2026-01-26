// _Scripts/MenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button playButton;
    public Button exitButton;

    void Start()
    {
        // Назначаем действия кнопкам
        playButton.onClick.AddListener(OnPlayClick);
        exitButton.onClick.AddListener(OnExitClick);
    }

    void OnPlayClick()
    {
        // Переходим на карту уровней
        SceneManager.LoadScene("MapScene");
    }

    void OnExitClick()
    {
        // Выход из игры
        Application.Quit();

        // В редакторе Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}