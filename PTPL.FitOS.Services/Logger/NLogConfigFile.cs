using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using System;

namespace PTPL.FitOS.Services
{
    public class NLogConfigFile
    {
        #region Fields
        /// <summary>
        /// Field declarations
        /// </summary>
        private static String accessKey = "";
        private static String secretKey = "";
        private static String region = "";
        #endregion Fields

        #region Constructor
        /// <summary>
        /// This is a service level constructor
        /// </summary>
        public NLogConfigFile(IConfiguration _config)
        {
            accessKey = _config.GetSection("S3")["AWSAccessKey"];
            secretKey = _config.GetSection("S3")["AWSSecretKey"];
            region = _config.GetSection("S3")["AWSRegion"];
        }
        #endregion

        #region Methods

        /// <summary>
        /// This  method will use to configure the nlog to the AWS.
        /// </summary>       
        /// <param name="fileItem"></param>
        /// <param name="ct"></param>       
        /// <remarks>Code documentation and Review is pending</remarks>
        //public void ConfigureNLog()
        //{
        //    try
        //    {
        //        var config = new LoggingConfiguration();
        //        var awsTarget = new AWSTarget()
        //        {
        //            LogGroup = "FitOS_Logs",
        //            LogStreamNamePrefix = "Server log - Colour module",
        //            Region = region,
        //            Credentials = new BasicAWSCredentials(accessKey, secretKey)
        //        };
        //        config.AddTarget("aws", awsTarget);                
        //        config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, awsTarget));                
        //        LogManager.Configuration = config;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        #endregion Methods
    }
}

