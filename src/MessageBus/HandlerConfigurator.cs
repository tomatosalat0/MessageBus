using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus
{
    public sealed class HandlerConfigurator
    {
        private readonly List<(Type IncommingHandler, Func<object, object> Configurator)> _registeredCallbacks = new List<(Type IncommingHandler, Func<object, object> Configurator)>();

        /// <summary>
        /// Specify the callback which should get used the input type is <typeparamref name="THandler"/>. Only if the
        /// input type matches, the <paramref name="configure"/> action will get executed.
        /// </summary>
        /// <remarks>If an object matches more than one registered <typeparamref name="THandler"/>, only the first one
        /// will get executed.</remarks>
        public HandlerConfigurator For<THandler>(Func<THandler, object> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            _registeredCallbacks.Add((typeof(THandler), o => configure((THandler)o)));
            return this;
        }

        internal object Apply(Type inputType, object input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            Func<object, object>? registeredHandler = AllCallbacksFor(inputType).FirstOrDefault();
            if (registeredHandler is not null)
                return registeredHandler(input);

            return input;
        }

        private IEnumerable<Func<object, object>> AllCallbacksFor(Type inputType)
        {
            List<Type> allTypes = new List<Type>();
            allTypes.Add(inputType);
            allTypes.AddRange(AllBaseTypes(inputType));
            allTypes.AddRange(AllInterfaces(allTypes.ToList()));

            foreach (var pair in _registeredCallbacks)
            {
                if (allTypes.Contains(pair.IncommingHandler))
                    yield return pair.Configurator;
            }
        }

        private IEnumerable<Type> AllInterfaces(IReadOnlyList<Type> types)
        {
            return types.SelectMany(p => p.GetInterfaces());
        }

        private IEnumerable<Type> AllBaseTypes(Type type)
        {
            if (type.BaseType is null)
                yield break;

            yield return type.BaseType;
            foreach (var p in AllBaseTypes(type.BaseType))
                yield return p;
        }
    }
}
