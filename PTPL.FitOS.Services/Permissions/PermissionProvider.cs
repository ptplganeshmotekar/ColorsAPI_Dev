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
    /// <summary>
    /// This is a permission provider class which is use for permission related transactions.
    /// </summary>
    public class PermissionProvider : IPermissionInterface
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
        public PermissionProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {                     
            _httpClientFactory = httpClientFactory;
        }

        #endregion Constructor

        #region Methods
        /// <summary>        
        /// This service method will use to fetch user permissions and returns success status.
        /// </summary>
        /// <param name="permissionsItem"></param>        
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetUserPermissions(dynamic permissionsItem, string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(permissionsItem, token, _httpClientFactory, "PermissionAPIs", "Permission/GetUserPermissions", "Post");
                string resString = result.ToString();
                if (!resString.Contains("item1") && !resString.Contains("item2"))
                {
                    throw new Exception(result.ToString());
                }
                else
                {
                    if (result.item2 == "Error")
                    {
                        string error = result.item1.ToString();
                        if (error.Contains("Exception of type 'System.Exception'"))
                        {
                            throw new Exception("Insufficient permission for the user.");
                        }
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
        /// This service method will returns user details eith user role
        /// </summary>       
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public async Task<object> GetUserDetail(string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls("", token, _httpClientFactory, "PermissionAPIs", "Users/GetUserWithRole", "Get");
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

        #endregion Methods
    }
}
