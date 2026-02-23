using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SimplePythonInterpreter
{
    private Dictionary<string, object> variables = new Dictionary<string, object>();
    private List<string> output = new List<string>();
    private Stack<bool> conditionStack = new Stack<bool>();
    private bool inIfBlock = false;
    private bool executeCurrentBlock = true;
    private int ifLineIndex = -1;

    public string Execute(string code)
    {
        output.Clear();
        variables.Clear();
        conditionStack.Clear();
        inIfBlock = false;
        executeCurrentBlock = true;

        string[] lines = code.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string originalLine = lines[i];
            string line = originalLine.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            try
            {
                // Пропускаем выполнение если мы внутри блока, который не должен выполняться
                if (inIfBlock && !executeCurrentBlock && !line.StartsWith("else:"))
                {
                    // Проверяем конец блока (отступ)
                    if (i + 1 < lines.Length && (lines[i + 1].StartsWith(" ") || lines[i + 1].StartsWith("\t")))
                        continue;
                    else
                    {
                        inIfBlock = false;
                        executeCurrentBlock = true;
                    }
                }

                // Присваивание
                if (line.Contains("=") && !line.Contains("==") && !line.Contains("!=") && !line.Contains("<=") && !line.Contains(">="))
                {
                    HandleAssignment(line);
                }
                // print
                else if (line.StartsWith("print"))
                {
                    HandlePrint(line);
                }
                // input
                else if (line.StartsWith("input"))
                {
                    HandleInput(line);
                }
                // if
                else if (line.StartsWith("if"))
                {
                    ifLineIndex = i;
                    HandleIf(line);
                }
                // elif
                else if (line.StartsWith("elif"))
                {
                    HandleElif(line);
                }
                // else
                else if (line.StartsWith("else:"))
                {
                    HandleElse();
                }
                // for
                else if (line.StartsWith("for"))
                {
                    HandleFor(line, lines, ref i);
                }
                // while
                else if (line.StartsWith("while"))
                {
                    HandleWhile(line, lines, ref i);
                }
                // встроенные функции
                else if (line.StartsWith("int(") || line.StartsWith("str(") || line.StartsWith("float("))
                {
                    HandleConversion(line);
                }
                else
                {
                    // Возможно это выражение (например, математическое)
                    try
                    {
                        object result = EvaluateExpression(line);
                        if (result != null)
                            output.Add(result.ToString());
                    }
                    catch
                    {
                        output.Add($"Ошибка: неизвестная команда '{line}'");
                    }
                }
            }
            catch (Exception e)
            {
                output.Add($"Ошибка в строке {i + 1}: {e.Message}");
            }
        }

        return string.Join("\n", output);
    }

    // Присваивание
    void HandleAssignment(string line)
    {
        string[] parts = line.Split('=');
        if (parts.Length != 2)
            throw new Exception("Неверный формат присваивания");

        string varName = parts[0].Trim();
        string valueStr = parts[1].Trim();

        // Вычисляем значение (может быть выражение)
        object value = EvaluateExpression(valueStr);
        variables[varName] = value;
    }

    // Вычисление выражений
    object EvaluateExpression(string expr)
    {
        expr = expr.Trim();

        // Строка в кавычках
        if ((expr.StartsWith("\"") && expr.EndsWith("\"")) ||
            (expr.StartsWith("'") && expr.EndsWith("'")))
        {
            return expr.Substring(1, expr.Length - 2);
        }

        // Переменная
        if (variables.ContainsKey(expr))
            return variables[expr];

        // Математическое выражение (упрощенно)
        try
        {
            // Заменяем переменные на значения
            string calcExpr = expr;
            foreach (var var in variables)
            {
                if (var.Value is int || var.Value is float)
                    calcExpr = calcExpr.Replace(var.Key, var.Value.ToString());
            }

            // Вычисляем (упрощенно, только для базовых операций)
            var dt = new System.Data.DataTable();
            var result = dt.Compute(calcExpr, "");
            return Convert.ToDouble(result);
        }
        catch
        {
            // Число с плавающей точкой
            if (expr.Contains("."))
            {
                if (float.TryParse(expr, out float floatVal))
                    return floatVal;
            }

            // Целое число
            if (int.TryParse(expr, out int intVal))
                return intVal;
        }

        throw new Exception($"Не удалось распознать значение: {expr}");
    }

    // print
    void HandlePrint(string line)
    {
        string content = line.Replace("print(", "").TrimEnd(')').Trim();

        if (string.IsNullOrEmpty(content))
        {
            output.Add("");
            return;
        }

        string[] args = content.Split(',');
        string printLine = "";

        foreach (string arg in args)
        {
            string trimmed = arg.Trim();
            object value = EvaluateExpression(trimmed);
            printLine += value?.ToString() + " ";
        }

        output.Add(printLine.Trim());
    }

    // input
    void HandleInput(string line)
    {
        string prompt = "";

        if (line.Contains("(") && line.Contains(")"))
        {
            prompt = line.Replace("input(", "").TrimEnd(')').Trim();

            if ((prompt.StartsWith("\"") && prompt.EndsWith("\"")) ||
                (prompt.StartsWith("'") && prompt.EndsWith("'")))
            {
                prompt = prompt.Substring(1, prompt.Length - 2);
                output.Add(prompt);
            }
        }

        // Пока просто возвращаем заглушку
        string testInput = "0";
        output.Add($">>> {testInput}");

        // Сохраняем в переменную _ если нужно
        variables["_"] = testInput;
    }

    // if
    void HandleIf(string line)
    {
        string condition = line.Replace("if", "").Replace(":", "").Trim();
        bool result = EvaluateCondition(condition);

        conditionStack.Push(result);
        inIfBlock = true;
        executeCurrentBlock = result;
    }

    // elif
    void HandleElif(string line)
    {
        if (conditionStack.Count == 0)
            throw new Exception("elif без if");

        // Если предыдущее условие было true, этот блок не выполняется
        if (conditionStack.Peek())
        {
            executeCurrentBlock = false;
        }
        else
        {
            string condition = line.Replace("elif", "").Replace(":", "").Trim();
            bool result = EvaluateCondition(condition);
            conditionStack.Pop();
            conditionStack.Push(result);
            executeCurrentBlock = result;
        }
    }

    // else
    void HandleElse()
    {
        if (conditionStack.Count == 0)
            throw new Exception("else без if");

        // else выполняется если все предыдущие условия false
        executeCurrentBlock = !conditionStack.Peek();
    }

    // Вычисление условий
    bool EvaluateCondition(string condition)
    {
        condition = condition.Trim();

        // Заменяем переменные
        foreach (var var in variables)
        {
            condition = condition.Replace(var.Key, var.Value.ToString());
        }

        // Сравнения
        if (condition.Contains("=="))
        {
            string[] parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
            object left = EvaluateExpression(parts[0].Trim());
            object right = EvaluateExpression(parts[1].Trim());
            return left.ToString() == right.ToString();
        }
        else if (condition.Contains("!="))
        {
            string[] parts = condition.Split(new[] { "!=" }, StringSplitOptions.None);
            object left = EvaluateExpression(parts[0].Trim());
            object right = EvaluateExpression(parts[1].Trim());
            return left.ToString() != right.ToString();
        }
        else if (condition.Contains(">"))
        {
            string[] parts = condition.Split('>');
            double left = Convert.ToDouble(EvaluateExpression(parts[0].Trim()));
            double right = Convert.ToDouble(EvaluateExpression(parts[1].Trim()));
            return left > right;
        }
        else if (condition.Contains("<"))
        {
            string[] parts = condition.Split('<');
            double left = Convert.ToDouble(EvaluateExpression(parts[0].Trim()));
            double right = Convert.ToDouble(EvaluateExpression(parts[1].Trim()));
            return left < right;
        }
        else if (condition.Contains(">="))
        {
            string[] parts = condition.Split(new[] { ">=" }, StringSplitOptions.None);
            double left = Convert.ToDouble(EvaluateExpression(parts[0].Trim()));
            double right = Convert.ToDouble(EvaluateExpression(parts[1].Trim()));
            return left >= right;
        }
        else if (condition.Contains("<="))
        {
            string[] parts = condition.Split(new[] { "<=" }, StringSplitOptions.None);
            double left = Convert.ToDouble(EvaluateExpression(parts[0].Trim()));
            double right = Convert.ToDouble(EvaluateExpression(parts[1].Trim()));
            return left <= right;
        }

        // Просто значение
        return Convert.ToBoolean(EvaluateExpression(condition));
    }

    // for
    void HandleFor(string line, string[] lines, ref int lineIndex)
    {
        // for i in range(5):
        string forLine = line.Replace("for", "").Replace(":", "").Trim();
        string[] parts = forLine.Split(new[] { "in" }, StringSplitOptions.None);

        if (parts.Length != 2)
            throw new Exception("Неверный формат for");

        string varName = parts[0].Trim();
        string rangeExpr = parts[1].Trim();

        // range(5) или range(1,5) или range(1,5,2)
        int start = 0, stop = 0, step = 1;

        if (rangeExpr.StartsWith("range(") && rangeExpr.EndsWith(")"))
        {
            string rangeParams = rangeExpr.Replace("range(", "").Replace(")", "");
            string[] rangeParts = rangeParams.Split(',');

            if (rangeParts.Length == 1)
            {
                stop = Convert.ToInt32(EvaluateExpression(rangeParts[0].Trim()));
            }
            else if (rangeParts.Length == 2)
            {
                start = Convert.ToInt32(EvaluateExpression(rangeParts[0].Trim()));
                stop = Convert.ToInt32(EvaluateExpression(rangeParts[1].Trim()));
            }
            else if (rangeParts.Length == 3)
            {
                start = Convert.ToInt32(EvaluateExpression(rangeParts[0].Trim()));
                stop = Convert.ToInt32(EvaluateExpression(rangeParts[1].Trim()));
                step = Convert.ToInt32(EvaluateExpression(rangeParts[2].Trim()));
            }
        }

        // Собираем тело цикла
        List<string> loopLines = new List<string>();
        int j = lineIndex + 1;
        while (j < lines.Length && (lines[j].StartsWith(" ") || lines[j].StartsWith("\t")))
        {
            loopLines.Add(lines[j].TrimStart());
            j++;
        }

        // Выполняем цикл
        for (int i = start; i < stop; i += step)
        {
            variables[varName] = i;

            foreach (string loopLine in loopLines)
            {
                if (string.IsNullOrEmpty(loopLine)) continue;

                if (loopLine.StartsWith("print"))
                    HandlePrint(loopLine);
                else if (loopLine.Contains("="))
                    HandleAssignment(loopLine);
            }
        }

        // Перемещаем указатель строк
        lineIndex = j - 1;
    }

    // while
    void HandleWhile(string line, string[] lines, ref int lineIndex)
    {
        string condition = line.Replace("while", "").Replace(":", "").Trim();

        // Собираем тело цикла
        List<string> loopLines = new List<string>();
        int j = lineIndex + 1;
        while (j < lines.Length && (lines[j].StartsWith(" ") || lines[j].StartsWith("\t")))
        {
            loopLines.Add(lines[j].TrimStart());
            j++;
        }

        // Выполняем цикл
        int maxIterations = 1000; // защита от бесконечного цикла
        int iter = 0;

        while (EvaluateCondition(condition) && iter < maxIterations)
        {
            foreach (string loopLine in loopLines)
            {
                if (string.IsNullOrEmpty(loopLine)) continue;

                if (loopLine.StartsWith("print"))
                    HandlePrint(loopLine);
                else if (loopLine.Contains("="))
                    HandleAssignment(loopLine);
            }
            iter++;
        }

        if (iter >= maxIterations)
            output.Add("Предупреждение: цикл остановлен после 1000 итераций");

        lineIndex = j - 1;
    }

    // int(), str(), float()
    void HandleConversion(string line)
    {
        if (line.StartsWith("int("))
        {
            string content = line.Replace("int(", "").TrimEnd(')').Trim();
            object val = EvaluateExpression(content);
            int result = Convert.ToInt32(val);
            variables["_"] = result;
            output.Add(result.ToString());
        }
        else if (line.StartsWith("str("))
        {
            string content = line.Replace("str(", "").TrimEnd(')').Trim();
            object val = EvaluateExpression(content);
            string result = val.ToString();
            variables["_"] = result;
            output.Add(result);
        }
        else if (line.StartsWith("float("))
        {
            string content = line.Replace("float(", "").TrimEnd(')').Trim();
            object val = EvaluateExpression(content);
            float result = Convert.ToSingle(val);
            variables["_"] = result;
            output.Add(result.ToString());
        }
    }

    // Проверка переменных после выполнения
    public bool HasVariable(string name)
    {
        return variables.ContainsKey(name);
    }

    public object GetVariable(string name)
    {
        return variables.ContainsKey(name) ? variables[name] : null;
    }

    public string GetOutput()
    {
        return string.Join("\n", output);
    }
}