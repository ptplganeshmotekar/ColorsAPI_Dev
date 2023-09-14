using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using PTPL.FitOS.DataModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    public class CoreProvider : ICoreInterface
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
        /// 
        public CoreProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {                     
            _httpClientFactory = httpClientFactory;            
        }

        #endregion Constructor

        #region Methods
        /// <summary>        
        /// This service method will returns shared records from sharedaccess table from db.
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="moduleName"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetSharedRecords(string moduleName, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls("", CommonDeclarations.accessToken, _httpClientFactory, "CoreAPIs", "SharedAccess/GetSharedRecords?ModuleName=" + moduleName + "", "Get");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(result.item1));
                    }                   
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>        
        /// This service method will returns shared accessdetails from sharedaccess table from db.
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="moduleName"></param>
        /// <param name="RecordID"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetAccessDetails(string moduleName, string RecordID, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls("", CommonDeclarations.accessToken, _httpClientFactory, "CoreAPIs", "SharedAccess/GetAccessDetails?ModuleName=" + moduleName + "&RecordID=" + RecordID + "", "Get");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(result.item1));
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>        
        /// This service method will delete shared accessdetails from sharedaccess table from db.
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="moduleName"></param>
        /// <param name="RecordID"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> DeleteAccessDetails(string moduleName, string RecordID, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls("", CommonDeclarations.accessToken, _httpClientFactory, "CoreAPIs", "SharedAccess/DeleteAccessDetails?ModuleName=" + moduleName + "&RecordID=" + RecordID + "", "Get");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(result.item1));
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>        
        /// This service method will add shared accessdetails to sharedaccess table.
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="moduleName"></param>
        /// <param name="RecordID"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> AddAccessDetails(dynamic sharedItem, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(sharedItem, CommonDeclarations.accessToken, _httpClientFactory, "CoreAPIs", "SharedAccess/AddSharedAccess", "Post");
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
        /// This service method will returns list of shared accessdetails from sharedaccess table from db.
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="moduleName"></param>
        /// <param name="RecordID"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetListOfAccessDetails(string moduleName, string RecordIDs, string Operation, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls("", CommonDeclarations.accessToken, _httpClientFactory,
                                "CoreAPIs", "SharedAccess/GetListOfAccessDetails?ModuleName=" + moduleName + "&RecordIDs=" + RecordIDs + "&Operation=" + Operation + "", "Get");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(result.item1));
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>        
        /// This service method will delete list of shared accessdetails from sharedaccess table from db.
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="moduleName"></param>
        /// <param name="RecordID"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> DeleteListOfAccessDetails(string moduleName, string RecordIDs, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls("", CommonDeclarations.accessToken, _httpClientFactory, "CoreAPIs", "SharedAccess/DeleteListOfAccessDetails?ModuleName=" + moduleName + "&RecordIDs=" + RecordIDs + "", "Get");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(result.item1));
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This service method will add recent activity data into database table
        /// </summary>
        /// <param name="recentActivities"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> AddRecentActivity(List<RecentActivityHelperClass> recentActivities, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(recentActivities, CommonDeclarations.accessToken, _httpClientFactory, "CoreAPIs", "RecentActivity/AddRecentActivities", "Post");
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
        /// This service method will delete favourite records data from database table
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="module"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> DeleteFavouritesItems(List<string> ids, string module, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(ids, CommonDeclarations.accessToken, _httpClientFactory, "CoreAPIs", "Favourites/DeleteFavouriteItemByRecorID?module="+module+"", "Post");
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


       

        #region ReadFile


        

        #endregion
    }
}

