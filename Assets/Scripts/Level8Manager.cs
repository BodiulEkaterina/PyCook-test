using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level8Manager : MonoBehaviour
{
    // ОСНОВНЫЕ ОБЪЕКТЫ
    public RectTransform pyBot;
    public RectTransform table;
    public RectTransform tableFront;
    public RectTransform fridge;
    public RectTransform fridgeFront;
    public RectTransform startPoint;

    // ПРЕДМЕТЫ (5 ПОМИДОРОВ)
    public Image[] tomatoes;
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
        "for i in range(5):",
        "    if has(\"tomato\"):",
        "        move_to(\"fridge\")",
        "    else:",
        "        move_to(\"table\")",
        "        take(\"tomato\")"
    };

    // ПЕРЕМЕННЫЕ
    private bool isExecuting = false;
    private bool hasTomato = false;
    private int tomatoesOnTable = 5;
    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;
    private int currentLevel = 8;

    void Start()
    {
        Debug.Log("Level 8 Manager запущен");

        runButton.onClick.AddListener(OnRunClick);
        resetButton.onClick.AddListener(OnResetClick);
        backToMapButton.onClick.AddListener(GoToMap);
        instructionButton.onClick.AddListener(ShowInstruction);

        if (heldItemImage != null)
        {
            heldItemImage.gameObject.SetActive(false);
        }

        if (tomatoes == null || tomatoes.Length < 5)
        {
            Debug.LogError("Нужно 5 помидоров для уровня 8!");
        }
        else
        {
            tomatoesOnTable = tomatoes.Length;
        }

        if (tableFront == null) tableFront = table;
        if (fridgeFront == null) fridgeFront = fridge;

        ShowInstruction();
    }

    void ShowInstruction()
    {
        if (instructionPanelPrefab == null)
        {
            consoleText.text = "ИНСТРУКЦИЯ: Используй цикл с условием";
            return;
        }

        currentInstructionPanel = Instantiate(instructionPanelPrefab);
        currentInstructionPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        Button startButton = FindButtonInChildren(currentInstructionPanel, "StartButton");
        if (startButton != null)
        {
            Text buttonText = startButton.GetComponentInChildren<Text>();
            if (buttonText != null) buttonText.text = "НАЧАТЬ";

            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => CloseInstruction());
        }

        Text[] allTexts = currentInstructionPanel.GetComponentsInChildren<Text>(true);

        foreach (Text text in allTexts)
        {
            if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
                continue;

            string textName = text.gameObject.name.ToLower();

            if (textName.Contains("main") || textName.Contains("instruction"))
            {
                text.text = "На столе 5 помидоров. Нужно перенести их все в холодильник.\n\nИспользуй цикл for с условием:\nЕсли в руках есть помидор - нести к холодильнику\nЕсли нет помидора - идти к столу и взять\n\nПример будет не полным, опирайся на полученнные знания.";
            }
            else if (textName.Contains("example") || textName.Contains("code"))
            {
                text.text = "Пиши в поле справа:\nfor i in range(5):\n    if has(\"tomato\"):\n        move_to(\"fridge\")\n    else:\n        move_to(\"table\")\n        take(\"tomato\")";
            }
        }

        InputField[] allInputFields = currentInstructionPanel.GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (inputField.gameObject.name.ToLower().Contains("example"))
            {
                inputField.text = "for i in range(5):\n    if has(\"...\"):\n        ...(\"...\")\n    else:\n        move_to(\"table\")\n        ...(\"...\")";
            }
        }

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

        // ИСПОЛЬЗУЕМ ИНТЕРПРЕТАТОР
        string validationResult = PyBotInterpreter.ValidateBasicCode(code, expectedCommands);

        if (validationResult == "OK")
        {
            consoleText.text = "Код правильный! Выполняю программу...";
            yield return new WaitForSeconds(1f);

            // Выполняем 5 итераций цикла
            for (int iteration = 0; iteration < 10; iteration++)
            {
                consoleText.text = $"Итерация {iteration + 1} из 10...";
                yield return new WaitForSeconds(0.5f);

                // Проверяем, есть ли помидор в руках
                if (hasTomato)
                {
                    consoleText.text = "У меня есть помидор! Несу к холодильнику...";
                    yield return StartCoroutine(MovePyBotTo(fridgeFront.anchoredPosition));

                    heldItemImage.gameObject.SetActive(false);
                    hasTomato = false;

                    consoleText.text = "Помидор доставлен!";
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    consoleText.text = "Нет помидора. Иду к столу...";
                    yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

                    // Находим первый активный помидор
                    bool foundTomato = false;
                    for (int i = 0; i < tomatoes.Length; i++)
                    {
                        if (tomatoes[i] != null && tomatoes[i].gameObject.activeSelf)
                        {
                            consoleText.text = "Беру помидор...";
                            yield return new WaitForSeconds(0.5f);

                            tomatoes[i].gameObject.SetActive(false);
                            tomatoesOnTable--;

                            heldItemImage.gameObject.SetActive(true);
                            heldItemImage.color = Color.red;
                            hasTomato = true;
                            foundTomato = true;

                            consoleText.text = "Помидор взят!";
                            yield return new WaitForSeconds(0.5f);
                            break;
                        }
                    }

                    if (!foundTomato)
                    {
                        consoleText.text = "Ошибка! На столе нет помидоров!";
                    }
                }

                yield return new WaitForSeconds(0.3f);
            }

            // Проверяем результат
            if (tomatoesOnTable == 0 && !hasTomato)
            {
                consoleText.text = "Успех! Все помидоры доставлены!";
                ShowSuccess();
            }
            else
            {
                consoleText.text = $"Ошибка! Осталось помидоров: {tomatoesOnTable}";
            }
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
        tomatoesOnTable = 5;

        pyBot.anchoredPosition = startPoint.anchoredPosition;

        if (codeInput != null)
        {
            codeInput.text = "";
        }

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
        Debug.Log("=== УРОВЕНЬ 8 ПРОЙДЕН ===");

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
            if (nextText != null) nextText.text = "Уровень 9";
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
        PlayerPrefs.SetInt("Level8_Passed", 1);
        PlayerPrefs.SetInt("Level9_Status", 1);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level8 пройден, Level9 открыт");
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene("Level9");
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