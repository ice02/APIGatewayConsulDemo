using Jaeger;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Ocelot.Tracing.Jaeger.Extensions
{
    public static class LogFields
    {
        public const string ErrorKind = "error.kind";
        public const string ErrorObject = "error.object";
        public const string Event = "event";
        public const string Message = "message";
        public const string Stack = "stack";
        public const string Error = "Error";
    }

    public static class LogDataExtensions
    {


        private static LogData Set(this LogData logField, string key, object value)
        {
            if (logField == null)
            {
                throw new ArgumentNullException(nameof(logField));
            }
            
            logField.Fields.ToImmutableDictionary().SetItem(key, value);

            return logField;
        }

        public static LogData Event(this LogData logField, string eventName)
        {
            return logField.Set(LogFields.Event, eventName);
        }

        public static LogData EventError(this LogData logField)
        {
            return logField.Set(LogFields.Event, LogFields.Error);
        }

        public static LogData Message(this LogData logField, string message)
        {
            return logField.Set(LogFields.Message, message);
        }

        public static LogData Stack(this LogData logField, string stack)
        {
            return logField.Set(LogFields.Stack, stack);
        }

        public static LogData ErrorKind(this LogData logField, string errorKind)
        {
            return logField.Set(LogFields.ErrorKind, errorKind);
        }

        public static LogData ErrorObject(this LogData logField, Exception exception)
        {
            return logField.Set(LogFields.ErrorObject, exception.Message);
        }

        public static LogData ErrorKind<TException>(this LogData logField) where TException : Exception
        {
            return logField.ErrorKind(typeof(TException).FullName);
        }

        public static LogData ErrorKind<TException>(this LogData logField, TException exception) where TException : Exception
        {
            return logField.ErrorKind(exception?.GetType()?.FullName);
        }

        public static LogData ClientSend(this LogData logField)
        {
            return logField?.Event("Client Send");
        }

        public static LogData ClientReceive(this LogData logField)
        {
            return logField?.Event("Client Receive");
        }

        public static LogData ServerSend(this LogData logField)
        {
            return logField?.Event("Server Send");
        }

        public static LogData ServerReceive(this LogData logField)
        {
            return logField?.Event("Server Receive");
        }
    }
}
