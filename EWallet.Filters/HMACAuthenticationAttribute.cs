using EWallet.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Filters;

namespace EWallet.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class HMACAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public bool AllowMultiple { get { return false; } }

        private const string HeaderName = "X-Digest";

        readonly HMACConfiguration hmacConfiguration;

        public HMACAuthenticationAttribute(IOptions<HMACConfiguration> hmacConfiguration)
        {
            this.hmacConfiguration = hmacConfiguration.Value;
        }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            if(context.Request.Content is null)
            {
                return;
            }

            var requestContent = await context.Request.Content.ReadAsStringAsync(cancellationToken);

            var hmac = ComputeHMAC(requestContent, hmacConfiguration.Secret);

            if (context.Request.Headers.Contains(HeaderName))
            {
                var headerValue = context.Request.Headers.GetValues(HeaderName).FirstOrDefault();

                if (hmac.Equals(headerValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            context.ErrorResult = new AuthenticationFailureResult($"Invalid or missing {HeaderName} header", context.Request);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static string ComputeHMAC(string content, string secretKey)
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public class AuthenticationFailureResult : IHttpActionResult
    {
        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
        {
            ReasonPhrase = reasonPhrase;
            Request = request;
        }

        public string ReasonPhrase { get; private set; }

        public HttpRequestMessage Request { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            response.RequestMessage = Request;
            response.ReasonPhrase = ReasonPhrase;
            return response;
        }
    }
}