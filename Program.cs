using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

Console.OutputEncoding = Encoding.UTF8;
List<string> history = new List<string>();
bool isRunning = true;

while (isRunning)
{
    Console.WriteLine("=== КАЛЬКУЛЯТОР ВИРАЗІВ ===");
    Console.WriteLine("1. Вихід");
    Console.WriteLine("2. Обчислити вираз");
    Console.WriteLine("3. Показати історію обчислень");
    Console.Write("Ваш вибір: ");

    string? choice = Console.ReadLine();

    switch (choice?.Trim())
    {
        case "1":
            Console.WriteLine("\nДо побачення!");
            isRunning = false;
            break;
        case "2":
            Calculate(history);
            break;
        case "3":
            ShowHistory(history);
            break;
        default:
            Console.WriteLine("\n!!! Невірний вибір. Оберіть 1, 2 або 3. !!!\n");
            break;
    }
}

void Calculate(List<string> history)
{
    Console.WriteLine("\n--- Обчислення виразу ---");
    Console.Write("Введіть вираз у форматі «число оператор число» (напр. 12 + 7): ");
    string? input = Console.ReadLine();

    if (!TryParseExpression(input, out double a, out string op, out double b))
    {
        Console.WriteLine("Помилка: невірний формат. Очікується «число оператор число», напр. 12 + 7.\n");
        return;
    }

    if (!TryCompute(a, op, b, out double result, out string error))
    {
        Console.WriteLine($"Помилка: {error}\n");
        return;
    }

    string record = $"{Format(a)} {op} {Format(b)} = {Format(result)}";
    history.Add(record);
    Console.WriteLine($"Результат: {record}\n");
}

bool TryParseExpression(string? input, out double a, out string op, out double b)
{
    a = b = 0;
    op = "";

    if (string.IsNullOrWhiteSpace(input))
        return false;

    string left, right;
    string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length == 3)
    {
        left = parts[0];
        op = parts[1];
        right = parts[2];
    }
    else
    {
        Match m = Regex.Match(input, @"^\s*(-?[\d.,]+)\s*([^\d\s.,]+)\s*(-?[\d.,]+)\s*$");
        if (!m.Success)
            return false;
        left = m.Groups[1].Value;
        op = m.Groups[2].Value;
        right = m.Groups[3].Value;
    }

    return TryParseNumber(left, out a) && TryParseNumber(right, out b);
}

bool TryParseNumber(string text, out double value) =>
    double.TryParse(text.Replace(',', '.'), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out value)
    && double.IsFinite(value);

bool TryCompute(double a, string op, double b, out double result, out string error)
{
    result = 0;
    error = "";

    if (op == "/" && b == 0)
    {
        error = "ділення на нуль неможливе.";
        return false;
    }

    double? value = op switch
    {
        "+" => a + b,
        "-" => a - b,
        "*" => a * b,
        "/" => a / b,
        _ => null
    };

    if (value is null)
    {
        error = $"оператор «{op}» не підтримується. Доступні: + - * /";
        return false;
    }

    if (!double.IsFinite(value.Value))
    {
        error = "результат виходить за межі допустимого діапазону.";
        return false;
    }

    result = value.Value;
    return true;
}

string Format(double x) => x.ToString("G15", CultureInfo.InvariantCulture);

void ShowHistory(List<string> history)
{
    Console.WriteLine("\n--- Історія обчислень за сеанс ---");

    if (history.Count == 0)
    {
        Console.WriteLine("Історія порожня: жодного обчислення ще не виконано.\n");
        return;
    }

    for (int i = 0; i < history.Count; i++)
        Console.WriteLine($"{i + 1}. {history[i]}");

    Console.WriteLine($"Усього обчислень: {history.Count}\n");
}
