using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageBus.Decorations.Validations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class CommandMessageValidationTests
    {
        [TestMethod]
        public async Task NoValidationWillCallTheUnderlyingHandler()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler.WithValidation());

            RegisterNewUserCommand scheduledCommand = new RegisterNewUserCommand();
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, handler.CallCount);
        }

        [TestMethod]
        public async Task NoValidationErrorWillCallTheUnderlyingHandler ()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CallbackValidator validators = new CallbackValidator(
                (command) => NoErrors()
            );
            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler
                .WithValidation(validators)
            );

            RegisterNewUserCommand scheduledCommand = new RegisterNewUserCommand();
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, handler.CallCount);
        }

        [TestMethod]
        public async Task WithValidationErrorWillThrowAnException()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CallbackValidator validators = new CallbackValidator(
                UserNameMustHaveAtLeastTwoCharacters
            );
            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler
                .WithValidation(validators)
            );

            RegisterNewUserCommand scheduledCommand = new RegisterNewUserCommand();
            MessageOperationFailedException exception = await Assert.ThrowsExceptionAsync<MessageOperationFailedException>(() => bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2)));

            Assert.AreEqual("The user name must have at least two characters", exception.Message);
            Assert.AreEqual(0, handler.CallCount);
        }

        [TestMethod]
        public async Task AllRegisteredValidatorsWillAlwaysGetExecuted()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CallbackValidator validators = new CallbackValidator(
                UserNameMustHaveAtLeastTwoCharacters,
                UserNameMustNotStartWithANumber
            );
            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler
                .WithValidation(validators)
            );

            RegisterNewUserCommand scheduledCommand = new RegisterNewUserCommand() { UserName = "1" };
            MessageOperationFailedException exception = await Assert.ThrowsExceptionAsync<MessageOperationFailedException>(() => bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2)));

            Assert.AreEqual("The user name must have at least two characters\nThe user name must not start with a number", exception.Message);
            Assert.AreEqual(0, handler.CallCount);
        }

        [TestMethod]
        public async Task UnderlyingHandlerWillGetExecutedWhenAllValidationsSucceed()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CallbackValidator validators = new CallbackValidator(
                UserNameMustHaveAtLeastTwoCharacters,
                UserNameMustNotStartWithANumber
            );
            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler
                .WithValidation(validators)
            );

            RegisterNewUserCommand scheduledCommand = new RegisterNewUserCommand() { UserName = "Foo" };
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, handler.CallCount);
        }

        private static IEnumerable<IValidationError> UserNameMustHaveAtLeastTwoCharacters(RegisterNewUserCommand command)
        {
            if ((command.UserName ?? string.Empty).Trim().Length < 2)
                yield return new SimpleValidation("The user name must have at least two characters");
        }

        private static IEnumerable<IValidationError> UserNameMustNotStartWithANumber(RegisterNewUserCommand command)
        {
            char value = command.UserName?.Length > 0 ? command.UserName[0] : '\0';
            if (value >= '0' && value <= '9')
                yield return new SimpleValidation("The user name must not start with a number");
        }

        private static IEnumerable<IValidationError> NoErrors()
        {
            yield break;
        }

        private class SimpleValidation : IValidationError
        {
            public SimpleValidation(string message)
            {
                if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace.", nameof(message));
                ErrorMessage = message;
            }

            public string ErrorMessage { get; }
        }

        [Topic("Commands/RegisterNewUser")]
        public class RegisterNewUserCommand : IMessageCommand
        {
            public string UserName { get; init; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }

        public class CallbackValidator : IMessageValidator<RegisterNewUserCommand>
        {
            private readonly IReadOnlyList<Func<RegisterNewUserCommand, IEnumerable<IValidationError>>> _validators;

            public CallbackValidator(params Func<RegisterNewUserCommand, IEnumerable<IValidationError>>[] validators)
            {
                _validators = validators;
            }

            public IValidationResult Validate(RegisterNewUserCommand command)
            {
                return new Result(_validators.SelectMany(p => p(command)));
            }

            private class Result : IValidationResult
            {
                public Result(IEnumerable<IValidationError> errors)
                {
                    Errors = errors.ToArray();
                }

                public IReadOnlyList<IValidationError> Errors { get; }
            }
        }

        public class CommandCounterHandler : IMessageCommandHandler<RegisterNewUserCommand>
        {
            private int _callCount;

            public int CallCount => _callCount;

            public void Handle(RegisterNewUserCommand command)
            {
                ++_callCount;
            }
        }
    }
}
