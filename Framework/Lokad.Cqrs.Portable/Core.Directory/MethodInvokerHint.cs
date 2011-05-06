﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class MethodInvokerHint
    {
        public readonly Type ConsumerTypeDefinition;
        public readonly Optional<Type> MessageContextType;
        public readonly string MethodName;
        public readonly bool HasContext;
        public readonly Type MessageInterface;

        public MethodInvokerHint(Type consumerTypeDefinition, Optional<Type> messageContextType, string methodName, Type messageinterface)
        {
            ConsumerTypeDefinition = consumerTypeDefinition;
            MessageContextType = messageContextType;
            MethodName = methodName;
            HasContext = messageContextType.HasValue;
            MessageInterface = messageinterface;
        }

        public static MethodInvokerHint FromConsumerSample<THandler>(Expression<Action<THandler>> expression)
        {
            if (false == typeof(THandler).IsGenericType)
                throw new InvalidOperationException("Type should be a generic like 'IConsumeMessage<IMessage>'");

            var arguments = typeof(THandler).GetGenericArguments();

            if (arguments.Length != 1)
                throw new InvalidOperationException("Expected one generic argument");

            if (!arguments[0].IsInterface)
                throw new InvalidOperationException("Expected interface message base");

            var messageInterface = arguments[0];


            var interfaceTypedMethod = ((MethodCallExpression)expression.Body).Method;
            var parameters = interfaceTypedMethod.GetParameters();
            if ((parameters.Length < 1) || (parameters.Length > 2)) //|| (parameters[0].ParameterType != typeof (string))
                throw new InvalidOperationException("Expression should consume object like: 'i => i.Consume(null)' or 'i => i.Consume(null,null))'");


            var declaringGenericInterface = typeof(THandler).GetGenericTypeDefinition();
            var matches = declaringGenericInterface
                .GetMethods()
                .Where(mi => mi.Name == interfaceTypedMethod.Name)
                .Where(mi => mi.ContainsGenericParameters)
                .Where(mi =>
                    {
                        var length = mi.GetParameters().Length;
                        return length == parameters.Length;
                    })
                    .ToArray();

            if (matches.Length == 0)
            {
                throw new InvalidOperationException("Can't find generic method definition");
            }
            var method = matches[0];

            var genericParameters = method.GetParameters();

            if (genericParameters.Length == 2)
            {
                return new MethodInvokerHint(declaringGenericInterface, genericParameters[1].ParameterType, method.Name, messageInterface);
            }
            return new MethodInvokerHint(declaringGenericInterface, Optional<Type>.Empty, method.Name, messageInterface);
        }
    }
}