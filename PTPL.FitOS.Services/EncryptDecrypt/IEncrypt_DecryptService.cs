using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

//This service class is used for encrypt and decrypt the data.
namespace PTPL.FitOS.Services
{
    public interface IEncrypt_DecryptService
    {
        /// <summary>
        /// This api will encrypt the input data using input key
        /// </summary>
        /// <param name="data"></param>             
        /// <param name="key"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<string> Encrypt(string data, string key);

        /// <summary>
        /// This api will decrypt the input data using input key
        /// </summary>
        /// <param name="data"></param>             
        /// <param name="key"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>
        Task<string> Decrypt(string data, string key);
    }
}
