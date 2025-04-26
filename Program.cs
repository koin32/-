using System.Text;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

interface CompressorDecompressorInter{
    public string Compressor(string inputData);
    public string Decompressor(string inputData);
}
class CompressorDecompressor : CompressorDecompressorInter{
    private bool LowercaseLatin(string input, ref string error)

    {
        if (string.IsNullOrEmpty(input))
        {
            error = "Строка не может быть пустой или null.";
            return false;
        }
    
        foreach (char c in input)
        {
            if (c < 'a' || c > 'z')
            {
                error = $"Строка должна содержать только строчные латинские буквы. Найден недопустимый символ: '{c}'";
                return false;
            }
        }
    
        return true;
    }
    private void ReadData(string inputData){
        try
        {
            string errorMessag = "start status errorMessag";
            bool checkMessag = LowercaseLatin(inputData, ref errorMessag);

            switch (checkMessag)
            {
                case false:
                    Console.WriteLine(errorMessag);
                    return;
            }
            
            return;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public string Compressor(string inputData)
    {
    ReadData(inputData);

    if (string.IsNullOrEmpty(inputData))
        return string.Empty;

    int iCompress = 1;  
    char lastChar = inputData[0];
    string outCompressStr = "";

    // Начинаем со второго символа
    for (int i = 1; i < inputData.Length; i++)
    {
        if (inputData[i] == lastChar)
        {
            iCompress++;
        }
        else
        {
            outCompressStr += lastChar;
            if (iCompress > 1)
            {
                outCompressStr += iCompress.ToString();
            }
            lastChar = inputData[i];
            iCompress = 1;  
        }
    }

    
    outCompressStr += lastChar;
    if (iCompress > 1)
    {
        outCompressStr += iCompress.ToString();
    }

    return outCompressStr;
}
    public string Decompressor(string inputData)
    {
    if (string.IsNullOrEmpty(inputData))
        return string.Empty;

    StringBuilder result = new StringBuilder();
    int i = 0;
    
    while (i < inputData.Length)
    {
        char currentChar = inputData[i++];
        
        StringBuilder numberStr = new StringBuilder();
        while (i < inputData.Length && char.IsDigit(inputData[i]))
        {
            numberStr.Append(inputData[i++]);
        }
        
        int count = numberStr.Length > 0 ? int.Parse(numberStr.ToString()) : 1;
        
        result.Append(currentChar, count);
    }
    
    return result.ToString();
}



}
//////////////////////////////////////////////////////////////////////////////
class Server
{
    // Юрал информацию на https://www.cyberforum.ru/blogs/2408863/10119.html
    private static int _count;
    private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    
    public static int Count
    {
        get
        {
            _rwLock.EnterReadLock();
            try{return _count;}
            finally{_rwLock.ExitReadLock();}
        }
    }
    
    public static void AddToCount(int value)
    {
        _rwLock.EnterWriteLock();
        try{_count += value;}
        finally{_rwLock.ExitWriteLock();}
    }
    
    public static int GetCount()
    {
        _rwLock.EnterWriteLock();
        try{int result = _count; _count = 0; return result;}
        finally{_rwLock.ExitWriteLock();}
    }
}
//////////////////////////////////////////////////////////////////////////////
public class LogProcessor
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private readonly string _problemsPath;
    public LogProcessor(string inputPath, string outputPath, string problemsPath = "problems.txt")
    {
        _inputPath = inputPath;
        _outputPath = outputPath;
        _problemsPath = problemsPath;
    }
    public void Process()
    {
        var validEntries = new List<string>();
        var problemEntries = new List<string>();
        foreach (string line in File.ReadLines(_inputPath))
        {
            if (LogParser.TryParse(line, out var parsedEntry))
            {
                validEntries.Add(LogFormatter.Format(parsedEntry));
            }
            else
            {
                problemEntries.Add(line);
            }
        }
        File.WriteAllLines(_outputPath, validEntries);
        if (problemEntries.Count > 0)
        {
            File.WriteAllLines(_problemsPath, problemEntries);
        }
    }
}
public static class LogParser
{
    private static readonly Regex Format1Regex = new Regex(
        @"^(\d{2})\.(\d{2})\.(\d{4})\s(\d{2}:\d{2}:\d{2}\.\d+)\s+(INFO|INFORMATION|WARN|WARNING|ERROR|DEBUG)\s+(.*)$",
        RegexOptions.Compiled);
    private static readonly Regex Format2Regex = new Regex(
        @"^(\d{4})-(\d{2})-(\d{2})\s(\d{2}:\d{2}:\d{2}\.\d+)\|\s*(INFO|WARN|ERROR|DEBUG)\|\d+\|([^|]+)\|(.*)$",
        RegexOptions.Compiled);
    public static bool TryParse(string line, out LogEntry entry)
    {
        entry = null;
        if (string.IsNullOrWhiteSpace(line))
            return false;
        // Попробуем первый формат
        var match = Format1Regex.Match(line);
        if (match.Success)
        {
            entry = new LogEntry
            {
                Date = $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}",
                Time = match.Groups[4].Value,
                Level = NormalizeLogLevel(match.Groups[5].Value),
                Method = "DEFAULT",
                Message = match.Groups[6].Value.Trim()
            };
            return true;
        }
        // Попробуем второй формат
        match = Format2Regex.Match(line);
        if (match.Success)
        {
            entry = new LogEntry
            {
                Date = $"{match.Groups[3].Value}-{match.Groups[2].Value}-{match.Groups[1].Value}",
                Time = match.Groups[4].Value,
                Level = NormalizeLogLevel(match.Groups[5].Value),
                Method = match.Groups[6].Value.Trim(),
                Message = match.Groups[7].Value.Trim()
            };
            return true;
        }
        return false;
    }
    private static string NormalizeLogLevel(string level)
    {
        return level.ToUpper() switch
        {
            "INFORMATION" => "INFO",
            "WARNING" => "WARN",
            _ => level.ToUpper()
        };
    }
}
public static class LogFormatter
{
    public static string Format(LogEntry entry)
    {
        return $"{entry.Date}\t{entry.Time}\t{entry.Level}\t{entry.Method}\t{entry.Message}";
    }
}
public class LogEntry
{
    public string Date { get; set; }
    public string Time { get; set; }
    public string Level { get; set; }
    public string Method { get; set; }
    public string Message { get; set; }
}

