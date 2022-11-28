using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MessageBus
{
    public static class MultipleHandlerImplementationExtension
    {
        /// <summary>
        /// Searches for all <see cref="IMessageEventHandler{TEvent}"/> or <see cref="IAsyncMessageEventHandler{TEvent}"/>
        /// implementations within the instance of <paramref name="handler"/> and registers each implementation
        /// to the provided <paramref name="messageBus"/>. The method returns a list of all subscription disposables
        /// returned by <see cref="IMessageBusHandler.RegisterEventHandler{TEvent}(IMessageEventHandler{TEvent})"/> or
        /// <see cref="IMessageBusHandler.RegisterEventHandler{TEvent}(IAsyncMessageEventHandler{TEvent})"/>.
        /// </summary>
        /// <remarks>If an exception during the regiration process happens, all previously generated subscriptions which happened within
        /// this method will get rolled back.</remarks>
        /// <exception cref="ArgumentException">Is thrown if the registration methods could not be found within <paramref name="messageBus"/>.</exception>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="handler"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Is thrown if <paramref name="handler"/> does not implement any <see cref="IMessageEventHandler{TEvent}"/>
        /// or <see cref="IAsyncMessageEventHandler{TEvent}"/>.</exception>
        /// <exception cref="TargetInvocationException">Is thrown if the registration failed.</exception>
        public static IReadOnlyList<IDisposable> RegisterAllEventHandlers(this IMessageBusHandler messageBus, object handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            return RegisterAllHandlers(
                messageBus,
                handler,
                registerMethodName: nameof(IMessageBusHandler.RegisterEventHandler),
                explicitInterfaceMethodName: $"{typeof(IMessageBusHandler).FullName}.{nameof(IMessageBusHandler.RegisterEventHandler)}",
                interfaceTypes: new[]
                {
                    (typeof(IMessageEventHandler<>), 1),
                    (typeof(IAsyncMessageEventHandler<>), 1)
                });
        }

        /// <summary>
        /// Searches for all <see cref="IMessageCommandHandler{TEvent}"/> or <see cref="IAsyncMessageCommandHandler{TEvent}"/>
        /// implementations within the instance of <paramref name="handler"/> and registers each implementation
        /// to the provided <paramref name="messageBus"/>. The method returns a list of all subscription disposables
        /// returned by <see cref="IMessageBusHandler.RegisterCommandHandler{TEvent}(IMessageCommandHandler{TEvent})"/> or
        /// <see cref="IMessageBusHandler.RegisterCommandHandler{TEvent}(IAsyncMessageCommandHandler{TEvent})"/>.
        /// </summary>
        /// <remarks>If an exception during the regiration process happens, all previously generated subscriptions which happened within
        /// this method will get rolled back.</remarks>
        /// <exception cref="ArgumentException">Is thrown if the registration methods could not be found within <paramref name="messageBus"/>.</exception>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="handler"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Is thrown if <paramref name="handler"/> does not implement any <see cref="IMessageCommandHandler{TEvent}"/>
        /// or <see cref="IAsyncMessageCommandHandler{TEvent}"/>.</exception>
        /// <exception cref="TargetInvocationException">Is thrown if the registration failed.</exception>
        public static IReadOnlyList<IDisposable> RegisterAllCommandHandlers(this IMessageBusHandler messageBus, object handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            return RegisterAllHandlers(
                messageBus,
                handler,
                registerMethodName: nameof(IMessageBusHandler.RegisterCommandHandler),
                explicitInterfaceMethodName: $"{typeof(IMessageBusHandler).FullName}.{nameof(IMessageBusHandler.RegisterCommandHandler)}",
                interfaceTypes: new[]
                {
                    (typeof(IMessageCommandHandler<>), 1),
                    (typeof(IAsyncMessageCommandHandler<>), 1)
                });
        }

        /// <summary>
        /// Searches for all <see cref="IMessageQueryHandler{TQuery, TQueryResult}"/> or <see cref="IAsyncMessageQueryHandler{TQuery, TQueryResult}"/>
        /// implementations within the instance of <paramref name="handler"/> and registers each implementation
        /// to the provided <paramref name="messageBus"/>. The method returns a list of all subscription disposables
        /// returned by <see cref="IMessageBusHandler.RegisterQueryHandler{TQuery, TQueryResult}(IMessageQueryHandler{TQuery, TQueryResult})"/> or
        /// <see cref="IMessageBusHandler.RegisterQueryHandler{TQuery, TQueryResult}(IAsyncMessageQueryHandler{TQuery, TQueryResult})"/>.
        /// </summary>
        /// <remarks>If an exception during the regiration process happens, all previously generated subscriptions which happened within
        /// this method will get rolled back.</remarks>
        /// <exception cref="ArgumentException">Is thrown if the registration methods could not be found within <paramref name="messageBus"/>.</exception>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="handler"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Is thrown if <paramref name="handler"/> does not implement any <see cref="IMessageQueryHandler{TQuery, TQueryResult}"/>
        /// or <see cref="IAsyncMessageQueryHandler{TQuery, TQueryResult}"/>.</exception>
        /// <exception cref="TargetInvocationException">Is thrown if the registration failed.</exception>
        public static IReadOnlyList<IDisposable> RegisterAllQueryHandlers(this IMessageBusHandler messageBus, object handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            return RegisterAllHandlers(
                messageBus,
                handler,
                registerMethodName: nameof(IMessageBusHandler.RegisterQueryHandler),
                explicitInterfaceMethodName: $"{typeof(IMessageBusHandler).FullName}.{nameof(IMessageBusHandler.RegisterQueryHandler)}",
                interfaceTypes: new[]
                {
                    (typeof(IMessageQueryHandler<,>), 2),
                    (typeof(IAsyncMessageQueryHandler<,>), 2)
                });
        }

        /// <summary>
        /// Searches for all <see cref="IMessageRpcHandler{TRpc, TRpcResult}"/> or <see cref="IAsyncMessageRpcHandler{TRpc, TRpcResult}"/>
        /// implementations within the instance of <paramref name="handler"/> and registers each implementation
        /// to the provided <paramref name="messageBus"/>. The method returns a list of all subscription disposables
        /// returned by <see cref="IMessageBusHandler.RegisterRpcHandler{TRpc, TRpcResult}(IMessageRpcHandler{TRpc, TRpcResult})"/> or
        /// <see cref="IMessageBusHandler.RegisterRpcHandler{TRpc, TRpcResult}(IAsyncMessageRpcHandler{TRpc, TRpcResult})"/>.
        /// </summary>
        /// <remarks>If an exception during the regiration process happens, all previously generated subscriptions which happened within
        /// this method will get rolled back.</remarks>
        /// <exception cref="ArgumentException">Is thrown if the registration methods could not be found within <paramref name="messageBus"/>.</exception>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="handler"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Is thrown if <paramref name="handler"/> does not implement any <see cref="IMessageRpcHandler{TRpc, TRpcResult}"/>
        /// or <see cref="IAsyncMessageRpcHandler{TRpc, TRpcResult}"/>.</exception>
        /// <exception cref="TargetInvocationException">Is thrown if the registration failed.</exception>
        public static IReadOnlyList<IDisposable> RegisterAllRpcHandlers(this IMessageBusHandler messageBus, object handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            return RegisterAllHandlers(
                messageBus,
                handler,
                registerMethodName: nameof(IMessageBusHandler.RegisterRpcHandler),
                explicitInterfaceMethodName: $"{typeof(IMessageBusHandler).FullName}.{nameof(IMessageBusHandler.RegisterRpcHandler)}",
                interfaceTypes: new[]
                {
                    (typeof(IMessageRpcHandler<,>), 2),
                    (typeof(IAsyncMessageRpcHandler<,>), 2)
                });
        }

        private static IReadOnlyList<IDisposable> RegisterAllHandlers(
            IMessageBusHandler messageBus,
            object handler,
            string registerMethodName,
            string explicitInterfaceMethodName,
            IReadOnlyList<(Type HandlerType, int MessageTypeCount)> interfaceTypes)
        {
            List<IDisposable> result = new List<IDisposable>();
            try
            {
                foreach ((Type HandlerType, int MessageTypeCount) pair in interfaceTypes)
                {
                    MethodInfo registerEventHandler = GetRegistrationMethod(messageBus, registerMethodName, explicitInterfaceMethodName, pair.HandlerType);
                    result.AddRange(
                        CollectInterfaceGenericTypes(handler, pair.HandlerType, pair.MessageTypeCount)
                            .Select(eventTypes => AssignHandler(messageBus, handler, registerEventHandler, eventTypes))
                    );
                }
            }
            catch (Exception)
            {
                // rollback
                foreach (var p in result)
                    p.Dispose();

                throw;
            }

            if (result.Count == 0)
                throw new InvalidOperationException($"No event handler implementation found in '{handler.GetType().FullName}'");

            return result;
        }

        /// <summary>
        /// Calls the event registration method of <paramref name="messageBus"/> within the instance provided by <paramref name="handler"/>.
        /// The method to call is defined by <paramref name="invoke"/>. Because the registration methods have a generic type parameter,
        /// the provided <paramref name="eventTypes"/> defines the actual types to pass.
        /// </summary>
        private static IDisposable AssignHandler(IMessageBusHandler messageBus, object handler, MethodInfo invoke, Type[] eventTypes)
        {
            object? subscription = invoke.MakeGenericMethod(eventTypes).Invoke(messageBus, new[] { handler });
            if (subscription is not IDisposable disposableSubscription)
                throw new InvalidOperationException($"Registration did not return a disposable");
            return disposableSubscription;
        }

        /// <summary>
        /// Return the generic type of all interfaces of the given <paramref name="interfaceType"/> which
        /// are implemented by the provided <paramref name="handler"/>.
        /// </summary>
        private static IEnumerable<Type[]> CollectInterfaceGenericTypes(object handler, Type interfaceType, int expectedGenericArgumentCount)
        {
            return handler
                .GetType()
                .GetInterfaces()
                .Where(p => p.IsGenericType)
                .Where(p => p.GetGenericTypeDefinition() == interfaceType)
                .Select(p => p.GetGenericArguments())
                .Where(p => p.Length == expectedGenericArgumentCount);
        }

        private static MethodInfo GetRegistrationMethod(IMessageBusHandler messageBus, string methodName, string explicitMethodName, Type eventHandlerInterfaceType)
        {
            MethodInfo[] potentialMethods = messageBus.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.MemberType == MemberTypes.Method && !p.IsStatic && p.ContainsGenericParameters)
                .Where(p => p.ReturnType == typeof(IDisposable))
                .Where(p =>
                {
                    ParameterInfo[] arguments = p.GetParameters();
                    return arguments.Length == 1 &&
                        arguments[0].ParameterType.IsGenericType &&
                        arguments[0].ParameterType.GetGenericTypeDefinition() == eventHandlerInterfaceType;
                })
                .ToArray();

            MethodInfo[] implicitInterfaceImplementation = potentialMethods
                .Where(p => p.Name == methodName)
                .ToArray();

            MethodInfo[] explicitInterfaceImplementation = potentialMethods
                .Where(p => p.Name == explicitMethodName)
                .ToArray();

            if (explicitInterfaceImplementation.Length == 1)
                return explicitInterfaceImplementation[0];

            if (potentialMethods.Length == 0)
                throw new ArgumentException($"Could not find the needed '{methodName}' method");

            if (potentialMethods.Length > 1)
                throw new ArgumentException($"Found {potentialMethods.Length} methods, but expected exactly one matching method for '{methodName}'");

            return potentialMethods[0];
        }
    }
}
