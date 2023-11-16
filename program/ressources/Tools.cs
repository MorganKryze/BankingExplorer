using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleAppVisuals;

namespace program.ressources;

public class Tools
{
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }
        if (recursive)
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
    }
    public static (int,string) WritePromptDefaultValue(string message, string? defaultValue, int? line = null, bool continuous = true)
    {
        line ??= Core.ContentHeigth;

        if (continuous)
        {
            Core.WriteContinuousString(message, line, negative: false, 1500, 50);
        }
        else
        {
            Core.WritePositionnedString(message, Placement.Center, negative: false, line, writeLine: true);
        }

        var field = new StringBuilder(defaultValue);
        ConsoleKeyInfo key;
        Console.CursorVisible = true;
        do
        {
            Core.ClearLine(line + 2);
            Console.SetCursorPosition(0, Console.CursorTop );
            Console.Write("{0," + (Console.WindowWidth / 2 - message.Length / 2 + 2) + "}", "> ");
            Console.Write($"{field}");
            key = Console.ReadKey();
            if (key.Key == ConsoleKey.Backspace && field.Length > 0)
                field.Remove(field.Length - 1, 1);
            else if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
                field.Append(key.KeyChar);
        } while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape);
        Console.CursorVisible = false;
        return key.Key == ConsoleKey.Enter ? (0,field.ToString()) : (-1,field.ToString());
    }

    public static (int,int) ScrollingTableSelector(string headers, int? line = null, bool negative = false, params string[] lines)
    {
        int valueOrDefault = line.GetValueOrDefault();
        if (!line.HasValue)
        {
            valueOrDefault = Core.ContentHeigth;
            line = valueOrDefault;
        }

        int num = 0;
        int totalWidth = (lines.Length != 0) ? lines.Max((string s) => s.Length) : 0;
        for (int i = 0; i < lines.Length; i++)
            lines[i] = lines[i].PadRight(totalWidth);

        Core.WriteContinuousString(headers, line, negative, 1500, 50, headers.Length);
        int num2 = line.Value + 1;
        while (true)
        {
            string[] array = new string[lines.Length];
            for (int j = 0; j < lines.Length ; j++)
            {
                array[j] = lines[j];
                Core.WritePositionnedString(j == num && j == lines.Length - 1 ? array[j].InsertString("┤ Ajouter une ligne ├", Placement.Center, true) : array[j], Placement.Center, negative: j == num, num2 + j);
            }

            switch (Console.ReadKey(intercept: true).Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.Z:
                    if (num == 0)
                    {
                        num = lines.Length - 1;
                    }
                    else if (num > 0)
                    {
                        num--;
                    }

                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    if (num == lines.Length - 1)
                    {
                        num = 0;
                    }
                    else if (num < lines.Length - 1)
                    {
                        num++;
                    }

                    break;
                case ConsoleKey.Enter:
                    Core.ClearMultipleLines(line, lines.Length + 1);
                    return (0,num);
                case ConsoleKey.Backspace:
                    Core.ClearMultipleLines(line, lines.Length + 1);
                    return (-2, num);
                case ConsoleKey.Escape:
                    Core.ClearMultipleLines(line, lines.Length + 1);
                    return (-1, -1);
            }
        }
    }
    public static int ScrollingMenuSelectorDefaultvalue(string question, int? defaultIndex = null, int? line = null, params string[] choices)
    {
        int valueOrDefault = line.GetValueOrDefault();
        if (!line.HasValue)
        {
            valueOrDefault = Core.ContentHeigth;
            line = valueOrDefault;
        }
        

        int num = defaultIndex ??= 0;
        int totalWidth = (choices.Length != 0) ? choices.Max((string s) => s.Length) : 0;
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i] = choices[i].PadRight(totalWidth);
        }

        Core.WriteContinuousString(question, line, negative: false, 1500, 50);
        int lineChoice = line.Value + 2;
        while (true)
        {
            string[] array = new string[choices.Length];
            for (int j = 0; j < choices.Length; j++)
            {
                if (j == num)
                {
                    array[j] = " ▶ " + choices[j] + "  ";
                    Core.WritePositionnedString(array[j], Placement.Center, negative: true, lineChoice + j);
                }
                else
                {
                    array[j] = "   " + choices[j] + "  ";
                    Core.WritePositionnedString(array[j], Placement.Center, negative: false, lineChoice + j);
                }
            }

            switch (Console.ReadKey(intercept: true).Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.Z:
                    if (num == 0)
                    {
                        num = choices.Length - 1;
                    }
                    else if (num > 0)
                    {
                        num--;
                    }

                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    if (num == choices.Length - 1)
                    {
                        num = 0;
                    }
                    else if (num < choices.Length - 1)
                    {
                        num++;
                    }

                    break;
                case ConsoleKey.Enter:
                    Core.ClearMultipleLines(line, choices.Length + 2);
                    return num;
                case ConsoleKey.Escape:
                    Core.ClearMultipleLines(line, choices.Length + 2);
                    return -1;
            }
        }
    }
}
