using Newtonsoft.Json;
using NLog;
using PTPL.FitOS.DataModels;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    /// <summary>
    /// This is a Log class which is used to log the info and error type logs.
    /// </summary>
    public static class LogService
    {
        /// <summary>
        /// This method is used to log the data type of Info
        /// </summary>
        /// <param name="logItem"></param>
        /// <param name="_logger"></param>        
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public static void LogInfoData(LogDTO logItem, Logger _logger)
        {
            try
            {
                if (string.IsNullOrEmpty(CommonDeclarations.accessToken))
                {
                    logItem.Organization = "UnAuthorized/Anonymous";
                    logItem.LoggedBy = "UnAuthorized/Anonymous user";
                }
                else
                {
                    logItem.Organization = CommonDeclarations.userOrg;
                    logItem.LoggedBy = CommonDeclarations.userEmail;
                }
                logItem.LogType = "Info";
                logItem.LoggedTime = DateTime.Now;
                _logger.Info(JsonConvert.SerializeObject(logItem));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This method is used to log the data type of Error
        /// </summary>
        /// <param name="logItem"></param>
        /// <param name="_logger"></param>        
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        public static void LogErrorData(LogDTO logItem, Logger _logger)
        {
            try
            {
                if (string.IsNullOrEmpty(CommonDeclarations.accessToken))
                {
                    logItem.Organization = "UnAuthorized/Anonymous";
                    logItem.LoggedBy = "UnAuthorized/Anonymous user";
                }
                else
                {
                    logItem.Organization = CommonDeclarations.userOrg;
                    logItem.LoggedBy = CommonDeclarations.userEmail;
                }
                logItem.LogType = "Error";
                logItem.LoggedTime = DateTime.Now;
                _logger.Error(JsonConvert.SerializeObject(logItem));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public static void UserActivityLogInfoData(UserActivityLogDTO UserActivityLogItem, Logger _logger)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(CommonDeclarations.accessToken))
        //        {

        //            UserActivityLogItem.UserId = "Unauthorized/Anonymous";
        //            UserActivityLogItem.UserEmail = "Unauthorized/Anonymous user";
        //        }
        //        else
        //        {
        //            UserActivityLogItem.UserId = CommonDeclarations.userID;
        //            UserActivityLogItem.UserEmail = CommonDeclarations.userEmail;
        //        }
        //        UserActivityLogItem.IpAddress = GetLocalIPAddress();
        //        _logger.Info(JsonConvert.SerializeObject(UserActivityLogItem));

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //public static string GetLocalIPAddress()
        //{
        //    var host = Dns.GetHostEntry(Dns.GetHostName());
        //    foreach (var ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            return ip.ToString();
        //        }
        //    }
        //    throw new Exception("No network adapters with an IPv4 address in the system!");
        //}
        //public static async Task<UserActivityLogDTO> UserActivityLog<T>(string action, T NewData, T OldData)
        //{
        //    {
        //        Logger activitylogger = LogManager.GetLogger("activityLog");
        //        UserActivityLogDTO useractivitylogDTO = new UserActivityLogDTO();
        //        useractivitylogDTO.UserId = CommonDeclarations.userID;
        //        useractivitylogDTO.UserEmail = CommonDeclarations.userEmail;
        //        useractivitylogDTO.ActionName = action;
        //        useractivitylogDTO.RoleId = CommonDeclarations.userRole;
        //        useractivitylogDTO.IpAddress = GetLocalIPAddress();


        //        useractivitylogDTO.OldData = OldData != null ? JsonConvert.SerializeObject(OldData) : null;
        //        useractivitylogDTO.NewData = NewData != null ? JsonConvert.SerializeObject(NewData) : null;

        //        UserActivityLogInfoData(useractivitylogDTO, activitylogger);
        //        return await Task.Run(() => useractivitylogDTO);


        //    }
        //}
    }
}
