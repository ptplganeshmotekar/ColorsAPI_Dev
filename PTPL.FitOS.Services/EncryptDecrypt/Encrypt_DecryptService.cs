using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

//This service class is used for encrypt and decrypt the data.
namespace PTPL.FitOS.Services
{
    public class Encrypt_DecryptService : IEncrypt_DecryptService
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
        public async Task<string> Encrypt(string data, string key)
        {
            if (!string.IsNullOrEmpty(data))
            {
                byte[] iv = new byte[16];
                byte[] array;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            {
                                streamWriter.Write(data);
                            }
                            array = memoryStream.ToArray();
                        }
                    }
                }
                return await Task.Run(() => Convert.ToBase64String(array));
            }
            else
            {
                return await Task.Run(() => data);
            }
        }

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
        public async Task<string> Decrypt(string data, string key)
        {
            if (!string.IsNullOrEmpty(data))
            {
                byte[] iv = new byte[16];
                byte[] cipherTextdata = Convert.FromBase64String(data);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream(cipherTextdata))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                data = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
                return await Task.Run(() => data);
            }
            else
            {
                return await Task.Run(() => data);
            }
        }
    }
}
