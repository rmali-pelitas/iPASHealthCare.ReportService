using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ReportService.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReportService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ReportController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("Report Service is running");
        }

        [HttpGet]
        [Route("GetPowerBIReport/workspace/{workspaceId}/report/{reportId}")]
        public async Task<IActionResult> GetPowerBIReport(string workspaceId, string reportId)
        {

            var accessToken = GetPowerBIAccessToken(_configuration);
           
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");
            try
            {
                using (var client = new PowerBIClient(new Uri(_configuration["PowerBI:ApiUrl"]), tokenCredentials))
                {

                    var wId = Guid.Parse(workspaceId);
                    var rId = Guid.Parse(reportId);

                    var report = await client.Reports.GetReportInGroupAsync(wId, rId);

                    Dataset ds = client.Datasets.GetDatasetInGroup(wId, report.DatasetId);

                    var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    var tokenResponse = await client.Reports.GenerateTokenAsync(wId, rId, generateTokenRequestParameters);

                    return Ok(new { token = tokenResponse.Token, report = report });
                }
            }
            catch (Exception e)
            {

                throw;
            }

        }


        public static string GetPowerBIAccessToken(IConfiguration _configuration)
        {
            try
            {

                string clientId = _configuration["PowerBI:ApplicationId"];

                string clientSecret = _configuration["PowerBI:ApplicationSecret"];

                //Resource Uri for Power BI API
                string resourceUri = _configuration["PowerBI:ResourceUrl"];

                //OAuth2 authority Uri
                string authorityUri = _configuration["PowerBI:AuthorityUrl"] + _configuration["PowerBI:DirectoryId"];

                //Get access token:
                // To call a Power BI REST operation, create an instance of AuthenticationContext and call AcquireToken
                // AuthenticationContext is part of the Active Directory Authentication Library NuGet package
                // To install the Active Directory Authentication Library NuGet package in Visual Studio,
                //  run "Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory" from the nuget Package Manager Console.

                // AcquireToken will acquire an Azure access token
                // Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint
                AuthenticationContext authContext = new AuthenticationContext(authorityUri);
                ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);

                var token = authContext.AcquireTokenAsync(resourceUri, clientCredential).Result.AccessToken;

                return token;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to fetch Power BI access token, exception details: ", e);
            }
            return "";

        }
    }
}
