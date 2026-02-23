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
    public Button levelButton18;
    public Button levelButton19;
    public Button levelButton20;
    public Button backButton;

    // КНОПКА ОЧИСТКИ ПРОГРЕССА
    public Button resetProgressButton;

    // ПРЕФАБ ПАНЕЛИ ПОДТВЕРЖДЕНИЯ
    public GameObject confirmPanelPrefab;

    // Цвета для кнопок
    public Color lockedColor = Color.gray;
    public Color unlockedColor = new Color(0.2f, 0.6f, 1f); // Синий
    public Color passedColor = Color.green;

    // ПЕРЕМЕННЫЕ
    private GameObject currentConfirmPanel;

    void Start()
    {
        Debug.Log("=== MAP MANAGER ЗАПУЩЕН ===");

        // ИНИЦИАЛИЗАЦИЯ ПРОГРЕССА
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

        // КНОПКА ОЧИСТКИ ПРОГРЕССА
        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.AddListener(ShowConfirmPanel);
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
            for (int i = 2; i <= 17; i++)
            {
                PlayerPrefs.SetInt($"Level{i}_Status", 0);
                PlayerPrefs.SetInt($"Level{i}_Passed", 0);
            }

            PlayerPrefs.SetInt("GameInitialized", 1);
            PlayerPrefs.Save();

            Debug.Log("Прогресс инициализирован");
        }
    }

    void ShowProgressDebug()
    {
        Debug.Log("=== ТЕКУЩИЙ ПРОГРЕСС ===");
        for (int i = 1; i <= 20; i++)
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
        SetupButton(levelButton18, 18);
        SetupButton(levelButton19, 19);
        SetupButton(levelButton20, 20);
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
        }
    }

    void LoadLevel(int levelNumber)
    {
        Debug.Log($"Загружаем уровень {levelNumber}");

        // Сохраняем выбранный уровень
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
            case 11:
                SceneManager.LoadScene("Level11");
                break;
            case 12:
                SceneManager.LoadScene("Level12");
                break;
            case 13:
                SceneManager.LoadScene("Level13");
                break;
            case 14:
                SceneManager.LoadScene("Level14");
                break;
            case 15:
                SceneManager.LoadScene("Level15");
                break;
            case 16:
                SceneManager.LoadScene("Level16");
                break;
            case 17:
                SceneManager.LoadScene("Level17");
                break;
            case 18:
                SceneManager.LoadScene("Level18");
                break;
            case 19:
                SceneManager.LoadScene("Level19");
                break;
            case 20:
                SceneManager.LoadScene("Level20");
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

    // ПОКАЗАТЬ ПАНЕЛЬ ПОДТВЕРЖДЕНИЯ
    void ShowConfirmPanel()
    {
        if (confirmPanelPrefab == null)
        {
            Debug.LogError("confirmPanelPrefab не назначен!");
            return;
        }

        // Если уже есть открытая панель - закрываем её
        if (currentConfirmPanel != null)
        {
            Destroy(currentConfirmPanel);
        }

        // Создаём панель
        currentConfirmPanel = Instantiate(confirmPanelPrefab);
        currentConfirmPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // Находим кнопки в префабе
        Button yesButton = FindButtonInChildren(currentConfirmPanel, "YesButton");
        Button noButton = FindButtonInChildren(currentConfirmPanel, "NoButton");

        // Назначаем действия
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(ResetProgress);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(CloseConfirmPanel);
        }

        // Находим текст
        Text confirmText = FindTextInChildren(currentConfirmPanel);
        if (confirmText != null)
        {
            confirmText.text = "Точно сбросить весь прогресс?\nВсе уровни будут закрыты, кроме первого.";
        }
    }

    // ЗАКРЫТЬ ПАНЕЛЬ ПОДТВЕРЖДЕНИЯ
    void CloseConfirmPanel()
    {
        if (currentConfirmPanel != null)
        {
            Destroy(currentConfirmPanel);
            currentConfirmPanel = null;
        }
    }

    // СБРОСИТЬ ПРОГРЕСС
    void ResetProgress()
    {
        Debug.Log("СБРАСЫВАЮ ВЕСЬ ПРОГРЕСС");

        // Удаляем все сохранения
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Прогресс сброшен");

        // Закрываем панель
        CloseConfirmPanel();

        // Перезагружаем карту
        SceneManager.LoadScene("MapScene");
    }

    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ПОИСКА
    Button FindButtonInChildren(GameObject parent, string buttonName)
    {
        Transform[] allChildren = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == buttonName)
            {
                return child.GetComponent<Button>();
            }
        }
        return null;
    }

    Text FindTextInChildren(GameObject parent)
    {
        Text[] allTexts = parent.GetComponentsInChildren<Text>(true);
        foreach (Text text in allTexts)
        {
            // Пропускаем текст на кнопках
            if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
            {
                continue;
            }
            return text;
        }
        return null;
    }

    // ОБНОВЛЕНИЕ ПРОГРЕССА ПОСЛЕ ПРОХОЖДЕНИЯ УРОВНЯ
    public static void MarkLevelAsPassed(int levelNumber)
    {
        Debug.Log($"Отмечаем уровень {levelNumber} как пройденный");

        // Отмечаем текущий уровень как пройденный
        PlayerPrefs.SetInt($"Level{levelNumber}_Passed", 1);

        // Открываем следующий уровень (если он есть)
        if (levelNumber < 20)
        {
            int nextLevel = levelNumber + 1;
            PlayerPrefs.SetInt($"Level{nextLevel}_Status", 1); // Открываем следующий
            Debug.Log($"Открываем уровень {nextLevel}");
        }

        PlayerPrefs.Save();

        // Выводим отладку
        Debug.Log($"Уровень {levelNumber} пройден! Сохранено.");
    }
}