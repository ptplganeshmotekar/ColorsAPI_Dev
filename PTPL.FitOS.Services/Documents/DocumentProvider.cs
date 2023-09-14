using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using PTPL.FitOS.DataModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    /// <summary>
    /// This service class is use for document related transaction.
    /// </summary>
    public class DocumentProvider : IDocumentInterface
    {
           
        #region Fields

        /// <summary>
        /// Field declarations
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        #endregion

        #region Constructor

        /// <summary>
        /// This is a service level constructor
        /// </summary> 
        public DocumentProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {         
            _httpClientFactory = httpClientFactory;         
        }
        #endregion Constructor

        #region Methods

        /// <summary>
        /// This service method will add document data and returns success status.
        /// </summary>
        /// <param name="docItem"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> AddDocumentAsync(dynamic docItem, string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(docItem, token, _httpClientFactory, "DocumentAPIs", "Document/AddDocumentItem", "Post");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error") { throw new Exception(Convert.ToString(result.item1)); }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This service method will add document-file data and returns success status.
        /// </summary>
        /// <param name="docFileItem"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> AddDocumentFileAsync(dynamic docFileItem, string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(docFileItem, token, _httpClientFactory, "DocumentAPIs", "Document/AddDocmentFileItem", "Post");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error") { throw new Exception(Convert.ToString(result.item1)); }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This service method will delete document-file relationship data and returns success status.
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="fileId"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> DeleteDocumentFileAsync(string docId, string fileId, string token, CancellationToken ct = default)
        {
            try
            {
                string documentfile = string.Empty;
                dynamic result = await Common.HttpRequestAsyncCalls(documentfile, token, _httpClientFactory, "DocumentAPIs", "Document/DeleteDocumentFileByID?RecordID=" + docId + "&FileID=" + fileId, "Post");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error") { throw new Exception(Convert.ToString(result.item1)); }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This service method will get documents data based on list of Id's and returns success status.
        /// </summary>
        /// <param name="docIds"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetDocumentsByIdsAsync(List<string> docIds, string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(docIds, token, _httpClientFactory, "DocumentAPIs", "Document/GetDocumentFromIds", "Post");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error") { throw new Exception(Convert.ToString(result.item1)); }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This service method will update document data and returns success status.
        /// </summary>
        /// <param name="docItem"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> UpdateDocumentAsync(dynamic docItem, string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(docItem, token, _httpClientFactory, "DocumentAPIs", "Document/UpdateDocumentItem", "Post");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error") { throw new Exception(Convert.ToString(result.item1)); }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Methods
    }
}
