using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // только UI, без UIElements

public class Level11Controller : MonoBehaviour
{
    // UI ЭЛЕМЕНТЫ
    public InputField CodeInput;        // поле ввода кода
    public Text TaskText;               // текст задания и вывода
    public Button RunButton;            // кнопка выполнения
    public Button ResetButton;          // кнопка сброса
    public Button BackButton;           // кнопка назад
    public Button CommandsButton;       // кнопка команд
    public Text LevelTitle;             // заголовок уровня

    // РОБОТ (только Image для цвета)
    public Image PyBotImage;            // изображение робота

    // ПРЕФАБЫ
    public GameObject instructionPanelPrefab;
    public GameObject successPanelPrefab;

    private bool isExecuting = false;
    private GameObject currentInstructionPanel;
    private GameObject currentSuccessPanel;

    private SimplePythonInterpreter python;

    void Start()
    {
        // Назначаем кнопки
        RunButton.onClick.AddListener(OnRunClick);
        ResetButton.onClick.AddListener(OnResetClick);
        BackButton.onClick.AddListener(GoToMap);
        CommandsButton.onClick.AddListener(ShowInstruction);

        // Настраиваем поле ввода для многострочности
        CodeInput.lineType = InputField.LineType.MultiLineNewline;
        CodeInput.text = "";

        // Заголовок и задание
        LevelTitle.text = "УРОВЕНЬ 11: ПЕРЕМЕННЫЕ";
        TaskText.text = "Создай переменную name с твоим именем и переменную age с твоим возрастом. Выведи их на экран.";

        // Создаем интерпретатор
        python = new SimplePythonInterpreter();

        // Синий цвет в начале
        SetRobotIdle();
    }

    void ShowInstruction()
    {
        if (instructionPanelPrefab == null) return;

        currentInstructionPanel = Instantiate(instructionPanelPrefab);
        currentInstructionPanel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        Button closeButton = currentInstructionPanel.GetComponentInChildren<Button>();
        if (closeButton != null)
            closeButton.onClick.AddListener(() => Destroy(currentInstructionPanel));
    }

    void OnRunClick()
    {
        if (isExecuting) return;
        StartCoroutine(RunCode());
    }

    IEnumerator RunCode()
    {
        isExecuting = true;

        // Желтый во время выполнения
        PyBotImage.color = Color.yellow;

        yield return new WaitForSeconds(0.5f);

        string code = CodeInput.text;

        // Выполняем код
        string result = python.Execute(code);

        // Показываем результат
        TaskText.text = "Результат:\n" + result + "\n\n";

        // Проверяем успешность
        CheckSuccess();

        isExecuting = false;
    }

    void CheckSuccess()
    {
        string output = python.GetOutput();
        Debug.Log("Output: " + output);  // посмотрим что выводит интерпретатор

        bool hasName = python.HasVariable("name");
        bool hasAge = python.HasVariable("age");
        bool hasPrint = !string.IsNullOrEmpty(output);

        Debug.Log($"hasName: {hasName}, hasAge: {hasAge}, hasPrint: {hasPrint}");

        object name = null;
        object age = null;
        bool validName = false;
        bool validAge = false;

        if (hasName)
        {
            name = python.GetVariable("name");
            validName = name != null && name.ToString() != "";
            Debug.Log($"name value: {name}, valid: {validName}");
        }

        if (hasAge)
        {
            age = python.GetVariable("age");
            validAge = age != null && age.ToString() != "";
            Debug.Log($"age value: {age}, valid: {validAge}");
        }

        if (hasName && hasAge && hasPrint && validName && validAge)
        {
            TaskText.text += $"\nВывод:\n{output}";
            TaskText.text += $"\n\nname = {name}, age = {age}";
            TaskText.text += "\nУровень пройден!";

            PyBotImage.color = Color.green;
            ShowSuccess();
        }
        else
        {
            TaskText.text += "\nОшибка: ";
            if (!hasName) TaskText.text += "нет переменной name ";
            else if (!validName) TaskText.text += "name пустая ";

            if (!hasAge) TaskText.text += "нет переменной age ";
            else if (!validAge) TaskText.text += "age пустая ";

            if (!hasPrint) TaskText.text += "нет вывода (print)";

            PyBotImage.color = Color.red;
        }
    }

    void SetRobotIdle()
    {
        PyBotImage.color = Color.blue;       // синий - ожидание
    }

    void OnResetClick()
    {
        StopAllCoroutines();
        isExecuting = false;

        CodeInput.text = "";
        TaskText.text = "Создай переменную name с твоим именем и переменную age с твоим возрастом. Выведи их на экран.";

        python = new SimplePythonInterpreter();
        PyBotImage.color = Color.blue;

        if (currentInstructionPanel != null)
            Destroy(currentInstructionPanel);

        if (currentSuccessPanel != null)
            Destroy(currentSuccessPanel);
    }

    void ShowSuccess()
    {
        Debug.Log("=== УРОВЕНЬ 11 ПРОЙДЕН ===");

        SaveProgress();

        if (successPanelPrefab == null)
        {
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
            if (nextText != null) nextText.text = "Уровень 12";
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
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("Level11_Passed", 1);
        PlayerPrefs.SetInt("Level12_Status", 1);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен: Level11 пройден, Level12 открыт");
    }

    void CloseSuccess()
    {
        if (currentSuccessPanel != null)
        {
            Destroy(currentSuccessPanel);
        }
        OnResetClick();
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene("Level12");
    }

    void GoToMap()
    {
        SceneManager.LoadScene("MapScene");
    }
}