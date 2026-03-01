using System;
using System.Threading.Tasks;
using CalculatorApp;

namespace CalculatorTests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Test : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCase : Attribute
    {
        public object[] Data { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Before : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class After : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeClass : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class AfterClass : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class Shared : Attribute { }

    public static class Check
    {
        public static void Equal(object a, object b)
        {
            if (a is double da && b is double db)
            {
                if (Math.Abs(da - db) > 0.0001)
                    throw new Exception($"{da} != {db}");
            }
            else if (!a.Equals(b))
                throw new Exception($"{a} != {b}");
        }

        public static void True(bool x) { if (!x) throw new Exception("не True"); }
        public static void False(bool x) { if (x) throw new Exception("не False"); }
        public static void Null(object x) { if (x != null) throw new Exception("не Null"); }
        public static void NotNull(object x) { if (x == null) throw new Exception("Null"); }

        public static void Contains<T>(T item, System.Collections.Generic.IEnumerable<T> list)
        {
            if (!System.Linq.Enumerable.Contains(list, item))
                throw new Exception("нет в списке");
        }
    }

    [Shared]
    public class CalcTests
    {
        Calc calc;

        [BeforeClass]
        public void BeforeAll() { calc = new Calc(); }

        [Before]
        public void Before() { }

        [After]
        public void After() { }

        [AfterClass]
        public void AfterAll() { }

        [Test]
        public void AddTest() { Check.Equal(5, calc.Add(2, 3)); }

        [Test]
        public void SubTest() { Check.Equal(3, calc.Sub(5, 2)); }

        [Test]
        public void MulTest() { Check.Equal(12, calc.Mul(3, 4)); }

        [TestCase(Data = new object[] { 6, 2, 3 })]
        [TestCase(Data = new object[] { 10, 2, 5 })]
        [TestCase(Data = new object[] { 3, 2, 1.5 })]
        public void DivTest(int a, int b, double exp)
        {
            Check.Equal(exp, calc.Div(a, b));
        }

        [Test]
        public void DivZeroTest()
        {
            try { calc.Div(5, 0); Check.True(false); }
            catch { Check.True(true); }
        }

        [Test]
        public async Task AddAsyncTest()
        {
            Check.Equal(7, await calc.AddAsync(3, 4));
        }
    }
    public class DataTests
    {
        Data data;

        [Before]
        public void Before() { data = new Data(); }

        [Test]
        public void AddTest()
        {
            data.Add(5); data.Add(10); data.Add(-3);
            Check.Equal(3, data.Count);
            Check.True(data.HasData);
        }

        [Test]
        public void AvgTest()
        {
            data.Add(10); data.Add(20); data.Add(30);
            double avg = data.Avg();
            if (Math.Abs(avg - 20) > 0.0001)
                throw new Exception($"{avg} != 20");
        }

        [Test]
        public void AvgEmptyTest()
        {
            try { data.Avg(); Check.True(false); }
            catch { Check.True(true); }
        }

        [Test]
        public void ClearTest()
        {
            data.Add(1); data.Add(2);
            Check.True(data.HasData);
            data.Clear();
            Check.False(data.HasData);
            Check.Equal(0, data.Count);
        }

        [Test]
        public void MaxMinTest()
        {
            data.Add(5); data.Add(15); data.Add(10);
            Check.Equal(15, data.Max());
            Check.Equal(5, data.Min());
        }
    }

    public class UserTests
    {
        User user;

        [Before]
        public void Before() { user = new User(); }

        [Test]
        public async Task GetNameTest()
        {
            Check.Equal("Иван", await user.GetName(1));
            Check.Equal("Петр", await user.GetName(2));
            Check.Equal("Мария", await user.GetName(3));
        }

        [Test]
        public async Task GetNameBadTest()
        {
            try { await user.GetName(999); Check.True(false); }
            catch { Check.True(true); }
        }

        [TestCase(Data = new object[] { 1, true })]
        [TestCase(Data = new object[] { 2, true })]
        [TestCase(Data = new object[] { 3, true })]
        [TestCase(Data = new object[] { 99, false })]
        public void ExistsTest(int id, bool exp)
        {
            Check.Equal(exp, user.Exists(id));
        }
    }
}