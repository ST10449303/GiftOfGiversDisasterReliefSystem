using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GiftOfGiversFunctions
{
    public class DonationTaxCertificateFunction
    {
        private readonly ILogger<DonationTaxCertificateFunction> _logger;

        public DonationTaxCertificateFunction(
            ILogger<DonationTaxCertificateFunction> logger)
        {
            _logger = logger;
        }

        [Function("GenerateTaxCertificate")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestData req)
        {
            _logger.LogInformation(
                "Tax certificate generation function was triggered.");

            var query = ParseQuery(req.Url.Query);

            string donorName = query.ContainsKey("donorName")
                ? query["donorName"]
                : "Anonymous Donor";

            string amount = query.ContainsKey("amount")
                ? query["amount"]
                : "0.00";

            string currency = query.ContainsKey("currency")
                ? query["currency"]
                : "ZAR";

            string certificateNumber =
                "GOG-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                Message = "Dummy tax certificate generated successfully.",
                CertificateNumber = certificateNumber,
                DonorName = donorName,
                DonationAmount = amount,
                Currency = currency,
                GeneratedDate = DateTime.Now.ToString("dd MMMM yyyy, HH:mm")
            });

            return response;
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(query))
                return result;

            query = query.TrimStart('?');

            foreach (var pair in query.Split('&'))
            {
                var parts = pair.Split('=', 2);

                if (parts.Length == 2)
                {
                    string key = Uri.UnescapeDataString(parts[0]);
                    string value = Uri.UnescapeDataString(parts[1]);

                    result[key] = value;
                }
            }

            return result;
        }
    }
}