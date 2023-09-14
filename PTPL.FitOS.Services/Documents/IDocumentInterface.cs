using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    public interface IDocumentInterface
    {
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
        Task<Object> AddDocumentAsync(dynamic docItem,string token, CancellationToken ct = default);

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
        Task<Object> UpdateDocumentAsync(dynamic docItem, string token, CancellationToken ct = default);

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
        Task<Object> DeleteDocumentFileAsync(string docId,string fileId,string token, CancellationToken ct = default);
        
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
        Task<Object> AddDocumentFileAsync(dynamic docFileItem,string token, CancellationToken ct = default);
        
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
        Task<Object> GetDocumentsByIdsAsync(List<string> docIds,string token, CancellationToken ct = default);
    }
}
