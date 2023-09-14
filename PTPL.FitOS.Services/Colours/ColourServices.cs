using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using PTPL.FitOS.DataContext;
using PTPL.FitOS.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    /// <summary>
    /// This service class contains all colour service methods
    /// </summary>s
    public class ColourServices : IColourServices
    {
        #region Fields

        /// <summary>
        /// Field declarations
        /// </summary>

        private readonly DbSet<ColoursDTO> _plColours;
        string module = "Colours";
        private readonly GetDataDBContext _dbContext;
        private readonly InsertUpdateDBContext _context;
        private readonly ISequenceService _plSequenceService;
        private readonly IColourDocumentService _colourDocumentService;
        private readonly IFileInterface _plFileInterface;
        private readonly ISupplierInterface _plSupplierInterface;
        private readonly IMaterialInterface _plMaterialInterface;
        private readonly IStyleInterface _plStyleInterface;
        private readonly IPermissionInterface _plPermissionInterface;
        private readonly IConfiguration _configuration;
        private readonly ICoreInterface _plCoreInterface;
        private readonly IEncrypt_DecryptService _encrypt_Decrypt;
        LogDTO log = new LogDTO();
        NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of the class
        /// </summary> 
        public ColourServices(GetDataDBContext dbContext, InsertUpdateDBContext context, ILogger<ColoursDTO> logger, ISequenceService plSequenceService,
            IColourDocumentService colourDocumentService, IFileInterface fileInterface, ISupplierInterface supplierInterface,
            IMaterialInterface materialInterface, IStyleInterface styleInterface, IPermissionInterface permissionInterface, IConfiguration configuration,
            ICoreInterface coreInterface, IEncrypt_DecryptService encrypt_Decrypt)
        {
            log.ApplicationType = CommonDeclarations.ApplicationType;
            log.Module = CommonDeclarations.module;
            log.ClassType = "Service-Level";
            log.LoggedTime = DateTime.Now;
            _context = context;
            _plColours = _context.ColoursDBSet;
            _dbContext = dbContext;
            _plSequenceService = plSequenceService;
            _colourDocumentService = colourDocumentService;
            _plFileInterface = fileInterface;
            _plSupplierInterface = supplierInterface;
            _plMaterialInterface = materialInterface;
            _plStyleInterface = styleInterface;
            _plPermissionInterface = permissionInterface;
            _configuration = configuration;
            _plCoreInterface = coreInterface;
            _encrypt_Decrypt = encrypt_Decrypt;
        }
        #endregion

        #region Methods
        /// <summary>
        /// This service method will use to Add colours based on parameters.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        public async Task<Object> AddColour(ColoursDTO obj, string userEmail, CancellationToken ct = default)
        {
            try
            {
                string isAdd = string.Empty;
                dynamic accessParam = new JObject();
                accessParam.Module = module;

                object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                dynamic accessIfo = (dynamic)accessRes;
                if (accessIfo != null)
                {
                    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to add " + module); }
                    isAdd = Convert.ToString(accessIfo.item1.isAdd);
                }
                else { throw new Exception("Insufficient permission to add " + module); }
                if (isAdd == "False") { throw new Exception("Insufficient permission to add " + module); }

                List<ColoursDTO> colList = new List<ColoursDTO>();
                colList.Add(obj);
                colList = await Encrypt_Decrypt_Data(colList, "Decrypt");
                obj = colList[0];

                string userOrg = Convert.ToString(accessIfo.item1.userOrg);

                obj.Sequence = _plSequenceService.GetNextSequence(module, userOrg);
                obj.CreatedByID = obj.ModifiedByID = Convert.ToString(accessIfo.item1.userID);
                obj.Org = Convert.ToString(accessIfo.item1.userOrg);

                dynamic fileList = null;

                if (!string.IsNullOrEmpty(obj.Thumbnail))
                {
                    dynamic fileItem = new JObject();
                    string lastFolderName = Path.GetFileName(Path.GetDirectoryName(obj.Thumbnail));
                    string filename = Path.GetFileName(obj.Thumbnail);
                    string filetype = Path.GetExtension(obj.Thumbnail);

                    fileItem.ID = lastFolderName;
                    fileItem.Name = filename;
                    fileItem.FileType = filetype;
                    fileItem.CreatedByID = fileItem.ModifiedByID = Convert.ToString(accessIfo.item1.userID);

                    object fileObject = await _plFileInterface.AddFileAsync(fileItem, ct);
                    dynamic file = (dynamic)fileObject;
                    if (file != null)
                    {
                        if (file.item2 == "Error")
                        {
                            throw new Exception(Convert.ToString(file.item1));
                        }
                        //Logging the info to aws cloudwatch
                        log.LogMessage = $"Thumbnail(File item) created successfully with fileid - {lastFolderName}.";
                        LogService.LogInfoData(log, _logger);
                    }

                    dynamic fileobj = file.item1;
                    string fileId = Convert.ToString(fileobj.id);
                    obj.Thumbnail = lastFolderName;
                    if (!string.IsNullOrEmpty(fileId))
                    {
                        object fileRes = await _plFileInterface.GetFileByAsync(fileId);
                        dynamic res = (dynamic)fileRes;
                        if (res != null)
                        {
                            if (res.item2 == "Error")
                            {
                                throw new Exception(Convert.ToString(res.item1));
                            }
                            //Logging the info to aws cloudwatch
                            log.LogMessage = $"Thumbnail(File item) fetched successfully with fileid - {fileId}.";
                            LogService.LogInfoData(log, _logger);
                            fileList = res.item1;

                            ////Logging UserActivityLogs in MongoDB
                            //var activitylog = LogService.UserActivityLog("Add", null, obj);
                        }
                    }
                }
                _context.ColoursDBSet.Add(obj);
                _context.SaveChanges();

                var resp = (from colr in _context.ColoursDBSet
                            where colr.ID == obj.ID
                            select new ColoursDTO
                            {
                                Sequence = colr.Sequence,
                                ID = colr.ID,
                                Name = colr.Name,
                                Classification = colr.Classification,
                                ColorStandard = colr.ColorStandard,
                                Description = colr.Description,
                                R = colr.R,
                                B = colr.B,
                                ColourSwatch = colr.ColourSwatch,
                                CreatedByID = colr.CreatedByID,
                                CreatedBy = !string.IsNullOrEmpty(colr.CreatedByID) ? Common.GetUserDetails(colr.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                                Hexcode = colr.Hexcode,
                                InternalRef = colr.InternalRef,
                                G = colr.G,
                                PantoneCode = colr.PantoneCode,
                                ModifiedOn = colr.ModifiedOn,
                                CreatedOn = colr.CreatedOn,
                                Status = colr.Status,
                                Thumbnail = colr.Thumbnail,
                                ThumbnailFiles = fileList,
                                IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                     where favourite.Module == module && favourite.RecordId == colr.ID
                                                                     && favourite.FavouriteTo == userEmail
                                                                     select favourite.ID).FirstOrDefault()) ? true : false
                            }).FirstOrDefault();

                log.LogMessage = $"Colour record created successfully with colourid - {resp.ID}";
                LogService.LogInfoData(log, _logger);

                List<ColoursDTO> colList1 = new List<ColoursDTO>();
                colList1.Add(resp);
                var encryptedData = await Encrypt_Decrypt_Data(colList1, "Encrypt");
                ColoursDTO encryptData = encryptedData[0];
                return await Task.Run(() => encryptData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<Object> BulkUpdateColour(List<ColoursDTO> colourObjs, string userEmail, CancellationToken ct = default)
        {
            try
            {
                #region Permission Code
                //string isUpdate = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;
                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to update " + module); }
                //    isUpdate = Convert.ToString(accessIfo.item1.isEdit);
                //}
                //else { throw new Exception("Insufficient permission to update " + module); }
                //if (isUpdate == "False") { throw new Exception("Insufficient permission to update " + module); }
                #endregion

                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;

                colourObjs = await Encrypt_Decrypt_Data(colourObjs, "Decrypt");

                List<string> colorIds = new List<string>();
                List<RecentActivityHelperClass> recentActivity = new List<RecentActivityHelperClass>();

                if (role != "Administrator")
                {
                    List<string> listOfIds = new List<string>();
                    foreach (var colNewItem in colourObjs.ToList()) { listOfIds.Add(colNewItem.ID); }

                    

                    var colItems = _context.ColoursDBSet.Where(x => listOfIds.Contains(x.ID)).Select(x => new { ID = x.ID, CreatedByID = x.CreatedByID }).ToList();
                    List<string> colIdList = new List<string>();
                    foreach (var item in colItems)
                    {
                        if (item.CreatedByID != userId) { colIdList.Add(item.ID); }
                    }

                    if (colIdList.Count() > 0)
                    {
                        string commaSepIds = string.Join(',', colIdList);
                        object getRes = await _plCoreInterface.GetListOfAccessDetails(module, commaSepIds, "Update", ct);
                        dynamic getSharedDetails = (dynamic)getRes;
                        if (getSharedDetails != null)
                        {
                            if (getSharedDetails.item2 == "Error") { throw new Exception(Convert.ToString(getSharedDetails.item1)); }
                        }
                    }
                }

                foreach (var updatedObj in colourObjs.ToList())
                {
                    var oldObj = _plColours.FirstOrDefault(x => x.ID == updatedObj.ID);
                    colorIds.Add(oldObj.ID);
                    bool isUpdated = false;
                    foreach (var property in updatedObj.GetType().GetProperties())
                    {
                        if (property.Name == "CreatedOn" || property.Name == "ModifiedOn" || property.Name == "Org" || property.Name == "Sequence" || property.Name == "Thumbnail")
                        {
                            continue;
                        }
                        if (property.PropertyType.Name == "DateTime")
                        {
                            if (property.GetValue(updatedObj).ToString() != DateTime.MinValue.ToString() &&
                                (property.GetValue(oldObj).ToString() == DateTime.MinValue.ToString() || property.GetValue(updatedObj).ToString() != property.GetValue(oldObj).ToString()))
                            {
                                property.SetValue(oldObj, oldObj.GetType().GetProperty(property.Name)
                                .GetValue(updatedObj, null));
                            }
                        }
                        else
                        {
                            if (property.GetValue(updatedObj, null) != null &&
                                (property.GetValue(oldObj, null) == null || property.GetValue(oldObj, null).ToString() != property.GetValue(updatedObj, null).ToString()))
                            {
                                property.SetValue(oldObj, oldObj.GetType().GetProperty(property.Name)
                                .GetValue(updatedObj, null));
                                isUpdated = true;

                                //Logging the info to aws cloudwatch
                                log.LogMessage = $"{property.Name} field is updated with value - {property.GetValue(updatedObj)}";
                                LogService.LogInfoData(log, _logger);
                            }
                        }
                    }
                    if (isUpdated)
                    {
                        oldObj.ModifiedByID = userId;
                        _context.ColoursDBSet.Update(oldObj);
                        _context.SaveChanges();

                        /// Bulk update data added to recent activity list object
                        /// 
                        var recentActivityData = new RecentActivityHelperClass();
                        recentActivityData.Module = module;
                        recentActivityData.RecordId = oldObj.ID;
                        recentActivityData.Description = "Bulk Update Colour";
                        recentActivity.Add(recentActivityData);

                        ////Logging UserActivitylogs in MongoDB
                        //var ActivityLog = LogService.UserActivityLog("BulkUpdateColour", colorIds, null);

                        /// Ends here
                        /// 
                    }
                }

                #region Add colour recent activity
                if (recentActivity.Count > 0)
                {
                    object raResult = await _plCoreInterface.AddRecentActivity(recentActivity, ct);
                    dynamic apiresult = (dynamic)raResult;
                    if (apiresult != null)
                    {
                        if (apiresult.item2 == "Error")
                        {
                            throw new Exception(Convert.ToString(apiresult.item1));
                        }
                    }
                }
                #endregion

                Object obj = await GetColoursFromIds(userEmail, colorIds, ct = default);

                string IdsList = colorIds.Count() > 0 ? String.Join(",", colorIds) : "'No Ids'";
                //Logging the info to aws cloudwatch
                log.LogMessage = $"Colour records updated successfully with ids {IdsList}.";
                LogService.LogInfoData(log, _logger);

                ////Logging UserActivitylogs in MongoDB
                //var Activitylog = LogService.UserActivityLog("BulkUpdateColour", null, userEmail);
                return await Task.Run(() => obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<Object> GetAllColours(string userEmail, string recordCount, string type, CancellationToken ct = default)
        {
            try
            {
                #region Permission Code
                //string isGet = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;

                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to get " + module); }
                //    isGet = Convert.ToString(accessIfo.item1.isGet);
                //}
                //else { throw new Exception("Insufficient permission to get " + module); }
                //if (isGet == "False") { throw new Exception("Insufficient permission to get " + module); }

                //bool isEdit = Convert.ToString(accessIfo.item1.isEdit) != "False" ? true : true;
                //bool isDelete = Convert.ToString(accessIfo.item1.isDelete) != "False" ? true : true;
                #endregion

                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;
                string userOrg = CommonDeclarations.userOrg;

                var coloursWithThumbnail = (from colours in _context.ColoursDBSet
                                            where colours.Thumbnail != null && colours.Thumbnail != ""
                                            select new { id = colours.ID, image = colours.Thumbnail }).ToList();

                Dictionary<string, string> colourImgQueryList = new Dictionary<string, string>();
                List<string> imageIds = new List<string>();
                for (int i = 0; i < coloursWithThumbnail.Count; i++)
                {
                    var colourImageItem = coloursWithThumbnail[i];
                    string materialId = colourImageItem.id;
                    var image_ID = colourImageItem.image;
                    if (!string.IsNullOrEmpty(image_ID))
                    {
                        imageIds.Add(image_ID);
                        if (!colourImgQueryList.ContainsKey(materialId))
                        {
                            colourImgQueryList.Add(materialId, image_ID);
                        }
                    }
                }

                Dictionary<string, object> fileObjList = new Dictionary<string, object>();
                if (imageIds.Count() > 0)
                {
                    var getFilesList = await _plFileInterface.GetFilesFromIDsAsync(imageIds, ct);
                    dynamic filesObj = (dynamic)getFilesList;
                    if (filesObj != null)
                    {
                        if (filesObj.item2 == "Error") { throw new Exception(Convert.ToString(filesObj.item1)); }
                        string FileJsonString = Convert.ToString(filesObj.item1);
                        List<object> FileListvalues = JsonConvert.DeserializeObject<List<object>>(FileJsonString);
                        if (FileListvalues.Count() > 0)
                        {
                            foreach (var item in FileListvalues)
                            {
                                dynamic fileItem = (dynamic)item;
                                string fileId = fileItem.id;
                                fileObjList.Add(fileId, fileItem);
                            }
                        }
                    }
                }

                Dictionary<string, object> ImgFilesList = new Dictionary<string, object>();
                if (fileObjList.Count() > 0)
                {
                    foreach (var item in colourImgQueryList)
                    {
                        ImgFilesList.Add(item.Key, fileObjList.GetValueOrDefault(item.Value));
                    }
                }

                //Logging the info to aws cloudwatch
                log.LogMessage = $"Colour thumbnails(File items) fetched successfully.";
                LogService.LogInfoData(log, _logger);

                ////Logging UserActivity Logs in MongoDB
                //var Activitylog = LogService.UserActivityLog("Get", null, recordCount);

                if (!string.IsNullOrEmpty(recordCount) && type == "Shared")
                {
                    int startIndex = 0;
                    int maxIndex = 0;

                    startIndex = Convert.ToInt32(recordCount.Split('-')[0].ToString());
                    maxIndex = Convert.ToInt32(recordCount.Split('-')[1].ToString());

                    if (startIndex > maxIndex)
                    {
                        throw new Exception("start index shouldn't be more then maxIndex. Format e:g [0-50]");
                    }
                    if (string.IsNullOrEmpty(startIndex.ToString()) || string.IsNullOrEmpty(maxIndex.ToString()))
                    {
                        throw new Exception("Record count parameter value not in a proper format e:g [0-50]");
                    }

                    List<ColoursDTO> data = await GetColourSharedData(recordCount, ImgFilesList, ct);
                    var encryptedData1 = await Encrypt_Decrypt_Data(data.GroupBy(x => x.ID).Select(g => g.FirstOrDefault()).ToList(), "Encrypt");
                    return await Task.Run(() => encryptedData1);
                }
                else if (!string.IsNullOrEmpty(recordCount) && type == "Created")
                {
                    List<ColoursDTO> data = await GetColourCreatedData(recordCount, ImgFilesList);
                    var encryptedData1 = await Encrypt_Decrypt_Data(data, "Encrypt");
                    return await Task.Run(() => encryptedData1);
                }
                else if (type == "All")
                {
                    List<ColoursDTO> data = await GetColourSharedData(recordCount, ImgFilesList, ct);
                    List<ColoursDTO> data1 = await GetColourCreatedData(recordCount, ImgFilesList);
                    var combinedData = data.Concat(data1);
                    var encryptedData = await Encrypt_Decrypt_Data(combinedData.GroupBy(x => x.ID).Select(g => g.FirstOrDefault()).ToList(), "Encrypt");
                    CommonDeclarations.totalRocordsCount = CommonDeclarations.totalCreatedCount + CommonDeclarations.totalSharedCount;
                    return await Task.Run(() => encryptedData);
                }
                else
                {
                    throw new Exception("Invalid parameters to the endpoint...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This service method will use to get/fetch list of colours based on colourids list.
        /// </summary>
        /// <param name="ColourIds"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        public async Task<Object> GetColoursFromIds(string userEmail, List<string> ColourIds, CancellationToken ct = default)
        {
            try
            {
                #region Permission Code
                //string isGet = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;

                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to get " + module); }
                //    isGet = Convert.ToString(accessIfo.item1.isGet);
                //}
                //else { throw new Exception("Insufficient permission to get " + module); }
                //if (isGet == "False") { throw new Exception("Insufficient permission to get " + module); }

                //bool isEdit = Convert.ToString(accessIfo.item1.isEdit) != "False" ? true : false;
                //bool isDelete = Convert.ToString(accessIfo.item1.isDelete) != "False" ? true : false;
                #endregion

                var result = (from colour in _context.ColoursDBSet
                              where ColourIds.Contains(colour.ID)
                              select new ColoursDTO
                              {
                                  Sequence = colour.Sequence,
                                  ID = colour.ID,
                                  Name = colour.Name,
                                  Classification = colour.Classification,
                                  ColorStandard = colour.ColorStandard,
                                  Description = colour.Description,
                                  R = colour.R,
                                  B = colour.B,
                                  ColourSwatch = colour.ColourSwatch,
                                  Hexcode = colour.Hexcode,
                                  InternalRef = colour.InternalRef,
                                  G = colour.G,
                                  PantoneCode = colour.PantoneCode,
                                  ModifiedOn = colour.ModifiedOn,
                                  CreatedOn = colour.CreatedOn,
                                  Status = colour.Status,
                                  DeleteStatus = colour.DeleteStatus,
                                  DeletedOn = colour.DeletedOn,
                                  IsEdit = true,
                                  IsDelete = true,
                                  IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                       where favourite.Module == module && favourite.RecordId == colour.ID
                                                                       && favourite.FavouriteTo == userEmail
                                                                       select favourite.ID).FirstOrDefault()) ? true : false
                              }).ToList();

                string IdsList = ColourIds.Count() > 0 ? String.Join(",", ColourIds) : "'No Ids'";

                //Logging the info to aws cloudwatch
                log.LogMessage = $"All colour records({result.Count()}) fetched successfully with id's - {IdsList}.";
                LogService.LogInfoData(log, _logger);

                ////Logging UserActivity Logs in MongoDB
                //var activitylog = LogService.UserActivityLog("Get", null, userEmail);

                var encryptedData = await Encrypt_Decrypt_Data(result.ToList(), "Encrypt");
                return await Task.Run(() => encryptedData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<Object> DeleteColourByID(string ColourID, string userEmail, CancellationToken ct = default)
        {
            try
            {
                #region Permission Code
                //string isDelete = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;

                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to delete " + module); }
                //    isDelete = Convert.ToString(accessIfo.item1.isDelete);
                //}
                //else { throw new Exception("Insufficient permission to delete " + module); }
                //if (isDelete == "False") { throw new Exception("Insufficient permission to delete " + module); }
                #endregion

                ColoursDTO ColourItem = _context.ColoursDBSet.Where(x => x.ID == ColourID).FirstOrDefault();
                if (ColourItem == null) { throw new Exception($"Colour record is not available with id {ColourID}."); }

                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;

                if (role != "Administrator" && ColourItem.CreatedByID != userId)
                {
                    object getRes = await _plCoreInterface.GetAccessDetails(module, ColourID, ct);
                    dynamic getSharedDetails = (dynamic)getRes;
                    if (getSharedDetails != null)
                    {
                        if (getSharedDetails.item2 == "Error") { throw new Exception(Convert.ToString(getSharedDetails.item1)); }
                        string value = Convert.ToString(getSharedDetails.item1);
                        if (value.Contains("recordID"))
                        {
                            throw new Exception($"Cannot delete colour record({ColourItem.Name}) that is shared to you, due to insufficient permission.");
                        }
                        else
                        {
                            throw new Exception($"Insufficient permission to delete colour record({ColourItem.Name}).");
                        }
                    }
                }

                List<string> colID = new List<string>();
                colID.Add(ColourID);


                /// Desc: Newly added code for Supplier-Colour relationship ---
                /// Name: Ganesh Motekar
                /// Date: July/26/2021
                /// Start: here
                object checkMatSuppCols = await _plMaterialInterface.CheckMaterialSupplierColours(colID, CommonDeclarations.accessToken, ct);
                dynamic resultcheckMatSuppCols = (dynamic)checkMatSuppCols;
                if (resultcheckMatSuppCols != null)
                {
                    if (resultcheckMatSuppCols.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(resultcheckMatSuppCols.item1));
                    }
                }
                /// End:here

                Object checkMatCols = await _plMaterialInterface.CheckMaterialColours(colID, CommonDeclarations.accessToken, ct);
                dynamic res1 = (dynamic)checkMatCols;
                if (res1 != null)
                {
                    if (res1.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(res1.item1));
                    }
                }

                Object checkStyCols = await _plStyleInterface.CheckStyleColourways(colID, CommonDeclarations.accessToken, ct);
                dynamic result1 = (dynamic)checkStyCols;
                if (result1 != null)
                {
                    if (result1.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(result1.item1));
                    }
                }

                List<ColourDocumentDTO> colDocs = _context.ColourDocumentsDBSet.Where(x => x.ColourId == ColourID).ToList();
                if (colDocs.Count > 0)
                {
                    //_context.ColourDocumentsDBSet.RemoveRange(colDocs);
                    //_context.SaveChanges();
                    //New code added below for soft delete
                    foreach (var item in colDocs)
                    {
                        item.DeletedOn = DateTime.Now;
                        item.DeleteStatus = true;
                    }
                    _context.ColourDocumentsDBSet.UpdateRange(colDocs);
                    _context.SaveChanges();
                }

                string thumbnailID = ColourItem.Thumbnail;
                if (!string.IsNullOrEmpty(thumbnailID))
                {
                    object fileObject = await _plFileInterface.DeleteFileAsync(thumbnailID);
                    dynamic file = (dynamic)fileObject;
                    if (file != null)
                    {
                        if (file.item2 == "Error")
                        {
                            throw new Exception(Convert.ToString(file.item1));
                        }
                        //Logging the info to aws cloudwatch
                        log.LogMessage = $"Thumbnail(File item) deleted successfully with id - {thumbnailID}.";
                        LogService.LogInfoData(log, _logger);
                    }
                }

                //Below lines code will delete if this record present favourites table
                //Starts here
                string ID = _context.FavouritesDBSet.Where(x => x.RecordId == ColourID && x.Module == module).Select(x => x.RecordId).FirstOrDefault();
                if (!string.IsNullOrEmpty(ID))
                {
                    List<string> Ids = new List<string>();
                    Ids.Add(ID);

                    object deleteFav = await _plCoreInterface.DeleteFavouritesItems(Ids, module, ct);
                    dynamic getDetailsFav = (dynamic)deleteFav;
                    if (getDetailsFav != null)
                    {
                        if (getDetailsFav.item2 == "Error") { throw new Exception(Convert.ToString(getDetailsFav.item1)); }
                    }
                }

                //Ends here

                object deleteRes = await _plCoreInterface.DeleteAccessDetails(module, ColourID);
                dynamic getDetails = (dynamic)deleteRes;
                if (getDetails != null)
                {
                    if (getDetails.item2 == "Error") { throw new Exception(Convert.ToString(getDetails.item1)); }
                }

                //Delete the item from database.
                //_context.ColoursDBSet.Remove(ColourItem);

                ColourItem.DeletedOn = DateTime.Now;
                ColourItem.DeleteStatus = true;
                _context.ColoursDBSet.Update(ColourItem);

                //save the changes made.
                _context.SaveChanges();

                //Logging the info to aws cloudwatch
                log.LogMessage = $"Colour record deleted successfully with id - {ColourID}.";
                LogService.LogInfoData(log, _logger);

                return await Task.Run(() => "Record deleted successfully");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This service method will use to delete multiple colours at a time based on parameters.(Bulk-Delete)
        /// </summary>
        /// <param name="bulkIds"></param>
        /// <param name="userEmail"></param>        
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        public async Task<Object> BulkDeleteColours(DeleteBulk bulkIds, string userEmail, CancellationToken ct = default)
        {
            try
            {
                #region Permission code
                //string isDelete = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;

                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to delete " + module); }
                //    isDelete = Convert.ToString(accessIfo.item1.isDelete);
                //}
                //else { throw new Exception("Insufficient permission to delete " + module); }
                //if (isDelete == "False") { throw new Exception("Insufficient permission to delete " + module); }    
                #endregion
                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;

                string[] idList = bulkIds.RecordIds;
                string commaSepIds = "";
                if (idList.Count() > 0)
                {
                    commaSepIds = string.Join(',', idList);
                }

                List<ColoursDTO> ColourItems = _context.ColoursDBSet.Where(t => idList.Contains(t.ID)).ToList();
                if (role != "Administrator")
                {
                    List<string> listOfIds = new List<string>();
                    listOfIds = idList.ToList();
                    var colItems = _context.ColoursDBSet.Where(x => listOfIds.Contains(x.ID)).Select(x => new { ID = x.ID, CreatedByID = x.CreatedByID }).ToList();
                    List<string> colIdList = new List<string>();
                    foreach (var item in colItems)
                    {
                        if (item.CreatedByID != userId) { colIdList.Add(item.ID); }
                    }

                    if (colIdList.Count() > 0)
                    {
                        string commaSepIdsList = string.Join(',', colIdList);
                        object getRes = await _plCoreInterface.GetListOfAccessDetails(module, commaSepIdsList, "Delete", ct);
                        dynamic getSharedDetails = (dynamic)getRes;
                        if (getSharedDetails != null)
                        {
                            if (getSharedDetails.item2 == "Error") { throw new Exception(Convert.ToString(getSharedDetails.item1)); }
                        }
                    }
                }

                //Below lines of code will checks whether the colour is being used in any relationships or not.(ex : Material-Colour relationship)
                //--------------------------------------------------------------------------------------------------------------------------------

                object checkMatSuppCols = await _plMaterialInterface.CheckMaterialSupplierColours(idList.ToList(), CommonDeclarations.accessToken, ct);
                dynamic resultcheckMatSuppCols = (dynamic)checkMatSuppCols;
                if (resultcheckMatSuppCols != null)
                {
                    if (resultcheckMatSuppCols.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(resultcheckMatSuppCols.item1));
                    }
                }

                Object checkMatCols = await _plMaterialInterface.CheckMaterialColours(idList.ToList(), CommonDeclarations.accessToken, ct);
                dynamic res1 = (dynamic)checkMatCols;
                if (res1 != null)
                {
                    if (res1.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(res1.item1));
                    }
                }

                Object checkStyCols = await _plStyleInterface.CheckStyleColourways(idList.ToList(), CommonDeclarations.accessToken, ct);
                dynamic result1 = (dynamic)checkStyCols;
                if (result1 != null)
                {
                    if (result1.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(result1.item1));
                    }
                }

                //Logging the info to aws cloudwatch
                log.LogMessage = $"Checked for all colour relationships where colour is being used or not.";
                LogService.LogInfoData(log, _logger);
                //--------------------------------------------------------------------------------------------------------------------------------

                List<ColourDocumentDTO> colDocs = _context.ColourDocumentsDBSet.Where(x => idList.Contains(x.ColourId)).ToList();
                if (colDocs.Count() > 0)
                {
                    foreach (var item in colDocs)
                    {
                        item.DeleteStatus = true;
                        item.DeletedOn = DateTime.Now;
                    }
                    //_context.ColourDocumentsDBSet.RemoveRange(colDocs);
                    _context.ColourDocumentsDBSet.UpdateRange(colDocs);
                    _context.SaveChanges();
                }

                List<string> imageIds = new List<string>();
                foreach (var colour in ColourItems)
                {
                    if (!string.IsNullOrEmpty(colour.Thumbnail))
                    {
                        imageIds.Add(colour.Thumbnail);
                    }
                }

                if (imageIds.Count() > 0)
                {
                    object fileRes = await _plFileInterface.DeleteFileByIDsListAsync(imageIds, ct);
                    dynamic file = (dynamic)fileRes;
                    if (file != null)
                    {
                        if (file.item2 == "Error")
                        {
                            throw new Exception(Convert.ToString(file.item1));
                        }
                        //Logging the info to aws cloudwatch
                        log.LogMessage = $"Thumbnail(File item) deleted successfully.";
                        LogService.LogInfoData(log, _logger);
                    }
                }

                //Below lines of code will delete the shared access data if colours are shared to any user.

                object deleteRes = await _plCoreInterface.DeleteListOfAccessDetails(module, commaSepIds, ct);
                dynamic getDetails = (dynamic)deleteRes;
                if (getDetails != null)
                {
                    if (getDetails.item2 == "Error") { throw new Exception(Convert.ToString(getDetails.item1)); }
                }
                //------------------------------------------------------------------------------------------------

                //Below lines code will delete if this record present favourites table
                //Starts here

                List<string> ids = _context.FavouritesDBSet.Where(x => idList.Contains(x.RecordId) && x.Module == module).Select(x => x.RecordId).ToList();
                if (ids.Count() > 0)
                {
                    object deleteFav = await _plCoreInterface.DeleteFavouritesItems(ids, module, ct);
                    dynamic getDetailsFav = (dynamic)deleteFav;
                    if (getDetailsFav != null)
                    {
                        if (getDetailsFav.item2 == "Error") { throw new Exception(Convert.ToString(getDetailsFav.item1)); }
                    }
                }

                //Ends here

                foreach (var item in ColourItems)
                {
                    item.DeletedOn = DateTime.Now;
                    item.DeleteStatus = true;
                }
                _context.ColoursDBSet.UpdateRange(ColourItems);
                _context.SaveChanges();

                string IdsList = idList.Count() > 0 ? String.Join(",", idList) : "'No Ids'";

                //Logging the info to aws cloudwatch
                log.LogMessage = $"Bulk colour records deleted successfully with ids - {IdsList}.";
                LogService.LogInfoData(log, _logger);

                return await Task.Run(() => "" + ColourItems.Count() + "  records deleted successfully.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<Object> UpdateColour(ColoursDTO plColours, string userEmail, CancellationToken ct = default)
        {
            try
            {
                #region Permission Code
                //string isUpdate = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;
                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to update " + module); }
                //    isUpdate = Convert.ToString(accessIfo.item1.isEdit);
                //}
                //else { throw new Exception("Insufficient permission to update " + module); }
                //if (isUpdate == "False") { throw new Exception("Insufficient permission to update " + module); }
                #endregion
                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;

                List<ColoursDTO> colList = new List<ColoursDTO>();
                colList.Add(plColours);
                colList = await Encrypt_Decrypt_Data(colList, "Decrypt");

                ColoursDTO updatedObj = colList[0];
                var oldObj = _plColours.FirstOrDefault(x => x.ID == updatedObj.ID);
                if (oldObj == null)
                {
                    throw new Exception("Colour is not available in database to update.");
                }

                ////Getiing The Old Data Record from ColourDBset
                //var olddata = _context.ColoursDBSet.AsNoTracking().Where(x => x.ID == plColours.ID).ToList();

                if (role != "Administrator" && oldObj.CreatedByID != userId)
                {
                    object getRes = await _plCoreInterface.GetAccessDetails(module, oldObj.ID, ct);
                    dynamic getSharedDetails = (dynamic)getRes;
                    if (getSharedDetails != null)
                    {
                        if (getSharedDetails.item2 == "Error") { throw new Exception(Convert.ToString(getSharedDetails.item1)); }
                        string value = Convert.ToString(getSharedDetails.item1);
                        if (value.Contains("recordID"))
                        {
                            string editAccess = Convert.ToString(getSharedDetails.item1.edit);
                            if (editAccess == "false" || editAccess == "False")
                            {
                                throw new Exception($"Cannot update colour record({oldObj.Name}) that is shared to you, due to insufficient permission.");
                            }
                        }
                        else
                        {
                            throw new Exception($"Insufficient permission to update colour record(" + oldObj.Name + ").");
                        }
                    }
                }

                bool isUpdated = false;
                foreach (var property in updatedObj.GetType().GetProperties())
                {
                    if (property.Name == "CreatedOn" || property.Name == "ModifiedOn" || property.Name == "Org" || property.Name == "Sequence")
                    {
                        continue;
                    }
                    if (property.PropertyType.Name == "DateTime")
                    {
                        if (property.GetValue(updatedObj).ToString() != DateTime.MinValue.ToString() &&
                            (property.GetValue(oldObj).ToString() == DateTime.MinValue.ToString() || property.GetValue(updatedObj).ToString() != property.GetValue(oldObj).ToString()))
                        {
                            property.SetValue(oldObj, oldObj.GetType().GetProperty(property.Name)
                            .GetValue(updatedObj, null));
                        }
                    }
                    else
                    {
                        if (property.Name == "Thumbnail" && property.GetValue(updatedObj, null) != null)
                        {
                            /// Thumbnail
                            string oldPropValue = Convert.ToString(property.GetValue(oldObj, null));
                            if (!string.IsNullOrEmpty(oldPropValue))
                            {
                                string oldFileId = oldPropValue.Split("/")[0];

                                if (!string.IsNullOrEmpty(oldFileId))
                                {
                                    object fileObject = await _plFileInterface.DeleteFileAsync(oldFileId, ct);
                                    dynamic file = (dynamic)fileObject;
                                    if (file != null)
                                    {
                                        if (file.item2 == "Error")
                                        {
                                            throw new Exception(Convert.ToString(file.item1));
                                        }
                                        //Logging the info to aws cloudwatch
                                        log.LogMessage = $"Thumbnail(File item) deleted successfully with id - {oldFileId}.";
                                        LogService.LogInfoData(log, _logger);

                                        //Logging UserActivitylogs in MongoDB
                                        //var activitylog = LogService.UserActivityLog("update", olddata, updatedObj);
                                    }
                                }
                            }
                            /// Update new file info.
                            string propValue = property.GetValue(updatedObj, null).ToString();
                            if (string.IsNullOrEmpty(propValue))
                            {
                                property.SetValue(oldObj, oldObj.GetType().GetProperty(property.Name)
                                .GetValue(updatedObj, null));
                                isUpdated = true;
                            }
                            else
                            {
                                string thumbnail_id = Path.GetFileName(Path.GetDirectoryName(propValue));
                                string thumbnail_fileName = Path.GetFileName(propValue);
                                string filetype = Path.GetExtension(propValue);
                                updatedObj.Thumbnail = thumbnail_id;
                                property.SetValue(oldObj, oldObj.GetType().GetProperty(property.Name)
                                    .GetValue(updatedObj, null));
                                isUpdated = true;

                                dynamic fileObj = new JObject();
                                fileObj.ID = thumbnail_id;
                                fileObj.Name = thumbnail_fileName;
                                fileObj.FileType = filetype;
                                fileObj.CreatedByID = fileObj.ModifiedByID = userId;

                                object fileObject = await _plFileInterface.AddFileAsync(fileObj, ct);
                                dynamic file = (dynamic)fileObject;
                                if (file != null)
                                {
                                    if (file.item2 == "Error")
                                    {
                                        throw new Exception(Convert.ToString(file.item1));
                                    }
                                    //Logging the info to aws cloudwatch
                                    log.LogMessage = $"Thumbnail(File item) created successfully with fileid - {thumbnail_id}.";
                                    LogService.LogInfoData(log, _logger);
                                }
                            }
                        }
                        else if (property.GetValue(updatedObj, null) != null &&
                            (property.GetValue(oldObj, null) == null || property.GetValue(oldObj, null).ToString() != property.GetValue(updatedObj, null).ToString()))
                        {
                            _logger.Info($"New updatable field - {property.Name}");
                            property.SetValue(oldObj, oldObj.GetType().GetProperty(property.Name)
                            .GetValue(updatedObj, null));
                            isUpdated = true;

                            //Logging the info to aws cloudwatch
                            log.LogMessage = $"{property.Name} field is updated with value - {property.GetValue(updatedObj)}";
                            LogService.LogInfoData(log, _logger);
                        }
                    }
                }
                if (isUpdated)
                {
                    oldObj.ModifiedByID = userId;
                    _context.ColoursDBSet.Update(oldObj);
                    _context.SaveChanges();
                    //await AddRecentActivity(module, oldObj.ID, "Update Colour By Id");
                }

                Object obj = await _colourDocumentService.GetColourDocumentsByColourID(plColours.ID, userEmail, ct);
                ColoursDTO col = (ColoursDTO)obj;
                bool isfav = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                    where favourite.Module == module && favourite.RecordId == col.ID
                                                         && favourite.FavouriteTo == userEmail
                                                    select favourite.ID).FirstOrDefault()) ? true : false;
                col.IsFavourite = isfav;

                //Logging the info to aws cloudwatch
                log.LogMessage = $"Colour record updated successfully with id - {plColours.ID}.";
                LogService.LogInfoData(log, _logger);

                return await Task.Run(() => col);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<object> AdvancedSearchForColours(AdvancedSearchEntity AdvancedColourObj, string userEmail, CancellationToken ct = default)
        {
            try
            {
                #region Permission Code
                //Access query
                //string isGet = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;

                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to get " + module); }
                //    isGet = Convert.ToString(accessIfo.item1.isGet);
                //}
                //else { throw new Exception("Insufficient permission to get " + module); }
                //if (isGet == "False") { throw new Exception("Insufficient permission to get " + module); }

                //bool isEdit = Convert.ToString(accessIfo.item1.isEdit) != "False" ? true : false;
                //bool isDelete = Convert.ToString(accessIfo.item1.isDelete) != "False" ? true : false;
                #endregion

                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;
                string userOrg = CommonDeclarations.userOrg;
                string key = _configuration.GetSection("Encrypt_Decrypt")["ColourKey"];

                List<string> status_list = null;
                string classification = await _encrypt_Decrypt.Decrypt(AdvancedColourObj.Classification, key);
                string[] status = AdvancedColourObj.Status;
                string keyword = await _encrypt_Decrypt.Decrypt(AdvancedColourObj.Keyword, key);
                string created = await _encrypt_Decrypt.Decrypt(AdvancedColourObj.createdBy, key);
                DateTime? created_from = AdvancedColourObj.CreatedFrom;
                DateTime? created_to = AdvancedColourObj.CreatedTo;
                DateTime? modified_from = AdvancedColourObj.ModifiedFrom;
                DateTime? modified_to = AdvancedColourObj.ModifiedTo;
                if (status != null && status.Length > 0)
                {
                    status_list = status.ToList();
                }

                //Validations for search criteria

                if (string.IsNullOrEmpty(classification) && string.IsNullOrEmpty(keyword) && string.IsNullOrEmpty(created) && (status_list == null) &&
                     created_from == null && created_to == null && modified_from == null && modified_to == null)
                {
                    throw new Exception("Please select any of serach criteria.");
                }
                if ((created_from != null && created_to == null) || (created_from == null && created_to != null))
                {
                    throw new Exception("Please select the Date.");
                }
                if ((modified_from != null && modified_to == null) || (modified_from == null && modified_to != null))
                {
                    throw new Exception("Please select the Date.");
                }

                var coloursWithThumbnail = (from colours in _context.ColoursDBSet
                                            where colours.Thumbnail != null && colours.Thumbnail != ""
                                            select new { id = colours.ID, image = colours.Thumbnail }).ToList();


                Dictionary<string, string> colourImgQueryList = new Dictionary<string, string>();
                List<string> imageIds = new List<string>();
                for (int i = 0; i < coloursWithThumbnail.Count; i++)
                {
                    var colourImageItem = coloursWithThumbnail[i];
                    string colourID = colourImageItem.id;
                    var image_ID = colourImageItem.image;
                    if (!string.IsNullOrEmpty(image_ID))
                    {
                        imageIds.Add(image_ID);
                        if (!colourImgQueryList.ContainsKey(colourID))
                        {
                            colourImgQueryList.Add(colourID, image_ID);
                        }
                    }
                }

                Dictionary<string, object> fileObjList = new Dictionary<string, object>();
                if (imageIds.Count() > 0)
                {
                    var getFilesList = await _plFileInterface.GetFilesFromIDsAsync(imageIds, ct);
                    dynamic filesObj = (dynamic)getFilesList;
                    if (filesObj != null)
                    {
                        if (filesObj.item2 == "Error") { throw new Exception(Convert.ToString(filesObj.item1)); }
                        string FileJsonString = Convert.ToString(filesObj.item1);
                        List<object> FileListvalues = JsonConvert.DeserializeObject<List<object>>(FileJsonString);
                        if (FileListvalues.Count() > 0)
                        {
                            foreach (var item in FileListvalues)
                            {
                                dynamic fileItem = (dynamic)item;
                                string fileId = fileItem.id;
                                fileObjList.Add(fileId, fileItem);
                            }
                        }
                    }
                }

                Dictionary<string, object> ImgFilesList = new Dictionary<string, object>();
                if (fileObjList.Count() > 0)
                {
                    foreach (var item in colourImgQueryList)
                    {
                        ImgFilesList.Add(item.Key, fileObjList.GetValueOrDefault(item.Value));
                    }
                }

                if (!string.IsNullOrEmpty(role) && role == "Administrator")
                {
                    //Getting result of search
                    var result = (from colour in _context.ColoursDBSet
                                  orderby colour.CreatedOn descending
                                  where ((!string.IsNullOrEmpty(keyword)) ?
                                                colour.Classification.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.B.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.ColorStandard.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.ColourSwatch.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.ColorStandard.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.G.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Hexcode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.InternalRef.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Org.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.PantoneCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.R.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Sequence.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Thumbnail.Contains(keyword, StringComparison.OrdinalIgnoreCase) : true) &&
                                    ((status_list != null) ? status_list.Any(x => colour.Status == x) : true) &&
                                    ((!string.IsNullOrEmpty(classification)) ? colour.Classification.Contains(classification, StringComparison.OrdinalIgnoreCase) : true) &&
                                    ((created_from != null && created_to != null) ? colour.CreatedOn >= created_from && colour.CreatedOn <= created_to : true) &&
                                    ((modified_from != null && modified_to != null) ? colour.ModifiedOn >= modified_from && colour.ModifiedOn <= modified_to : true) &&
                                    colour.Org == userOrg
                                  select new ColoursDTO
                                  {
                                      Sequence = colour.Sequence,
                                      ID = colour.ID,
                                      Name = colour.Name,
                                      Classification = colour.Classification,
                                      ColorStandard = colour.ColorStandard,
                                      Description = colour.Description,
                                      R = colour.R,
                                      B = colour.B,
                                      ColourSwatch = colour.ColourSwatch,
                                      Hexcode = colour.Hexcode,
                                      InternalRef = colour.InternalRef,
                                      G = colour.G,
                                      CreatedByID = colour.CreatedByID,
                                      CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                                      PantoneCode = colour.PantoneCode,
                                      ModifiedOn = colour.ModifiedOn,
                                      CreatedOn = colour.CreatedOn,
                                      Status = colour.Status,
                                      Org = colour.Org,
                                      Thumbnail = colour.Thumbnail,
                                      DeleteStatus = colour.DeleteStatus,
                                      DeletedOn = colour.DeletedOn,
                                      //IsEdit = isEdit,
                                      //IsDelete = isDelete,
                                      ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null,
                                      IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                           where favourite.Module == module && favourite.RecordId == colour.ID
                                                                           && favourite.FavouriteTo == userEmail
                                                                           select favourite.ID).FirstOrDefault()) ? true : false
                                  });

                    if (result.Count() == 0)
                    {
                        throw new Exception("No records found.");
                    }

                    if (!string.IsNullOrEmpty(created) && result.Count() != 0)
                    {
                        List<ColoursDTO> listResp = new List<ColoursDTO>();
                        foreach (var item in result.ToList())
                        {
                            if (!string.IsNullOrEmpty(item.CreatedByID))
                            {
                                dynamic userResult = (dynamic)Common.GetUserDetails(item.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration);
                                string fname = Convert.ToString(userResult.firstName);
                                string lname = Convert.ToString(userResult.lastName);
                                string Fullname = fname + lname;
                                if (!string.IsNullOrEmpty(Fullname))
                                {
                                    if (Fullname.Contains(created, StringComparison.OrdinalIgnoreCase))
                                    {
                                        listResp.Add(item);
                                    }
                                }
                            }
                        }
                        if (listResp.Count() == 0) { throw new Exception("no records found for given serach criteria."); }

                        //Logging the info to aws cloudwatch                        
                        log.LogMessage = $"Colour records({listResp.Count()}) fetched successfully based on search criteria.";
                        LogService.LogInfoData(log, _logger);

                        List<ColoursDTO> encryptedData = await Encrypt_Decrypt_Data(listResp, "Encrypt");
                        return await Task.Run(() => encryptedData);
                    }
                    if (result.Count() == 0)
                    {
                        throw new Exception("No records found for given search criteria.");
                    }

                    //Logging the info to aws cloudwatch                    
                    log.LogMessage = $"Colour records({result.Count()}) fetched successfully based on search criteria.";
                    LogService.LogInfoData(log, _logger);

                    List<ColoursDTO> encryptedData1 = await Encrypt_Decrypt_Data(result.ToList(), "Encrypt");
                    return await Task.Run(() => encryptedData1);
                }
                else
                {
                    //Getting result of search
                    var result = (from colour in _context.ColoursDBSet
                                  orderby colour.CreatedOn descending
                                  where ((!string.IsNullOrEmpty(keyword)) ?
                                                colour.Classification.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.B.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.ColorStandard.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.ColourSwatch.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.ColorStandard.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.G.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Hexcode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.InternalRef.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Org.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.PantoneCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.R.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Sequence.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                                colour.Thumbnail.Contains(keyword, StringComparison.OrdinalIgnoreCase) : true) &&
                                    ((status_list != null) ? status_list.Any(x => colour.Status == x) : true) &&
                                    ((!string.IsNullOrEmpty(classification)) ? colour.Classification.Contains(classification, StringComparison.OrdinalIgnoreCase) : true) &&
                                    ((created_from != null && created_to != null) ? colour.CreatedOn >= created_from && colour.CreatedOn <= created_to : true) &&
                                    ((modified_from != null && modified_to != null) ? colour.ModifiedOn >= modified_from && colour.ModifiedOn <= modified_to : true) && colour.CreatedByID == userId &&
                                    (colour.Org == userOrg || colour.Org == null || colour.Org == "")
                                  select new ColoursDTO
                                  {
                                      Sequence = colour.Sequence,
                                      ID = colour.ID,
                                      Name = colour.Name,
                                      Classification = colour.Classification,
                                      ColorStandard = colour.ColorStandard,
                                      Description = colour.Description,
                                      R = colour.R,
                                      B = colour.B,
                                      ColourSwatch = colour.ColourSwatch,
                                      Hexcode = colour.Hexcode,
                                      InternalRef = colour.InternalRef,
                                      G = colour.G,
                                      CreatedByID = colour.CreatedByID,
                                      CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                                      PantoneCode = colour.PantoneCode,
                                      ModifiedOn = colour.ModifiedOn,
                                      CreatedOn = colour.CreatedOn,
                                      Status = colour.Status,
                                      Org = colour.Org,
                                      Thumbnail = colour.Thumbnail,
                                      DeleteStatus = colour.DeleteStatus,
                                      DeletedOn = colour.DeletedOn,
                                      //IsEdit = isEdit,
                                      //IsDelete = isDelete,
                                      ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null,
                                      IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                           where favourite.Module == module && favourite.RecordId == colour.ID
                                                                           && favourite.FavouriteTo == userEmail
                                                                           select favourite.ID).FirstOrDefault()) ? true : false
                                  });

                    if (result.Count() == 0)
                    {
                        throw new Exception("No records found.");
                    }

                    if (!string.IsNullOrEmpty(created) && result.Count() != 0)
                    {
                        List<ColoursDTO> listResp = new List<ColoursDTO>();
                        foreach (var item in result.ToList())
                        {
                            if (!string.IsNullOrEmpty(item.CreatedByID))
                            {
                                dynamic userResult = (dynamic)Common.GetUserDetails(item.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration);
                                string fname = Convert.ToString(userResult.firstName);
                                string lname = Convert.ToString(userResult.lastName);
                                string Fullname = fname + lname;
                                if (!string.IsNullOrEmpty(Fullname))
                                {
                                    if (Fullname.Contains(created, StringComparison.OrdinalIgnoreCase))
                                    {
                                        listResp.Add(item);
                                    }
                                }
                            }
                        }
                        if (listResp.Count() == 0) { throw new Exception("no records found for given serach criteria."); }

                        //Logging the info to aws cloudwatch
                        log.LogMessage = $"Colour records({listResp.Count()}) fetched successfully based on search criteria.";
                        LogService.LogInfoData(log, _logger);

                        List<ColoursDTO> encryptedData = await Encrypt_Decrypt_Data(listResp, "Encrypt");
                        return await Task.Run(() => encryptedData);
                    }
                    if (result.Count() == 0)
                    {
                        throw new Exception("No records found for given search criteria.");
                    }

                    //Logging the info to aws cloudwatch                    
                    log.LogMessage = $"Colour records({result.Count()}) fetched successfully based on search criteria.";
                    LogService.LogInfoData(log, _logger);

                    List<ColoursDTO> encryptedData1 = await Encrypt_Decrypt_Data(result.ToList(), "Encrypt");
                    return await Task.Run(() => encryptedData1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<Object> GetColourByID(string ColourID, string userEmail, CancellationToken ct = default)
        {
            try
            {
                ColoursDTO colourItem = _context.ColoursDBSet.Where(x => x.ID == ColourID).FirstOrDefault();
                if (colourItem == null) { throw new Exception("Colour record is not available."); }

                #region Permission Code
                //string isGet = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;

                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to get " + module); }
                //    isGet = Convert.ToString(accessIfo.item1.isGet);
                //}
                //else { throw new Exception("Insufficient permission to get " + module); }
                //if (isGet == "False") { throw new Exception("Insufficient permission to get " + module); }

                //bool isEdit = Convert.ToString(accessIfo.item1.isEdit) != "False" ? true : false;
                //bool isDelete = Convert.ToString(accessIfo.item1.isDelete) != "False" ? true : false;
                #endregion

                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;

                if (role != "Administrator" && colourItem.CreatedByID != userId)
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

                var color = (from colr in _context.ColoursDBSet
                             where colr.ID == ColourID
                             select new { ID = colr.ID, imageId = colr.Thumbnail }).FirstOrDefault();

                dynamic FileList = null;
                string thumbnail_Id = color.imageId;
                //Get the File Item
                if (!string.IsNullOrEmpty(thumbnail_Id))
                {
                    object fileRes = await _plFileInterface.GetFileByAsync(thumbnail_Id);
                    dynamic res = (dynamic)fileRes;
                    if (res != null)
                    {
                        if (res.item2 == "Error")
                        {
                            throw new Exception(Convert.ToString(res.item1));
                        }

                        //Logging the info to aws cloudwatch                        
                        log.LogMessage = $"Colour thumbnail(File item) fetched successfully with fileid - {thumbnail_Id}.";
                        LogService.LogInfoData(log, _logger);
                        FileList = res.item1;
                    }
                }

                var result = (from colour in _context.ColoursDBSet
                              where colour.ID == ColourID
                              select new ColoursDTO
                              {
                                  Sequence = colour.Sequence,
                                  ID = colour.ID,
                                  Name = colour.Name,
                                  Classification = colour.Classification,
                                  ColorStandard = colour.ColorStandard,
                                  Description = colour.Description,
                                  R = colour.R,
                                  B = colour.B,
                                  ColourSwatch = colour.ColourSwatch,
                                  Hexcode = colour.Hexcode,
                                  InternalRef = colour.InternalRef,
                                  G = colour.G,
                                  PantoneCode = colour.PantoneCode,
                                  ModifiedOn = colour.ModifiedOn,
                                  CreatedOn = colour.CreatedOn,
                                  Status = colour.Status,
                                  ThumbnailFiles = FileList,
                                  DeleteStatus = colour.DeleteStatus,
                                  DeletedOn = colour.DeletedOn,
                                  IsEdit = true,
                                  IsDelete = true,
                                  IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                       where favourite.Module == module && favourite.RecordId == colour.ID
                                                                       && favourite.FavouriteTo == userEmail
                                                                       select favourite.ID).FirstOrDefault()) ? true : false
                              }).FirstOrDefault();

                //Logging the info to aws cloudwatch                
                log.LogMessage = $"Colour record fetched successfully with id - {ColourID}.";
                LogService.LogInfoData(log, _logger);

                ////Logging UserActivity Logs In MongoDB
                //var activityLog = LogService.UserActivityLog("Get", result, null);

                List<ColoursDTO> colList = new List<ColoursDTO>();
                colList.Add(result);

                var encryptedData = await Encrypt_Decrypt_Data(colList, "Encrypt");
                ColoursDTO encryptData = encryptedData[0];
                return await Task.Run(() => encryptData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<object> GetColoursFromIds(List<string> colourIds, string userEmail, CancellationToken ct = default)
        {
            try
            {
                #region Permission Code
                //string isGet = string.Empty;
                //dynamic accessParam = new JObject();
                //accessParam.Module = module;

                //object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                //dynamic accessIfo = (dynamic)accessRes;
                //if (accessIfo != null)
                //{
                //    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to get " + module); }
                //    isGet = Convert.ToString(accessIfo.item1.isGet);
                //}
                //else { throw new Exception("Insufficient permission to get " + module); }
                //if (isGet == "False") { throw new Exception("Insufficient permission to get " + module); }
                #endregion

                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;

                if (role != "Administrator")
                {
                    if (colourIds.Count() > 0)
                    {
                        var colItems1 = _context.ColoursDBSet.Where(x => colourIds.Contains(x.ID)).Select(x => new { ID = x.ID, CreatedByID = x.CreatedByID }).ToList();
                        List<string> colIdList = new List<string>();
                        foreach (var item in colItems1)
                        {
                            if (item.CreatedByID != userId) { colIdList.Add(item.ID); }
                        }

                        if (colIdList.Count() > 0)
                        {
                            string commaSepIds = string.Join(',', colIdList);
                            object getRes = await _plCoreInterface.GetListOfAccessDetails(module, commaSepIds, "Get", ct);
                            dynamic getSharedDetails = (dynamic)getRes;
                            if (getSharedDetails != null)
                            {
                                if (getSharedDetails.item2 == "Error") { throw new Exception(Convert.ToString(getSharedDetails.item1)); }
                            }
                        }
                    }
                }

                IList<ColoursDTO> colObjs = (from col in _context.ColoursDBSet
                                             where colourIds.Contains(col.ID)
                                             select col).ToList();

                var colourThumbnailItems = _context.ColoursDBSet.Where(x => colourIds.Contains(x.ID))
                                .Select(x => new { thumbnailId = x.Thumbnail, colourId = x.ID }).ToList();
                Dictionary<string, string> colourImgQueryList = new Dictionary<string, string>();
                List<string> imageIds = new List<string>();
                for (int i = 0; i < colourThumbnailItems.Count; i++)
                {
                    var colImageItem = colourThumbnailItems[i];
                    string colourId = colImageItem.colourId;
                    var image_ID = colImageItem.thumbnailId;
                    if (!string.IsNullOrEmpty(image_ID))
                    {
                        imageIds.Add(image_ID);
                        if (!colourImgQueryList.ContainsKey(colourId))
                        {
                            colourImgQueryList.Add(colourId, image_ID);
                        }
                    }
                }

                Dictionary<string, object> fileObjList = new Dictionary<string, object>();
                if (imageIds.Count() > 0)
                {
                    var getFilesList = await _plFileInterface.GetFilesFromIDsAsync(imageIds, ct);
                    dynamic filesObj = (dynamic)getFilesList;
                    if (filesObj != null)
                    {
                        if (filesObj.item2 == "Error") { throw new Exception(Convert.ToString(filesObj.item1)); }
                        string FileJsonString = Convert.ToString(filesObj.item1);
                        List<object> FileListvalues = JsonConvert.DeserializeObject<List<object>>(FileJsonString);
                        if (FileListvalues.Count() > 0)
                        {
                            foreach (var item in FileListvalues)
                            {
                                dynamic fileItem = (dynamic)item;
                                string fileId = fileItem.id;
                                fileObjList.Add(fileId, fileItem);
                            }
                        }
                    }
                }

                Dictionary<string, object> ImgFilesList = new Dictionary<string, object>();
                if (fileObjList.Count() > 0)
                {
                    foreach (var item in colourImgQueryList)
                    {
                        if (!ImgFilesList.ContainsKey(item.Key))
                        {
                            ImgFilesList.Add(item.Key, fileObjList.GetValueOrDefault(item.Value));
                        }
                    }
                }

                IList<ColoursDTO> colItems = new List<ColoursDTO>();
                foreach (var colour in colObjs)
                {
                    colour.CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null;
                    string colThumbID = colour.Thumbnail;
                    if (!string.IsNullOrEmpty(colThumbID))
                    {
                        //Logging the info to aws cloudwatch
                        log.LoggedBy = CommonDeclarations.userEmail;
                        log.LogMessage = $"Colour thumbnail(File item) fetched successfully with fileid - {colThumbID}.";
                        colour.ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null;
                    }
                    colItems.Add(colour);
                }

                string IdsList = colourIds.Count() > 0 ? String.Join(",", colourIds) : "'No Ids'";

                //Logging the info to aws cloudwatch                
                log.LogMessage = $"Colour records fetched successfully with id's - {IdsList}.";
                LogService.LogInfoData(log, _logger);

                ////Logging UserActivity Logs In MongoDB
                //var ActivityLog = LogService.UserActivityLog("Get", colourIds, null);

                var encryptedData = await Encrypt_Decrypt_Data(colObjs.ToList(), "Encrypt");
                return await Task.Run(() => encryptedData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<object> CheckColourDocuments(List<string> docIds, string userEmail, CancellationToken ct = default)
        {
            try
            {
                int count = _context.ColourDocumentsDBSet.Where(x => docIds.Contains(x.DocumentId)).Count();
                if (count > 0)
                {
                    if (docIds.Count() == 1)
                    {
                        throw new Exception("Delete operation failed. document is used in colour-document relationship. ");
                    }
                    else if (docIds.Count() > 1)
                    {
                        throw new Exception("Delete operation failed. Some of the documents are used in colour-document relationship.");
                    }
                }

                string IdsList = docIds.Count() > 0 ? String.Join(",", docIds) : "'No Ids'";

                //Logging the info to aws cloudwatch                
                log.LogMessage = $"Checked colour and document relations successfully with document ids - {IdsList}.";
                LogService.LogInfoData(log, _logger);

                ////Logging UserActivity Logs in MongoDB
                //var ActivityLog = LogService.UserActivityLog("CheckColour", docIds, null);

                return await Task.Run(() => "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        public async Task<List<ColoursDTO>> Encrypt_Decrypt_Data(List<ColoursDTO> colourObjs, string operation)
        {
            try
            {
                string key = _configuration.GetSection("Encrypt_Decrypt")["ColourKey"];

                if (operation == "Encrypt")
                {
                    foreach (var colour in colourObjs)
                    {
                        colour.B = !string.IsNullOrEmpty(colour.B) ? await _encrypt_Decrypt.Encrypt(colour.B, key) : colour.B;
                        colour.Classification = !string.IsNullOrEmpty(colour.Classification) ? await _encrypt_Decrypt.Encrypt(colour.Classification, key) : colour.Classification;
                        colour.ColorStandard = !string.IsNullOrEmpty(colour.ColorStandard) ? await _encrypt_Decrypt.Encrypt(colour.ColorStandard, key) : colour.ColorStandard;
                        colour.ColourSwatch = !string.IsNullOrEmpty(colour.ColourSwatch) ? await _encrypt_Decrypt.Encrypt(colour.ColourSwatch, key) : colour.ColourSwatch;
                        colour.Description = !string.IsNullOrEmpty(colour.Description) ? await _encrypt_Decrypt.Encrypt(colour.Description, key) : colour.Description;
                        colour.G = !string.IsNullOrEmpty(colour.G) ? await _encrypt_Decrypt.Encrypt(colour.G, key) : colour.G;
                        colour.Hexcode = !string.IsNullOrEmpty(colour.Hexcode) ? await _encrypt_Decrypt.Encrypt(colour.Hexcode, key) : colour.Hexcode;
                        colour.InternalRef = !string.IsNullOrEmpty(colour.InternalRef) ? await _encrypt_Decrypt.Encrypt(colour.InternalRef, key) : colour.InternalRef;
                        colour.Name = !string.IsNullOrEmpty(colour.Name) ? await _encrypt_Decrypt.Encrypt(colour.Name, key) : colour.Name;
                        colour.PantoneCode = !string.IsNullOrEmpty(colour.PantoneCode) ? await _encrypt_Decrypt.Encrypt(colour.PantoneCode, key) : colour.PantoneCode;
                        colour.R = !string.IsNullOrEmpty(colour.R) ? await _encrypt_Decrypt.Encrypt(colour.R, key) : colour.R;
                        colour.Sequence = !string.IsNullOrEmpty(colour.Sequence) ? await _encrypt_Decrypt.Encrypt(colour.Sequence, key) : colour.Sequence;
                        colour.Status = !string.IsNullOrEmpty(colour.Status) ? await _encrypt_Decrypt.Encrypt(colour.Status, key) : colour.Status;
                        colour.Org = !string.IsNullOrEmpty(colour.Org) ? await _encrypt_Decrypt.Encrypt(colour.Org, key) : colour.Org;
                        if (!string.IsNullOrEmpty(colour.Thumbnail))
                        {
                            if (colour.ThumbnailFiles != null)
                            {
                                string fileName = colour.ThumbnailFiles.name;
                                string fileThumbnail = colour.ThumbnailFiles.thumbnail;
                                string fileOrg = colour.ThumbnailFiles.org;

                                colour.ThumbnailFiles.name = !string.IsNullOrEmpty(fileName) ? await _encrypt_Decrypt.Encrypt(fileName, key) : null;
                                colour.ThumbnailFiles.thumbnail = !string.IsNullOrEmpty(fileThumbnail) ? await _encrypt_Decrypt.Encrypt(fileThumbnail, key) : null;
                                colour.ThumbnailFiles.org = !string.IsNullOrEmpty(fileOrg) ? await _encrypt_Decrypt.Encrypt(fileOrg, key) : null;
                            }
                        }

                        if (!string.IsNullOrEmpty(colour.CreatedByID))
                        {
                            if (colour.CreatedBy != null)
                            {
                                string email = colour.CreatedBy.email;
                                string fname = colour.CreatedBy.firstName;
                                string lname = colour.CreatedBy.lastName;
                                string thumbnail = colour.CreatedBy.imageUrl;
                                colour.CreatedBy.email = !string.IsNullOrEmpty(email) ? await _encrypt_Decrypt.Encrypt(email, key) : null;
                                colour.CreatedBy.firstName = !string.IsNullOrEmpty(fname) ? await _encrypt_Decrypt.Encrypt(fname, key) : null;
                                colour.CreatedBy.lastName = !string.IsNullOrEmpty(lname) ? await _encrypt_Decrypt.Encrypt(lname, key) : null;
                                colour.CreatedBy.imageUrl = !string.IsNullOrEmpty(thumbnail) ? await _encrypt_Decrypt.Encrypt(thumbnail, key) : null;
                            }
                        }

                        if (!string.IsNullOrEmpty(colour.ModifiedByID))
                        {
                            if (colour.ModifiedBy != null)
                            {
                                string email = colour.ModifiedBy.email;
                                string fname = colour.ModifiedBy.firstName;
                                string lname = colour.ModifiedBy.lastName;
                                string thumbnail = colour.ModifiedBy.imageUrl;
                                colour.ModifiedBy.email = !string.IsNullOrEmpty(email) ? await _encrypt_Decrypt.Encrypt(email, key) : null;
                                colour.ModifiedBy.firstName = !string.IsNullOrEmpty(fname) ? await _encrypt_Decrypt.Encrypt(fname, key) : null;
                                colour.ModifiedBy.lastName = !string.IsNullOrEmpty(lname) ? await _encrypt_Decrypt.Encrypt(lname, key) : null;
                                colour.ModifiedBy.imageUrl = !string.IsNullOrEmpty(thumbnail) ? await _encrypt_Decrypt.Encrypt(thumbnail, key) : null;
                            }
                        }
                    }
                }
                else if (operation == "Decrypt")
                {
                    foreach (var colour in colourObjs)
                    {
                        colour.B = !string.IsNullOrEmpty(colour.B) ? await _encrypt_Decrypt.Decrypt(colour.B, key) : colour.B;
                        colour.Classification = !string.IsNullOrEmpty(colour.Classification) ? await _encrypt_Decrypt.Decrypt(colour.Classification, key) : colour.Classification;
                        colour.ColorStandard = !string.IsNullOrEmpty(colour.ColorStandard) ? await _encrypt_Decrypt.Decrypt(colour.ColorStandard, key) : colour.ColorStandard;
                        colour.ColourSwatch = !string.IsNullOrEmpty(colour.ColourSwatch) ? await _encrypt_Decrypt.Decrypt(colour.ColourSwatch, key) : colour.ColourSwatch;
                        colour.Description = !string.IsNullOrEmpty(colour.Description) ? await _encrypt_Decrypt.Decrypt(colour.Description, key) : colour.Description;
                        colour.G = !string.IsNullOrEmpty(colour.G) ? await _encrypt_Decrypt.Decrypt(colour.G, key) : colour.G;
                        colour.Hexcode = !string.IsNullOrEmpty(colour.Hexcode) ? await _encrypt_Decrypt.Decrypt(colour.Hexcode, key) : colour.Hexcode;
                        colour.InternalRef = !string.IsNullOrEmpty(colour.InternalRef) ? await _encrypt_Decrypt.Decrypt(colour.InternalRef, key) : colour.InternalRef;
                        colour.Name = !string.IsNullOrEmpty(colour.Name) ? await _encrypt_Decrypt.Decrypt(colour.Name, key) : colour.Name;
                        colour.PantoneCode = !string.IsNullOrEmpty(colour.PantoneCode) ? await _encrypt_Decrypt.Decrypt(colour.PantoneCode, key) : colour.PantoneCode;
                        colour.R = !string.IsNullOrEmpty(colour.R) ? await _encrypt_Decrypt.Decrypt(colour.R, key) : colour.R;
                        colour.Status = !string.IsNullOrEmpty(colour.Status) ? await _encrypt_Decrypt.Decrypt(colour.Status, key) : colour.Status;
                    }
                }
                return await Task.Run(() => colourObjs);
            }
            catch (Exception)
            {
                throw;
            }
        }

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
        public async Task<Object> GetAllSharedColours(string userEmail, string recordCount, CancellationToken ct = default)
        {
            try
            {
                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;
                string userOrg = CommonDeclarations.userOrg;

                var coloursWithThumbnail = (from colours in _context.ColoursDBSet
                                            where colours.Thumbnail != null && colours.Thumbnail != ""
                                            select new { id = colours.ID, image = colours.Thumbnail }).ToList();

                Dictionary<string, string> colourImgQueryList = new Dictionary<string, string>();
                List<string> imageIds = new List<string>();
                for (int i = 0; i < coloursWithThumbnail.Count; i++)
                {
                    var colourImageItem = coloursWithThumbnail[i];
                    string materialId = colourImageItem.id;
                    var image_ID = colourImageItem.image;
                    if (!string.IsNullOrEmpty(image_ID))
                    {
                        imageIds.Add(image_ID);
                        if (!colourImgQueryList.ContainsKey(materialId))
                        {
                            colourImgQueryList.Add(materialId, image_ID);
                        }
                    }
                }

                Dictionary<string, object> fileObjList = new Dictionary<string, object>();
                if (imageIds.Count() > 0)
                {
                    var getFilesList = await _plFileInterface.GetFilesFromIDsAsync(imageIds, ct);
                    dynamic filesObj = (dynamic)getFilesList;
                    if (filesObj != null)
                    {
                        if (filesObj.item2 == "Error") { throw new Exception(Convert.ToString(filesObj.item1)); }
                        string FileJsonString = Convert.ToString(filesObj.item1);
                        List<object> FileListvalues = JsonConvert.DeserializeObject<List<object>>(FileJsonString);
                        if (FileListvalues.Count() > 0)
                        {
                            foreach (var item in FileListvalues)
                            {
                                dynamic fileItem = (dynamic)item;
                                string fileId = fileItem.id;
                                fileObjList.Add(fileId, fileItem);
                            }
                        }
                    }
                }

                Dictionary<string, object> ImgFilesList = new Dictionary<string, object>();
                if (fileObjList.Count() > 0)
                {
                    foreach (var item in colourImgQueryList)
                    {
                        ImgFilesList.Add(item.Key, fileObjList.GetValueOrDefault(item.Value));
                    }
                }
                //Logging the info to aws cloudwatch
                log.LogMessage = $"Colour thumbnails(File items) fetched successfully.";
                LogService.LogInfoData(log, _logger);

                if (!string.IsNullOrEmpty(recordCount))
                {
                    int startIndex = 0;
                    int maxIndex = 0;

                    startIndex = Convert.ToInt32(recordCount.Split('-')[0].ToString());
                    maxIndex = Convert.ToInt32(recordCount.Split('-')[1].ToString());

                    if (startIndex > maxIndex)
                    {
                        throw new Exception("start index shouldn't be more then maxIndex. Format e:g [0-50]");
                    }
                    if (string.IsNullOrEmpty(startIndex.ToString()) || string.IsNullOrEmpty(maxIndex.ToString()))
                    {
                        throw new Exception("Record count parameter value not in a proper format e:g [0-50]");
                    }

                    object getSharedRecords = await _plCoreInterface.GetSharedRecords(module, ct);
                    dynamic sharedRecords = (dynamic)getSharedRecords;
                    List<string> SharedcolIds = new List<string>();
                    if (sharedRecords != null)
                    {
                        if (sharedRecords.item2 == "Error") { throw new Exception(Convert.ToString(sharedRecords.item1)); }
                        string recordids = Convert.ToString(sharedRecords.item1.recordids);
                        if (!string.IsNullOrEmpty(recordids))
                        {
                            string[] list = recordids.Split(',');
                            for (int i = 0; i < list.Length; i++)
                            {
                                SharedcolIds.Add(list[i]);
                            }
                        }
                    }

                    /// Added lazyloading filter to the result
                    var filterResult = SharedcolIds.Skip(startIndex).Take(maxIndex - startIndex).ToList();

                    CommonDeclarations.totalRocordsCount = SharedcolIds.Count();

                    var sharedres = (from colour in _context.ColoursDBSet
                                     where filterResult.Contains(colour.ID)
                                     orderby colour.Name
                                     select new ColoursDTO
                                     {
                                         ID = colour.ID,
                                         Name = colour.Name,
                                         Description = colour.Description,
                                         Classification = colour.Classification,
                                         ColorStandard = colour.ColorStandard,
                                         R = colour.R,
                                         B = colour.B,
                                         ColourSwatch = colour.ColourSwatch,
                                         Hexcode = colour.Hexcode,
                                         InternalRef = colour.InternalRef,
                                         G = colour.G,
                                         PantoneCode = colour.PantoneCode,
                                         CreatedByID = colour.CreatedByID,
                                         CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                                         Status = colour.Status,
                                         Sequence = colour.Sequence,
                                         Thumbnail = colour.Thumbnail,
                                         ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null,
                                         IsEdit = true,
                                         IsDelete = true,
                                         DeleteStatus = colour.DeleteStatus,
                                         DeletedOn = colour.DeletedOn,
                                         IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                              where favourite.Module == module && favourite.RecordId == colour.ID
                                                                              && favourite.FavouriteTo == userEmail
                                                                              select favourite.ID).FirstOrDefault()) ? true : false
                                     }).ToList();

                    //Logging the info to aws cloudwatch
                    log.LogMessage = $"All shared colour records({sharedres.Count()}) fetched successfully.";
                    LogService.LogInfoData(log, _logger);


                    var encryptedData = await Encrypt_Decrypt_Data(sharedres.GroupBy(x => x.ID).Select(g => g.FirstOrDefault()).ToList(), "Encrypt");
                    return await Task.Run(() => encryptedData);
                }
                else
                {
                    object getSharedRecords = await _plCoreInterface.GetSharedRecords(module, ct);
                    dynamic sharedRecords = (dynamic)getSharedRecords;
                    List<string> SharedcolIds = new List<string>();
                    if (sharedRecords != null)
                    {
                        if (sharedRecords.item2 == "Error") { throw new Exception(Convert.ToString(sharedRecords.item1)); }
                        string recordids = Convert.ToString(sharedRecords.item1.recordids);
                        if (!string.IsNullOrEmpty(recordids))
                        {
                            string[] list = recordids.Split(',');
                            for (int i = 0; i < list.Length; i++)
                            {
                                SharedcolIds.Add(list[i]);
                            }
                        }
                    }

                    CommonDeclarations.totalRocordsCount = SharedcolIds.Count();

                    var sharedres = (from colour in _context.ColoursDBSet
                                     where SharedcolIds.Contains(colour.ID)
                                     orderby colour.CreatedOn
                                     select new ColoursDTO
                                     {
                                         ID = colour.ID,
                                         Name = colour.Name,
                                         Description = colour.Description,
                                         Classification = colour.Classification,
                                         ColorStandard = colour.ColorStandard,
                                         R = colour.R,
                                         B = colour.B,
                                         ColourSwatch = colour.ColourSwatch,
                                         Hexcode = colour.Hexcode,
                                         InternalRef = colour.InternalRef,
                                         G = colour.G,
                                         PantoneCode = colour.PantoneCode,
                                         CreatedByID = colour.CreatedByID,
                                         CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                                         Status = colour.Status,
                                         Sequence = colour.Sequence,
                                         Thumbnail = colour.Thumbnail,
                                         ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null,
                                         IsEdit = true,
                                         IsDelete = true,
                                         DeleteStatus = colour.DeleteStatus,
                                         DeletedOn = colour.DeletedOn,
                                         IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                              where favourite.Module == module && favourite.RecordId == colour.ID
                                                                              && favourite.FavouriteTo == userEmail
                                                                              select favourite.ID).FirstOrDefault()) ? true : false
                                     }).ToList();

                    //Logging the info to aws cloudwatch
                    log.LogMessage = $"All shared colour records({sharedres.Count()}) fetched successfully.";
                    LogService.LogInfoData(log, _logger);

                    ////Logging UserActivityLogs in MongoDB
                    //var ActivityLog = LogService.UserActivityLog("Get", null, recordCount);

                    var encryptedData = await Encrypt_Decrypt_Data(sharedres.GroupBy(x => x.ID).Select(g => g.FirstOrDefault()).ToList(), "Encrypt");
                    return await Task.Run(() => encryptedData);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This api service method is use to return shared colours from db based on record count.
        /// </summary>
        /// <param name="recordCount"></param>
        /// <param name="ImgFilesList"></param>
        /// <param name="ct"></param>
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        private async Task<List<ColoursDTO>> GetColourSharedData(string recordCount, Dictionary<string, object> ImgFilesList, CancellationToken ct = default)
        {
            try
            {
                int startIndex = 0;
                int maxIndex = 0;
                if (!string.IsNullOrEmpty(recordCount))
                {
                    startIndex = Convert.ToInt32(recordCount.Split('-')[0].ToString());
                    maxIndex = Convert.ToInt32(recordCount.Split('-')[1].ToString());

                    if (startIndex > maxIndex)
                    {
                        throw new Exception("start index shouldn't be more then maxIndex. Format e:g [0-50]");
                    }
                    if (string.IsNullOrEmpty(startIndex.ToString()) || string.IsNullOrEmpty(maxIndex.ToString()))
                    {
                        throw new Exception("Record count parameter value not in a proper format e:g [0-50]");
                    }
                }

                object getSharedRecords = await _plCoreInterface.GetSharedRecords(module, ct);
                dynamic sharedRecords = (dynamic)getSharedRecords;
                List<string> SharedcolIds = new List<string>();
                if (sharedRecords != null)
                {
                    if (sharedRecords.item2 == "Error") { throw new Exception(Convert.ToString(sharedRecords.item1)); }
                    string recordids = Convert.ToString(sharedRecords.item1.recordids);
                    if (!string.IsNullOrEmpty(recordids))
                    {
                        string[] list = recordids.Split(',');
                        for (int i = 0; i < list.Length; i++)
                        {
                            SharedcolIds.Add(list[i]);
                        }
                    }
                }

                CommonDeclarations.totalRocordsCount = SharedcolIds.Count();
                CommonDeclarations.totalSharedCount = SharedcolIds.Count();
                var filterResult = new List<string>();
                if (!string.IsNullOrEmpty(recordCount))
                {
                    /// Added lazyloading filter to the result
                    filterResult = SharedcolIds.Skip(startIndex).Take(maxIndex - startIndex).ToList();
                }
                else
                {
                    filterResult = SharedcolIds.ToList();
                }

                var sharedres = (from colour in _context.ColoursDBSet
                                 where filterResult.Contains(colour.ID)
                                 orderby colour.Name
                                 select new ColoursDTO
                                 {
                                     ID = colour.ID,
                                     Name = colour.Name,
                                     Description = colour.Description,
                                     Classification = colour.Classification,
                                     ColorStandard = colour.ColorStandard,
                                     R = colour.R,
                                     B = colour.B,
                                     ColourSwatch = colour.ColourSwatch,
                                     Hexcode = colour.Hexcode,
                                     InternalRef = colour.InternalRef,
                                     G = colour.G,
                                     PantoneCode = colour.PantoneCode,
                                     CreatedByID = colour.CreatedByID,
                                     CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                                     Status = colour.Status,
                                     Sequence = colour.Sequence,
                                     Thumbnail = colour.Thumbnail,
                                     ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null,
                                     IsEdit = true,
                                     IsDelete = true,
                                     DeleteStatus = colour.DeleteStatus,
                                     DeletedOn = colour.DeletedOn,
                                     IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                          where favourite.Module == module && favourite.RecordId == colour.ID
                                                                          && favourite.FavouriteTo == CommonDeclarations.userEmail
                                                                          select favourite.ID).FirstOrDefault()) ? true : false
                                 }).ToList();

                //Logging the info to aws cloudwatch
                log.LogMessage = $"All shared colour records({sharedres.Count()}) fetched successfully.";
                LogService.LogInfoData(log, _logger);

                ////Logging Useractivitylog in MongoDB
                //var activitylog = LogService.UserActivityLog("Get", ImgFilesList, null);

                return await Task.Run(() => sharedres);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// This api service method is use to return the colours from db based on record count.
        /// </summary>
        /// <param name="recordCount"></param>
        /// <param name="ImgFilesList"></param>        
        /// <returns>returns object value based on paramters</returns>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation and Review is pending</remarks>
        private async Task<List<ColoursDTO>> GetColourCreatedData(string recordCount, Dictionary<string, object> ImgFilesList)
        {
            try
            {
                string userId = CommonDeclarations.userID;
                string role = CommonDeclarations.userRole;
                string userOrg = CommonDeclarations.userOrg;

                CommonDeclarations.totalCreatedCount = 0;
                int startIndex = 0;
                int maxIndex = 0;
                if (!string.IsNullOrEmpty(recordCount))
                {
                    startIndex = Convert.ToInt32(recordCount.Split('-')[0].ToString());
                    maxIndex = Convert.ToInt32(recordCount.Split('-')[1].ToString());

                    if (startIndex > maxIndex)
                    {
                        throw new Exception("start index shouldn't be more then maxIndex. Format e:g [0-50]");
                    }
                    if (string.IsNullOrEmpty(startIndex.ToString()) || string.IsNullOrEmpty(maxIndex.ToString()))
                    {
                        throw new Exception("Record count parameter value not in a proper format e:g [0-50]");
                    }
                }

                if ((!string.IsNullOrEmpty(role) && role == "Administrator") && !string.IsNullOrEmpty(userOrg))
                {

                    var totalRowCount = _context.ColoursDBSet.Where(w => w.Org == userOrg);
                    CommonDeclarations.totalRocordsCount = totalRowCount.Count();
                    CommonDeclarations.totalCreatedCount = totalRowCount.Count();
                    var filterResult = new List<ColoursDTO>();

                    if (!string.IsNullOrEmpty(recordCount))
                    {
                        filterResult = _context.ColoursDBSet.OrderByDescending(o => o.CreatedOn)
                                        .Where(w => w.Org == userOrg)
                                        .Skip(startIndex)
                                        .Take(maxIndex - startIndex).ToList();
                    }
                    else
                    {
                        filterResult = _context.ColoursDBSet.OrderByDescending(o => o.CreatedOn)
                                        .Where(w => w.Org == userOrg)
                                        .ToList();
                    }

                    var result = filterResult
                        .Select(colour => new ColoursDTO
                        {
                            ID = colour.ID,
                            Name = colour.Name,
                            Description = colour.Description,
                            Classification = colour.Classification,
                            ColorStandard = colour.ColorStandard,
                            R = colour.R,
                            B = colour.B,
                            ColourSwatch = colour.ColourSwatch,
                            Hexcode = colour.Hexcode,
                            InternalRef = colour.InternalRef,
                            G = colour.G,
                            PantoneCode = colour.PantoneCode,
                            CreatedByID = colour.CreatedByID,
                            CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                            Status = colour.Status,
                            Sequence = colour.Sequence,
                            Thumbnail = colour.Thumbnail,
                            ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null,
                            IsEdit = true,
                            IsDelete = true,
                            DeleteStatus = colour.DeleteStatus,
                            DeletedOn = colour.DeletedOn,
                            IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                 where favourite.Module == module && favourite.RecordId == colour.ID
                                                                 && favourite.FavouriteTo == CommonDeclarations.userEmail
                                                                 select favourite.ID).FirstOrDefault()) ? true : false
                        }).ToList();


                    //Logging the info to aws cloudwatch                    
                    log.LogMessage = $"All colour records({result.Count()}) fetched successfully.";
                    LogService.LogInfoData(log, _logger);

                    //var encryptedData = await Encrypt_Decrypt_Data(result.ToList(), "Encrypt");
                    return await Task.Run(() => result);
                }
                else
                {
                    var totalRowCount = _context.ColoursDBSet.Where(w => w.Org == userOrg || w.Org == null || w.Org == "").Where(u => u.CreatedByID == userId);
                    CommonDeclarations.totalRocordsCount = totalRowCount.Count();
                    CommonDeclarations.totalCreatedCount = totalRowCount.Count();

                    var filterResult = new List<ColoursDTO>();

                    if (!string.IsNullOrEmpty(recordCount))
                    {
                        filterResult = _context.ColoursDBSet.OrderByDescending(o => o.CreatedOn)
                        .Where(w => w.Org == userOrg || w.Org == null || w.Org == "").Where(u => u.CreatedByID == userId)
                        .Skip(startIndex)
                        .Take(maxIndex - startIndex).ToList();
                    }
                    else
                    {
                        filterResult = _context.ColoursDBSet.OrderByDescending(o => o.CreatedOn)
                         .Where(w => w.Org == userOrg || w.Org == null || w.Org == "").Where(u => u.CreatedByID == userId)
                         .ToList();
                    }

                    var result = filterResult
                        .Select(colour => new ColoursDTO
                        {
                            ID = colour.ID,
                            Name = colour.Name,
                            Description = colour.Description,
                            Classification = colour.Classification,
                            ColorStandard = colour.ColorStandard,
                            R = colour.R,
                            B = colour.B,
                            ColourSwatch = colour.ColourSwatch,
                            Hexcode = colour.Hexcode,
                            InternalRef = colour.InternalRef,
                            G = colour.G,
                            PantoneCode = colour.PantoneCode,
                            CreatedByID = colour.CreatedByID,
                            CreatedBy = !string.IsNullOrEmpty(colour.CreatedByID) ? Common.GetUserDetails(colour.CreatedByID, "", _configuration.GetConnectionString("DefaultConnection"), _configuration) : null,
                            Status = colour.Status,
                            Sequence = colour.Sequence,
                            Thumbnail = colour.Thumbnail,
                            ThumbnailFiles = ImgFilesList.ContainsKey(colour.ID) ? ImgFilesList.GetValueOrDefault(colour.ID) : null,
                            IsEdit = true,
                            IsDelete = true,
                            DeleteStatus = colour.DeleteStatus,
                            DeletedOn = colour.DeletedOn,
                            IsFavourite = !string.IsNullOrEmpty((from favourite in _context.FavouritesDBSet
                                                                 where favourite.Module == module && favourite.RecordId == colour.ID
                                                                 && favourite.FavouriteTo == CommonDeclarations.userEmail
                                                                 select favourite.ID).FirstOrDefault()) ? true : false
                        }).ToList();

                    //Logging the info to aws cloudwatch
                    log.LogMessage = $"All colour records({result.Count()}) fetched successfully.";
                    LogService.LogInfoData(log, _logger);

                    ////Logging UseractivityLogs in MongoDb
                    //var activitylog = LogService.UserActivityLog("Get", recordCount, null);

                    return await Task.Run(() => result);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objExcel"></param>
        /// <param name="userEmail"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Object> AddExcelColour(List<ColoursDTO> objExcel, string userEmail, CancellationToken ct = default)
        {
            try
            {
                string isAdd = string.Empty;
                dynamic accessParam = new JObject();
                accessParam.Module = module;
                
                object accessRes = await _plPermissionInterface.GetUserPermissions(accessParam, CommonDeclarations.accessToken, ct);
                dynamic accessIfo = (dynamic)accessRes;
                if (accessIfo != null)
                {
                    if (accessIfo.item2 == "Error") { throw new Exception("Insufficient permission to add " + module); }
                    isAdd = Convert.ToString(accessIfo.item1.isAdd);
                }
                else { throw new Exception("Insufficient permission to add " + module); }
                if (isAdd == "False") { throw new Exception("Insufficient permission to add " + module); }

                string userOrg = Convert.ToString(accessIfo.item1.userOrg);

                foreach (var item in objExcel)
                {
                    item.Sequence = _plSequenceService.GetNextSequence(module, userOrg);
                    item.CreatedByID = item.ModifiedByID = Convert.ToString(accessIfo.item1.userID);
                    item.Org = Convert.ToString(accessIfo.item1.userOrg);
                }

                _context.ColoursDBSet.AddRange(objExcel);
                _context.SaveChanges();

                var encryptedData = await Encrypt_Decrypt_Data(objExcel, "Encrypt");
                return await Task.Run(() => encryptedData[0]);

            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
    }
}
