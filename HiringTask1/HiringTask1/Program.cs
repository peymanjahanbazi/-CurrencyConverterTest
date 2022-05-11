using HiringTask1;

CurrencyConverter cc = new();
List<Tuple<string, string, double>> config = new()
{
    ("USD", "CAD", 1.34).ToTuple(),
    ("CAD", "GBP", 0.58).ToTuple(),
    ("USD", "EUR", 0.86).ToTuple()
};
cc.UpdateConfiguration(config);

Console.WriteLine(cc.Convert("USD", "CAD", 10));
Console.WriteLine(cc.Convert("USD", "GBP", 10));
Console.WriteLine(cc.Convert("EUR", "CAD", 10));