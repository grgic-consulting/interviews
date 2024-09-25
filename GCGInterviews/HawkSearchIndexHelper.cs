using GCGInterviews;
using GCGInterviews.Interfaces;
using GCGInterviews.Models;
using GCGInterviews.Models.HawkSearch;
using GCGInterviews.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using static GCGInterviews.ZnodeDependencyResolver;

namespace Znode.Engine.ERPConnector
{
    public class HawkSearchIndexHelper
    {
        const string schedulerName = "ReindexHawkSearchProducts";

        private IHawksearchClient _hawksearchClient;
        private Dictionary<string, string> _attributeCodesMapping;
        private string _indexName;
        private Dictionary<string, string> _facets;
        private PriceTierListModel _lstTierPrices;
        private PriceSKUListModel _lstProductPrices;
        private int PortalId;
        private List<PimProdcutDetails> _allPimProductsDetails;
        private PublishProductListModel _allPublishedProducts;
        private HawkSearchLogHelper logHelper;
        private IZnodeRepository<ZnodePriceListPortal> _priceListPortalRepository;

        public HawkSearchIndexHelper()
        {
        }

        public virtual bool ReindexProducts()
        {
            try
            {
                logHelper = new HawkSearchLogHelper(isActive: true);

                var eRPTaskSchedulerCache = new ERPTaskSchedulerService();
                int erpSchedulerId = eRPTaskSchedulerCache.GetSchedulerIdByTouchPointName(schedulerName, 0, null);

                logHelper.StartImportLog(erpSchedulerId);

                InitializeHSClient();

                //List of all PIM products
                _allPimProductsDetails = GetZnodePimProductList();

                //List of published products
                _allPublishedProducts = GetZnodePublishedProductList();

                if (_allPimProductsDetails?.Count > 0 && _allPublishedProducts?.PublishProducts?.Count > 0)
                {
                    var priceService = GetService<ICustomPriceService>();
                    //Get all price data from ZnodePrice
                    _lstProductPrices = priceService.GetAllProductPriceData(expands: new System.Collections.Specialized.NameValueCollection(), filters: new FilterCollection(),
                     sorts: new System.Collections.Specialized.NameValueCollection(), page: new System.Collections.Specialized.NameValueCollection());

                    //Get Price Tier From ZnodePriceTier
                    _lstTierPrices = priceService.GetAllTierPriceData(expands: new System.Collections.Specialized.NameValueCollection(), filters: new FilterCollection(),
                     sorts: new System.Collections.Specialized.NameValueCollection(), page: new System.Collections.Specialized.NameValueCollection());

                    var (toSubmit, toDelete) = BuildListsToCreateAndDelete();

                    //Rebuild HS index
                    RebuildIndex(toSubmit, toDelete);
                }

                return true;
            }
            catch (Exception ex)
            {
                logHelper.FinishImportLog(false);
                return false;
            }
        }

        private void InitializeHSClient()
        {
            var globalAttributeEntityService = GetService<IGlobalAttributeGroupEntityService>();
            GlobalAttributeEntityDetailsModel globalAttributes = globalAttributeEntityService.GetEntityAttributeDetails(1, "Store");
            var baseFieldUrl = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchBaseAPI"))?.AttributeValue;
            var baseIndexingUrl = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchIndexApi"))?.AttributeValue;
            var baseSearchUrl = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchSearchingAPI"))?.AttributeValue;
            var autocompleteUrl = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchAutocompleteUrl"))?.AttributeValue;
            var apiKey = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchApiKey"))?.AttributeValue;
            var clientId = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchClientId"))?.AttributeValue;
            _indexName = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchIndexName"))?.AttributeValue;
            _hawksearchClient = new HawksearchClient(baseFieldUrl, baseIndexingUrl, baseSearchUrl, autocompleteUrl, clientId, apiKey, logHelper);


            bool isError = false;
            string errorMessage = string.Empty;
            var productAttributeCode = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchProductAttributeCode"))?.AttributeValue;
            try
            {
                _attributeCodesMapping = GetAttributeCodeMapping(productAttributeCode);
            }
            catch (Exception ex)
            {
                logHelper.UpdateImportLog("Error", 93, "Product Attributes Error (possible duplicate): " + ex.Message);
                isError = true;
                errorMessage = ex.Message;
            }


            var productFacets = globalAttributes.Attributes.FirstOrDefault(x => x.AttributeCode.StartsWith("HawkSearchFacets"))?.AttributeValue;

            try
            {
                _facets = GetAttributeCodeMapping(productFacets);
            }
            catch (Exception ex)
            {
                logHelper.UpdateImportLog("Error", 93, "Facet Error (possible duplicate): " + ex.Message);
                isError = true;
                errorMessage = ex.Message;
            }

            if (isError)
            {
                logHelper.FinishImportLog(false);
                throw new Exception(errorMessage);
            }

            IZnodeRepository<ZnodePortal> _portal = new ZnodeRepository<ZnodePortal>();
            PortalId = _portal.Table.FirstOrDefault().PortalId;
        }

