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
    /// This is a file provider class which is use for file related transactions.
    /// </summary>
    public class FileProvider : IFileInterface
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
        public FileProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {                   
            _httpClientFactory = httpClientFactory;         
        }

        #endregion Constructor

        #region Methods
        /// <summary>
        /// This service method will add file data and returns success status.
        /// </summary>       
        /// <param name="fileItem"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> AddFileAsync(dynamic fileItem, CancellationToken ct = default)
        {
            try
            {

                dynamic result = await Common.HttpRequestAsyncCalls(fileItem, CommonDeclarations.accessToken, _httpClientFactory, "DocumentAPIs", "Files/AddFileItem", "Post");
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
        /// This service method will delete file data and returns success status.
        /// </summary>
        /// <param name="FileID"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> DeleteFileAsync(string FileID, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(FileID, CommonDeclarations.accessToken, _httpClientFactory, "DocumentAPIs", "Files/DeleteFileByID?FileID=" + FileID + " ", "Post");
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
        /// This service method will get/fetch the file data and returns success status.
        /// </summary>
        /// <param name="filedId"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetFileByAsync(string filedId, CancellationToken ct = default)
        {
            try
            {

                dynamic result = await Common.HttpRequestAsyncCalls(filedId, CommonDeclarations.accessToken, _httpClientFactory, "DocumentAPIs", "Files/GetFileItemByID?FileID=" + filedId + "", "Get");
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
        /// This service method will get/fetch the file's data based on list of id's and returns success status.
        /// </summary>
        /// <param name="filedId"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetFilesFromIDsAsync(List<string> filedIds, CancellationToken ct = default)
        {
            try
            {

                dynamic result = await Common.HttpRequestAsyncCalls(filedIds, CommonDeclarations.accessToken, _httpClientFactory, "DocumentAPIs", "Files/GetFileItemFromIDs", "Post");
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
        /// his service method will delete the file's data based on list of id's and returns success status.
        /// </summary>
        /// <param name="FileID"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> DeleteFileByIDsListAsync(List<string> FileIDs, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(FileIDs, CommonDeclarations.accessToken, _httpClientFactory, "DocumentAPIs", "Files/DeleteFileByIDsList", "Post");
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
