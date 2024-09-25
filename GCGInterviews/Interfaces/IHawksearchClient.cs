using GCGInterviews.Models.HawkSearch;
using GCGInterviews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Interfaces
{
    public interface IHawksearchClient
    {
        HttpResponseMessage CreateIndex(List<FieldDefinition> fieldDefinitions, string suffix = "");
        HttpResponseMessage RemoveDocuments(string indexName, List<string> identities);

        List<HttpResponseMessage> AddDocuments(string indexName, List<SubmitDocument> documents);

        List<HttpResponseMessage> CreateFields(List<FieldDefinition> fieldDefinitions);

        HttpResponseMessage RebuildAll(string name);

        List<HttpResponseMessage> CreateFacets(List<FieldDefinition> fieldDefinitions);
    }
}