        private List<PimProdcutDetails> GetZnodePimProductList()
        {
            var productService = GetService<IProductService>();
            ProductDetailsListModel productDetails = productService.GetProductList(expands: new NameValueCollection(), filters: new FilterCollection(),
                sorts: new NameValueCollection(), page: new NameValueCollection());
            var serializePimData = JsonConvert.SerializeObject(productDetails.XmlDataList);
            return JsonConvert.DeserializeObject<List<PimProdcutDetails>>(serializePimData);
        }

        private PublishProductListModel GetZnodePublishedProductList()
        {
            var publishProductService = GetService<IPublishProductService>();
            var expands = new NameValueCollection
            {
                { "seo", null }
            };
            return publishProductService.GetPublishProductList(expands,
                GetRequiredFilters(), sorts: new NameValueCollection(), page: new NameValueCollection());
        }

        private (List<SubmitDocument> toSubmit, List<string> toDelete) BuildListsToCreateAndDelete()
        {
            List<SubmitDocument> lstSubmitDocs = new List<SubmitDocument>();
            var toDelete = new List<string>();
            var fieldDefinitions = new List<FieldDefinition>();

            foreach (PublishProductModel publishProduct in _allPublishedProducts.PublishProducts)
            {
                try
                {
                    if (!publishProduct.IsActive)
                    {
                        var pimProductId = _allPimProductsDetails.FirstOrDefault(x => x.SKU.Equals(publishProduct.SKU, StringComparison.OrdinalIgnoreCase))?.PimProductId;
                        if (!string.IsNullOrEmpty(pimProductId) && int.TryParse(pimProductId, out var idInt) && idInt > 0)
                        {
                            toDelete.Add(pimProductId.ToString());
                        }
                        continue;
                    }
                    fieldDefinitions = new List<FieldDefinition>();
                    SubmitDocument submitDoc = GetProductFields(publishProduct, ref fieldDefinitions);
                    lstSubmitDocs.Add(submitDoc);
                }
                catch (Exception ex)
                {
                }
            }
            return (lstSubmitDocs, toDelete);
        }

        private void RebuildIndex(List<SubmitDocument> documentsToSubmit, List<string> documentIdsToDelete)
        {

            bool isSuccess = true;
            string errorMessage = string.Empty;
            try
            {
                var removedFromIndexResult = _hawksearchClient.RemoveDocuments(_indexName, documentIdsToDelete);
                var removedResultString = removedFromIndexResult.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(removedResultString))
                {
                    logHelper.UpdateImportLog("Remove inactive documents", 93, GetRemovedResultsForLog(removedResultString));
                }

                var fieldDefinitions = new List<FieldDefinition>();
                SubmitDocument submitDoc = GetProductFields(_allPublishedProducts.PublishProducts.FirstOrDefault(), ref fieldDefinitions);

                CreateHsIndexAndFacets(fieldDefinitions);

                var responseIndexing = _hawksearchClient.AddDocuments(_indexName, documentsToSubmit).FirstOrDefault();
                var contentIndexing = responseIndexing.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(contentIndexing))
                {
                    AddDocumentsResponse addDocumentsResponse = JsonConvert.DeserializeObject<AddDocumentsResponse>(contentIndexing);
                    string logEntry = string.Format("Status: {0}, Total: {1}, Succeeded: {2}, Failed: {3}, Warnings: {4}", addDocumentsResponse.Status, addDocumentsResponse.Summary.Total, addDocumentsResponse.Summary.Succeeded, addDocumentsResponse.Summary.Failed, addDocumentsResponse.Summary.Warnings);
                    logHelper.UpdateImportLog("Add Documents", 93, logEntry);
                }
                if (!responseIndexing.IsSuccessStatusCode)
                {
                    isSuccess = false;
                    errorMessage = "Product Index Error: " + contentIndexing;
                }

