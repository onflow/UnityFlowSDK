using System;

namespace DapperLabs.Flow.Sdk.Exceptions
{
	/// <summary>
	/// A FlowError is returned when a Flow SDK operation fails.
	/// </summary>
	public class FlowError
	{
		/// <summary>
		/// A message describing the failure
		/// </summary>
		public string Message { get; private set; }
		
		/// <summary>
		/// The stack trace of the failure if it was caused by an exception
		/// </summary>
		public string StackTrace { get; private set; }
		
		/// <summary>
		/// The exception that caused the failure, if one occured
		/// </summary>
		public Exception Exception { get; private set; }

		internal FlowError (Exception ex)
		{
			Message = ex.Message;
			StackTrace = ex.StackTrace;
			Exception = ex.InnerException;
		}

		internal FlowError(string message, Exception ex)
		{
			Message = message;
			if (ex != null)
            {
				StackTrace = ex.StackTrace;
				Exception = ex.InnerException;
			}
		}

		internal FlowError(string message)
		{
			Message = message;
		}
	}
}
