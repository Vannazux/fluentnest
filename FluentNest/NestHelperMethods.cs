﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Nest;

namespace FluentNest
{
    public static class NestHelperMethods
    {
        public static string FirstCharacterToLower(this string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
        
        public static AggregationDescriptor<T> IntoDateHistogram<T>(this AggregationDescriptor<T> innerAggregation, Expression<Func<T, Object>> fieldGetter,DateInterval interval) where T : class
        {
            AggregationDescriptor<T> v = new AggregationDescriptor<T>();
            var fieldName = GetName(fieldGetter);
            v.DateHistogram(fieldName, dr=>
            {
                DateHistogramAggregationDescriptor<T> dateAggDesc = new DateHistogramAggregationDescriptor<T>();
                dateAggDesc.Field(fieldGetter).Interval(interval);
                return dateAggDesc.Aggregations(x => innerAggregation);
            });

            return v;
        }
        
        public static AggregationDescriptor<T> DateHistogram<T>(this AggregationDescriptor<T> agg, Expression<Func<T, Object>> fieldGetter, DateInterval dateInterval) where T : class
        {
            return agg.DateHistogram(GetName(fieldGetter), x => x.Field(fieldGetter).Interval(dateInterval));
        }

        private static string GetName<T,K>(this Expression<Func<T, K>> exp)
        {
            MemberExpression body = exp.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

         
            return body.Member.Name;
        }

        public static string GetAggName<T, K>(this Expression<Func<T, K>> exp, AggType type)
        {
            MemberExpression body = exp.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }


            return type + body.Member.Name;
        }


        public static IList<HistogramItem> GetDateHistogram<T>(this KeyItem item, Expression<Func<T, Object>> fieldGetter)
        {
            var histogramItem = item.DateHistogram(GetName(fieldGetter));
            return histogramItem.Items;
        }

        public static IList<HistogramItem> GetDateHistogram<T>(this AggregationsHelper aggs, Expression<Func<T, Object>> fieldGetter)
        {
            var histogramItem = aggs.DateHistogram(GetName(fieldGetter));
            return histogramItem.Items;
        }

        public static SearchDescriptor<T> FilterOn<T>(this SearchDescriptor<T> searchDescriptor,Expression<Func<T, Object>> fieldGetter, object value) where T:class
        {
            return searchDescriptor.Filter(x => x.Term(fieldGetter, value));
        }

        public static FilterContainer GenerateComparisonFilter<T>(this Expression expression, ExpressionType type) where T : class
        {
            var binaryExpression = expression as BinaryExpression;
            
            var value = GetValue(binaryExpression.Right);
            var memberAccessor = binaryExpression.Left as MemberExpression;
            var fieldName = GetFieldNameFromMember(memberAccessor);
            
            if (value is DateTime)
            {
                return GenerateComparisonFilter<T>((DateTime)value, type, fieldName);
            }
            else if (value is double)
            {
                return GenerateComparisonFilter<T>((double)value, type, fieldName);
            }
            else if (value is int)
            {
                return GenerateComparisonFilter<T>((long)(int)value, type, fieldName);
            }
            else if (value is long)
            {
                return GenerateComparisonFilter<T>((long)value, type, fieldName);
            }
            throw new InvalidOperationException("Comparison on non-supported type");
        }

        public static FilterContainer GenerateComparisonFilter<T>(DateTime value, ExpressionType type, string fieldName) where T : class
        {
            var filterDescriptor = new FilterDescriptor<T>();
            if (type == ExpressionType.LessThan)
            {
                return filterDescriptor.Range(x => x.Lower(value).OnField(fieldName));
            }
            else if (type == ExpressionType.GreaterThan)
            {
                return filterDescriptor.Range(x => x.Greater(value).OnField(fieldName));
            }
            else if (type == ExpressionType.LessThanOrEqual)
            {
                return filterDescriptor.Range(x => x.LowerOrEquals(value).OnField(fieldName));
            }
            else if (type == ExpressionType.GreaterThanOrEqual)
            {
                return filterDescriptor.Range(x => x.GreaterOrEquals(value).OnField(fieldName));
            }
            throw new NotImplementedException();
        }

