using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CognitiveSearch.Azure.AppInsights;
using CognitiveSearch.Azure.Search;
using CognitiveSearch.Azure.Storage.Blobs;
using CognitiveSearch.WebApi.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CognitiveSearch.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FacetsFilteringController : ControllerBase
    {
        private readonly BlobStorageConfig _storageConfig;

        public FacetsFilteringController(BlobStorageConfig storageConfig)
        {
            this._storageConfig = storageConfig;
        }

        private readonly string insertNodeQuery = @"MERGE Facet as target
                        USING (select @name, @type) AS T (name, type)
                        ON (T.name = target.name AND T.type = target.type)
                        WHEN MATCHED THEN
                        UPDATE SET target.weight = target.weight+1
                        WHEN NOT MATCHED THEN
                        INSERT VALUES (T.name, 1, T.type);";

        private readonly string insertEdgeQuery = @"MERGE RelatedTo as target
                        USING ((select @source, @target) AS T (sourceName, targetName)
                        JOIN facet as facet1 ON T.sourceName = facet1.name
                        JOIN facet as facet2 ON T.targetName = facet2.name)
                        ON MATCH(facet1-(target)->facet2)
                        WHEN MATCHED THEN
	                        UPDATE SET target.weight = target.weight+1
                        WHEN NOT MATCHED THEN
	                        INSERT VALUES(facet1.$node_id, facet2.$node_id, 1);";

        private void UpdateGraph(List<string> list, string facetName)
        {

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Server=tcp:xiwuoil2-sql.database.windows.net,1433;Initial Catalog=facetsGraph;Persist Security Info=False;User ID=cloudsa;Password=Yukon900Yukon900;";
                conn.Open();

                for (int i = 0; i < list.Count; i++)
                {
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = insertNodeQuery;
                        command.Parameters.AddWithValue("@name", list[i]);
                        command.Parameters.AddWithValue("@type", facetName);
                        command.ExecuteNonQuery();
                    }
                    for (int j = 0; j <= i; j++)
                    {
                        using (SqlCommand command = conn.CreateCommand())
                        {
                            command.CommandText = insertEdgeQuery;
                            command.Parameters.AddWithValue("@source", list[i]);
                            command.Parameters.AddWithValue("@target", list[j]);
                            command.ExecuteNonQuery();
                        }
                        if (j != i)
                        {
                            using (SqlCommand command = conn.CreateCommand())
                            {
                                command.CommandText = insertEdgeQuery;
                                command.Parameters.AddWithValue("@source", list[j]);
                                command.Parameters.AddWithValue("@target", list[i]);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> FilterFacets(string facetname, FacetsFilteringRequest searchRequest)
        {
            // Calculate the response for each value.
            var response = new FacetsFilteringResponse();
            response.Values = new List<FacetRecord>();

            string[] commonList = new string[] { };
            string[] list = new string[] { };
            try
            {
                BlobStorageConfig config = _storageConfig.Copy();
                config.ContainerName = config.FacetsFilteringContainerName;
                string s = await BlobStorageClient.ReadBlobAsync(config, "commonfilters.txt");
                commonList = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                s = await BlobStorageClient.ReadBlobAsync(config, string.Format("{0}.txt", facetname));
                list = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception)
            {

            }
            Trace.TraceInformation("facet: " + facetname);
            Trace.TraceInformation("number of values:" + searchRequest.Values.Count);
            foreach (var record in searchRequest.Values)
            {
                if (record == null || record.RecordId == null) continue;

                FacetRecord responseRecord = new FacetRecord();
                responseRecord.RecordId = record.RecordId;

                try
                {
                    List<string> restrictionList = new List<string>(commonList);
                    restrictionList.AddRange(list);

                    var outputRecord = new FacetData();
                    outputRecord.Facets = new List<string>();
                    Trace.TraceInformation("number of facets:" + record.Data.Facets.Count);

                    // replace all non-alphbic characters before comparision
                    Regex rgx = new Regex("[^a-zA-Z0-9 ]");

                    for (int i = 0; i < restrictionList.Count; i++)
                    {
                        restrictionList[i] = rgx.Replace(restrictionList[i], "").ToLower();
                    }

                    foreach (string phrase in record.Data.Facets)
                    {
                        var str = rgx.Replace(phrase, "").ToLower();
                        if (!string.IsNullOrEmpty(str))
                        {
                            //lower case the first letter
                            //str = Char.ToLower(str[0]) + str.Substring(1);

                            if (!restrictionList.Contains(str))
                            {
                                outputRecord.Facets.Add(phrase);
                            }
                        }
                    }

                    //UpdateGraph(outputRecord.Facets, facetname);

                    responseRecord.Data = outputRecord;
                }
                catch (Exception e)
                {
                }
                finally
                {
                    response.Values.Add(responseRecord);
                }
            }


            return new OkObjectResult(response);
        }
    }
}