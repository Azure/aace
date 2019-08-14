using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SampleSkills
{
    public class DocHeaderExtraction
    {
        #region Class used to deserialize the request
        public class InputRecord
        {
            public string RecordId { get; set; }
            public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        }

        private class WebApiRequest
        {
            public List<InputRecord> Values { get; set; }
        }
        #endregion

        #region Classes used to serialize the response
        public class OutputRecord
        {
            public class OutputRecordMessage
            {
                public string Message { get; set; }
            }

            public string RecordId { get; set; }
            public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
            public List<OutputRecordMessage> Errors { get; set; }
            public List<OutputRecordMessage> Warnings { get; set; }
        }

        private class WebApiResponse
        {
            public WebApiResponse()
            {
                this.values = new List<OutputRecord>();
            }

            public List<OutputRecord> values { get; set; }
        }
        #endregion

        [FunctionName("doc-headers")]
        public static IActionResult RunHeaderExtraction([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log, ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;

            log.LogInformation($"{skillName}: C# HTTP trigger function processed a request.");

            // Read input, deserialize it and validate it.
            var data = GetStructuredInput(req.Body);
            if (data == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema.");
            }
            if (data.Values.Count() != 1)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array: Skill requires exactly 1 image per request.");
            }
            var response = new WebApiResponse();
            foreach (var record in data.Values)
            {
                if (record == null || record.RecordId == null) continue;

                OutputRecord responseRecord = new OutputRecord();
                responseRecord.RecordId = record.RecordId;

                try
                {
                    var fileLocation = string.Format("{0}?{1}", record.Data["path"], record.Data["token"].ToString().StartsWith("?") ? record.Data["token"].ToString().Substring(1) : record.Data["token"]);
                    using (WebClient webConnection = new WebClient())
                    {

                        using (var mStream = new MemoryStream(webConnection.DownloadData(fileLocation)))
                        {
                            if (record.Data["contentType"].ToString() == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                            {
                                using (var doc = WordprocessingDocument.Open(mStream, false))
                                {
                                    var paragraphs = doc.MainDocumentPart.Document.Body
                                     .OfType<Paragraph>()
                                     .Where(p => p.ParagraphProperties != null &&
                                                 p.ParagraphProperties.ParagraphStyleId != null &&
                                                 p.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading")).ToList();

                                    //var allStyles = DocumentFormat.OpenXml.Wordprocessing.
                                    responseRecord.Data["headings"] = paragraphs.Select(a => a.InnerText).ToList();
                                }
                            }
                            responseRecord.Data["headings"] = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogInformation($"{skillName}: Error {e.Message}");

                    // Something bad happened, log the issue.
                    var error = new OutputRecord.OutputRecordMessage
                    {
                        Message = e.Message
                    };

                    responseRecord.Errors = new List<OutputRecord.OutputRecordMessage>
                    {
                        error
                    };
                }
                finally
                {
                    response.values.Add(responseRecord);
                }

            }
            return new OkObjectResult(response);
        }

        private static WebApiRequest GetStructuredInput(Stream requestBody)
        {
            string request = new StreamReader(requestBody).ReadToEnd();

            var data = JsonConvert.DeserializeObject<WebApiRequest>(request);
            if (data == null)
            {
                return null;
            }
            return data;
        }
    }
}
