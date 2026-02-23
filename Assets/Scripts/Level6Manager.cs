using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level6Manager : MonoBehaviour
{
    // ОСНОВНЫЕ ОБЪЕКТЫ
    public RectTransform pyBot;
    public RectTransform table;
    public RectTransform tableFront;
    public RectTransform fridge;
    public RectTransform fridgeFront;
    public RectTransform startPoint;

    // ПРЕДМЕТЫ
    public Image tomatoOnTable;
    public Image heldItemImage;

    // UI ЭЛЕМЕНТЫ
    public InputField codeInput;
    public Text consoleText;
    public Button runButton;
    public Button resetButton;
    public Button backToMapButton;
    public Button instructionButton;
    public Text taskTitle;

    // ПРЕФАБЫ
    public GameObject instructionPanelPrefab;
    public GameObject successPanelPrefab;

    // НАСТРОЙКИ
    public float moveSpeed = 300f;

    // ПРАВИЛЬНЫЙ КОД ДЛЯ УРОВНЯ
    private string[] expectedCommands = new string[]
    {
        "take(\"tomato\")",
        "if has(\"tomato\"):",
        "    move_to(\"fridge\")"
    };

    // ПЕРЕМЕННЫЕ
    private bool isExecuting = false;
    private bool hasTomato = false;
    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;
    private int currentLevel = 6;

    void Start()
    {
        Debug.Log("Level 6 Manager запущен");

        // Назначаем обработчики кнопок
        runButton.onClick.AddListener(OnRunClick);
        resetButton.onClick.AddListener(OnResetClick);
        backToMapButton.onClick.AddListener(GoToMap);
        instructionButton.onClick.AddListener(ShowInstruction);

        // Скрываем инвентарь
        if (heldItemImage != null)
        {
            heldItemImage.gameObject.SetActive(false);
        }

        // Проверяем ссылки
        if (tableFront == null) tableFront = table;
        if (fridgeFront == null) fridgeFront = fridge;

        // Показываем инструкцию
        ShowInstruction();
    }

    void ShowInstruction()
    {
        if (instructionPanelPrefab == null)
        {
            consoleText.text = "ИНСТРУКЦИЯ: Используй условный оператор if";
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
            if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
                continue;

            string textName = text.gameObject.name.ToLower();

            if (textName.Contains("main") || textName.Contains("instruction"))
            {
                text.text = "Используй условный оператор if для проверки условий.\n\nPyBot проверит, есть ли помидор, и только потом отнесет его в холодильник.\n\nПример будет не полным, опирайся на полученнные знания.";
            }
            else if (textName.Contains("example") || textName.Contains("code"))
            {
                text.text = "Пиши в поле справа:\ntake(\"tomato\")\nif has(\"tomato\"):\n    move_to(\"fridge\")";
            }
        }

        // ЗАПОЛНЯЕМ ПОЛЯ ВВОДА
        InputField[] allInputFields = currentInstructionPanel.GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (inputField.gameObject.name.ToLower().Contains("example"))
            {
                inputField.text = "...(\"tomato\")\nif has(\"tomato\"):\n    ...(\"fridge\")";
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

        string code = codeInput.text;

        string validationResult = PyBotInterpreter.ValidateBasicCode(code, expectedCommands);

        if (validationResult == "OK")
        {
            consoleText.text = "Код правильный! Выполняю программу...";
            yield return new WaitForSeconds(1f);

            // 1. Идём к столу и берём помидор
            consoleText.text = "Иду к столу...";
            yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

            consoleText.text = "Беру помидор...";
            yield return new WaitForSeconds(0.5f);

            if (tomatoOnTable != null)
            {
                tomatoOnTable.gameObject.SetActive(false);
            }

            if (heldItemImage != null)
            {
                heldItemImage.gameObject.SetActive(true);
                heldItemImage.color = Color.red;
            }

            hasTomato = true;

            // 2. Проверяем условие if
            consoleText.text = "Проверяю условие... есть ли помидор?";
            yield return new WaitForSeconds(1f);

            consoleText.text = "Условие истинно! Есть помидор. Несу к холодильнику...";
            yield return new WaitForSeconds(1f);

            // 3. Идём к холодильнику
            yield return StartCoroutine(MovePyBotTo(fridgeFront.anchoredPosition));

            consoleText.text = "Успех! Условный оператор выполнен!";
            yield return new WaitForSeconds(0.5f);
            ShowSuccess();
        }
        else
        {
            consoleText.text = validationResult;
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
        hasTomato = false;

        pyBot.anchoredPosition = startPoint.anchoredPosition;

        if (codeInput != null)
        {
            codeInput.text = "";
        }

        if (tomatoOnTable != null)
        {
            tomatoOnTable.gameObject.SetActive(true);
        }

        if (heldItemImage != null)
        {
            heldItemImage.gameObject.SetActive(false);
        }

        consoleText.text = "Сброшено. Введи код и нажми ВЫПОЛНИТЬ";

        if (currentInstructionPanel != null)
        {
            Destroy(currentInstructionPanel);
        }
        if (currentSuccessPanel != null)
        {
            Destroy(currentSuccessPanel);
        }

        runButton.interactable = true;
        codeInput.interactable = true;
    }

    void ShowSuccess()
    {
        Debug.Log("=== УРОВЕНЬ 6 ПРОЙДЕН ===");

        SaveProgress();

        if (successPanelPrefab == null)
        {
            consoleText.text = "УРОВЕНЬ ПРОЙДЕН! Возврат на карту через 3 секунды...";
            Invoke("GoToMap", 3f);
            return;
        }

        currentSuccessPanel = Instantiate(successPanelPrefab);
        currentSuccessPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        Button[] successButtons = currentSuccessPanel.GetComponentsInChildren<Button>(true);

        if (successButtons.Length >= 3)
        {
            Button nextLevelBtn = successButtons[0];
            Text nextText = nextLevelBtn.GetComponentInChildren<Text>();
            if (nextText != null) nextText.text = "Уровень 7";
            nextLevelBtn.onClick.AddListener(LoadNextLevel);

            Button mapBtn = successButtons[1];
            Text mapText = mapBtn.GetComponentInChildren<Text>();
            if (mapText != null) mapText.text = "На карту";
            mapBtn.onClick.AddListener(GoToMap);

            Button retryBtn = successButtons[2];
            Text retryText = retryBtn.GetComponentInChildren<Text>();
            if (retryText != null) retryText.text = "Повторить";
            retryBtn.onClick.AddListener(CloseSuccess);
        }
        else if (successButtons.Length >= 1)
        {
            successButtons[0].onClick.AddListener(GoToMap);
        }

        runButton.interactable = false;
        codeInput.interactable = false;
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("Level6_Passed", 1);
        PlayerPrefs.SetInt("Level7_Status", 1);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level6 пройден, Level7 открыт");
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene("Level7");
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