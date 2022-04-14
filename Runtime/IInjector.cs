using System;
using System.Collections.Generic;
using Injections.Descriptions;

namespace Injections
{
    public interface IInjector : IInject, IResolve, IEnumerable<KeyValuePair<Type, IResolver>>
    {
        void Register(Type type, IResolver resolver);
        void UnRegister(Type type);
        IResolver GetResolver(Type type, bool includeInParents);
        DescriptionProvider DescriptionProvider { get; }
        Type InjectAttribute { get; }
    }
}