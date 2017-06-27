using System;

namespace Cleverbot.Exceptions
{
    public class CleverbotApiException : Exception
    {
        public CleverbotApiException(string errorDetails)
            : base($"An exception has occured with Cleverbot's API. Please view the details: {errorDetails}")
        {
        }
    }
}
