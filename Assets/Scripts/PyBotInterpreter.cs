using System;
using System.Text.RegularExpressions;

public static class PyBotInterpreter
{
    public static string ValidateBasicCode(string userCode, string[] expectedCommands)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return "Ошибка! Введи команду!";
        }

        string[] userLines = userCode.Trim().Split('\n');

        if (userLines.Length != expectedCommands.Length)
        {
            if (expectedCommands.Length == 1)
                return $"Ошибка! Нужна 1 команда, а у тебя {userLines.Length}";
            else
                return $"Ошибка! Нужно {expectedCommands.Length} команд, а у тебя {userLines.Length}";
        }

        for (int i = 0; i < userLines.Length; i++)
        {
            string userCmd = NormalizeCommandForComparison(userLines[i], i, expectedCommands);
            string expectedCmd = NormalizeCommandForComparison(expectedCommands[i], i, expectedCommands);

            if (expectedCmd.Contains("for") && expectedCmd.Contains("range"))
            {
                if (!IsForLoopCorrect(userCmd))
                {
                    if (i == 0)
                        return "Ошибка! Проверь строку с for: for i in range(3):";
                    else
                        return "Ошибка! Команда внутри цикла должна быть с отступом (4 пробела или tab)";
                }
            }
            else if (expectedCmd.Contains("if") && expectedCmd.Contains("has"))
            {
                if (!IsIfStatementCorrect(userCmd))
                {
                    return $"Ошибка! Проверь строку {i + 1}: {expectedCommands[i]}";
                }
            }
            else if (expectedCmd.Contains("elif"))
            {
                if (!IsElifStatementCorrect(userCmd))
                {
                    return $"Ошибка! Проверь строку {i + 1}: {expectedCommands[i]}";
                }
            }
            else if (expectedCmd.Contains("else"))
            {
                if (!IsElseStatementCorrect(userCmd))
                {
                    return $"Ошибка! Проверь строку {i + 1}: должно быть 'else:'";
                }
            }
            else if (expectedCmd.StartsWith("    ") || expectedCmd.Contains("    "))
            {
                if (!userCmd.StartsWith("    ") && !userCmd.StartsWith("\t"))
                {
                    return $"Ошибка! Команда должна быть с отступом (4 пробела или tab)";
                }

                string userCmdTrimmed = userCmd.Trim();
                string expectedCmdTrimmed = expectedCmd.Trim();

                if (userCmdTrimmed != expectedCmdTrimmed)
                {
                    string expectedShow = expectedCommands[i].Trim();
                    expectedShow = expectedShow.Replace("\"", "'");
                    return $"Ошибка! Должно быть: {expectedShow}";
                }
            }
            else if (userCmd != expectedCmd)
            {
                string expectedShow = expectedCommands[i].Replace("\"", "'");
                return $"Ошибка в команде {i + 1}! Должно быть: {expectedShow}";
            }
        }

        return "OK";
    }

    private static string NormalizeCommandForComparison(string cmd, int lineIndex, string[] allCommands)
    {
        if (string.IsNullOrEmpty(cmd)) return "";

        string original = cmd;

        if (cmd.StartsWith("    ") || cmd.StartsWith("\t"))
        {
            return original;
        }

        cmd = cmd.Trim().ToLower();
        cmd = Regex.Replace(cmd, @"\s+", "");
        cmd = cmd.Replace("'", "\"");

        return cmd;
    }

    private static bool IsForLoopCorrect(string userCmd)
    {
        string cmd = userCmd.ToLower();
        bool hasFor = cmd.Contains("for");
        bool hasIn = cmd.Contains("in");
        bool hasRange = cmd.Contains("range(");
        bool hasColon = cmd.Trim().EndsWith(":");

        return hasFor && hasIn && hasRange && hasColon;
    }

    private static bool IsIfStatementCorrect(string userCmd)
    {
        string cmd = userCmd.ToLower();
        bool hasIf = cmd.Contains("if");
        bool hasHas = cmd.Contains("has");
        bool hasColon = cmd.Trim().EndsWith(":");

        return hasIf && hasHas && hasColon;
    }

    private static bool IsElifStatementCorrect(string userCmd)
    {
        string cmd = userCmd.ToLower();
        bool hasElif = cmd.Contains("elif");
        bool hasHas = cmd.Contains("has");
        bool hasColon = cmd.Trim().EndsWith(":");

        return hasElif && hasHas && hasColon;
    }

    private static bool IsElseStatementCorrect(string userCmd)
    {
        string cmd = userCmd.ToLower().Trim();
        return cmd == "else:" || cmd == "else :";
    }
}