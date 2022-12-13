using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DapperLabs.FlowSDK.DevWallet")]

namespace DapperLabs.Flow.Sdk.Exceptions
{
    internal class FlowException : Exception
    {
        internal FlowException(string message) : base(message) { }
        internal FlowException(string message, Exception inner) : base(message, inner) { }

    }
}
