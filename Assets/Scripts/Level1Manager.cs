using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level1Manager : MonoBehaviour
{
    // ОСНОВНЫЕ ОБЪЕКТЫ
    public RectTransform pyBot;
    public RectTransform fridge;
    public RectTransform fridgeFront;
    public RectTransform startPoint;

    // UI ЭЛЕМЕНТЫ
    public InputField codeInput;
    public Text consoleText;
    public Button runButton;
    public Button resetButton;
    public Button backToMapButton;
    public Text taskTitle;

    // ПРЕФАБЫ
    public GameObject instructionPanelPrefab;
    public GameObject successPanelPrefab;

    // НАСТРОЙКИ
    public float moveSpeed = 300f;

    // ПРАВИЛЬНЫЙ КОД ДЛЯ УРОВНЯ
    private string correctCode = "move_to(\"fridge\")";

    // ПЕРЕМЕННЫЕ
    private bool isExecuting = false;
    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;
    private int currentLevel = 1;

    void Start()
    {
        Debug.Log("Level 1 Manager запущен");

        // Назначаем обработчики кнопок
        runButton.onClick.AddListener(OnRunClick);
        resetButton.onClick.AddListener(OnResetClick);
        backToMapButton.onClick.AddListener(GoToMap);

        // Если нет точки перед холодильником, используем сам холодильник
        if (fridgeFront == null)
        {
            fridgeFront = fridge;
        }

        // Показываем инструкцию
        ShowInstruction();
    }

    void ShowInstruction()
    {
        if (instructionPanelPrefab == null)
        {
            consoleText.text = "ИНСТРУКЦИЯ: Используй команду move_to(\"fridge\")";
            return;
        }

        // Создаём панель инструкции
        currentInstructionPanel = Instantiate(instructionPanelPrefab);
        currentInstructionPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // НАХОДИМ КНОПКУ СТАРТА
        Button startButton = FindButtonInChildren(currentInstructionPanel, "StartButton");
        if (startButton == null)
        {
            startButton = currentInstructionPanel.GetComponentInChildren<Button>();
        }

        if (startButton != null)
        {
            // Настраиваем текст кнопки
            Text buttonText = startButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "НАЧАТЬ";
            }

            // Назначаем действие для кнопки
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => {
                CloseInstruction();
            });
        }

        // ЗАПОЛНЯЕМ ТЕКСТОВЫЕ ПОЛЯ
        Text[] allTexts = currentInstructionPanel.GetComponentsInChildren<Text>(true);

        foreach (Text text in allTexts)
        {
            // Пропускаем текст на кнопках
            if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
            {
                continue;
            }

            string textName = text.gameObject.name.ToLower();

            if (textName.Contains("title"))
            {
                text.text = "УРОВЕНЬ 1: ОСНОВЫ ДВИЖЕНИЯ";
            }
            else if (textName.Contains("main") || textName.Contains("instruction"))
            {
                text.text = "Используй команду move_to(), чтобы PyBot двигался к указанному месту.\n\nPyBot подойдет к холодильнику и остановится перед ним.\n\nПиши точно как в примере.";
            }
            else if (textName.Contains("example") || textName.Contains("code"))
            {
                text.text = "Пиши в поле справа:\nmove_to(\"fridge\")";
            }
        }

        // ЗАПОЛНЯЕМ ПОЛЯ ВВОДА
        InputField[] allInputFields = currentInstructionPanel.GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (inputField.gameObject.name.ToLower().Contains("example"))
            {
                inputField.text = "move_to(\"fridge\")";
            }
        }

        // Блокируем игровой процесс
        runButton.interactable = false;
        codeInput.interactable = false;
        resetButton.interactable = false;
    }

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

    void CloseInstruction()
    {
        if (currentInstructionPanel != null)
        {
            Destroy(currentInstructionPanel);
        }

        // Разблокируем игровой процесс
        runButton.interactable = true;
        codeInput.interactable = true;
        resetButton.interactable = true;

        consoleText.text = "Введи команду и нажми ВЫПОЛНИТЬ";
    }

    void OnRunClick()
    {
        if (isExecuting)
        {
            consoleText.text = "Уже выполняется!";
            return;
        }

        StartCoroutine(RunCode());
    }

    void OnResetClick()
    {
        ResetLevel();
    }

    IEnumerator RunCode()
    {
        isExecuting = true;
        consoleText.text = "Проверяю код...";

        yield return new WaitForSeconds(0.5f);

        string code = codeInput.text.Trim(); // Убираем лишние пробелы

        // ТОЧНАЯ ПРОВЕРКА КОДА ДЛЯ УРОВНЯ 1
        if (code == correctCode)
        {
            consoleText.text = "Код правильный! Иду к холодильнику...";
            yield return new WaitForSeconds(0.5f);

            // 1. Идём к холодильнику
            yield return StartCoroutine(MovePyBotTo(fridgeFront.anchoredPosition));

            // 2. Успех
            consoleText.text = "Успех! PyBot дошел до холодильника!";
            ShowSuccess();
        }
        else
        {
            consoleText.text = "Ошибка! Попробуй ещё раз!";
        }

        isExecuting = false;
    }

    IEnumerator MovePyBotTo(Vector2 targetPosition)
    {
        Vector2 startPos = pyBot.anchoredPosition;
        float distance = Vector2.Distance(startPos, targetPosition);
        float duration = distance / moveSpeed;
        float timePassed = 0f;

        while (timePassed < duration)
        {
            pyBot.anchoredPosition = Vector2.Lerp(startPos, targetPosition, timePassed / duration);
            timePassed += Time.deltaTime;
            yield return null;
        }

        pyBot.anchoredPosition = targetPosition;
    }

    void ResetLevel()
    {
        StopAllCoroutines();
        isExecuting = false;

        // Возвращаем PyBot на стартовую позицию
        pyBot.anchoredPosition = startPoint.anchoredPosition;

        // ОЧИЩАЕМ ПОЛЕ ВВОДА
        if (codeInput != null)
        {
            codeInput.text = "";
        }

        // Очищаем сообщения в консоли
        consoleText.text = "Сброшено. Введи команду и нажми ВЫПОЛНИТЬ";

        // Закрываем окна если они открыты
        if (currentInstructionPanel != null)
        {
            Destroy(currentInstructionPanel);
        }
        if (currentSuccessPanel != null)
        {
            Destroy(currentSuccessPanel);
        }

        // Разблокируем кнопки
        runButton.interactable = true;
        codeInput.interactable = true;
    }

    void ShowSuccess()
    {
        Debug.Log("=== УРОВЕНЬ 1 ПРОЙДЕН ===");

        // Сохраняем прогресс
        SaveProgress();

        if (successPanelPrefab == null)
        {
            consoleText.text = "УРОВЕНЬ ПРОЙДЕН! Возврат на карту через 3 секунды...";
            Invoke("GoToMap", 3f);
            return;
        }

        // Создаём панель успеха
        currentSuccessPanel = Instantiate(successPanelPrefab);
        currentSuccessPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // НАХОДИМ ВСЕ КНОПКИ В ПРЕФАБЕ
        Button[] successButtons = currentSuccessPanel.GetComponentsInChildren<Button>(true);

        if (successButtons.Length >= 3)
        {
            // Кнопка 1: Следующий уровень
            Button nextLevelBtn = successButtons[0];
            Text nextText = nextLevelBtn.GetComponentInChildren<Text>();
            if (nextText != null) nextText.text = "Уровень 2";
            nextLevelBtn.onClick.AddListener(LoadNextLevel);

            // Кнопка 2: На карту
            Button mapBtn = successButtons[1];
            Text mapText = mapBtn.GetComponentInChildren<Text>();
            if (mapText != null) mapText.text = "На карту";
            mapBtn.onClick.AddListener(GoToMap);

            // Кнопка 3: Повторить
            Button retryBtn = successButtons[2];
            Text retryText = retryBtn.GetComponentInChildren<Text>();
            if (retryText != null) retryText.text = "Повторить";
            retryBtn.onClick.AddListener(CloseSuccess);
        }
        else if (successButtons.Length >= 1)
        {
            // Если только одна кнопка - делаем её "На карту"
            successButtons[0].onClick.AddListener(GoToMap);
        }

        // Блокируем игровой процесс
        runButton.interactable = false;
        codeInput.interactable = false;
    }

    void SaveProgress()
    {
        // Отмечаем уровень 1 как пройденный
        PlayerPrefs.SetInt("Level1_Passed", 1);

        // Открываем уровень 2
        PlayerPrefs.SetInt("Level2_Status", 1); // 1 = открыт

        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level1 пройден, Level2 открыт");
    }

    void LoadNextLevel()
    {
        Debug.Log("Загружаем следующий уровень...");

        // Загружаем Level2
        SceneManager.LoadScene("Level2");
    }

    void CloseSuccess()
    {
        if (currentSuccessPanel != null)
        {
            Destroy(currentSuccessPanel);
        }
        ResetLevel();
    }

    void GoToMap()
    {
        SceneManager.LoadScene("MapScene");
    }
}