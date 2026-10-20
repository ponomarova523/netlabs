using System.Globalization;
using System.Text;


Console.OutputEncoding = Encoding.UTF8;

List<DeliveryMethod> methods = new List<DeliveryMethod>
{
    new StandardDelivery("Нова пошта (склад)", 12.5),
    new StandardDelivery("Укрпошта", 8),
    new ExpressDelivery("Експрес-кур'єр", 20, 1.5, 50)
};

bool isRunning = true;

while (isRunning)
{
    Console.WriteLine("=== СПОСОБИ ДОСТАВКИ ЗАМОВЛЕНЬ ===");
    Console.WriteLine("1. Вихід");
    Console.WriteLine("2. Додати спосіб доставки");
    Console.WriteLine("3. Розрахувати вартість доставки");
    Console.WriteLine("4. Показати всі способи доставки");
    Console.WriteLine("5. Відсортувати від найдешевшого до найдорожчого");
    Console.Write("Ваш вибір: ");

    string? choice = Console.ReadLine();

    try
    {
        switch (choice?.Trim())
        {
            case "1":
                Console.WriteLine("\nДо побачення!");
                isRunning = false;
                break;
            case "2":
                AddMethod();
                break;
            case "3":
                CalculateDelivery();
                break;
            case "4":
                ShowMethods();
                break;
            case "5":
                SortMethods();
                break;
            default:
                Console.WriteLine("\n!!! Невірний вибір. Оберіть число від 1 до 5. !!!\n");
                break;
        }
    }
    catch (InvalidDeliveryDataException ex)
    {
        Console.WriteLine($"\nПомилка даних: {ex.Message}\n");
    }
}

void AddMethod()
{
    Console.WriteLine("\n--- Додавання способу доставки ---");
    Console.WriteLine("Тип: 1 - стандартна, 2 - експрес");
    Console.Write("Оберіть тип: ");
    string? type = Console.ReadLine()?.Trim();

    if (type != "1" && type != "2")
    {
        Console.WriteLine("Невідомий тип доставки.\n");
        return;
    }

    Console.Write("Назва: ");
    string name = Console.ReadLine() ?? "";

    double rate = ReadNumber("Ставка за кг (грн): ");

    DeliveryMethod method;
    if (type == "1")
    {
        method = new StandardDelivery(name, rate);
    }
    else
    {
        double multiplier = ReadNumber("Коефіцієнт швидкості (>= 1): ");
        double surcharge = ReadNumber("Надбавка (грн): ");
        method = new ExpressDelivery(name, rate, multiplier, surcharge);
    }

    methods.Add(method);
    Console.WriteLine($"\nДодано: {method.Describe()}\n");
}

void CalculateDelivery()
{
    Console.WriteLine("\n--- Розрахунок вартості доставки ---");

    if (methods.Count == 0)
    {
        Console.WriteLine("Довідник порожній. Спочатку додайте спосіб доставки.\n");
        return;
    }

    PrintList();
    Console.Write("Номер способу: ");

    if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > methods.Count)
    {
        Console.WriteLine("Невірний номер способу доставки.\n");
        return;
    }

    double weight = ReadNumber("Вага посилки (кг): ");
    DeliveryMethod selected = methods[index - 1];
    double cost = selected.CalculateCost(weight);

    Console.WriteLine($"\nСпосіб: {selected.Name}");
    Console.WriteLine($"Вага: {weight:F2} кг");
    Console.WriteLine($"Вартість доставки: {cost:F2} грн\n");
}

void ShowMethods()
{
    Console.WriteLine("\n--- Способи доставки ---");

    if (methods.Count == 0)
    {
        Console.WriteLine("Довідник порожній.\n");
        return;
    }

    PrintList();
    Console.WriteLine();
}

void SortMethods()
{
    Console.WriteLine("\n--- Сортування за вартістю (контрольна вага 1 кг) ---");

    if (methods.Count == 0)
    {
        Console.WriteLine("Довідник порожній.\n");
        return;
    }

    methods.Sort();

    for (int i = 0; i < methods.Count; i++)
        Console.WriteLine($"{i + 1}. {methods[i].Name} - {methods[i].ReferenceCost:F2} грн");

    Console.WriteLine();
}

void PrintList()
{
    int i = 1;
    foreach (DeliveryMethod method in methods)
    {
        Console.WriteLine($"{i}. {method.Describe()}");
        i++;
    }
}

double ReadNumber(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? text = Console.ReadLine();

        if (text is not null &&
            double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double value) &&
            double.IsFinite(value))
            return value;

        Console.WriteLine("Введіть коректне число.");
    }
}

public class InvalidDeliveryDataException : Exception
{
    public InvalidDeliveryDataException(string message) : base(message)
    {
    }
}

public abstract class DeliveryMethod : IComparable<DeliveryMethod>
{
    private string name = "";
    private double ratePerKg;

    protected DeliveryMethod(string name, double ratePerKg)
    {
        Name = name;
        RatePerKg = ratePerKg;
    }

    public string Name
    {
        get => name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidDeliveryDataException("Назва способу доставки не може бути порожньою.");
            name = value.Trim();
        }
    }

    public double RatePerKg
    {
        get => ratePerKg;
        set
        {
            if (!double.IsFinite(value) || value < 0)
                throw new InvalidDeliveryDataException("Ставка за кілограм не може бути від'ємною.");
            ratePerKg = value;
        }
    }

    public double ReferenceCost => CalculateCost(1);

    public abstract double CalculateCost(double weightKg);

    public virtual string Describe() => $"{Name}, ставка: {RatePerKg:F2} грн/кг";

    protected static void ValidateWeight(double weightKg)
    {
        if (!double.IsFinite(weightKg) || weightKg < 0)
            throw new InvalidDeliveryDataException("Вага посилки не може бути від'ємною.");
    }

    public int CompareTo(DeliveryMethod? other)
    {
        if (other is null)
            return 1;
        return ReferenceCost.CompareTo(other.ReferenceCost);
    }
}

public class StandardDelivery : DeliveryMethod
{
    public StandardDelivery(string name, double ratePerKg) : base(name, ratePerKg)
    {
    }

    public override double CalculateCost(double weightKg)
    {
        ValidateWeight(weightKg);
        return weightKg * RatePerKg;
    }

    public override string Describe() => $"[Стандартна] {base.Describe()}";
}

public class ExpressDelivery : DeliveryMethod
{
    private double speedMultiplier;
    private double surcharge;

    public ExpressDelivery(string name, double ratePerKg, double speedMultiplier, double surcharge)
        : base(name, ratePerKg)
    {
        SpeedMultiplier = speedMultiplier;
        Surcharge = surcharge;
    }

    public double SpeedMultiplier
    {
        get => speedMultiplier;
        set
        {
            if (!double.IsFinite(value) || value < 1)
                throw new InvalidDeliveryDataException("Коефіцієнт швидкості має бути не меншим за 1.");
            speedMultiplier = value;
        }
    }

    public double Surcharge
    {
        get => surcharge;
        set
        {
            if (!double.IsFinite(value) || value < 0)
                throw new InvalidDeliveryDataException("Надбавка не може бути від'ємною.");
            surcharge = value;
        }
    }

    public override double CalculateCost(double weightKg)
    {
        ValidateWeight(weightKg);
        return weightKg * RatePerKg * SpeedMultiplier + Surcharge;
    }

    public override string Describe() =>
        $"[Експрес] {base.Describe()}, коефіцієнт: {SpeedMultiplier:F2}, надбавка: {Surcharge:F2} грн";
}