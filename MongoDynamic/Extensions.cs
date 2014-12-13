using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
   internal static  class Extensions
    {
      

        public static string GetMethodHelper<T>(this Expression<Action<T>> expression)
        {
            Contract.Requires(expression != null);
            return expression.ExtractMethod();
        }

        public static string[] GetMethodNames<T>(params Expression<Action<T>>[] expressions)
        {
            return expressions == null
                       ? Enumerable.Empty<string>().ToArray()
                       : expressions.Select(expression => expression.ExtractMethod()).ToArray();
        }

        

        private static string ExtractMethod<T>(this Expression<Action<T>> expression)
        {
            var methodCall = expression.Body as MethodCallExpression;
            if (methodCall == null)
            {
                throw new ArgumentException("expression");
            }
            var method = methodCall.Method;
            return method.Name;
        }

        /// <summary>
        /// Helper Method
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetMemberName(this LambdaExpression expression)
        {
            MemberExpression memberExpression;
            if (expression.Body is UnaryExpression)
            {
                var unary = (UnaryExpression)expression.Body;
                memberExpression = (MemberExpression)unary.Operand;
            }
            else memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }
    }
}