                //Rebuild the index , this should be the last step
                var responseRebuildIndex = _hawksearchClient.RebuildAll(_indexName);
                var rebuildIndexResponseContent = responseRebuildIndex.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(rebuildIndexResponseContent))
                {
                    RebuildIndexResponseModel rebuildIndexResponseModel = JsonConvert.DeserializeObject<RebuildIndexResponseModel>(rebuildIndexResponseContent);
                    foreach (IndexingResponse responses in rebuildIndexResponseModel.IndexingResponses)
                    {
                        logHelper.UpdateImportLog("Rebuild Index", 93, responses.Message);
                    }
                }
                if (!responseRebuildIndex.IsSuccessStatusCode)
                {
                    isSuccess = false;
                    errorMessage = "Rebuild Index Error: " + rebuildIndexResponseContent;
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                isSuccess = false;
            }

            logHelper.FinishImportLog(isSuccess);
            if (!isSuccess)
            {
                throw new Exception(errorMessage);
            }
        }

        private static string GetRemovedResultsForLog(string resultsStringFromApi)
        {
            var removedResponse = JsonConvert.DeserializeObject<RemoveDocumentsResponse>(resultsStringFromApi);
            var itemsNotFound = removedResponse?.Items?.Where(x => x.Message == "Item not found").ToList();

            if (itemsNotFound?.Count > 0)
            {
                // Hawksearch returns a failure if we try to remove documents that have already been removed.
                // This is not considered a failure in terms of running the Reindex Hawksearch touchpoint
                // There is no known way to identify inactive products that have already been removed from the index to prevent the reported failures.
                removedResponse.Summary.Failed -= itemsNotFound.Count;
                removedResponse.Items = removedResponse.Items.Except(itemsNotFound).ToList();
                if (removedResponse.Summary.Failed == 0)
                {
                    removedResponse.Status = removedResponse.Summary.Warnings > 0 ? "Warning" : "Success";
                }
                return JsonConvert.SerializeObject(removedResponse);
            }

            return resultsStringFromApi;
        }

        private Dictionary<string, string> GetAttributeCodeMapping(string productAttributeCode)
        {
            var attributeCodes = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(productAttributeCode))
                return attributeCodes;

            foreach (var strAttributeCode in productAttributeCode.Split(new char[] { '|' }))
            {
                var codeMapping = strAttributeCode.Split(new char[] { ':' });

                if (codeMapping.Length != 2)
                    continue;

                attributeCodes.Add(codeMapping[0], codeMapping[1]);
            }

            return attributeCodes;
        }

        private SubmitDocument GetProductFields(PublishProductModel publishedProduct, ref List<FieldDefinition> fieldDefinitions)
        {
            SubmitDocument submitDoc = new SubmitDocument();
            PimProdcutDetails product = _allPimProductsDetails.Where(x => x.SKU.Equals(publishedProduct.SKU, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            List<PriceSKUModel> productPriceLists = _lstProductPrices?.PriceSKUList.Where(x => x.SKU.Equals(publishedProduct.SKU, StringComparison.OrdinalIgnoreCase)).ToList();

            _priceListPortalRepository = new ZnodeRepository<ZnodePriceListPortal>();
            IZnodeRepository<ZnodePortal> _portal = new ZnodeRepository<ZnodePortal>();
            PortalId = _portal.Table.FirstOrDefault().PortalId;

            var storePriceListId = (from s in _priceListPortalRepository.Table
                                    where s.PortalId == PortalId
                                    select s.PriceListId).FirstOrDefault();

            productPriceLists = productPriceLists.Where(x => x.PriceListId == storePriceListId)?.ToList();

            PriceSKUModel pricingDetail = productPriceLists.FirstOrDefault();
            var productUrl = GetProductUrl(publishedProduct);
            publishedProduct.PimProductId = int.Parse(product.PimProductId);
            var publishedProductId = GetFieldValue(publishedProduct.PimProductId.ToString());


            //the fields: OriginalItemId, publishedid, title... should be created already on HS end 
            submitDoc.Fields.Add(new SubmitField { Name = "OriginalItemId", Values = new List<string> { GetFieldValue(publishedProduct.PimProductId.ToString()) } });
            submitDoc.Fields.Add(new SubmitField { Name = "publishedid", Values = new List<string> { GetFieldValue(publishedProduct?.PublishProductId.ToString()) } });
            submitDoc.Fields.Add(new SubmitField { Name = "producttype", Values = new List<string> { GetFieldValue(publishedProduct?.ProductType) } });
            submitDoc.Fields.Add(new SubmitField { Name = "title", Values = new List<string> { GetFieldValue(publishedProduct?.Name) } });
            submitDoc.Fields.Add(new SubmitField { Name = "sku", Values = new List<string> { GetFieldValue(publishedProduct?.SKU) } });
            submitDoc.Fields.Add(new SubmitField { Name = "pageurl", Values = new List<string> { GetFieldValue(productUrl) } });
            submitDoc.Fields.Add(new SubmitField { Name = "tabtypes", Values = new List<string> { "Product" } });

            foreach (var attributeMapping in _attributeCodesMapping)
            {
                var zdAttributeCode = attributeMapping.Value;
                var attributeValue = new List<string>();

                if (string.Equals(zdAttributeCode, "RetailPrice", StringComparison.OrdinalIgnoreCase))
                {
                    attributeValue = new List<string> { GetFieldValue(pricingDetail?.RetailPrice?.ToString()) };
                    fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "Single" });
                }
                else if (string.Equals(zdAttributeCode, "SalesPrice", StringComparison.OrdinalIgnoreCase))
                {
                    attributeValue = new List<string> { GetFieldValue(pricingDetail?.SalesPrice?.ToString()) };
                    fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "Single" });
                }
                else if (string.Equals(zdAttributeCode, "CostPrice", StringComparison.OrdinalIgnoreCase))
                {
                    attributeValue = new List<string> { GetFieldValue(pricingDetail?.CostPrice?.ToString()) };
                    fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "Single" });
                }
                else if (string.Equals(zdAttributeCode, "ProductImage", StringComparison.OrdinalIgnoreCase))
                {
                    attributeValue = new List<string> { GetFieldValue(publishedProduct.ImageSmallPath) };
                }
                else if (string.Equals(zdAttributeCode, "TierPrices", StringComparison.OrdinalIgnoreCase))
                {
                    var tierPrices = new List<PriceTierModel>();
                    var strTierPrices = "";
                    var con = "";

                    if (publishedProduct != null && pricingDetail != null)
                    {
                        tierPrices = _lstTierPrices?.TierPriceList?.Where(x => x.SKU.Equals(publishedProduct.SKU, StringComparison.OrdinalIgnoreCase) && x.PriceListId == pricingDetail.PriceListId)?.ToList();

                        if (tierPrices != null)
                        {
                            foreach (var tierPrice in tierPrices)
                            {
                                strTierPrices += con + tierPrice.Quantity + "_" + tierPrice.Price;
                                con = "|";
                            }
                        }
                    }

                    attributeValue = new List<string> { GetFieldValue(strTierPrices) };
                }
                else if (string.Equals(zdAttributeCode, "Family", StringComparison.OrdinalIgnoreCase))
                {
                    var attributeFamilyName = product.AttributeFamily;
                    attributeValue = new List<string> { GetFieldValue(attributeFamilyName) };
                    fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "String" });
                }
                else if (string.Equals(zdAttributeCode, "CategoryName", StringComparison.OrdinalIgnoreCase))
                {
                    attributeValue = new List<string> { GetFieldValue(publishedProduct.CategoryName) };
                    fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "String" });
                }
                else if (string.Equals(zdAttributeCode, "Category", StringComparison.OrdinalIgnoreCase))
                {
                    var categoryService = GetService<ICategoryService>();
                    var associatedCategories = categoryService.GetAssociatedCategoriesToProducts(Convert.ToInt32(publishedProductId),
                                                                                                associatedProducts: false, expands: new NameValueCollection(),
                                                                                                filters: new FilterCollection(), sorts: new NameValueCollection(),
                                                                                                page: new NameValueCollection())?.CategoryProducts;
                    if (associatedCategories?.Count > 0)
                    {
                        var productCategories = associatedCategories.Select(x => x.Categoryid.ToString()).ToList();
                        var associatedCategoriesIds = new List<string>();

                        foreach (string categoryId in productCategories)
                        {
                            var cid = '"' + categoryId + '"';
                            associatedCategoriesIds.Add(cid);
                        }

                        attributeValue = new List<string> { string.Join(",", associatedCategoriesIds) };
                    }

                    fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "String" });
                }
                else
                {
                    var thisAttribute = publishedProduct?.Attributes?.FirstOrDefault(x => x.AttributeCode == zdAttributeCode);
                    if (thisAttribute != null)
                    {
                        switch (thisAttribute.AttributeTypeName)
                        {
                            case "SimpleSelect":
                                attributeValue = new List<string> { GetFieldValue(thisAttribute.SelectValues[0].Value) };
                                fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "String" });
                                break;
                            case "MultiSelect":
                                attributeValue = (List<string>)thisAttribute.SelectValues.Select(o => o.Value).ToList();
                                fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "String" });
                                break;
                            default:
                                attributeValue = new List<string> { GetFieldValue(publishedProduct?.Attributes?.FirstOrDefault(x => x.AttributeCode == zdAttributeCode)?.AttributeValues) };
                                fieldDefinitions.Add(new FieldDefinition { Name = attributeMapping.Key, Type = "String" });
                                break;
                        }
                    }
                }

                submitDoc.Fields.Add(new AttributeField { Name = attributeMapping.Key, Values = attributeValue });
            }


            productPriceLists.ForEach(list => {
                submitDoc.Fields.Add(new AttributeField { Name = "retail_price_" + list.ListName, Values = new List<string> { list.RetailPrice.ToString() } });
                submitDoc.Fields.Add(new AttributeField { Name = "sale_price_" + list.ListName, Values = new List<string> { list.SalesPrice.ToString() } });
            });
            //Hard coded value for task TWCZNODE-405
            submitDoc.Fields.Add(new AttributeField { Name = "retail_price_resl", Values = new List<string> { "100" } });
            submitDoc.Fields.Add(new AttributeField { Name = "sale_price_resl", Values = new List<string> { "90" } });
            submitDoc.Fields.Add(new AttributeField { Name = "retail_price_trad", Values = new List<string> { "50" } });
            submitDoc.Fields.Add(new AttributeField { Name = "sale_price_trad", Values = new List<string> { "40" } });
            submitDoc.Fields.Add(new AttributeField { Name = "retail_price_gen", Values = new List<string> { "10" } });
            submitDoc.Fields.Add(new AttributeField { Name = "sale_price_gen", Values = new List<string> { "5" } });

            return submitDoc;
        }

        public string GetFieldValue(string value)
        {
            //an error is thrown if the value == null
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value;
        }

        private string GetProductUrl(PublishProductModel publishedProduct)
        {
            if (publishedProduct == null)
                return "";

            var indexBaseZnodeUrl = "some url"; // Simplified for testing;

            if (!string.IsNullOrEmpty(publishedProduct.SEOUrl))
                return Path.Combine(indexBaseZnodeUrl, publishedProduct.SEOUrl.ToLower());

            var productId = publishedProduct.ConfigurableProductId > 0 ? publishedProduct.ConfigurableProductId : publishedProduct.PublishProductId;
            return Path.Combine(indexBaseZnodeUrl, "product", productId.ToString());
        }


        protected FilterCollection GetRequiredFilters()
        {
            FilterCollection filters = new FilterCollection
            {
                { "LocaleId".ToString().ToLower(), "Equals", "1" },
                { "PortalId".ToString().ToLower(), "Equals", PortalId.ToString() },
                { "revisontype".ToLower(), "Equals", "production" }
            };
            return filters;
        }

        private void CreateHsIndexAndFacets(List<FieldDefinition> fieldDefinitions)
        {
            // var fieldDefinitions = new List<FieldDefinition>();
            var fields = _hawksearchClient.CreateFields(fieldDefinitions);

            //create missing facets
            var facetDefinitions = new List<FieldDefinition>();

            foreach (var f in _facets)
            {
                if (fieldDefinitions.Any(s => string.Equals(f.Key, s.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    var type = "System.String";

                    if (f.Value == "Single")
                    {
                        type = "System.Decimal";
                    }

                    facetDefinitions.Add(new FieldDefinition { Name = f.Key, Type = type });
                }
            }

            var facets = _hawksearchClient.CreateFacets(facetDefinitions);
        }
    }
}
