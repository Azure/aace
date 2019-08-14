using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.Azure.Search
{
    public static class FacetGraphGenerator
    {
        public static async Task<JObject> GetGraphNodes(SearchConfig searchConfig, string q, string facetName)
        {
            // Calculate nodes for 3 levels
            var dataset = new JObject();
            int MaxEdges = 30;
            int MaxLevels = 3;
            int CurrentLevel = 1;
            int CurrentNodes = 0;

            var graphEdges = new List<FDGraphEdge>();

            // If blank search, assume they want to search everything
            if (string.IsNullOrWhiteSpace(q))
            {
                q = "*";
            }
            // If facetName is blank, default to keyPhrases
            if (string.IsNullOrWhiteSpace(facetName))
            {
                facetName = "keyPhrases";
            }

            // Create a node map that will map a facet to a node - nodemap[0] always equals the q term
            var NodeMap = new Dictionary<string, int>
            {
                [q] = CurrentNodes
            };

            var NextLevelTerms = new List<string>
            {
                q
            };

            // Iterate through the nodes up to 3 levels deep to build the nodes or when I hit max number of nodes
            while ((NextLevelTerms.Count() > 0) && (CurrentLevel <= MaxLevels) && (graphEdges.Count() < MaxEdges))
            {
                q = NextLevelTerms.First();
                NextLevelTerms.Remove(q);
                if (NextLevelTerms.Count() == 0)
                {
                    CurrentLevel++;
                }
                var response = await GetFacets(searchConfig, q, facetName, 10);
                if (response != null)
                {
                    IList<FacetResult> facetResults = (response.Facets)[facetName];
                    foreach (var facet in facetResults)
                    {
                        int node = -1;
                        if (NodeMap.TryGetValue(facet.Value.ToString(), out node) == false)
                        {
                            // This is a new node
                            CurrentNodes++;
                            node = CurrentNodes;
                            NodeMap[facet.Value.ToString()] = node;
                        }
                        // Add this facet to the fd list
                        if (NodeMap[q] != NodeMap[facet.Value.ToString()])
                        {
                            graphEdges.Add(new FDGraphEdge { Source = NodeMap[q], Target = NodeMap[facet.Value.ToString()] });
                            if (CurrentLevel < MaxLevels)
                            {
                                NextLevelTerms.Add(facet.Value.ToString());
                            }
                        }
                    }
                }
            }

            // Create nodes
            var nodes = new JArray();
            int nodeNumber = 0;
            foreach (KeyValuePair<string, int> entry in NodeMap)
            {
                nodes.Add(JObject.Parse("{name: \"" + entry.Key.Replace("\"", "") + "\"" + ", id: " + entry.Value + "}"));
                nodeNumber++;
            }

            // Create edges
            var edges = new JArray();
            foreach (var entry in graphEdges)
            {
                edges.Add(JObject.Parse("{source: " + entry.Source + ", target: " + entry.Target + "}"));
            }

            dataset.Add(new JProperty("links", edges));
            dataset.Add(new JProperty("nodes", nodes));

            return await Task.FromResult(dataset);
        }

        private static async Task<DocumentSearchResult<Document>> GetFacets(SearchConfig searchConfig, string searchText, string facetName, int maxCount = 30)
        {
            // Execute search based on query string
            try
            {
                using (var searchClient = new SearchServiceClient(searchConfig.ServiceName, new SearchCredentials(searchConfig.Key)))
                {
                    var sp = new SearchParameters()
                    {
                        SearchMode = SearchMode.Any,
                        Top = 10,
                        Select = new List<string>() { "id" },
                        Facets = new List<string>() { $"{facetName}, count:{maxCount}" },
                        QueryType = QueryType.Full
                    };

                    return await Task.FromResult(searchClient.Indexes.GetClient(searchConfig.IndexName).Documents.Search(searchText, sp));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }
    }
}