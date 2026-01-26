using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level4Manager : MonoBehaviour
{
    // ОСНОВНЫЕ ОБЪЕКТЫ
    public RectTransform pyBot;
    public RectTransform table;
    public RectTransform tableFront;
    public RectTransform startPoint;

    // ПРЕДМЕТЫ (МАССИВ ИЗ 3 ПОМИДОРОВ)
    public Image[] tomatoes; // Привяжи все 3 помидора в Inspector
    public Image heldItemImage;

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
    private string correctCode = "for i in range(3):\n    take(\"tomato\")";

    // ПЕРЕМЕННЫЕ
    private bool isExecuting = false;
    private int tomatoesTaken = 0;
    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;
    private int currentLevel = 4;

    void Start()
    {
        Debug.Log("Level 4 Manager запущен");

        // Назначаем обработчики кнопок
        runButton.onClick.AddListener(OnRunClick);
        resetButton.onClick.AddListener(OnResetClick);
        backToMapButton.onClick.AddListener(GoToMap);

        // Скрываем инвентарь
        if (heldItemImage != null)
        {
            heldItemImage.gameObject.SetActive(false);
        }

        // Проверяем что есть помидоры
        if (tomatoes == null || tomatoes.Length == 0)
        {
            Debug.LogError("Не назначены помидоры для уровня 4!");
        }
        else
        {
            Debug.Log($"Загружено {tomatoes.Length} помидоров");
        }

        // Если нет точки перед столом, используем сам стол
        if (tableFront == null)
        {
            tableFront = table;
        }

        // Показываем инструкцию
        ShowInstruction();
    }

    void ShowInstruction()
    {
        if (instructionPanelPrefab == null)
        {
            consoleText.text = "ИНСТРУКЦИЯ: Используй цикл for чтобы взять 3 помидора";
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
                text.text = "УРОВЕНЬ 4: ЦИКЛ FOR";
            }
            else if (textName.Contains("main") || textName.Contains("instruction"))
            {
                text.text = "Используй цикл for, чтобы повторять команды.\n\nrange(3) означает повторить 3 раза.\n\nВажно: команды внутри цикла должны быть с отступом! Пиши точно как в примере.";
            }
            else if (textName.Contains("example") || textName.Contains("code"))
            {
                text.text = "Пиши в поле справа:\nfor i in range(3):\n    take(\"tomato\")";
            }
        }

        // ЗАПОЛНЯЕМ ПОЛЯ ВВОДА
        InputField[] allInputFields = currentInstructionPanel.GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (inputField.gameObject.name.ToLower().Contains("example"))
            {
                inputField.text = "for i in range(3):\n    take(\"tomato\")";
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

        consoleText.text = "Введи код и нажми ВЫПОЛНИТЬ";
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

        // ТОЧНАЯ ПРОВЕРКА КОДА ДЛЯ УРОВНЯ 4
        if (code == correctCode)
        {
            consoleText.text = "Код правильный! Выполняю цикл...";
            yield return new WaitForSeconds(1f);

            // 1. Идём к столу
            consoleText.text = "Иду к столу...";
            yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

            // 2. Берём 3 помидора (цикл)
            for (int i = 0; i < 3; i++)
            {
                if (i < tomatoes.Length && tomatoes[i] != null)
                {
                    consoleText.text = $"Беру помидор {i + 1} из 3...";
                    yield return new WaitForSeconds(0.7f);

                    // Прячем помидор
                    tomatoes[i].gameObject.SetActive(false);
                    tomatoesTaken++;

                    // Меняем цвет инвентаря
                    if (heldItemImage != null)
                    {
                        heldItemImage.gameObject.SetActive(true);
                        float colorValue = 0.7f + (i * 0.1f);
                        heldItemImage.color = new Color(1f, colorValue, colorValue);
                    }
                }
            }

            // 3. Успех
            consoleText.text = "Успех! Цикл выполнен!";
            yield return new WaitForSeconds(0.5f);
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
        tomatoesTaken = 0;

        // Возвращаем PyBot на стартовую позицию
        pyBot.anchoredPosition = startPoint.anchoredPosition;

        // ОЧИЩАЕМ ПОЛЕ ВВОДА
        if (codeInput != null)
        {
            codeInput.text = "";
        }

        // Возвращаем все помидоры на стол
        if (tomatoes != null)
        {
            foreach (Image tomato in tomatoes)
            {
                if (tomato != null)
                {
                    tomato.gameObject.SetActive(true);
                }
            }
        }

        // Очищаем инвентарь
        if (heldItemImage != null)
        {
            heldItemImage.gameObject.SetActive(false);
        }

        // Очищаем сообщения в консоли
        consoleText.text = "Сброшено. Введи код и нажми ВЫПОЛНИТЬ";

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
        Debug.Log("=== УРОВЕНЬ 4 ПРОЙДЕН ===");

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
            if (nextText != null) nextText.text = "Уровень 5";
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
        // Отмечаем уровень 4 как пройденный
        PlayerPrefs.SetInt("Level4_Passed", 1);

        // Открываем уровень 5
        PlayerPrefs.SetInt("Level5_Status", 1); // 1 = открыт

        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level4 пройден, Level5 открыт");
    }

    void LoadNextLevel()
    {
        Debug.Log("Загружаем следующий уровень...");

        // Загружаем Level5
        SceneManager.LoadScene("Level5");
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