using System;

namespace TestingLibrary
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

    [AttributeUsage(AttributeTargets.Method)]
    public class TimeoutAttribute : Attribute
    {
        public int Milliseconds { get; }
        public TimeoutAttribute(int ms) => Milliseconds = ms;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestCaseSource : Attribute
    {
        public string MethodName { get; }
        public TestCaseSource(string methodName) => MethodName = methodName;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class Category : Attribute
    {
        public string Name { get; }
        public Category(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Priority : Attribute
    {
        public int Level { get; }
        public Priority(int level) => Level = level;
    }
}