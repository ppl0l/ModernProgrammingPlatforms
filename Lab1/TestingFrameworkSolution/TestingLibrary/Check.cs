using System;
using System.Collections.Generic;
using System.Linq;

namespace TestingLibrary
{
    public static class Check
    {
        public static void Eq(object a, object b)
        {
            if (a is double da && b is double db)
            {
                if (Math.Abs(da - db) > 0.0001)
                    throw new Exception($"{da} != {db}");
            }
            else if (!a.Equals(b))
                throw new Exception($"{a} != {b}");
        }

        public static void True(bool x)
        {
            if (!x) throw new Exception("не True");
        }

        public static void False(bool x)
        {
            if (x) throw new Exception("не False");
        }

        public static void Null(object x)
        {
            if (x != null) throw new Exception("не Null");
        }

        public static void NotNull(object x)
        {
            if (x == null) throw new Exception("Null");
        }

        public static void Contains<T>(T item, IEnumerable<T> list)
        {
            if (!list.Contains(item))
                throw new Exception("нет в списке");
        }
    }
}