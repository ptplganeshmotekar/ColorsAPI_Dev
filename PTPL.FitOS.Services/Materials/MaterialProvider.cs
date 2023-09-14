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
    /// This service class is use for material related services.
    /// </summary>
    public class MaterialProvider : IMaterialInterface
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
        public MaterialProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
        }

        #endregion Constructor       

        #region Methods
        /// <summary>
        /// This service method will check the material-colour realationship and returns success status.
        /// </summary>
        /// <param name="colIds"></param>
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
        public async Task<object> CheckMaterialColours(List<string> colIds, string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(colIds, token, _httpClientFactory, "MaterialAPIs", "Materials/CheckMaterialColours", "Post");
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
        /// This service method will check the material-supplier-colour realationship and returns success status.
        /// </summary>
        /// <param name="colIds"></param>
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
        public async Task<object> CheckMaterialSupplierColours(List<string> colIds, string token, CancellationToken ct = default)
        {
            try
            {
                dynamic result = await Common.HttpRequestAsyncCalls(colIds, token, _httpClientFactory, "MaterialAPIs", "Materials/CheckMaterialSupplierColour", "Post");
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
