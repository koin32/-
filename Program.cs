using System;
using System.ComponentModel.DataAnnotations;
using System.Text;


class Tasks1{
    string commonStr = "";
    public Tasks1(){
        ReadData(Console.ReadLine());
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
            
            Console.WriteLine(Decompressor(Compressor(inputData))); //////
            Console.WriteLine(Compressor(inputData)); //////
            return;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    private string Compressor(string inputData)
{
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
    
    private string Decompressor(string compressedData)
    {
    if (string.IsNullOrEmpty(compressedData))
        return string.Empty;

    StringBuilder result = new StringBuilder();
    int i = 0;
    
    while (i < compressedData.Length)
    {
        char currentChar = compressedData[i++];
        
        StringBuilder numberStr = new StringBuilder();
        while (i < compressedData.Length && char.IsDigit(compressedData[i]))
        {
            numberStr.Append(compressedData[i++]);
        }
        
        int count = numberStr.Length > 0 ? int.Parse(numberStr.ToString()) : 1;
        
        result.Append(currentChar, count);
    }
    
    return result.ToString();
}

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


}

class Program
{
    static void Main(string[] args)
    {
        Tasks1 tasks1 = new Tasks1();
        Console.ReadKey();
    }
}
