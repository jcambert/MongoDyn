using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
namespace MongoDyn
{
    public static class QueryBuilder
    {
        public static IMongoQuery BuildQuery(LambdaExpression lambda, string keyName)
        {
            if (lambda.Body is MethodCallExpression)
            {
                return BuildMethodCallExpression((MethodCallExpression)lambda.Body);
            }
            if (lambda.Body is BinaryExpression)
            {
                return BuildBinaryExpression((BinaryExpression)lambda.Body, keyName);
            }
            if (lambda.Body is MemberExpression)
            {

            }
            //dummy
            return null;
            return Query.All("", BsonArray.Create(new object()));
        }

        //TODO: Implementar
        private static IMongoQuery BuildMethodCallExpression(MethodCallExpression expression)
        {
            //throw new NotImplementedException();
            return null;
        }

        private static IMongoQuery BuildBinaryExpression(BinaryExpression binaryExpression, string keyName)
        {
            var op = binaryExpression.NodeType;

            var memberName = string.Empty;

            if (binaryExpression.Left is MemberExpression)
            {
                var left = (MemberExpression)binaryExpression.Left;
                memberName = left.Member.Name;
            }
            //else if (binaryExpression.Left is ParameterExpression)
            //{
            //    var left = (ParameterExpression)binaryExpression.Left;
            //    var type = left.Type;
            //    var 
            //}

            if (memberName == keyName)
                memberName = "_id";

            object value = null;
            if (binaryExpression.Right is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)binaryExpression.Right;
                if (unaryExpression.Operand is ConstantExpression)
                {
                    var constantExpression = (ConstantExpression)unaryExpression.Operand;
                    value = constantExpression.Value;
                }
            }
            else if (binaryExpression.Right is MemberExpression)
            {
                var right = (MemberExpression)binaryExpression.Right;
                var constant = (ConstantExpression)right.Expression;
                var memberInfo = constant.Value.GetType().GetField(right.Member.Name);
                value = memberInfo.GetValue(constant.Value);

            }
            else if (binaryExpression.Right is ConstantExpression)
            {
                var constantExpression = (ConstantExpression)binaryExpression.Right;
                value = constantExpression.Value;
            }
            else if (binaryExpression.Right is BinaryExpression)
            {
                var binary = (BinaryExpression)binaryExpression.Right;
                throw new NotImplementedException();
            }

            if (value == null)
                return null;

            var bsonValue = BsonValue.Create(value);

            IMongoQuery query = null;
            switch (op)
            {
                case ExpressionType.Equal: query = Query.EQ(memberName, bsonValue);
                    break;
                case ExpressionType.GreaterThan:
                    query = Query.GT(memberName, bsonValue);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    query = Query.GTE(memberName, bsonValue);
                    break;
                case ExpressionType.LessThan:
                    query = Query.LT(memberName, bsonValue);
                    break;
                case ExpressionType.LessThanOrEqual:
                    query = Query.LTE(memberName, bsonValue);
                    break;
            }
            return query;
        }
    }
}
