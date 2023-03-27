using System.Globalization;

namespace UserManagement.Exceptions;
/// <summary>
/// Custom exception class for throwing application specific exceptions(e.g. for validation) <br/>
/// that can be caught and handled within the application
/// </summary>
public class AppException : Exception
{
    public AppException() : base() { }
    public AppException(string message) : base(message) { }
    public AppException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    { }
}