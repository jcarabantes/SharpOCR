using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

class SharpOCR
{
    static void Main(string[] args)
    {
        ShowBanner();
        string filePath = "";
        string regexPattern = "";
        string language = "eng";
        bool foundMatches = false; // Flag to track matches

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-f" && i + 1 < args.Length)
                filePath = args[i + 1];
            else if (args[i] == "-p" && i + 1 < args.Length)
                regexPattern = args[i + 1];
            else if (args[i] == "-l" && i + 1 < args.Length)
                language = args[i + 1];
        }

        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(regexPattern))
        {
            Console.WriteLine("Usage: SharpOCR.exe -f <file_containing_images_path> -p <pattern> [-l <language>]");
            Console.WriteLine("Example: SharpOCR.exe -f images.txt -p 'passw|token' -l eng");
            Console.WriteLine("Note: eng is set by default");
            return;
        }

        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadLines(filePath))
            {
                string imagePath = line.Trim('"'); // Trim potential quotes
                if (File.Exists(imagePath))
                {
                    if (ProcessFile(imagePath, regexPattern, language))
                        foundMatches = true;
                }
                else
                {
                    Console.WriteLine($"File not found: {imagePath}");
                }
            }
        }
        else
        {
            Console.WriteLine($"Error, file not found: {filePath}");
        }
        if (!foundMatches)
        {
            Console.WriteLine("No matches found in the whole list.");
        }
    }

    private static void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write("[SUCCESS] ");
        Console.ResetColor();
        Console.Write(message);
    }

    private static void ShowBanner()
    {
        Console.WriteLine();
        Console.WriteLine(@" .d8888b.  888                                 .d88888b.   .d8888b.  8888888b.  ");
        Console.WriteLine(@"d88P  Y88b 888                                d88P' 'Y88b d88P  Y88b 888   Y88b ");
        Console.WriteLine(@"Y88b.      888                                888     888 888    888 888    888 ");
        Console.WriteLine(@" 'Y888b.   88888b.   8888b.  888d888 88888b.  888     888 888        888   d88P ");
        Console.WriteLine(@"    'Y88b. 888 '88b     '88b 888P'   888 '88b 888     888 888        8888888P'  ");
        Console.WriteLine(@"      '888 888  888 .d888888 888     888  888 888     888 888    888 888 T88b   ");
        Console.WriteLine(@"Y88b  d88P 888  888 888  888 888     888 d88P Y88b. .d88P Y88b  d88P 888  T88b  ");
        Console.WriteLine(@" 'Y8888P'  888  888 'Y888888 888     88888P'   'Y88888P'   'Y8888P'  888   T88b ");
        Console.WriteLine(@"                                     888                                        ");
        Console.WriteLine(@"                                     888                                        ");
        Console.WriteLine(@"                                     888                                        ");
        Console.WriteLine("\nVersion 1.0.0 - Javier Carabantes\n\n");
    }

    static bool ProcessFile(string filePath, string regexPattern, string language)
    {
        string extractedText = filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? ExtractTextFromPDF(filePath)
            : ExtractTextFromImage(filePath, language);

        MatchCollection matches = Regex.Matches(extractedText, regexPattern, RegexOptions.IgnoreCase);
        
        if (matches.Count > 0)
        {
            Success($"Matches in {filePath}: ");
            foreach (Match match in matches)
            {
                Console.Write($"{match.Value} ");
            }
            Console.WriteLine();
            return true;
        }
        return false;
    }

    static string ExtractTextFromPDF(string pdfPath)
    {
        using (PdfDocument pdf = PdfDocument.Open(pdfPath))
        {
            return string.Join("\n", pdf.GetPages().Select(p => ContentOrderTextExtractor.GetText(p)));
        }
    }

    static string ExtractTextFromImage(string imagePath, string language)
    {
        string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        using (var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
        {
            engine.SetVariable("debug_file", "NUL"); // Suppress debug output
            using (var img = Pix.LoadFromFile(imagePath))
            {
                return engine.Process(img).GetText();
            }
        }
    }
}
