using System.Net;
using TheBoys.Models;

namespace TheBoys.Middleware;

/// <summary>
/// It will format the exception
/// </summary>
public static class ExceptionFormatter
{
    private static void SerializeInnerException(Exception? e, TextWriter writer)
    {
        if (e != null)
        {
            writer.WriteLine("Inner Exception: {0}", e.Message);
            writer.WriteLine("Inner Exception: {0}", e.GetType());
            SerializeInnerException(e.InnerException, writer);
        }
    }
    /// <summary>
    /// Serialize object to string
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static string SerializeToString(Exception exception)
    {
        var sourceMethod = string.Empty;
        var sourceType = string.Empty;
        var methodBase = exception.TargetSite;
        if (methodBase != null)
        {
            sourceMethod = methodBase.ToString();
            var declaringType = methodBase.DeclaringType;
            sourceType = declaringType == null ? string.Empty : declaringType.AssemblyQualifiedName;
        }

        var writer = new StringWriter();

        writer.WriteLine("User: Anonymous");
        writer.WriteLine("Message: {0}", exception.Message);
        writer.WriteLine("ExceptionType: {0}", exception.GetType());
        writer.WriteLine("SourceType: {0}", sourceType);
        writer.WriteLine("SourceMethod: {0}", sourceMethod);
        writer.WriteLine("StackTrace:");
        writer.WriteLine(exception.StackTrace);
        SerializeInnerException(exception, writer);
        return writer.ToString();
    }
    /// <summary>
    /// It will return error detail object
    /// </summary>
    /// <param name="error"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public static ErrorDetails ErrorMessage(string error, int status = (int)HttpStatusCode.BadRequest)
    {
        var errorobject = new ErrorDetails
        {
            Error =
            {
                Message = error,
                StatusCode = status
            }
        };
        return errorobject;
    }
}