//////////////////////////////////////////////////////////////////////////////
class Program
{

    static void Task1(){
        CompressorDecompressor tasks1 = new CompressorDecompressor();
        Console.Write("Введите набор строчных латинских букв: ");
        string testStr = Console.ReadLine();
        testStr = tasks1.Compressor(testStr);
        Console.WriteLine("Сжатый текст: " + testStr);
        testStr = tasks1.Decompressor(testStr);
        Console.WriteLine("Расжатый текст: " + testStr);

        Console.ReadKey();
    }

    static void ReaderWorker(string name)
    {
        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            int value = Server.Count;
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{name}] Прочитано: {value}");
            
            Thread.Sleep(random.Next(50, 150)); // Имитация работы
        }
    }
    
    static void WriterWorker(string name)
    {
        var random = new Random();
        for (int i = 0; i < 3; i++)
        {
            int addValue = random.Next(1, 10);
            
            Server.AddToCount(addValue);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{name}] Добавлено: {addValue}");
            
            Thread.Sleep(random.Next(300, 600)); // Имитация работы
        }
    }

    static void Task2()
    {
        var readers = new Task[5];
        var writers = new Task[2];
        
        for (int i = 0; i < readers.Length; i++)
        {
            readers[i] = Task.Run(() => ReaderWorker($"Reader-{i + 1}"));
        }
        
        for (int i = 0; i < writers.Length; i++)
        {
            writers[i] = Task.Run(() => WriterWorker($"Writer-{i + 1}"));
        }
        
        Task.WaitAll(readers);
        Task.WaitAll(writers);
    }
    
    static void Task3(){
        Console.Write("Входной файл: ");
        string inputFile = Console.ReadLine();
        Console.Write("Выходной файл: ");
        string outputFile = Console.ReadLine();
        try
        {
            var processor = new LogProcessor(inputFile, outputFile);
            processor.Process();
            Console.WriteLine("Обработка завершена успешно!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("\\/\\/\\/\\/\\/\\/\\/\\/ Задача 1 \\/\\/\\/\\/\\/\\/\\/\\/");
        Task1();
        Console.WriteLine("\\/\\/\\/\\/\\/\\/\\/\\/ Задача 2 \\/\\/\\/\\/\\/\\/\\/\\/");
        Task2();
        Console.WriteLine("\\/\\/\\/\\/\\/\\/\\/\\/ Задача 3 \\/\\/\\/\\/\\/\\/\\/\\/");
        Task3();

    }
}