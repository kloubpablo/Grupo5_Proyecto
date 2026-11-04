using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace WebApplicationAPP.Services;

/// <summary>
/// Envia correos con Gmail API por HTTPS; admite texto plano o HTML con alternativa de texto.
/// Conserva los mismos nombres de configuracion y parametros que el servicio anterior.
/// </summary>
public sealed class GmailApiEmailSender
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GmailApiEmailSender(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task EnviarAsync(
        string destinatario,
        string asunto,
        string contenido,
        CancellationToken cancellationToken = default,
        string? contenidoHtml = null)
    {
        string remitente = Requerido("GmailApi:Remitente");
        string clientId = Requerido("GmailApi:ClientId");
        string clientSecret = Requerido("GmailApi:ClientSecret");
        string refreshToken = Requerido("GmailApi:RefreshToken");

        using var formulario = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        using var tokenResponse = await _http.PostAsync(
            "https://oauth2.googleapis.com/token", formulario, cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            // Nunca incluir credenciales ni tokens en los logs.
            throw new InvalidOperationException(
                $"No se pudo renovar la autorización de Gmail (HTTP {(int)tokenResponse.StatusCode}).");
        }

        await using var tokenStream = await tokenResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var tokenJson = await JsonDocument.ParseAsync(tokenStream, cancellationToken: cancellationToken);

        string? accessToken = tokenJson.RootElement.GetProperty("access_token").GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("Google no devolvió un access token.");

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress("Yampi BarberShop", remitente));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;

        // Version de texto y version HTML dentro de un mismo mensaje MIME.
        mensaje.Body = string.IsNullOrWhiteSpace(contenidoHtml)
            ? new TextPart("plain") { Text = contenido }
            : new BodyBuilder
            {
                TextBody = contenido,
                HtmlBody = contenidoHtml
            }.ToMessageBody();

        using var memoria = new MemoryStream();
        await mensaje.WriteToAsync(memoria, cancellationToken);

        string raw = Convert.ToBase64String(memoria.ToArray())
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        using var solicitud = new HttpRequestMessage(
            HttpMethod.Post,
            "https://gmail.googleapis.com/gmail/v1/users/me/messages/send")
        {
            Content = JsonContent.Create(new { raw })
        };
        solicitud.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var respuesta = await _http.SendAsync(solicitud, cancellationToken);
        if (!respuesta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Gmail API rechazó el envío (HTTP {(int)respuesta.StatusCode}).");
        }
    }

    private string Requerido(string nombre)
    {
        string? valor = _config[nombre];
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException($"Falta configurar la variable {nombre}.");
        return valor;
    }
}