        public static FilterContainer GenerateComparisonFilter<T>(long value, ExpressionType type, string fieldName) where T : class
        {
            var filterDescriptor = new FilterDescriptor<T>();
            if (type == ExpressionType.LessThan)
            {
                return filterDescriptor.Range(x => x.Lower(value).OnField(fieldName));
            }
            else if (type == ExpressionType.GreaterThan)
            {
                return filterDescriptor.Range(x => x.Greater(value).OnField(fieldName));
            }
            else if (type == ExpressionType.LessThanOrEqual)
            {
                return filterDescriptor.Range(x => x.LowerOrEquals(value).OnField(fieldName));
            }
            else if (type == ExpressionType.GreaterThanOrEqual)
            {
                return filterDescriptor.Range(x => x.GreaterOrEquals(value).OnField(fieldName));
            }
            throw new NotImplementedException();
        }

        public static FilterContainer GenerateComparisonFilter<T>(double value, ExpressionType type, string fieldName) where T : class
        {
            var filterDescriptor = new FilterDescriptor<T>();
            if (type == ExpressionType.LessThan)
            {
                return filterDescriptor.Range(x => x.Lower(value).OnField(fieldName));
            }
            else if (type == ExpressionType.GreaterThan)
            {
                return filterDescriptor.Range(x => x.Greater(value).OnField(fieldName));
            }
            else if (type == ExpressionType.LessThanOrEqual)
            {
                return filterDescriptor.Range(x => x.LowerOrEquals(value).OnField(fieldName));
            }
            else if (type == ExpressionType.GreaterThanOrEqual)
            {
                return filterDescriptor.Range(x => x.GreaterOrEquals(value).OnField(fieldName));
            }
            throw new NotImplementedException();
        }

        public static FilterContainer GenerateEqualityFilter<T>(this Expression expression) where T : class
        {
            var binaryExpression = expression as BinaryExpression;
            var value = GetValue(binaryExpression.Right);
            var filterDescriptor = new FilterDescriptor<T>();
            var fieldName = GetFieldName(binaryExpression.Left);
            return filterDescriptor.Term(fieldName,value);
        }

        public static FilterContainer GenerateNotEqualFilter<T>(this Expression expression) where T : class
        {
            var equalityFilter = GenerateEqualityFilter<T>(expression);
            var filterDescriptor = new FilterDescriptor<T>();
            return filterDescriptor.Not(x => equalityFilter);
        }

        public static FilterContainer GenerateBoolFilter<T>(this Expression expression) where T : class
        {
            var filterDescriptor = new FilterDescriptor<T>();
            var fieldName = GenerateFilterName(expression);
            return filterDescriptor.Term(fieldName, true);
        }


        public static string GetFieldName(this Expression expression)
        {
            if (expression is MemberExpression)
            {
                return GetFieldNameFromMember(expression as MemberExpression);
            }else if (expression is UnaryExpression)
            {
                var unary = expression as UnaryExpression;
                return GetFieldName(unary.Operand);
            }
            throw  new NotImplementedException();
        }

        public static string GetFieldNameFromMember(this MemberExpression expression)
        {
            return FirstCharacterToLower(expression.Member.Name);
        }

        public static FilterContainer GenerateFilterDescription<T>(this Expression expression) where T:class
        {
            var expType = expression.NodeType;
            
            if (expType == ExpressionType.AndAlso)
            {
                var binaryExpression = expression as BinaryExpression;
                var leftFilter = GenerateFilterDescription<T>(binaryExpression.Left);
                var rightFilter = GenerateFilterDescription<T>(binaryExpression.Right);
                var filterDescriptor = new FilterDescriptor<T>();
                return filterDescriptor.And(leftFilter, rightFilter);

            }
            else if (expType == ExpressionType.Or || expType == ExpressionType.OrElse)
            {
                var binaryExpression = expression as BinaryExpression;
                var leftFilter = GenerateFilterDescription<T>(binaryExpression.Left);
                var rightFilter = GenerateFilterDescription<T>(binaryExpression.Right);
                var filterDescriptor = new FilterDescriptor<T>();
                return filterDescriptor.Or(leftFilter, rightFilter);
            }
            else if (expType == ExpressionType.Equal)
            {
                return GenerateEqualityFilter<T>(expression);
            }
            else if(expType == ExpressionType.LessThan || expType == ExpressionType.GreaterThan || expType == ExpressionType.LessThanOrEqual || expType == ExpressionType.GreaterThanOrEqual)
            {
                return GenerateComparisonFilter<T>(expression,expType);
            }
            else if (expType == ExpressionType.MemberAccess)
            {
                var memberExpression = expression as MemberExpression;
                //here we handle binary expressions in the from .field.hasValue
                if (memberExpression.Member.Name == "HasValue")
                {
                    var parentFieldExpression = (memberExpression.Expression as MemberExpression);
                    var parentFieldName = GetFieldNameFromMember(parentFieldExpression);

                    var filterDescriptor = new FilterDescriptor<T>();
                    return filterDescriptor.Exists(parentFieldName);
                }
                var isProperty = memberExpression.Member.MemberType == MemberTypes.Property;
                if (isProperty)
                {
                    var propertyType = ((PropertyInfo) memberExpression.Member).PropertyType;
                    if (propertyType == typeof (bool))
                    {
                        return GenerateBoolFilter<T>(memberExpression);
                    }
                }
            }
            else if (expType == ExpressionType.Lambda)
            {
                var lambda = expression as LambdaExpression;
                return GenerateFilterDescription<T>(lambda.Body);
            }else if (expType == ExpressionType.NotEqual)
            {
                return GenerateNotEqualFilter<T>(expression);
            }
            throw  new NotImplementedException();
        }

