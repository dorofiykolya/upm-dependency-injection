﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Injections.Descriptions
{
    public abstract class MethodBaseDescription : MemberDescription
    {
        private readonly bool _optional;
        private readonly ParameterInfo[] _parameters;
        private readonly Func<object, object[], object> _invoker;
        private readonly string _name;

        protected MethodBaseDescription(MethodBase methodInfo, Attribute attribute, bool optional = false, ParameterInfo[] parameters = null) : base(
            methodInfo, attribute)
        {
            _optional = optional;
            if (methodInfo != null)
            {
                _invoker = methodInfo.Invoke;
                _name = methodInfo.Name;
                _parameters = parameters ?? methodInfo.GetParameters();
            }
        }

        public ParameterInfo[] Parameters => _parameters;

        public override void SetValue(object target, object value)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object target)
        {
            throw new NotImplementedException();
        }

        public override void Apply(object target, Type targetType, IInjector injector)
        {
            var parameters = GetParameterValues(targetType, injector);
            _invoker(target, parameters);
        }

        protected virtual object[] GetParameterValues(Type targetType, IInjector injector)
        {
            if (_parameters == null)
            {
                return new object[0];
            }

            List<object> parameters = new List<object>();
            ParameterInfo[] parameterInfos = _parameters;
            int length = parameterInfos.Length;
            for (int i = 0; i < length; i++)
            {
                Type parameterType = parameterInfos[i].ParameterType;
                var isLazy = parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Lazy<>);
                var provider = injector.GetResolver(isLazy ? parameterType.GetGenericArguments()[0] : parameterType,
                    true);
                if (provider == null)
                {
                    if (parameterInfos[i].IsOptional)
                    {
                        parameters.Add(parameterInfos[i].DefaultValue);
                        continue;
                    }

                    if (_optional)
                    {
                        parameters.Add(null);
                        continue; //TODO: Check optional parameters are in order (last) for this break to work, else use continue
                    }

                    throw new InvalidOperationException(
                        "Injector is missing a mapping to handle constructor injection into target type '"
                        + targetType.FullName + "'. \nTarget dependency: " + parameterType.FullName +
                        ", method: " + _name + ", parameter: " + (i + 1)
                    );
                }

                parameters.Add(provider.Resolve(injector, targetType));
            }

            return parameters.ToArray();
        }
    }
}