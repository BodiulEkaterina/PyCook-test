using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level9Manager : MonoBehaviour
{
    // ОСНОВНЫЕ ОБЪЕКТЫ
    public RectTransform pyBot;
    public RectTransform table;
    public RectTransform tableFront;
    public RectTransform fridge;
    public RectTransform fridgeFront;
    public RectTransform startPoint;

    // ПРЕДМЕТЫ (разные фрукты)
    public Image[] tomatoes;     // 2 помидора
    public Image[] apples;       // 2 яблока
    public Image[] bananas;      // 2 банана
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
        "if has(\"tomato\"):",
        "    move_to(\"fridge\")",
        "elif has(\"apple\"):",
        "    move_to(\"fridge\")",
        "elif has(\"banana\"):",
        "    move_to(\"fridge\")",
        "else:",
        "    move_to(\"table\")",
        "    take(\"tomato\")"
    };

    // ПЕРЕМЕННЫЕ
    private bool isExecuting = false;
    private bool hasItem = false;
    private string currentItem = "";

    private int tomatoesCollected = 0;
    private int applesCollected = 0;
    private int bananasCollected = 0;

    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;
    private int currentLevel = 9;

    void Start()
    {
        Debug.Log("Level 9 Manager запущен");

        runButton.onClick.AddListener(OnRunClick);
        resetButton.onClick.AddListener(OnResetClick);
        backToMapButton.onClick.AddListener(GoToMap);
        instructionButton.onClick.AddListener(ShowInstruction);

        if (heldItemImage != null)
        {
            heldItemImage.gameObject.SetActive(false);
        }

        if (tableFront == null) tableFront = table;
        if (fridgeFront == null) fridgeFront = fridge;

        ShowInstruction();
    }

    void ShowInstruction()
    {
        if (instructionPanelPrefab == null)
        {
            consoleText.text = "ИНСТРУКЦИЯ: Используй множественные условия";
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
                text.text = "На столе разные фрукты: помидоры, яблоки и бананы.\n\nНужно собрать по одному фрукту каждого вида и отнести в холодильник.\n\nИспользуй if-elif-else для проверки.\n\nПример будет не полным, опирайся на полученнные знания.";
            }
            else if (textName.Contains("example") || textName.Contains("code"))
            {
                text.text = "Пиши в поле справа:\nif has(\"tomato\"):\n    move_to(\"fridge\")\nelif has(\"apple\"):\n    move_to(\"fridge\")\nelif has(\"banana\"):\n    move_to(\"fridge\")\nelse:\n    move_to(\"table\")\n    take(\"tomato\")";
            }
        }

        InputField[] allInputFields = currentInstructionPanel.GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (inputField.gameObject.name.ToLower().Contains("example"))
            {
                inputField.text = "if has(\"tomato\"):\n    ...(\"fridge\")\nelif has(\"apple\"):\n    ...(\"...\")\nelif has(\"banana\"):\n    ...(\"...\")\nelse:\n    move_to(\"...\")\n    take(\"...\")";
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

            int itemsToCollect = 3;
            int itemsCollected = 0;

            while (itemsCollected < itemsToCollect)
            {
                if (hasItem)
                {
                    consoleText.text = $"У меня {GetRussianName(currentItem)}! Несу к холодильнику...";
                    yield return StartCoroutine(MovePyBotTo(fridgeFront.anchoredPosition));

                    if (currentItem == "tomato") tomatoesCollected++;
                    else if (currentItem == "apple") applesCollected++;
                    else if (currentItem == "banana") bananasCollected++;

                    itemsCollected++;
                    hasItem = false;
                    heldItemImage.gameObject.SetActive(false);
                    currentItem = "";

                    consoleText.text = $"Фрукт доставлен! Собрано: помидоров {tomatoesCollected}, яблок {applesCollected}, бананов {bananasCollected}";
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    consoleText.text = "Иду к столу за фруктом...";
                    yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

                    string fruitToTake = GetNextFruitToCollect();

                    if (fruitToTake == "tomato")
                    {
                        bool found = false;
                        for (int i = 0; i < tomatoes.Length; i++)
                        {
                            if (tomatoes[i] != null && tomatoes[i].gameObject.activeSelf)
                            {
                                tomatoes[i].gameObject.SetActive(false);
                                currentItem = "tomato";
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            consoleText.text = "Взял помидор!";
                            heldItemImage.gameObject.SetActive(true);
                            heldItemImage.color = Color.red;
                            hasItem = true;
                        }
                    }
                    else if (fruitToTake == "apple")
                    {
                        bool found = false;
                        for (int i = 0; i < apples.Length; i++)
                        {
                            if (apples[i] != null && apples[i].gameObject.activeSelf)
                            {
                                apples[i].gameObject.SetActive(false);
                                currentItem = "apple";
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            consoleText.text = "Взял яблоко!";
                            heldItemImage.gameObject.SetActive(true);
                            heldItemImage.color = Color.green;
                            hasItem = true;
                        }
                    }
                    else if (fruitToTake == "banana")
                    {
                        bool found = false;
                        for (int i = 0; i < bananas.Length; i++)
                        {
                            if (bananas[i] != null && bananas[i].gameObject.activeSelf)
                            {
                                bananas[i].gameObject.SetActive(false);
                                currentItem = "banana";
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            consoleText.text = "Взял банан!";
                            heldItemImage.gameObject.SetActive(true);
                            heldItemImage.color = Color.yellow;
                            hasItem = true;
                        }
                    }

                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (tomatoesCollected >= 1 && applesCollected >= 1 && bananasCollected >= 1)
            {
                consoleText.text = "Успех! Собраны все виды фруктов!";
                ShowSuccess();
            }
            else
            {
                consoleText.text = $"Ошибка! Нужно собрать по одному каждого вида. Сейчас: помидоров {tomatoesCollected}, яблок {applesCollected}, бананов {bananasCollected}";
            }
        }
        else
        {
            consoleText.text = validationResult;
        }

        isExecuting = false;
    }

    private string GetRussianName(string item)
    {
        switch (item)
        {
            case "tomato": return "помидор";
            case "apple": return "яблоко";
            case "banana": return "банан";
            default: return "фрукт";
        }
    }

    private string GetNextFruitToCollect()
    {
        if (tomatoesCollected == 0) return "tomato";
        if (applesCollected == 0) return "apple";
        if (bananasCollected == 0) return "banana";
        return "tomato";
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
        hasItem = false;
        currentItem = "";
        tomatoesCollected = 0;
        applesCollected = 0;
        bananasCollected = 0;

        pyBot.anchoredPosition = startPoint.anchoredPosition;

        if (codeInput != null)
        {
            codeInput.text = "";
        }

        if (tomatoes != null)
        {
            foreach (Image item in tomatoes)
                if (item != null) item.gameObject.SetActive(true);
        }
        if (apples != null)
        {
            foreach (Image item in apples)
                if (item != null) item.gameObject.SetActive(true);
        }
        if (bananas != null)
        {
            foreach (Image item in bananas)
                if (item != null) item.gameObject.SetActive(true);
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
        Debug.Log("=== УРОВЕНЬ 9 ПРОЙДЕН ===");

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
            if (nextText != null) nextText.text = "Уровень 10";
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
        PlayerPrefs.SetInt("Level9_Passed", 1);
        PlayerPrefs.SetInt("Level10_Status", 1);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level9 пройден, Level10 открыт");
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene("Level10");
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