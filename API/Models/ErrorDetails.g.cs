using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TheBoys.Models;

/// <summary>
/// Error model
/// </summary>
[Serializable]
public class ErrorDetails
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ErrorDetails()
    {
        Error = new ErrorMessage();
    }
    /// <summary>
    /// Error property
    /// </summary>
    public ErrorMessage Error { get; set; }
    /// <summary>
    /// Extension method
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
                {
                    OverrideSpecifiedNames = false
                }
            }
        });
    }
}
/// <summary>
/// Model class
/// </summary>
public class ErrorMessage
{    /// <summary>
    /// Status code
    /// </summary>
    public int StatusCode { get; set; } = 0;
    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Exception message
    /// </summary>
    public string InnerExceptionMessage { get; set; } = string.Empty;
}