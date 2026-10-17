using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AkarangaBank.Web.Services;

public sealed class EmailSender : IEmailSender
{
    private const string BrevoSendEmailEndpoint = "https://api.brevo.com/v3/smtp/email";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(HttpClient httpClient, IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
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
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            var details = responseBody.Length <= 1000 ? responseBody : responseBody[..1000];
            throw new HttpRequestException(
                $"L'API Brevo a refusé l'e-mail (HTTP {(int)response.StatusCode}) : {details}",
                null,
                response.StatusCode);
        }

        string? messageId = null;
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("messageId", out var id)
                && id.ValueKind == JsonValueKind.String)
                messageId = id.GetString();
        }
        catch (JsonException)
        {
            // Brevo accepted the request; the body may be empty or use a future response format.
        }

        _logger.LogInformation("Brevo a accepté un e-mail transactionnel. MessageId={MessageId}", messageId ?? "non fourni");
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
