using GCGInterviews.Interfaces;
using GCGInterviews.Models;
using GCGInterviews.Models.HawkSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews
{
    public class HawksearchClient : IHawksearchClient
    {
        string _baseFieldUrl;
        string _baseIndexingUrl;
        string _baseSearchUrl;
        string _autocompleteUrl;
        string _clientId;
        string _apiKey;
        HawkSearchLogHelper _logger;
        double _documentSizeLimitInMB;
        double _batchSizeLimitInMB;
        int _maxBatchCapacity;

        public HawksearchClient(string baseFieldUrl, string baseIndexingUrl, string baseSearchUrl, string autocompleteUrl, string clientId, string apiKey, HawkSearchLogHelper logger, double documentSizeLimitInMB = 4, double batchSizeLimitInMB = 4, int maxBatchCapacity = 125)
        {
            _baseFieldUrl = baseFieldUrl;
            _baseIndexingUrl = baseIndexingUrl;
            _baseSearchUrl = baseSearchUrl;
            _clientId = clientId;
            _apiKey = apiKey;
            _logger = logger;
            _maxBatchCapacity = maxBatchCapacity;
            _documentSizeLimitInMB= documentSizeLimitInMB;
            _batchSizeLimitInMB = batchSizeLimitInMB;
        }

        public HttpResponseMessage CreateIndex(List<FieldDefinition> fieldDefinitions, string suffix = "")
        {
            // Simplified for test purposes
            return new HttpResponseMessage();
        }
        public HttpResponseMessage RemoveDocuments(string indexName, List<string> identities)
        {
            // Simplified for test purposes
            return new HttpResponseMessage();
        }

        public List<HttpResponseMessage> AddDocuments(string indexName, List<SubmitDocument> documents)
        {
            // Simplified for test purposes
            return new List<HttpResponseMessage> { new HttpResponseMessage() };
        }

        public List<HttpResponseMessage> CreateFields(List<FieldDefinition> fieldDefinitions)
        {
            // Simplified for test purposes
            return new List<HttpResponseMessage> { new HttpResponseMessage() };
        }

        public HttpResponseMessage RebuildAll(string name)
        {
            // Simplified for test purposes
            return new HttpResponseMessage();
        }

        public List<HttpResponseMessage> CreateFacets(List<FieldDefinition> fieldDefinitions)
        {
            // Simplified for test purposes
            return new List<HttpResponseMessage> { new HttpResponseMessage() };
        }
    }
}
