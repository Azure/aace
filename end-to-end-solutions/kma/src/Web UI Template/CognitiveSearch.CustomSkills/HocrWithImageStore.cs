using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleSkills
{
    public class HocrWithImageStore
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

        #region Hocr Datatypes
        // Uses HOCR format for representing the document metadata.
        // See https://en.wikipedia.org/wiki/HOCR
        public class HocrDocument
        {
            private readonly string header = @"
            <?xml version='1.0' encoding='UTF-8'?>
            <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
            <html xmlns='http://www.w3.org/1999/xhtml' xml:lang='en' lang='en'>
            <head>
                <title></title>
                <meta http-equiv='Content-Type' content='text/html;charset=utf-8' />
                <meta name='ocr-system' content='Microsoft Cognitive Services' />
                <meta name='ocr-capabilities' content='ocr_page ocr_carea ocr_par ocr_line ocrx_word'/>
            </head>
            <body>";
            private readonly string footer = "</body></html>";

            public HocrDocument(IEnumerable<HocrPage> pages)
            {
                Metadata = header + Environment.NewLine + string.Join(Environment.NewLine, pages.Select(p => p.Metadata)) + Environment.NewLine + footer;
                Text = string.Join(Environment.NewLine, pages.Select(p => p.Text));
            }

            public string Metadata { get; set; }

            public string Text { get; set; }
        }
        public class OcrLayoutText
        {
            public string Language { get; set; }
            public string Text { get; set; }
            public List<NormalizedLine> Lines { get; set; }
            public List<NormalizedWord> Words { get; set; }
        }

        public class NormalizedLine
        {
            public List<Point> BoundingBox { get; set; }
            public string Text { get; set; }
        }

        public class NormalizedWord
        {
            public List<Point> BoundingBox { get; set; }
            public string Text { get; set; }
        }

        public class Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }
        }
        public class OcrImageMetadata
        {
            public OcrLayoutText HandwrittenLayoutText { get; set; }
            public OcrLayoutText LayoutText { get; set; }
            public string ImageStoreUri { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
        #endregion

        public class HocrPage
        {
            public string Metadata { get { return metadata.ToString(); } }
            public string Text { get { return text.ToString(); } }

            StringWriter metadata = new StringWriter();
            StringWriter text = new StringWriter() { NewLine = " " };

            public HocrPage(OcrImageMetadata imageMetadata, int pageNumber, Dictionary<string, string> wordAnnotations = null)
            {
                // page
                metadata.WriteLine($"<div class='ocr_page' id='page_{pageNumber}' title='image \"{imageMetadata.ImageStoreUri}\"; bbox 0 0 {imageMetadata.Width} {imageMetadata.Height}; ppageno {pageNumber}'>");
                metadata.WriteLine($"<div class='ocr_carea' id='block_{pageNumber}_1'>");

                IEnumerable<IEnumerable<NormalizedWord>> wordGroups;
                if (imageMetadata.HandwrittenLayoutText != null && imageMetadata.LayoutText != null)
                {
                    if (imageMetadata.HandwrittenLayoutText.Text.Length > imageMetadata.LayoutText.Text.Length)
                    {
                        wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.HandwrittenLayoutText.Lines, imageMetadata.HandwrittenLayoutText.Words);
                    }
                    else
                    {
                        wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.LayoutText.Lines, imageMetadata.LayoutText.Words);
                    }
                }
                else if (imageMetadata.HandwrittenLayoutText != null)
                {
                    wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.HandwrittenLayoutText.Lines, imageMetadata.HandwrittenLayoutText.Words);
                }
                else
                {
                    wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.LayoutText.Lines, imageMetadata.LayoutText.Words);
                }

                int li = 0;
                int wi = 0;
                foreach (IEnumerable<NormalizedWord> words in wordGroups)
                {
                    metadata.WriteLine($"<span class='ocr_line' id='line_{pageNumber}_{li}' title='baseline -0.002 -5; x_size 30; x_descenders 6; x_ascenders 6'>");

                    foreach (NormalizedWord word in words)
                    {
                        string annotation = "";
                        if (wordAnnotations != null && wordAnnotations.TryGetValue(word.Text, out string wordAnnotation))
                        {
                            annotation = $"data-annotation='{wordAnnotation}'";
                        }
                        string bbox = word.BoundingBox != null && word.BoundingBox.Count == 4 ? $"bbox {word.BoundingBox[0].X} {word.BoundingBox[0].Y} {word.BoundingBox[2].X} {word.BoundingBox[2].Y}" : "";
                        metadata.WriteLine($"<span class='ocrx_word' id='word_{pageNumber}_{li}_{wi}' title='{bbox}' {annotation}>{word.Text}</span>");
                        text.WriteLine(word.Text);
                        wi++;
                    }
                    li++;
                    metadata.WriteLine("</span>"); // Line
                }
                metadata.WriteLine("</div>"); // Reading area
                metadata.WriteLine("</div>"); // Page
            }
            private IEnumerable<IEnumerable<NormalizedWord>> BuildOrderedWordGroupsFromBoundingBoxes(List<NormalizedLine> lines, List<NormalizedWord> words)
            {
                List<LineWordGroup> lineGroups = new List<LineWordGroup>();
                foreach (NormalizedLine line in lines)
                {
                    LineWordGroup currGroup = new LineWordGroup(line);
                    foreach (NormalizedWord word in words)
                    {
                        if (CheckIntersection(line.BoundingBox, word.BoundingBox) && line.Text.Contains(word.Text))
                        {
                            currGroup.Words.Add(word);
                        }
                    }
                    lineGroups.Add(currGroup);
                }
                return lineGroups.OrderBy(grp => grp.Line.BoundingBox.Select(p => p.Y).Max()).Select(grp => grp.Words.FirstOrDefault()?.BoundingBox == null ? grp.Words.ToArray() : grp.Words.OrderBy(l => l.BoundingBox[0].X).ToArray());
            }

            private bool CheckIntersection(List<Point> line, List<Point> word)
            {
                int lineLeft = line.Select(pt => pt.X).Min();
                int lineTop = line.Select(pt => pt.Y).Min();
                int lineRight = line.Select(pt => pt.X).Max();
                int lineBottom = line.Select(pt => pt.Y).Max();

                int wordLeft = word.Select(pt => pt.X).Min();
                int wordTop = word.Select(pt => pt.Y).Min();
                int wordRight = word.Select(pt => pt.X).Max();
                int wordBottom = word.Select(pt => pt.Y).Max();

                return !(wordLeft > lineRight
                    || wordRight < lineLeft
                    || wordTop > lineBottom
                    || wordBottom < lineTop);
            }
            private class LineWordGroup
            {
                public NormalizedLine Line;
                public List<NormalizedWord> Words;

                public LineWordGroup(NormalizedLine line)
                {
                    Line = line;
                    Words = new List<NormalizedWord>();
                }
            }
        }


        [FunctionName("image-store")]
        public static async Task<IActionResult> RunImageStore(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log, ExecutionContext executionContext)
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

            string blobStorageConnectionString = Environment.GetEnvironmentVariable("BlobStorageAccountConnectionString", EnvironmentVariableTarget.Process);
            string blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName", EnvironmentVariableTarget.Process);
            if (String.IsNullOrEmpty(blobStorageConnectionString) || String.IsNullOrEmpty(blobContainerName))
            {
                return new BadRequestObjectResult($"{skillName} - Information for the blob storage account is missing");
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var libraryContainer = blobClient.GetContainerReference(blobContainerName);

            // Calculate the response for each value.
            var response = new WebApiResponse();

            foreach (var record in data.Values)
            {
                if (record == null || record.RecordId == null) continue;

                OutputRecord responseRecord = new OutputRecord();
                responseRecord.RecordId = record.RecordId;

                try
                {
                    var blockBlob = libraryContainer.GetBlockBlobReference(Guid.NewGuid().ToString());
                    if (!await blockBlob.ExistsAsync())
                    {
                        using (var stream = new MemoryStream(Convert.FromBase64String(record.Data["imageData"].ToString())))
                        {
                            await blockBlob.UploadFromStreamAsync(stream);

                            blockBlob.Properties.ContentType = "image/jpg";
                            await blockBlob.SetPropertiesAsync();
                        }
                    }

                    log.LogInformation($"{skillName}: Saving image to {blockBlob.Uri.ToString()}.");

                    responseRecord.Data["imageStoreUri"] = blockBlob.Uri.ToString();

                }
                catch (Exception e)
                {
                    log.LogInformation($"{skillName}: Error {e.Message}.");

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

        [FunctionName("hocr-generator")]
        public static IActionResult RunHocrGenerator([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log, ExecutionContext executionContext)
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
                    log.LogInformation($"{skillName}: List was received {record.Data["ocrImageMetadataList"]}.");

                    List<OcrImageMetadata> imageMetadataList = JsonConvert.DeserializeObject<List<OcrImageMetadata>>(JsonConvert.SerializeObject(record.Data["ocrImageMetadataList"]));

                    List<HocrPage> pages = new List<HocrPage>();

                    for (int i = 0; i < imageMetadataList.Count; i++)
                    {
                        pages.Add(new HocrPage(imageMetadataList[i], i));
                    }

                    HocrDocument hocrDocument = new HocrDocument(pages);
                    responseRecord.Data["hocrDocument"] = hocrDocument;
                }
                catch (Exception e)
                {
                    log.LogInformation($"{skillName}: Error {e.Message}.");

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
