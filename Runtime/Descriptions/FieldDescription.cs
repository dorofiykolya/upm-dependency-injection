﻿using System;
using System.Reflection;

namespace Injections.Descriptions
{
    public class FieldDescription : MemberDescription
    {
        private readonly Type _type;
        private readonly Func<object, object> _getValue;
        private readonly Action<object, object> _setValue;
        private readonly Type _fieldType;

        public FieldDescription(FieldInfo fieldInfo, Attribute attribute) : base(fieldInfo, attribute)
        {
            if (fieldInfo != null)
            {
                _getValue = fieldInfo.GetValue;
                _setValue = fieldInfo.SetValue;
                _fieldType = fieldInfo.FieldType;
                _type = fieldInfo.FieldType;
                if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(Lazy<>))
                {
                    _type = _type.GetGenericArguments()[0];
                }
            }
        }

        public override MemberKind Kind => MemberKind.Field;

        public override Type Type => _fieldType;

        public override Type ProviderType => _type;

        public override void SetValue(object target, object value)
        {
            if (_setValue != null)
            {
                _setValue(target, value);
            }
        }

        public override object GetValue(object target)
        {
            if (_getValue != null)
            {
                return _getValue(target);
            }

            return null;
        }

        public override void Apply(object target, Type targetType, IInjector injector)
        {
            var provider = injector.GetResolver(ProviderType, true);
            SetValue(target, provider.Resolve(injector, targetType));
        }
    }
}