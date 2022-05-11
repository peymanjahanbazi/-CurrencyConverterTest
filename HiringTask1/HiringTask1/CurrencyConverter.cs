using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiringTask1;

public class CurrencyConverter
{
    private bool _configured = false;
    private readonly Dictionary<string, double> _ratio = new();
    private readonly Dictionary<string, int> _currencyIndex = new();
    private readonly object _lck = new();

    public CurrencyConverter()
    {
    }

    public void ClearConfiguration()
    {
        lock (_lck)
        {
            _configured = false;
        }
    }

    public double Convert(string fromCurrency, string toCurrency, double amount)
    {
        if (!_configured)
        {
            throw new Exception("Not Configured");
        }
        if (!_currencyIndex.ContainsKey(fromCurrency))
        {
            throw new Exception($"Invalid currency {fromCurrency}");
        }
        if (!_currencyIndex.ContainsKey(toCurrency))
        {
            throw new Exception($"Invalid currency {toCurrency}");
        }
        double ratio = _ratio[$"{_currencyIndex[fromCurrency]}-{_currencyIndex[toCurrency]}"];
        if (ratio < 0)
        {
            throw new Exception($"{fromCurrency} is not convartable to {toCurrency}");
        }
        return ratio * amount;
    }

    public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
    {
        lock (_lck)
        {
            _configured = false;
            _currencyIndex.Clear();
            _ratio.Clear();
            int idx = 0;
            foreach (var conversionRate in conversionRates)
            {
                if (!_currencyIndex.ContainsKey(conversionRate.Item1))
                {
                    _currencyIndex.Add(conversionRate.Item1, idx++);
                }
                if (!_currencyIndex.ContainsKey(conversionRate.Item2))
                {
                    _currencyIndex.Add(conversionRate.Item2, idx++);
                }
            }
            int currencyCount = _currencyIndex.Count;
            int[,] graph = new int[currencyCount, currencyCount];
            double[,] currencyRatio = new double[currencyCount, currencyCount];
            for (int i = 0; i < currencyCount; i++)
            {
                for (int j = 0; j < currencyCount; j++)
                {
                    graph[i, j] = currencyCount * 2;
                    currencyRatio[i, j] = -1.0;
                }
            }
            foreach (var conversionRate in conversionRates)
            {
                graph[_currencyIndex[conversionRate.Item1], _currencyIndex[conversionRate.Item2]] = 1;
                graph[_currencyIndex[conversionRate.Item2], _currencyIndex[conversionRate.Item1]] = 1;
                currencyRatio[_currencyIndex[conversionRate.Item1], _currencyIndex[conversionRate.Item2]] = conversionRate.Item3;
                currencyRatio[_currencyIndex[conversionRate.Item2], _currencyIndex[conversionRate.Item1]] = 1.0 / conversionRate.Item3;
            }
            CalculateRatio(graph, currencyRatio, currencyCount);
            _configured = true;
        }
    }

    private void CalculateRatio(int[,] graph, double[,] currencyRatio, int verticesCount)
    {
        int[,] distance = new int[verticesCount, verticesCount];
        int[,] next = new int[verticesCount, verticesCount];

        for (int i = 0; i < verticesCount; ++i)
        {
            for (int j = 0; j < verticesCount; ++j)
            {
                distance[i, j] = graph[i, j];
                if (graph[i, j] == 1)
                {
                    next[i, j] = j;
                }
                else
                {
                    next[i, j] = -1;
                }
            }
        }

        for (int k = 0; k < verticesCount; k++)
        {
            for (int i = 0; i < verticesCount; i++)
            {
                for (int j = 0; j < verticesCount; j++)
                {
                    if (distance[i, k] + distance[k, j] < distance[i, j])
                    {
                        distance[i, j] = distance[i, k] + distance[k, j];
                        next[i, j] = k;
                    }
                }
            }
        }

        for (int i = 0; i < verticesCount; i++)
        {
            for (int j = i + 1; j < verticesCount; j++)
            {
                if (distance[i, j] >= verticesCount)
                {
                    _ratio[$"{i}-{j}"] = -1.0;
                    continue;
                }
                double cost = ConstructPath(i, j, next, currencyRatio);
                _ratio[$"{i}-{j}"] = cost;
                _ratio[$"{j}-{i}"] = 1.0 / cost;
            }
        }
    }

    private double ConstructPath(int s, int d, int[,] next, double[,] currencyRatio)
    {
        double cost = 1.0;
        while (s != d)
        {
            int n = next[s, d];
            cost *= currencyRatio[s, n];
            s = n;
        }
        return cost;
    }
}