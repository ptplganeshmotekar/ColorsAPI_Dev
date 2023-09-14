using PTPL.FitOS.DataModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    public interface IColourServices
    {
        /// <summary>
        /// This service method will use to Add colours based on parameters.
        /// </summary>
        /// <param name="plColours"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> AddColour(ColoursDTO plColours, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This endpoint will use to Update colours based on parameters.
        /// </summary>
        /// <param name="plColours"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> UpdateColour(ColoursDTO plColours, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to Get/Fetch colours based on parameters.
        /// </summary>        
        /// <param name="userEmail"></param>        
        /// /// <param name="recordCount"></param>     
        /// <param name="type"></param>     
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> GetAllColours(string userEmail, string recordCount, string type, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to delete colours based on RecordID.
        /// </summary>
        /// <param name="ColourID"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> DeleteColourByID(string ColourID, string userEmail,  CancellationToken ct = default);

        /// <summary>
        /// This service method will use to delete multiple colours at a time based on parameters.(Bulk-Delete)
        /// </summary>
        /// <param name="coloursIds"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> BulkDeleteColours(DeleteBulk coloursIds, string userEmail,CancellationToken ct = default);

        /// <summary>
        /// This service method will use to update multiple colours at a time based on parameters.(Bulk-Update)
        /// </summary>
        /// <param name="colourObjs"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> BulkUpdateColour(List<ColoursDTO> colourObjs, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will used for advanced search of colours.
        /// </summary>
        /// <param name="AdvancedColourObj"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> AdvancedSearchForColours(AdvancedSearchEntity AdvancedColourObj, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to get/fetch list of colours based on colourids list.
        /// </summary>
        /// <param name="colourIds"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<object> GetColoursFromIds(List<string> colourIds,string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to get/fetch colour details based on colour ID.
        /// </summary>
        /// <param name="ColourID"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> GetColourByID(string ColourID, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to check colour-document relationship existance based on colour Id's list.
        /// </summary>
        /// <param name="docIds"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> CheckColourDocuments(List<string> docIds, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to encrypt and decrypt tyhe colour details.
        /// </summary>                             
        /// <param name="colourObjs"></param>
        /// <param name="operation"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<List<ColoursDTO>> Encrypt_Decrypt_Data(List<ColoursDTO> colourObjs, string operation);

        /// <summary>
        /// This service method will use to get/fetch shared colours based on user login.
        /// </summary>        
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> GetAllSharedColours(string userEmail, string recordCount, CancellationToken ct = default);

        Task<Object> AddExcelColour(List<ColoursDTO> objExcel, string userEmail, CancellationToken ct = default);
    }
}
