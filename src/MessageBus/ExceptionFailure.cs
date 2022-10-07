using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MessageBus
{
    internal class ExceptionFailure : IFailureDetails
    {
        /// <summary>
        /// The category which gets used for all exceptions.
        /// </summary>
        public static string CategoryName => "$DotNetException";

        public static string KeyExceptionTypeName => "Type";

        public static string KeyExceptionStackTrace => "StackTrace";

        public static string KeyExceptionSource => "Source";

        public static string KeyInnerException => "InnerException";

        public ExceptionFailure(Exception ex)
        {
            Category = CategoryName;
            Message = ex.Message;
            StatusCode = ex.HResult;
            Details = BuildExceptionDetails(ex);
        }

        [return: NotNullIfNotNull("ex")]
        private static IReadOnlyDictionary<string, object?>? BuildExceptionDetails(Exception? ex)
        {
            if (ex is null)
                return null;

            return new Dictionary<string, object?>()
            {
                [KeyExceptionTypeName] = ex.GetType().FullName ?? ex.GetType().Name,
                [KeyExceptionStackTrace] = ex.StackTrace,
                [KeyExceptionSource] = ex.Source,
                [KeyInnerException] = BuildExceptionDetails(ex.InnerException)
            };
        }

        public string Category { get; private init; }

        public string Message { get; private init; }

        public int StatusCode { get; private init; }

        public IReadOnlyDictionary<string, object?> Details { get; }
    }
}