        public static string GenerateFilterName(this Expression expression)
        {
            var expType = expression.NodeType;

            if (expType == ExpressionType.AndAlso || expType == ExpressionType.Or || expType == ExpressionType.OrElse || expType == ExpressionType.Equal 
                || expType == ExpressionType.LessThan || expType == ExpressionType.GreaterThan || expType == ExpressionType.LessThanOrEqual || expType == ExpressionType.GreaterThanOrEqual
                || expType == ExpressionType.NotEqual)
            {
                var binaryExpression = expression as BinaryExpression;
                var leftFilterName = GenerateFilterName(binaryExpression.Left);
                var rightFilterName = GenerateFilterName(binaryExpression.Right);
                return leftFilterName + "_" + expType + "_" + rightFilterName;
            }
            else if (expType == ExpressionType.MemberAccess)
            {
                var memberExpression = expression as MemberExpression;
                //here we handle binary expressions in the from .field.hasValue
                if (memberExpression.Member.Name == "HasValue")
                {
                    var parentFieldExpression = (memberExpression.Expression as MemberExpression);
                    var parentFieldName = GetFieldNameFromMember(parentFieldExpression);
                    return parentFieldName + ".hasValue";
                }
                return GetFieldNameFromMember(memberExpression);
            }
            else if (expType == ExpressionType.Lambda)
            {
                var lambda = expression as LambdaExpression;
                return GenerateFilterName(lambda.Body);
            }
            else if (expType == ExpressionType.Convert)
            {
                var unary = expression as UnaryExpression;
                return GenerateFilterName(unary.Operand);
            }else if (expType == ExpressionType.Constant)
            {
                var constExp = expression as ConstantExpression;
                return constExp.Value.ToString();
            }
            throw new NotImplementedException();
        }

        public static SearchDescriptor<T> FilterOn<T>(this SearchDescriptor<T> searchDescriptor, Expression<Func<T, bool>> filterRule) where T : class
        {
            var binaryExpression = filterRule.Body as BinaryExpression;
            var filterDescriptor = GenerateFilterDescription<T>(binaryExpression);
            return searchDescriptor.Filter(f => filterDescriptor);           
        }

        public static SearchDescriptor<T> FilteredOn<T>(this SearchDescriptor<T> searchDescriptor, Expression<Func<T, bool>> filterRule) where T : class
        {           
            var binaryExpression = filterRule.Body as BinaryExpression;            
            return searchDescriptor.Query(q => q.Filtered(fil=>fil.Filter(f=>GenerateFilterDescription<T>(binaryExpression))));
        }

        public static SearchDescriptor<T> FilteredOn<T>(this SearchDescriptor<T> searchDescriptor, FilterContainer container) where T : class
        {
            return searchDescriptor.Query(q => q.Filtered(fil => fil.Filter(f => container)));
        }

        public static FilterContainer AndFilteredOn<T>(this FilterContainer queryDescriptor, Expression<Func<T, bool>> filterRule) where T : class
        {
            var filterDescriptor = new FilterDescriptor<T>();
            var binaryExpression = filterRule.Body as BinaryExpression;
            var newPartOfQuery = GenerateFilterDescription<T>(binaryExpression);
            return filterDescriptor.And(newPartOfQuery, queryDescriptor);            
        }

        public static FilterContainer CreateFilter<T>(Expression<Func<T, bool>> filterRule) where T : class
        {
            return GenerateFilterDescription<T>(filterRule);
        }

        private static object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        public static T Parse<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static AggsContainer<T> AsContainer<T> (this AggregationsHelper aggs)
        {
            return new AggsContainer<T>(aggs);
        }
    }
}