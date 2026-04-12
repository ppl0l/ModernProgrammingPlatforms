using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        public static void That(Expression<Func<bool>> expr)
        {
            var func = expr.Compile();
            if (!func())
            {
                string detail = ParseExpression(expr.Body);
                throw new Exception($"Провал выражения: {detail}");
            }
        }

        private static string ParseExpression(Expression body)
        {
            if (body is BinaryExpression bin)
            {
                var leftVal = Expression.Lambda(bin.Left).Compile().DynamicInvoke();
                var rightVal = Expression.Lambda(bin.Right).Compile().DynamicInvoke();

                string leftName = GetFriendlyName(bin.Left);
                string rightName = GetFriendlyName(bin.Right);
                string op = GetOperatorSymbol(bin.NodeType);

                return $"{leftName}({leftVal}) {op} {rightName}({rightVal})";
            }
            return body.ToString();
        }

        private static string GetFriendlyName(Expression expr)
        {
            if (expr is MemberExpression mem)
                return mem.Member.Name;
            if (expr is ConstantExpression con)
                return con.Value.ToString();
            return expr.ToString();
        }

        private static string GetOperatorSymbol(ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Equal => "==",
                ExpressionType.NotEqual => "!=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.LessThan => "<",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThanOrEqual => "<=",
                _ => type.ToString()
            };
        }
    }
}