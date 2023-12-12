using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;


public class HotKey
{
    public ConsoleKey Key { get; set; }
    public string Action { get; set; }
    public string FilePath { get; set; }
}


public static class HotKeySerializer
{
    public static void Serialize(List<HotKey> hotKeys, string filePath)
    {
        string json = JsonConvert.SerializeObject(hotKeys);
        File.WriteAllText(filePath, json);
    }

    public static List<HotKey> Deserialize(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<HotKey>>(json);
        }

        return new List<HotKey>();
    }
}


public static class Menu
{
    public enum KeyPress
    {
        F1 = -1,
        F2 = -2,
        F3 = -3,
        F10 = -4,
        Backspace = -5
    }

    public static KeyPress ShowMenu(List<HotKey> hotKeys)
    {
        Console.Clear();
        Console.WriteLine("Горячие клавиши:");
        foreach (HotKey hotKey in hotKeys)
        {
            Console.WriteLine($"Клавиша: {hotKey.Key}, Действие: {hotKey.Action}, Файл: {hotKey.FilePath}");
        }

        Console.WriteLine();
        Console.WriteLine("Выберите опцию:");
        Console.WriteLine("1 - Создать новую горячую клавишу (F1)");
        Console.WriteLine("2 - Изменить существующую горячую клавишу (F2)");
        Console.WriteLine("3 - Удалить существующую горячую клавишу (F3)");
        Console.WriteLine("4 - Вернуться в меню выполнения горячих клавиш (F10)");
        Console.WriteLine("5 - Выйти (Backspace)");

        ConsoleKeyInfo keyInfo = Console.ReadKey();
        Console.WriteLine();

        if (Enum.TryParse(typeof(KeyPress), keyInfo.Key.ToString(), out object keyPress))
        {
            return (KeyPress)keyPress;
        }

        return 0;
    }
}

public class HotKeyManager
{
    private List<HotKey> hotKeys;
    private string filePath;

    public HotKeyManager(string filePath)
    {
        this.filePath = filePath;
        hotKeys = HotKeySerializer.Deserialize(filePath);
    }

    public void Run()
    {
        bool running = true;

        while (running)
        {
            Menu.KeyPress keyPress = Menu.ShowMenu(hotKeys);

            switch (keyPress)
            {
                case Menu.KeyPress.F1:
                    CreateHotKey();
                    break;
                case Menu.KeyPress.F2:
                    EditHotKey();
                    break;
                case Menu.KeyPress.F3:
                    DeleteHotKey();
                    break;
                case Menu.KeyPress.F10:
                    RunHotKeys();
                    break;
                case Menu.KeyPress.Backspace:
                    running = false;
                    break;
                default:
                    Console.WriteLine("Неправильный выбор. Попробуйте снова.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private void CreateHotKey()
    {
        Console.Clear();
        Console.WriteLine("Введите клавишу (1 - 9):");
        ConsoleKeyInfo keyInfo = Console.ReadKey();
        ConsoleKey key = keyInfo.Key;

        Console.WriteLine();
        Console.WriteLine("Введите действие для горячей клавиши:");
        string action = Console.ReadLine();

        Console.WriteLine("Введите путь до файла, который должен открыться:");
        string filePath = Console.ReadLine();

        hotKeys.Add(new HotKey { Key = key, Action = action, FilePath = filePath });

        HotKeySerializer.Serialize(hotKeys, filePath);

        Console.WriteLine("Горячая клавиша успешно создана!");
        Console.ReadKey();
    }

    private void EditHotKey()
    {
        Console.Clear();
        Console.WriteLine("Введите клавишу (1 - 9) для изменения:");
        ConsoleKeyInfo keyInfo = Console.ReadKey();
        ConsoleKey key = keyInfo.Key;

        HotKey hotKey = hotKeys.Find(hk => hk.Key == key);
        if (hotKey != null)
        {
            Console.WriteLine();
            Console.WriteLine($"Изменение горячей клавиши {hotKey.Key}");
            Console.WriteLine("Введите новое действие для горячей клавиши:");
            string action = Console.ReadLine();

            Console.WriteLine("Введите новый путь до файла, который должен открыться:");
            string filePath = Console.ReadLine();

            hotKey.Action = action;
            hotKey.FilePath = filePath;

            HotKeySerializer.Serialize(hotKeys, filePath);

            Console.WriteLine("Горячая клавиша успешно изменена!");
        }
        else
        {
            Console.WriteLine($"Горячая клавиша {key} не существует!");
        }

        Console.ReadKey();
    }

    private void DeleteHotKey()
    {
        Console.Clear();
        Console.WriteLine("Введите клавишу (1 - 9) для удаления:");
        ConsoleKeyInfo keyInfo = Console.ReadKey();
        ConsoleKey key = keyInfo.Key;

        HotKey hotKey = hotKeys.Find(hk => hk.Key == key);
        if (hotKey != null)
        {
            hotKeys.Remove(hotKey);
            HotKeySerializer.Serialize(hotKeys, filePath);

            Console.WriteLine();
            Console.WriteLine($"Горячая клавиша {hotKey.Key} успешно удалена!");
        }
        else
        {
            Console.WriteLine($"Горячая клавиша {key} не существует!");
        }

        Console.ReadKey();
    }

    private void RunHotKeys()
    {
        Console.Clear();
        Console.WriteLine("Меню выполнения горячих клавиш.");
        Console.WriteLine("Нажмите соответствующую горячую клавишу для запуска:");

        Dictionary<ConsoleKey, HotKey> hotKeyMap = hotKeys.ToDictionary(hk => hk.Key, hk => hk);

        ConsoleKeyInfo keyPress = Console.ReadKey();
        ConsoleKey key = keyPress.Key;

        if (hotKeyMap.ContainsKey(key))
        {
            HotKey hotKey = hotKeyMap[key];


            try
            {
                Process.Start(hotKey.FilePath);
                Console.WriteLine($"Выполняется горячая клавиша: {hotKey.Key}");
                Console.WriteLine($"Действие: {hotKey.Action}, Путь до файла: {hotKey.FilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске процесса: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Горячая клавиша {key} не существует!");
        }

        Console.ReadKey();
    }
}

class Program
{
    static void Main(string[] args)
    {
        string filePath = "hotkeys.json";
        HotKeyManager hotKeyManager = new HotKeyManager(filePath);
        hotKeyManager.Run();
    }
}