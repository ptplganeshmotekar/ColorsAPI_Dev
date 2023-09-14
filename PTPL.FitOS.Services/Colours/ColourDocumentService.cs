using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using PTPL.FitOS.DataContext;
using PTPL.FitOS.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    /// <summary>
    /// This service class is use for colour-document related services.
    /// </summary>
    public class ColourDocumentService : IColourDocumentService
    {
        #region Fields

        /// <summary>
        /// Field declarations
        /// </summary>
        string module = "Colours";
        private readonly IDocumentInterface _plDocumentService;
        private readonly IFileInterface _plFileInterface;
        private readonly IPermissionInterface _plPermissionInterface;
        private readonly ICoreInterface _plCoreInterface;
        private readonly Lazy<IColourServices> _plColourServices;
        private readonly DbSet<ColourDocumentDTO> _colourDocuments;
        private readonly GetDataDBContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly InsertUpdateDBContext _context;
        LogDTO log = new LogDTO();
        NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructor
        /// <summary>
        /// This is a service level constructor
        /// </summary>
        public ColourDocumentService(GetDataDBContext dbContext, InsertUpdateDBContext context, ILogger<ColourDocumentDTO> logger, IDocumentInterface documentInterface,
            IFileInterface fileInterface, IPermissionInterface permissionInterface, ICoreInterface coreInterface, IConfiguration configuration
           /* Lazy<IColourServices> colourServices*/)
        {
            log.ApplicationType = CommonDeclarations.ApplicationType;
            log.Module = CommonDeclarations.module;
            log.ClassType = "Service-Level";
            log.LoggedTime = DateTime.Now;
            _context = context;
            _plDocumentService = documentInterface;
            _plFileInterface = fileInterface;
            _plPermissionInterface = permissionInterface;
            _plCoreInterface = coreInterface;
          //  _plColourServices = colourServices;
            _colourDocuments = _context.ColourDocumentsDBSet;
            _dbContext = dbContext;
            _configuration = configuration;
        }
        #endregion

        #region Methods
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
        public async Task<ColourDocumentDTO> AddColourDocument(ColourDocumentDTO plColourDocuments, string userEmail, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    throw new Exception("Incorrect / invalid user email id.");
                }
                _context.Add(plColourDocuments);
                _context.SaveChanges();

                //Logging the info to aws cloudwatch                
                log.LogMessage = $"Colour-Document relationship added successfully with " +
                             $"colourid - {plColourDocuments.ColourId} and documentid - {plColourDocuments.DocumentId}.";
                LogService.LogInfoData(log, _logger);

                return await Task.Run(() => plColourDocuments);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<ColourDocumentDTO> AddColourDocumentItem(ColourDocumentDTO plColourDocuments, CancellationToken ct = default)
        {
            try
            {
                _context.Add(plColourDocuments);
                _context.SaveChanges();

                //Logging the info to aws cloudwatch                
                log.LogMessage = $"Colour-Document relationship added successfully with " +
                             $"colourid - {plColourDocuments.ColourId} and documentid - {plColourDocuments.DocumentId}.";
                LogService.LogInfoData(log, _logger);

                return await Task.Run(() => plColourDocuments);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<Object> GetColourDocumentsByColourID(string ColourID, string userEmail, CancellationToken ct = default)
        {
            try
            {
                ColoursDTO colourObj = _context.ColoursDBSet.Where(col => col.ID == ColourID).FirstOrDefault();//.ToList();
                if (colourObj == null) { throw new Exception("Colour record is not available."); }

                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;

                if (role != "Administrator" && colourObj.CreatedByID != userId)
                {
                    object getRes = await _plCoreInterface.GetAccessDetails(module, ColourID, ct);
                    dynamic getSharedDetails = (dynamic)getRes;
                    if (getSharedDetails != null)
                    {
                        if (getSharedDetails.item2 == "Error") { throw new Exception(Convert.ToString(getSharedDetails.item1)); }
                        string value = Convert.ToString(getSharedDetails.item1);
                        if (!value.Contains("recordID"))
                        {
                            throw new Exception("Insufficient permission to get colour record..");
                        }
                    }
                }

                List<string> docIds = _context.ColourDocumentsDBSet.Where(x => x.ColourId == ColourID).Select(x => x.DocumentId).ToList();
                if (docIds.Count() > 0)
                {
                    object docsList = await _plDocumentService.GetDocumentsByIdsAsync(docIds, CommonDeclarations.accessToken, ct);

                    //Logging the info to aws cloudwatch                    
                    log.LogMessage = $"Api call made to document application to fetch colur-documents with colourid - {ColourID}.";
                    LogService.LogInfoData(log, _logger);

                    dynamic documents = (dynamic)docsList;
                    if (documents != null)
                    {
                        if (documents.item2 == "Error")
                        {
                            throw new Exception(Convert.ToString(documents.item1));
                        }
                    }
                    colourObj.Documents = documents.item1;

                    //Logging the info to aws cloudwatch                    
                    log.LogMessage = $"Colour and document " +
                                 $"relationship fetched successfully with colourid - {ColourID}.";
                    LogService.LogInfoData(log, _logger);
                }

                dynamic FileList = null;
                if (!string.IsNullOrEmpty(colourObj.Thumbnail))
                {
                    object fileRes = await _plFileInterface.GetFileByAsync(colourObj.Thumbnail, ct);
                    dynamic res = (dynamic)fileRes;
                    if (res != null)
                    {
                        if (res.item2 == "Error")
                        {
                            throw new Exception(Convert.ToString(res.item1));
                        }
                        //Logging the info to aws cloudwatch
                        log.LogMessage = $"Thumbnail(File item) fetched successfully with fileid - {colourObj.Thumbnail}.";
                        LogService.LogInfoData(log, _logger);
                        FileList = res.item1;
                    }
                }
                colourObj.ThumbnailFiles = FileList;
                colourObj.ColourDocuments = null;

                colourObj.CreatedBy = !string.IsNullOrEmpty(colourObj.CreatedByID) ? Common.GetUserDetails(colourObj.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null;

                //Logging the info to aws cloudwatch                
                log.LogMessage = $"Colour record fetched successfully with colourid - {ColourID}.";
                LogService.LogInfoData(log, _logger);

                List<ColoursDTO> colList = new List<ColoursDTO>();
                colList.Add(colourObj);

                var encryptedData = await _plColourServices.Value.Encrypt_Decrypt_Data(colList, "Encrypt");
                return await Task.Run(() => encryptedData[0]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<Object> DeleteColourDocument(string ColourID, string DocumentID, string userEmail, CancellationToken ct = default)
        {
            try
            {
                ColourDocumentDTO colourDocumentItem = _context.ColourDocumentsDBSet.FirstOrDefault(x => x.ColourId == ColourID && x.DocumentId == DocumentID);
                if (colourDocumentItem != null)
                {
                    colourDocumentItem.DeletedOn = DateTime.Now;
                    colourDocumentItem.DeleteStatus = true;
                    //_context.ColourDocumentsDBSet.Remove(colourDocumentItem);
                    _context.ColourDocumentsDBSet.Update(colourDocumentItem);
                    _context.SaveChanges();

                    //Logging the info to aws cloudwatch                
                    log.LogMessage = $"Deleted colour and document relationship" +
                                 $" successfully with colourid - {ColourID} and documentid - {DocumentID}..";
                    LogService.LogInfoData(log, _logger);
                }
                else
                {
                    //Logging the info to aws cloudwatch                
                    log.LogMessage = $"Colour and document relationship" +
                                 $" not deleted with colourid - {ColourID} and documentid - {DocumentID}..";
                    LogService.LogInfoData(log, _logger);
                }

                return await Task.Run(() => DocumentID);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Methods
    }
}
