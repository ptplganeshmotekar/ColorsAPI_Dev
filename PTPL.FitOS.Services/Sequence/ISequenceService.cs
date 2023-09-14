using PTPL.FitOS.DataModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    public interface ISequenceService
    {
        /// <summary>
        /// This service method will use to get next sequence based on parameters.
        /// </summary>                     
        /// <param name="ct"></param>
        /// <param name="sequenceName"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>
        string GetNextSequence(string sequenceName, string org, CancellationToken ct = default);
    }
}
