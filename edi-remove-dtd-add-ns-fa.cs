using System.IO;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bidone.Function
{
    public static class edi_remove_dtd_add_ns_fa
    {
        [FunctionName("edi-remove-dtd-add-ns-fa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["namespace"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //remove DOCTYPE
            Regex doctypePattern = new Regex("<!DOCTYPE.+?>");
            requestBody =doctypePattern.Replace(requestBody, string.Empty);
            //Add Namespace
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;
            StringBuilder sb = new StringBuilder();
            NameTable nt = new NameTable();
            XmlNamespaceManager nsmanager = new XmlNamespaceManager(nt);
            nsmanager.AddNamespace(string.Empty, name);
            XmlParserContext context = new XmlParserContext(nt, nsmanager, "cXML", null, null, null, null, null, XmlSpace.None);;

            using (XmlReader reader = XmlReader.Create(new StringReader(requestBody),settings, context))
            {
                while (await reader.ReadAsync())
                {
                    sb.AppendLine(reader.ReadOuterXml());
                }
            }

            return name != null
                ? (ActionResult)new OkObjectResult(sb.ToString())
                : new BadRequestObjectResult("Please pass a namespace on the query string or in the request body");
        }

    }
}
