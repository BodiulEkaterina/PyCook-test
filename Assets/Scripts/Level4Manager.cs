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
        "for i in range(3):",
        "    take(\"tomato\")"
    };

    // ПЕРЕМЕННЫЕ
    private bool isExecuting = false;
    private int tomatoesTaken = 0;
    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;
    private int currentLevel = 4;

    void Start()
    {
        Debug.Log("Level 4 Manager запущен");

        runButton.onClick.AddListener(OnRunClick);
        resetButton.onClick.AddListener(OnResetClick);
        backToMapButton.onClick.AddListener(GoToMap);
        instructionButton.onClick.AddListener(ShowInstruction);

        if (heldItemImage != null)
        {
            heldItemImage.gameObject.SetActive(false);
        }

        if (tomatoes == null || tomatoes.Length == 0)
        {
            Debug.LogError("Не назначены помидоры для уровня 4!");
        }

        if (tableFront == null)
        {
            tableFront = table;
        }

        ShowInstruction();
    }

    void ShowInstruction()
    {
        if (instructionPanelPrefab == null)
        {
            consoleText.text = "ИНСТРУКЦИЯ: Используй цикл for чтобы взять 3 помидора";
            return;
        }

        currentInstructionPanel = Instantiate(instructionPanelPrefab);
        currentInstructionPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        Button startButton = FindButtonInChildren(currentInstructionPanel, "StartButton");
        if (startButton == null)
        {
            startButton = currentInstructionPanel.GetComponentInChildren<Button>();
        }

        if (startButton != null)
        {
            Text buttonText = startButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "НАЧАТЬ";
            }

            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => {
                CloseInstruction();
            });
        }

        Text[] allTexts = currentInstructionPanel.GetComponentsInChildren<Text>(true);

        foreach (Text text in allTexts)
        {
            if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
                continue;

            string textName = text.gameObject.name.ToLower();

            if (textName.Contains("main") || textName.Contains("instruction"))
            {
                text.text = "Используй цикл for, чтобы повторять команды.\n\nrange(3) означает повторить 3 раза.\n\nВажно: команды внутри цикла должны быть с отступом (клавиша tab)! Пиши опираясь на пример.";
            }
            else if (textName.Contains("example") || textName.Contains("code"))
            {
                text.text = "Пиши в поле справа:\nfor i in range(3):\n    take(\"tomato\")";
            }
        }

        InputField[] allInputFields = currentInstructionPanel.GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (inputField.gameObject.name.ToLower().Contains("example"))
            {
                inputField.text = "for i in range(3):\n    take(\"tomato\")";
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

        string validationResult = PyBotInterpreter.ValidateBasicCode(code, expectedCommands);

        if (validationResult == "OK")
        {
            consoleText.text = "Код правильный! Выполняю цикл...";
            yield return new WaitForSeconds(1f);

            consoleText.text = "Иду к столу...";
            yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

            for (int i = 0; i < 3; i++)
            {
                if (i < tomatoes.Length && tomatoes[i] != null)
                {
                    consoleText.text = $"Беру помидор {i + 1} из 3...";
                    yield return new WaitForSeconds(0.7f);

                    tomatoes[i].gameObject.SetActive(false);
                    tomatoesTaken++;

                    if (heldItemImage != null)
                    {
                        heldItemImage.gameObject.SetActive(true);
                        float colorValue = 0.7f + (i * 0.1f);
                        heldItemImage.color = new Color(1f, colorValue, colorValue);
                    }
                }
            }

            consoleText.text = "Успех! Цикл выполнен!";
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
        tomatoesTaken = 0;

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
        Debug.Log("=== УРОВЕНЬ 4 ПРОЙДЕН ===");

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
            if (nextText != null) nextText.text = "Уровень 5";
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
        PlayerPrefs.SetInt("Level4_Passed", 1);
        PlayerPrefs.SetInt("Level5_Status", 1);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level4 пройден, Level5 открыт");
    }

    void LoadNextLevel()
    {
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