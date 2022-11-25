using System.Threading.Tasks;

namespace MessageBus.Decorations.Validations
{
    public interface IMessageValidator<TMessage>
    {
        /// <summary>
        /// Execute the validation for the provided <paramref name="message"/> and
        /// returns the result of that validation.
        /// </summary>
        IValidationResult Validate(TMessage message);
    }

    public interface IAsyncMessageValidator<TMessage>
    {
        /// <summary>
        /// Execute the validation for the provided <paramref name="message"/> and
        /// returns the result of that validation.
        /// </summary>
        Task<IValidationResult> ValidateAsync(TMessage message);
    }
}
