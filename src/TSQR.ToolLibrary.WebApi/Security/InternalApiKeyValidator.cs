using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace TSQR.ToolLibrary.WebApi.Security;

/// <summary>
/// Shared-secret guard for service-to-service endpoints that are deliberately
/// not exposed through the public gateway.
/// </summary>
public static class InternalApiKeyValidator
{
    public const string HeaderName = "X-Internal-Api-Key";

    public static bool IsValid(HttpRequest request, IConfiguration configuration)
    {
        var expected = configuration["InternalApi:Key"];
        if (string.IsNullOrWhiteSpace(expected))
            return false;

        var provided = request.Headers[HeaderName].ToString();
        if (provided.Length != expected.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
    }
}
