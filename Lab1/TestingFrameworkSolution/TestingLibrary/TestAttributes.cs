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
}