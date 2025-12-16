using EcommerceIntegration.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceIntegration.Infrastructure.Shopify;

/// <summary>
/// Validates Shopify webhook signatures using HMAC-SHA256.
/// </summary>
public class ShopifySignatureValidator : IWebhookSignatureValidator
{
    public bool ValidateSignature(string payload, string signature, string secret)
    {
        // TODO: Implement HMAC-SHA256 signature validation
        // 1. Compute HMAC-SHA256 of payload using secret
        // 2. Base64 encode the result
        // 3. Compare with provided signature
        
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToBase64String(computedHash);
        
        return string.Equals(computedSignature, signature, StringComparison.Ordinal);
    }
}
