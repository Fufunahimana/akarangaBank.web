using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AkarangaBank.Web.Services;

public sealed class EmailSender : IEmailSender
{
    private const string BrevoSendEmailEndpoint = "https://api.brevo.com/v3/smtp/email";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EmailSender(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var apiKey = _configuration["Email:ApiKey"];
        var fromAddress = _configuration["Email:FromAddress"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("La configuration Email:ApiKey (clé API Brevo) est obligatoire.");
        if (string.IsNullOrWhiteSpace(fromAddress))
            throw new InvalidOperationException("La configuration Email:FromAddress est obligatoire.");

        var fromName = _configuration["Email:FromName"];
        var payload = new BrevoEmailRequest(
            new BrevoEmailAddress(fromAddress, string.IsNullOrWhiteSpace(fromName) ? "Akaranga Bank" : fromName.Trim()),
            [new BrevoEmailAddress(email)],
            subject,
            htmlMessage);

        using var request = new HttpRequestMessage(HttpMethod.Post, BrevoSendEmailEndpoint)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.TryAddWithoutValidation("api-key", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private sealed record BrevoEmailRequest(
        [property: JsonPropertyName("sender")] BrevoEmailAddress Sender,
        [property: JsonPropertyName("to")] IReadOnlyList<BrevoEmailAddress> To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("htmlContent")] string HtmlContent);

    private sealed record BrevoEmailAddress(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [property: JsonPropertyName("name")] string? Name = null);
}
