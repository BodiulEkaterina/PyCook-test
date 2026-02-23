using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level10Manager : MonoBehaviour
{
    // ОСНОВНЫЕ ОБЪЕКТЫ
    public RectTransform pyBot;
    public RectTransform table;
    public RectTransform tableFront;
    public RectTransform fridge;
    public RectTransform fridgeFront;
    public RectTransform startPoint;

    // ПРЕДМЕТЫ
    public Image[] tomatoes;     // 3 помидора
    public Image[] apples;       // 3 яблока
    public Image[] bananas;      // 3 банана
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

    // ПРАВИЛЬНЫЙ КОД ДЛЯ УРОВНЯ (ТОЛЬКО ДЛЯ ПРОВЕРКИ, НЕ ПОКАЗЫВАЕМ)
    private string[] expectedCommands = new string[]
    {
        "for i in range(3):",
        "    take(\"tomato\")",
        "    move_to(\"fridge\")",
        "for i in range(3):",
        "    take(\"apple\")",
        "    move_to(\"fridge\")",
        "for i in range(3):",
        "    take(\"banana\")",
        "    move_to(\"fridge\")"
    };

    // ПЕРЕМЕННЫЕ
    private bool isExecuting = false;
    private bool hasItem = false;
    private string currentItem = "";

    // Счетчики собранных фруктов
    private int tomatoesCollected = 0;
    private int applesCollected = 0;
    private int bananasCollected = 0;
    private int tomatoesOnTable = 3;
    private int applesOnTable = 3;
    private int bananasOnTable = 3;

    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;
    private int currentLevel = 10;

    void Start()
    {
        Debug.Log("Level 10 Manager запущен - ФИНАЛЬНЫЙ УРОВЕНЬ РАЗДЕЛА!");

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
            consoleText.text = "ИНСТРУКЦИЯ: Собери все фрукты!";
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
                text.text = "Поздравляю! Ты дошел до финального уровня раздела!\n\nНа столе 9 фруктов: 3 помидора, 3 яблока и 3 банана.\n\nНужно собрать их все и отнести в холодильник.\n\nПодсказок больше не будет!";
            }
            else if (textName.Contains("example") || textName.Contains("code"))
            {
                text.text = "ПОДСКАЗОК НЕТ! Пиши код сам.";
            }
        }

        // УБИРАЕМ ПРИМЕР КОДА ИЗ ПОЛЯ ВВОДА
        InputField[] allInputFields = currentInstructionPanel.GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (inputField.gameObject.name.ToLower().Contains("example"))
            {
                inputField.text = "Пиши код сам...";
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

        consoleText.text = "Напиши код самостоятельно и нажми ВЫПОЛНИТЬ";
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

        // Проверяем код через интерпретатор (без показа правильного ответа)
        if (!IsCodeCorrect(code))
        {
            consoleText.text = "Ошибка! Код не работает. Попробуй ещё раз.";
            isExecuting = false;
            yield break;
        }

        consoleText.text = "Код правильный! Выполняю программу...";
        yield return new WaitForSeconds(1f);

        // Этап 1: Собираем помидоры (3 штуки)
        consoleText.text = "Этап 1: Собираю помидоры...";
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
            // Идем к столу
            yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

            // Берем помидор
            bool found = false;
            for (int j = 0; j < tomatoes.Length; j++)
            {
                if (tomatoes[j] != null && tomatoes[j].gameObject.activeSelf)
                {
                    tomatoes[j].gameObject.SetActive(false);
                    currentItem = "tomato";
                    hasItem = true;
                    tomatoesOnTable--;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                consoleText.text = $"Взял помидор {i + 1} из 3";
                heldItemImage.gameObject.SetActive(true);
                heldItemImage.color = Color.red;
                yield return new WaitForSeconds(0.5f);

                // Несем к холодильнику
                yield return StartCoroutine(MovePyBotTo(fridgeFront.anchoredPosition));

                heldItemImage.gameObject.SetActive(false);
                hasItem = false;
                tomatoesCollected++;

                consoleText.text = $"Помидор {tomatoesCollected} доставлен!";
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Этап 2: Собираем яблоки (3 штуки)
        consoleText.text = "Этап 2: Собираю яблоки...";
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

            bool found = false;
            for (int j = 0; j < apples.Length; j++)
            {
                if (apples[j] != null && apples[j].gameObject.activeSelf)
                {
                    apples[j].gameObject.SetActive(false);
                    currentItem = "apple";
                    hasItem = true;
                    applesOnTable--;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                consoleText.text = $"Взял яблоко {i + 1} из 3";
                heldItemImage.gameObject.SetActive(true);
                heldItemImage.color = Color.green;
                yield return new WaitForSeconds(0.5f);

                yield return StartCoroutine(MovePyBotTo(fridgeFront.anchoredPosition));

                heldItemImage.gameObject.SetActive(false);
                hasItem = false;
                applesCollected++;

                consoleText.text = $"Яблоко {applesCollected} доставлено!";
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Этап 3: Собираем бананы (3 штуки)
        consoleText.text = "Этап 3: Собираю бананы...";
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(MovePyBotTo(tableFront.anchoredPosition));

            bool found = false;
            for (int j = 0; j < bananas.Length; j++)
            {
                if (bananas[j] != null && bananas[j].gameObject.activeSelf)
                {
                    bananas[j].gameObject.SetActive(false);
                    currentItem = "banana";
                    hasItem = true;
                    bananasOnTable--;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                consoleText.text = $"Взял банан {i + 1} из 3";
                heldItemImage.gameObject.SetActive(true);
                heldItemImage.color = Color.yellow;
                yield return new WaitForSeconds(0.5f);

                yield return StartCoroutine(MovePyBotTo(fridgeFront.anchoredPosition));

                heldItemImage.gameObject.SetActive(false);
                hasItem = false;
                bananasCollected++;

                consoleText.text = $"Банан {bananasCollected} доставлено!";
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Проверяем результат
        if (tomatoesCollected == 3 && applesCollected == 3 && bananasCollected == 3)
        {
            consoleText.text = "ПОЗДРАВЛЯЮ! Ты прошел финальный уровень раздела!";
            yield return new WaitForSeconds(1f);
            ShowSuccess();
        }
        else
        {
            consoleText.text = $"Ошибка! Собрано: помидоров {tomatoesCollected}, яблок {applesCollected}, бананов {bananasCollected}";
        }

        isExecuting = false;
    }

    // Проверяет структуру кода (без показа правильного ответа)
    private bool IsCodeCorrect(string code)
    {
        if (string.IsNullOrEmpty(code)) return false;

        string[] lines = code.Trim().Split('\n');

        // Проверяем количество строк (должно быть 9)
        if (lines.Length != 9) return false;

        // Нормализуем строки для проверки
        string[] normalizedLines = new string[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            normalizedLines[i] = lines[i].Trim().ToLower()
                .Replace("'", "\"")
                .Replace(" ", "");
        }

        // Проверяем структуру (без конкретных значений в range)

        // Строка 1: for i in range(3): или for i in range(3)
        if (!normalizedLines[0].StartsWith("foriinrange(") ||
            (!normalizedLines[0].EndsWith("):") && !normalizedLines[0].EndsWith(")")))
            return false;

        // Строка 2: take("tomato")
        if (normalizedLines[1] != "take(\"tomato\")")
            return false;

        // Строка 3: move_to("fridge")
        if (normalizedLines[2] != "move_to(\"fridge\")")
            return false;

        // Строка 4: for i in range(3):
        if (!normalizedLines[3].StartsWith("foriinrange("))
            return false;

        // Строка 5: take("apple")
        if (normalizedLines[4] != "take(\"apple\")")
            return false;

        // Строка 6: move_to("fridge")
        if (normalizedLines[5] != "move_to(\"fridge\")")
            return false;

        // Строка 7: for i in range(3):
        if (!normalizedLines[6].StartsWith("foriinrange("))
            return false;

        // Строка 8: take("banana")
        if (normalizedLines[7] != "take(\"banana\")")
            return false;

        // Строка 9: move_to("fridge")
        if (normalizedLines[8] != "move_to(\"fridge\")")
            return false;

        return true;
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
        tomatoesOnTable = 3;
        applesOnTable = 3;
        bananasOnTable = 3;

        pyBot.anchoredPosition = startPoint.anchoredPosition;

        if (codeInput != null)
        {
            codeInput.text = "";
        }

        // Возвращаем все предметы на стол
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

        consoleText.text = "Сброшено. Напиши код самостоятельно";

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
        Debug.Log("=== УРОВЕНЬ 10 ПРОЙДЕН ===");

        SaveProgress();

        if (successPanelPrefab == null)
        {
            consoleText.text = "УРОВЕНЬ ПРОЙДЕН! Возврат на карту через 3 секунды...";
            Invoke("GoToMap", 3f);
            return;
        }

        currentSuccessPanel = Instantiate(successPanelPrefab);
        currentSuccessPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // НАХОДИМ ВСЕ КНОПКИ В ПРЕФАБЕ
        Button[] successButtons = currentSuccessPanel.GetComponentsInChildren<Button>(true);

        if (successButtons.Length >= 3)
        {
            // Кнопка 1: Следующий уровень
            Button nextLevelBtn = successButtons[0];
            Text nextText = nextLevelBtn.GetComponentInChildren<Text>();
            if (nextText != null) nextText.text = "Уровень 11";
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
            successButtons[0].onClick.AddListener(GoToMap);
        }

        runButton.interactable = false;
        codeInput.interactable = false;
    }

    void SaveProgress()
    {
        // Отмечаем уровень 10 как пройденный
        PlayerPrefs.SetInt("Level10_Passed", 1);

        // Открываем уровень 11
        PlayerPrefs.SetInt("Level11_Status", 1); // 1 = открыт

        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level10 пройден, Level11 открыт");
    }

    void LoadNextLevel()
    {
        Debug.Log("Загружаем следующий уровень...");

        // Загружаем Level11
        SceneManager.LoadScene("Level11");
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