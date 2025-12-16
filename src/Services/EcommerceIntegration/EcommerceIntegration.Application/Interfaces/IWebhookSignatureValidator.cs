namespace EcommerceIntegration.Application.Interfaces;

/// <summary>
/// Interface for validating webhook signatures.
/// </summary>
public interface IWebhookSignatureValidator
{
    bool ValidateSignature(string payload, string signature, string secret);
}
