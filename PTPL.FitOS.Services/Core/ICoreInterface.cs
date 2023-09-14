using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PTPL.FitOS.DataModels;

namespace PTPL.FitOS.Services
{
    public interface ICoreInterface
    {
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
        Task<Object> GetSharedRecords(string moduleName, CancellationToken ct = default);

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
        Task<Object> GetAccessDetails(string moduleName, string RecordID, CancellationToken ct = default);

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
        Task<Object> DeleteAccessDetails(string moduleName, string RecordID, CancellationToken ct = default);

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
        Task<Object> AddAccessDetails(dynamic sharedItem, CancellationToken ct = default);

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
        Task<object> GetListOfAccessDetails(string moduleName, string RecordIDs, string Operation, CancellationToken ct = default);

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
        Task<object> DeleteListOfAccessDetails(string moduleName, string RecordIDs, CancellationToken ct = default);

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
        Task<object> AddRecentActivity(List<RecentActivityHelperClass> recentActivities, CancellationToken ct = default);

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
        Task<object> DeleteFavouritesItems(List<string> ids, string module, CancellationToken ct = default);
    }
}
