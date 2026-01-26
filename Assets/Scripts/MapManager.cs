using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    // Ссылки на кнопки уровней
    public Button levelButton1;
    public Button levelButton2;
    public Button levelButton3;
    public Button levelButton4;
    public Button levelButton5;
    public Button levelButton6;
    public Button levelButton7;
    public Button levelButton8;
    public Button levelButton9;
    public Button levelButton10;
    public Button levelButton11;
    public Button levelButton12;
    public Button levelButton13;
    public Button levelButton14;
    public Button levelButton15;
    public Button levelButton16;
    public Button levelButton17;
    public Button backButton;

    // Цвета для кнопок
    public Color lockedColor = Color.gray;
    public Color unlockedColor = new Color(0.2f, 0.6f, 1f); // Синий
    public Color passedColor = Color.green;

    void Start()
    {
        Debug.Log("=== MAP MANAGER ЗАПУЩЕН ===");

        // ИНИЦИАЛИЗАЦИЯ ПРОГРЕССА (если первый запуск)
        InitializeProgress();

        // Показываем текущий прогресс в консоли
        ShowProgressDebug();

        // Настраиваем все кнопки
        SetupAllButtons();

        // Кнопка назад
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToMenu);
        }
    }

    void InitializeProgress()
    {
        // Если игра запускается впервые
        if (!PlayerPrefs.HasKey("GameInitialized"))
        {
            Debug.Log("Первая инициализация игры");

            // Уровень 1 всегда открыт
            PlayerPrefs.SetInt("Level1_Status", 1); // 1 = открыт
            PlayerPrefs.SetInt("Level1_Passed", 0); // 0 = не пройден

            // Остальные закрыты
            PlayerPrefs.SetInt("Level2_Status", 0); // 0 = закрыт
            PlayerPrefs.SetInt("Level2_Passed", 0);

            PlayerPrefs.SetInt("Level3_Status", 0);
            PlayerPrefs.SetInt("Level3_Passed", 0);

            PlayerPrefs.SetInt("Level4_Status", 0);
            PlayerPrefs.SetInt("Level4_Passed", 0);

            PlayerPrefs.SetInt("Level5_Status", 0);
            PlayerPrefs.SetInt("Level5_Passed", 0);

            PlayerPrefs.SetInt("Level6_Status", 0);
            PlayerPrefs.SetInt("Level6_Passed", 0);

            PlayerPrefs.SetInt("Level757_Status", 0);
            PlayerPrefs.SetInt("Level7_Passed", 0);

            PlayerPrefs.SetInt("Level8_Status", 0);
            PlayerPrefs.SetInt("Level8_Passed", 0);

            PlayerPrefs.SetInt("Level9_Status", 0);
            PlayerPrefs.SetInt("Level9_Passed", 0);

            PlayerPrefs.SetInt("Level10_Status", 0);
            PlayerPrefs.SetInt("Level10_Passed", 0);

            PlayerPrefs.SetInt("GameInitialized", 1);
            PlayerPrefs.Save();

            Debug.Log("Прогресс инициализирован");
        }
    }

    void ShowProgressDebug()
    {
        Debug.Log("=== ТЕКУЩИЙ ПРОГРЕСС ===");
        for (int i = 1; i <= 17; i++)
        {
            int status = PlayerPrefs.GetInt($"Level{i}_Status", 0);
            int passed = PlayerPrefs.GetInt($"Level{i}_Passed", 0);

            string statusStr = status == 1 ? "ОТКРЫТ" : "ЗАКРЫТ";
            string passedStr = passed == 1 ? "ПРОЙДЕН" : "НЕ ПРОЙДЕН";

            Debug.Log($"Уровень {i}: {statusStr}, {passedStr}");
        }
    }

    void SetupAllButtons()
    {
        // Настраиваем каждую кнопку
        SetupButton(levelButton1, 1);
        SetupButton(levelButton2, 2);
        SetupButton(levelButton3, 3);
        SetupButton(levelButton4, 4);
        SetupButton(levelButton5, 5);
        SetupButton(levelButton6, 6);
        SetupButton(levelButton7, 7);
        SetupButton(levelButton8, 8);
        SetupButton(levelButton9, 9);
        SetupButton(levelButton10, 10);
        SetupButton(levelButton11, 11);
        SetupButton(levelButton12, 12);
        SetupButton(levelButton13, 13);
        SetupButton(levelButton14, 14);
        SetupButton(levelButton15, 15);
        SetupButton(levelButton16, 16);
        SetupButton(levelButton17, 17);
    }

    void SetupButton(Button button, int levelNumber)
    {
        if (button == null)
        {
            Debug.LogError($"Кнопка уровня {levelNumber} не назначена!");
            return;
        }

        // Получаем статус уровня
        int status = PlayerPrefs.GetInt($"Level{levelNumber}_Status", 0);
        int passed = PlayerPrefs.GetInt($"Level{levelNumber}_Passed", 0);

        Debug.Log($"Настройка кнопки уровня {levelNumber}: статус={status}, пройден={passed}");

        if (status == 1) // Уровень открыт
        {
            button.interactable = true;

            // Текст кнопки
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = levelNumber.ToString();
            }

            // Цвет кнопки
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (passed == 1)
                {
                    buttonImage.color = passedColor; // Зелёный - пройден
                }
                else
                {
                    buttonImage.color = unlockedColor; // Синий - открыт, но не пройден
                }
            }

            // Назначаем действие
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => LoadLevel(levelNumber));

            Debug.Log($"Уровень {levelNumber}: ОТКРЫТ (пройден: {passed == 1})");
        }
        else // Уровень закрыт
        {
            button.interactable = false;

            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "Х";
            }

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = lockedColor;
            }

            Debug.Log($"Уровень {levelNumber}: ЗАКРЫТ");
        }
    }

    void LoadLevel(int levelNumber)
    {
        Debug.Log($"Загружаем уровень {levelNumber}");

        // Сохраняем выбранный уровень (на всякий случай)
        PlayerPrefs.SetInt("SelectedLevel", levelNumber);
        PlayerPrefs.Save();

        // Загружаем сцену
        switch (levelNumber)
        {
            case 1:
                SceneManager.LoadScene("Level1");
                break;
            case 2:
                SceneManager.LoadScene("Level2");
                break;
            case 3:
                SceneManager.LoadScene("Level3");
                break;
            case 4:
                SceneManager.LoadScene("Level4");
                break;
            case 5:
                SceneManager.LoadScene("Level5");
                break;
            case 6:
                SceneManager.LoadScene("Level6");
                break;
            case 7:
                SceneManager.LoadScene("Level7");
                break;
            case 8:
                SceneManager.LoadScene("Level8");
                break;
            case 9:
                SceneManager.LoadScene("Level9");
                break;
            case 10:
                SceneManager.LoadScene("Level10");
                break;
            default:
                Debug.LogError($"Неизвестный уровень: {levelNumber}");
                break;
        }
    }

    void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    // ОБНОВЛЕНИЕ ПРОГРЕССА ПОСЛЕ ПРОХОЖДЕНИЯ УРОВНЯ
    public static void MarkLevelAsPassed(int levelNumber)
    {
        Debug.Log($"Отмечаем уровень {levelNumber} как пройденный");

        // Отмечаем текущий уровень как пройденный
        PlayerPrefs.SetInt($"Level{levelNumber}_Passed", 1);

        // Открываем следующий уровень (если он есть)
        if (levelNumber < 17)
        {
            int nextLevel = levelNumber + 1;
            PlayerPrefs.SetInt($"Level{nextLevel}_Status", 1); // Открываем следующий
            Debug.Log($"Открываем уровень {nextLevel}");
        }

        PlayerPrefs.Save();

        // Выводим отладку
        Debug.Log($"Уровень {levelNumber} пройден! Сохранено.");
    }

    public void ResetProgress()
    {
        Debug.Log("СБРАСЫВАЮ ВЕСЬ ПРОГРЕСС");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Прогресс сброшен");

        // Перезагружаем карту
        SceneManager.LoadScene("MapScene");
    }
}