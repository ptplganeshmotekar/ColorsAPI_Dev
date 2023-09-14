using PTPL.FitOS.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    public interface IColourDocumentService
    {
        /// <summary>
        /// This service method will use to Add colour-document relationships based on parameters.
        /// </summary>
        /// <param name="plColourDocuments"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<ColourDocumentDTO> AddColourDocument(ColourDocumentDTO plColourDocuments, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to Add colour-document relationships based on parameters.
        /// </summary>
        /// <param name="plColourDocuments"></param>
        /// <param name="userItem"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<ColourDocumentDTO> AddColourDocumentItem(ColourDocumentDTO plColourDocuments, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to get colour-document relationships based on parameters.
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
        Task<Object> GetColourDocumentsByColourID(string ColourID, string userEmail, CancellationToken ct = default);

        /// <summary>
        /// This service method will use to Delete colour-document relationships based on parameters.
        /// </summary>
        /// <param name="ColourID"></param>
        /// <param name="DocumentID"></param>        
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<Object> DeleteColourDocument(string ColourID, string DocumentID, string userEmail, CancellationToken ct = default);        
    }
}
