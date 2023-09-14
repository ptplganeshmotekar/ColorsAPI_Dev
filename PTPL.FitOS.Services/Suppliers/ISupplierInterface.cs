using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    public interface ISupplierInterface
    {
        /// <summary>
        /// This service method will use to check style and colour relationship and returns success status.
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
        Task<Object> CheckSupplierColours(List<string> colIds,string token, CancellationToken ct = default);
    }
}
