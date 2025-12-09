using System;

namespace Amuse.UI.Exceptions
{
    public class UnrecoverableException : Exception
    {
        public UnrecoverableException(string message, Exception innerException)
        : base(message, innerException) { }


        /// <summary>
        /// Checks if the exception is unrecoverable and throws if required
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <exception cref="UnrecoverableException"></exception>
        public static void ThrowIf(Exception exception)
        {
            var errorMessage = GetErrorMessage(exception.Message);
            if (!string.IsNullOrEmpty(errorMessage))
                throw new UnrecoverableException(errorMessage, exception);
        }

        private static string GetErrorMessage(string exceptionMessage)
        {
            // OnnxRuntime
            // 80004005: Unspecified Error
            // 8000FFFF: Catastrophic Failure
            // 887A0005: The GPU decvice instance has been suspended

            if (exceptionMessage.Contains("80004005"))
                return "80004005: Unspecified Error";
            if (exceptionMessage.Contains("8000FFFF"))
                return "8000FFFF: Catastrophic Failure";
            if (exceptionMessage.Contains("887A0005"))
                return "887A0005: The GPU decvice instance has been suspended";

            return null;
        }
    }
}
