using TheBoys.Logger;
using TheBoys.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace TheBoys.Middleware;

/// <summary>
/// It will handle all exception error
/// </summary>
/// <remarks>
/// Constructor of ExceptionMiddleware
/// </remarks>
/// <param name="next"></param>
/// <param name="log"></param>
public class ExceptionMiddleware(RequestDelegate next, ILoggerService log)
{
    /// <summary>
    /// Invoke method
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (FieldAccessException fx)
        {
            log.Error(ExceptionFormatter.SerializeToString(fx));
            await HandleExceptionAsync(httpContext, fx, (int)HttpStatusCode.Conflict);
        }
        catch (ValidationException vx)
        {
            log.Error(ExceptionFormatter.SerializeToString(vx));
            await HandleExceptionAsync(httpContext, vx, (int)HttpStatusCode.NotAcceptable);
        }
        catch (UnauthorizedAccessException ex)
        {
            log.Error(ExceptionFormatter.SerializeToString(ex));
            await HandleExceptionAsync(httpContext, ex, (int)HttpStatusCode.Unauthorized);
        }
        catch (ArgumentException vx)
        {
            log.Error(ExceptionFormatter.SerializeToString(vx));
            await HandleExceptionAsync(httpContext, vx, (int)HttpStatusCode.NotAcceptable);
        }
        catch (Exception ex)
        {
            log.Error(ExceptionFormatter.SerializeToString(ex));
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode = (int)HttpStatusCode.InternalServerError)
    {
        context.Response.ContentType = "application/json";
        var errorobject = new ErrorDetails();

        switch (statusCode)
        {
            case (int)HttpStatusCode.Found:
                context.Response.StatusCode = statusCode;
                errorobject.Error.StatusCode = statusCode;
                errorobject.Error.Message = exception.Message;
                break;
            default:
                errorobject = ErrorResponse(statusCode, exception);
                context.Response.StatusCode = errorobject.Error.StatusCode;
                break;
        }

        return context.Response.WriteAsync(errorobject.ToString());
    }
    private static ErrorDetails ErrorResponse(int statusCode, Exception ex)
    {
        //Sample formate for custom error =>"error_409_abcd_abcd_...."
        int code = 0;
        bool IsCustomError = false;
        var errorobject = new ErrorDetails();
        if (ex.InnerException != null)
        {
            errorobject.Error.InnerExceptionMessage = ex.InnerException.Message;
        }
        if (Convert.ToString(ex.Message).Contains("error_"))
        {
            var errorMsg = Convert.ToString(ex.Message).Split('_');
            IsCustomError = errorMsg.Length > 1 && int.TryParse(Convert.ToString(errorMsg[1]), out code);
        }
        if (IsCustomError)
        {
            errorobject.Error.StatusCode = code;
            errorobject.Error.Message = ex.Message;

            return errorobject;
        }
        errorobject.Error.StatusCode = statusCode;
        errorobject.Error.Message = ex.Message;
        return errorobject;
    }
}
/// <summary>
/// Inject into application builder
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    /// <summary>
    /// UseCustomExceptionHandler
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}