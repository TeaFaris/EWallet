using EWallet.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace EWallet.Middlewares
{
    public class HMACAuthenticationMiddleware : IMiddleware
    {
        private const string DigestHeader = "X-Digest";
        private const string UserIdHeader = "X-UserId";

        readonly HMACConfiguration hmacConfiguration;

        public HMACAuthenticationMiddleware(IOptions<HMACConfiguration> hmacConfiguration)
        {
            this.hmacConfiguration = hmacConfiguration.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.Request.Headers.TryGetValue(DigestHeader, out var headerValue))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var unauthorizedResponse = new UnauthorizedResponse { Message = $"Digest in {DigestHeader} header is not provided." };
                await context.Response.WriteAsync(JsonSerializer.Serialize(unauthorizedResponse));
                return;
            }

            if (!context.Request.Headers.TryGetValue(UserIdHeader, out var userIdHeaderValue) || string.IsNullOrEmpty(userIdHeaderValue))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var unauthorizedResponse = new UnauthorizedResponse { Message = $"User id in {UserIdHeader} header is not provided." };
                await context.Response.WriteAsync(JsonSerializer.Serialize(unauthorizedResponse));
                return;
            }

            var requestContent = await GetRequestBodyAsync(context);

            var hmac = ComputeHMAC(requestContent, hmacConfiguration.Secret);

            if (!string.IsNullOrEmpty(requestContent) && !hmac.Equals(headerValue, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var unauthorizedResponse = new UnauthorizedResponse { Message = $"Digest in {DigestHeader} header is not valid." };
                await context.Response.WriteAsync(JsonSerializer.Serialize(unauthorizedResponse));
                return;
            }

            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userIdHeaderValue)
            }, "HMAC");

            context.User = new ClaimsPrincipal(identity);
            await next(context);
        }

        private static async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            var bodyStream = new StreamReader(context.Request.Body);
            var bodyText = await bodyStream.ReadToEndAsync();
            return bodyText;
        }

        private static string ComputeHMAC(string content, string secretKey)
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}