using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace PTPL.FitOS
{

    /// <summary>
    ///  This class is use to create publich generic methods and declaration for the application.
    ///  Created By  : Ganesh Motekar 
    ///  Created on  : Feb/08/2021
    ///  -----------------------------------------------------------------------
    ///  Modified By : Guna Sekhar Kanigiri
    ///  Modified on : Feb/11/2021
    ///  Purpose     : Added new parameters to the HttpRequestAsyncCalls method, 
    ///                Added Get and Post method type conditions    
    ///  -------------------------------------------------------------------------
    ///  Modified By : 
    ///  Modified on : 
    ///  Purpose     : 
    /// </summary>
    public static class Common
    {  
        /// <summary>
        /// This method is use to create public which is generic methods for the application.
        /// </summary>        
        /// <param name="myObj"></param>
        /// <param name="token"></param>
        /// <param name="requestURI"></param>       
        /// <returns>Returns data value based on request parameters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="SqlConnection Error"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public static async Task<object> HttpRequestAsyncCalls<T>(this T myObj, string token, IHttpClientFactory httpClient, string requestAPI, string requestURI, string httpMethodType)
        {
            dynamic result = string.Empty;
            try
            {
                var client = httpClient.CreateClient(requestAPI);
                HttpRequestMessage message = new HttpRequestMessage();
                message.Headers.Add("Accept", "application/json");
                if (httpMethodType == "Get") { message.Method = HttpMethod.Get; }
                else
                {
                    message.Method = HttpMethod.Post;
                    if (myObj.ToString() == "") { }
                    else
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(myObj), System.Text.Encoding.UTF8, "application/json");
                    }
                }
                message.RequestUri = new Uri(client.BaseAddress.ToString() + requestURI);                
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                HttpResponseMessage response = await client.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<dynamic>(resp);
                    return result;
                }
                else
                {
                    result = response.ReasonPhrase;
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                return result;
            }
        }

        /// <summary>
        /// This method is use to fetch user details based on paramaters.
        /// </summary>        
        /// <param name="createdbyId"></param>
        /// <param name="Email"></param>
        /// <param name="connectionstring"></param>       
        /// <param name="config"></param>       
        /// <returns>Returns data value based on request parameters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="SqlConnection Error"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public static object GetUserDetails(string createdbyId,string Email, string connectionstring, IConfiguration config)
        {
            try
            {
                dynamic userObj = new JObject();
                string userID = string.Empty;
                string firstName = string.Empty;
                string lastName = string.Empty;
                string email = string.Empty;
                string query = string.Empty;
                string org = string.Empty;
                string pictureID = string.Empty;
                string fileName = string.Empty;

                if (!string.IsNullOrEmpty(createdbyId))
                {
                    query = "Select PlUsers.FirstName,PlUsers.LastName,PlUsers.ID,PlUsers.EMail,PlUsers.Org,PlUsers.PictureID,PlFile.Name" +
                        " from PlUsers left join PlFile on PlUsers.PictureID = PlFile.ID  where PlUsers.ID = '" + createdbyId + "'";
                }
                else
                {
                    query = "Select PlUsers.FirstName,PlUsers.LastName,PlUsers.ID,PlUsers.EMail,PlUsers.Org,PlUsers.PictureID,PlFile.Name" +
                     " from PlUsers left join PlFile on PlUsers.PictureID = PlFile.ID  where PlUsers.EMail = '" + Email + "'";
                }

                using (MySqlConnection con = new MySqlConnection(connectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                firstName = Convert.ToString(rdr[0]);
                                lastName = Convert.ToString(rdr[1]);
                                userID = Convert.ToString((rdr[2]));
                                email = Convert.ToString(rdr[3]);
                                org = Convert.ToString(rdr[4]);
                                pictureID = Convert.ToString(rdr[5]);
                                fileName = Convert.ToString(rdr[6]);
                            }
                            rdr.Close();
                        }
                    }
                    con.Close();
                }
                userObj.firstName = firstName;
                userObj.lastName = lastName;
                userObj.id = userID;
                userObj.email = email;

                if (!string.IsNullOrEmpty(pictureID))
                {
                    string imageUrl = GetFileURL(fileName, pictureID, config);
                    userObj.imageUrl = imageUrl;
                }
                else { userObj.imageUrl = null; }
                return userObj;
            }
            catch (Exception)
            {
                throw;
            }            
        }

        /// <summary>
        /// This method is use to fetch s3 url based on paramaters.
        /// </summary>        
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <param name="_config"></param>              
        /// <returns>Returns data value based on request parameters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="SqlConnection Error"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public static string GetFileURL(string fileName, string directory, IConfiguration _config)
        {
            try
            {
                string accessKey = "";
                string secretKey = "";
                string bucketName = "";
                RegionEndpoint bucketRegion = RegionEndpoint.USEast1;

                accessKey = _config.GetSection("S3")["AWSAccessKey"];
                secretKey = _config.GetSection("S3")["AWSSecretKey"];
                bucketName = _config.GetSection("S3")["BucketName"];

                IAmazonS3 _s3Client = new AmazonS3Client(accessKey, secretKey, bucketRegion);
                var fileTransferUtility = new TransferUtility(_s3Client);
                var bucketPath = !string.IsNullOrWhiteSpace(directory)
                    ? bucketName + @"/" + directory
                    : bucketName;
                var request = new GetObjectRequest()
                {
                    BucketName = bucketPath,
                    Key = fileName
                };

                var expiryUrlRequest = new GetPreSignedUrlRequest();
                expiryUrlRequest.BucketName = bucketPath;
                expiryUrlRequest.Key = fileName;
                expiryUrlRequest.Expires = DateTime.Now.AddDays(1);

                string url = _s3Client.GetPreSignedURL(expiryUrlRequest);
                return url;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Please check the provided AWS Credentials for S3.");
                }
                else
                {
                    throw new Exception(amazonS3Exception.ToString());
                }
            }
        }
    }    


    /// <summary>
    /// This class is use to declare a global variable and access them into entire application.
    /// </summary>
    /// <returns>returns object value</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    public static class CommonDeclarations
    {
        public static string ConnectionString
        {
            get;
            set;
        }
        public static string ConnectionString1
        {
            get;
            set;
        }
        public static string accessToken { get; set; }
        public static string userEmail { get; set; }
        public static string userOrg { get; set; }
        public static string userID { get; set; }
        public static string userRole { get; set; }

        public static string ApplicationType = ".NetCore API Logs";
        public static string module = "Colours";        
        public static Logger logger { get; set; }

        public static int totalRocordsCount { get; set; }

        public static int totalCreatedCount { get; set; }
        public static int totalSharedCount { get; set; }

        //public static bool UseMongoDB { get; set; }

        public static string PageAccessed { get; set; }

    }
}
