using System.Collections.Generic;

namespace MessageBus.Decorations.Validations
{
    public interface IValidationResult
    {
        /// <summary>
        /// Contains the list of validation errors. If no
        /// error occured, this list will be empty.
        /// </summary>
        IReadOnlyList<IValidationError> Errors { get; }
    }

    public interface IValidationError
    {
        /// <summary>
        /// The message describing the validation problem.
        /// </summary>
        string ErrorMessage { get; }
    }
}
