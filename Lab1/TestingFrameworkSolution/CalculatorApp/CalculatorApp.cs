using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalculatorApp
{
    public class Calc
    {
        public int Add(int a, int b) => a + b;
        public int Sub(int a, int b) => a - b;
        public int Mul(int a, int b) => a * b;
        public double Div(int a, int b) => b == 0 ? throw new Exception("На 0 делить нельзя") : (double)a / b;
        public async Task<int> AddAsync(int a, int b) { await Task.Delay(10); return a + b; }
    }

    public class Data
    {
        List<int> nums = new List<int>();
        public void Add(int x) => nums.Add(x);
        public void Clear() => nums.Clear();
        public double Avg() => nums.Count == 0 ? throw new Exception("Нет данных") : nums.Average();
        public int Max() => nums.Max();
        public int Min() => nums.Min();
        public bool HasData => nums.Any();
        public int Count => nums.Count;
    }

    public class User
    {
        Dictionary<int, string> db = new Dictionary<int, string> { { 1, "Саша" }, { 2, "Петр" }, { 3, "Мария" } };
        public async Task<string> GetName(int id) { await Task.Delay(5); return db.ContainsKey(id) ? db[id] : throw new Exception("Нет такого"); }
        public bool Exists(int id) => db.ContainsKey(id);
    }
}