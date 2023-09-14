using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using PTPL.FitOS.DataContext;
using PTPL.FitOS.DataModels;
using PTPL.FitOS.LibraryAPI.ResponseMessages;
using PTPL.FitOS.Services;
using static System.Net.Mime.MediaTypeNames;

namespace PTPL.FitOS.ColoursAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/ColourDocuments")]
    [ApiController]
    #pragma warning disable
    public class ColourDocumentController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// Field declarations
        /// </summary>
        string module = "Colours";
        private IConfiguration _configuration;
        private readonly IDocumentInterface _plPdocumentInterface;
        private readonly IFileInterface _plFileInterface;
        private readonly InsertUpdateDBContext _context;
        private readonly IPermissionInterface _plPermissionInterface;
        private readonly ICoreInterface _plCoreInterface;
        private readonly IEncrypt_DecryptService _encrypt_Decrypt;
        private readonly IColourDocumentService _colourDocumentService;        
        private readonly GetDataDBContext _dbContext;        
        LogDTO log = new LogDTO();
        #endregion Fields

        #region Constructor

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        
        public ColourDocumentController(GetDataDBContext dbContext, IColourDocumentService colourDocumentService, IConfiguration Configuration,
            IDocumentInterface documentInterface,IFileInterface fileInterface,InsertUpdateDBContext context,IPermissionInterface permissionInterface,
            ICoreInterface coreInterface,IEncrypt_DecryptService encrypt_DecryptService)
        {            
            log.ApplicationType = CommonDeclarations.ApplicationType;
            log.Module = CommonDeclarations.module;            
            log.ClassType = "Controller-Level";
            log.LoggedTime = DateTime.Now;
            _dbContext = dbContext;            
            _colourDocumentService = colourDocumentService;           
            _configuration = Configuration;
            _plPdocumentInterface = documentInterface;
            _plFileInterface = fileInterface;
            _context = context;
            _plPermissionInterface = permissionInterface;
            _plCoreInterface = coreInterface;
            _encrypt_Decrypt = encrypt_DecryptService;
        }
        #endregion Constructor

        #region Methods

        /// <summary>
        /// This endpoint will use to Add/Update/Delete colour-document relationships based on form data.
        /// </summary>  
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        [HttpPost, Route("AddUpdateDeleteColourDocument")]
        public async Task<IActionResult> AddUpdateDeleteColourDocument(CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'AddUpdateDeleteColourDocument'.";
                LogService.LogInfoData(log, _logger);

                string userID = userObj.userId;
                string role = userObj.userRole;
                string userOrg = userObj.userOrg;
                string token = CommonDeclarations.accessToken;
                string key = _configuration.GetSection("Encrypt_Decrypt")["DocumentKey"];

                if (string.IsNullOrEmpty(userID)) { throw new Exception("User not found."); }                

                string colourId = this.HttpContext.Request.Form["colorid"];
                string docsToDelete = this.HttpContext.Request.Form["docdelete"];

                ColoursDTO colourItem = _context.ColoursDBSet.Where(x => x.ID == colourId).FirstOrDefault();
                if (colourItem == null) { throw new Exception("Colour record is not available."); }

                if (role != "Administrator" && colourItem.CreatedByID != userID)
                {
                    object getRes = await _plCoreInterface.GetAccessDetails(module, colourItem.ID, ct);
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
                                throw new Exception($"Cannot update colour and relationships({colourItem.Name}) that is shared to you, due to insufficient permission.");
                            }
                        }
                        else
                        {
                            throw new Exception($"Insufficient permission to update colour and relationships(" + colourItem.Name + ").");
                        }
                    }
                }                                                                                

                if (!string.IsNullOrEmpty(docsToDelete))
                {
                    if (role != "Administrator" && colourItem.CreatedByID != userID)
                    {
                        object getRes = await _plCoreInterface.GetAccessDetails(module, colourId, ct);
                        dynamic getSharedDetails = (dynamic)getRes;
                        if (getSharedDetails != null)
                        {
                            if (getSharedDetails.item2 == "Error") { throw new Exception(Convert.ToString(getSharedDetails.item1)); }
                            string value = Convert.ToString(getSharedDetails.item1);
                            if (value.Contains("recordID"))
                            {
                                throw new Exception($"Cannot delete colour and relationships({colourItem.Name}) that is shared to you, due to insufficient permission.");
                            }
                            else
                            {
                                throw new Exception($"Insufficient permission to delete colour and relationships({colourItem.Name}).");
                            }
                        }
                    }

                    string[] deleteIds = docsToDelete.Split(',');
                    for (int k = 0; k < deleteIds.Length; k++)
                    {
                        object colorDocObjResult = await _colourDocumentService.DeleteColourDocument(colourId, deleteIds[k], userEmail, ct);                                                
                    }
                }

                int count = Convert.ToInt32(this.HttpContext.Request.Form["doccount"]);
                var fd = this.HttpContext.Request.Form;

                for (int d = 0; d < count; d++)
                {
                    string name = "";
                    string classification = "";
                    string base_Classification = "";
                    string docId = "";
                    string documentNo = "";
                    string description = "";
                    string labLocation = "";
                    string designatedUser = "";
                    string status = "";
                    string reportStatus = "";
                    string notes = "";
                    string tag = "";
                    string fileDelete = "";

                    tag = await _encrypt_Decrypt.Decrypt(fd["tag[" + d + "]"], key);
                    docId = fd["docid[" + d + "]"];
                    name = await _encrypt_Decrypt.Decrypt(fd["docname[" + d + "]"], key);
                    base_Classification = await _encrypt_Decrypt.Decrypt(fd["baseclassification[" + d + "]"], key);
                    classification = await _encrypt_Decrypt.Decrypt(fd["docclassification[" + d + "]"], key);
                    documentNo = await _encrypt_Decrypt.Decrypt(fd["documentNo[" + d + "]"], key);
                    description = await _encrypt_Decrypt.Decrypt(fd["docdescription[" + d + "]"], key);
                    labLocation = await _encrypt_Decrypt.Decrypt(fd["doclablocation[" + d + "]"], key);
                    designatedUser = await _encrypt_Decrypt.Decrypt(fd["designatedUser[" + d + "]"], key);
                    status = await _encrypt_Decrypt.Decrypt(fd["docstatus[" + d + "]"], key);
                    reportStatus = await _encrypt_Decrypt.Decrypt(fd["docreportstatus[" + d + "]"], key);
                    notes = await _encrypt_Decrypt.Decrypt(fd["docnotes[" + d + "]"], key);
                    fileDelete = fd["filedelete[" + d + "]"];

                    int docfileCount = Convert.ToInt32(fd["docfilecount[" + d + "]"]);

                    dynamic documentsItem = new JObject();
                    documentsItem.ID = docId;
                    documentsItem.BaseClassification = base_Classification;
                    documentsItem.Classification = classification;
                    documentsItem.Name = name;
                    documentsItem.DocumentNo = documentNo;
                    documentsItem.Description = description;
                    documentsItem.LabLocation = labLocation;
                    documentsItem.DesignatedUser = designatedUser;
                    documentsItem.Status = status;
                    documentsItem.ReportStatus = reportStatus;
                    documentsItem.Notes = notes;
                    documentsItem.Org = userOrg;

                    if (tag == "Add")
                    {
                        documentsItem.ID = "";
                        object docObject = await _plPdocumentInterface.AddDocumentAsync(documentsItem, token, ct);
                        dynamic file = (dynamic)docObject;
                        if (file != null)
                        {
                            if (file.item2 == "Error")
                            {
                                throw new Exception(Convert.ToString(file.item1));
                            }
                            docId = Convert.ToString(file.item1);
                        }
                        if (string.IsNullOrEmpty(docId) == false)
                        {
                            ColourDocumentDTO cdObj = new ColourDocumentDTO();
                            cdObj.ColourId = colourId;
                            cdObj.DocumentId = docId;
                            cdObj.CreatedByID = cdObj.ModifiedByID = userID;
                            cdObj.Org = userOrg;
                            ColourDocumentDTO colorDocObj = await _colourDocumentService.AddColourDocumentItem(cdObj, ct);
                            docId = colorDocObj.DocumentId;
                        }

                        dynamic sharedAccessItem = new JObject();
                        sharedAccessItem.ParentModule = module;
                        sharedAccessItem.ParentRecordID = colourId;
                        sharedAccessItem.ParentCreatedID = colourItem.CreatedByID;
                        sharedAccessItem.Module = "Documents";
                        sharedAccessItem.RecordID = docId;
                        sharedAccessItem.UserID = colourItem.CreatedByID;
                        sharedAccessItem.Edit = true;

                        object sharedRes = await _plCoreInterface.AddAccessDetails(sharedAccessItem, ct);
                        dynamic result = (dynamic)sharedRes;
                        if (result != null)
                        {
                            if (result.item2 == "Error")
                            {
                                throw new Exception(Convert.ToString(result.item1));
                            }
                        }
                    }
                    else if (tag == "Update")
                    {
                        object fileObject = await _plPdocumentInterface.UpdateDocumentAsync(documentsItem, token, ct);
                        dynamic file = (dynamic)fileObject;
                        if (file != null)
                        {
                            if (file.item2 == "Error")
                            {
                                throw new Exception(Convert.ToString(file.item1));
                            }
                        }
                    }                    

                    // If any files deleted from existing Document
                    if (string.IsNullOrEmpty(fileDelete) == false)
                    {
                        string[] fileArray = fileDelete.Split(',');
                        int fileArrayCount = fileArray.Length;
                        if (fileArrayCount > 0)
                        {
                            for (int f = 0; f < fileArrayCount; f++)
                            {
                                string fileId = fileArray[f];
                                object fileObject = await _plPdocumentInterface.DeleteDocumentFileAsync(docId, fileId, token, ct);
                                dynamic file = (dynamic)fileObject;
                                if (file != null)
                                {
                                    if (file.item2 == "Error")
                                    {
                                        throw new Exception(Convert.ToString(file.item1));
                                    }
                                }
                            }
                        }
                    }
                    int selectedFileIndex = 0;
                    for (var i = 0; i < docfileCount; i++)
                    {
                        string filepath = await _encrypt_Decrypt.Decrypt(fd["file" + d + "[" + selectedFileIndex + "]"],key);
                        if (string.IsNullOrEmpty(filepath) == false)
                        {
                            string lastFolderName = Path.GetFileName(Path.GetDirectoryName(filepath));
                            string filename = Path.GetFileName(filepath);
                            string filetype = Path.GetExtension(filepath);

                            dynamic fileItem = new JObject();
                            fileItem.ID = lastFolderName;
                            fileItem.Name = filename;
                            fileItem.FileType = filetype;
                            fileItem.CreatedByID = fileItem.ModifiedByID = userID;

                            object fileObject = await _plFileInterface.AddFileAsync(fileItem, ct);
                            dynamic file = (dynamic)fileObject;
                            if (file != null)
                            {
                                if (file.item2 == "Error")
                                {
                                    throw new Exception(Convert.ToString(file.item1));
                                }
                                //Logging the info to aws cloudwatch
                                log.LogMessage = $"File record(File item) created successfully with fileid - {lastFolderName}.";
                                LogService.LogInfoData(log, _logger);
                            }

                            dynamic fileobj = file.item1;
                            string fileId = Convert.ToString(fileobj.id);

                            dynamic docFileItem = new JObject();
                            docFileItem.DocumentID = docId;
                            docFileItem.FileID = fileId;

                            object docFileObj = await _plPdocumentInterface.AddDocumentFileAsync(docFileItem, token);
                            dynamic docFile = (dynamic)docFileObj;
                            if (docFile != null)
                            {
                                if (docFile.item2 == "Error")
                                {
                                    throw new Exception(Convert.ToString(docFile.item1));
                                }
                            }                            
                            selectedFileIndex++;
                        }
                    }
                }
                
                Object resultObj = await _colourDocumentService.GetColourDocumentsByColourID(colourId, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LogMessage = "Colour-Document relationship updated successfully.";
                LogService.LogInfoData(log, _logger);

                return new OkObjectResult(new Tuple<object, string>(resultObj, "Success"));
            }
            catch (Exception ex)
            {
                if (CommonDeclarations.logger == null)
                {
                    //initLogger(_configuration);
                }
                string errorMessage = "";
                log.LoggedBy = !string.IsNullOrEmpty(CommonDeclarations.userEmail) ? CommonDeclarations.userEmail : "UnAuthorized/Anonymous user";
                if (ex != null && ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage = ex.InnerException.Message;
                }
                else if (ex != null && !string.IsNullOrEmpty(ex.Message))
                {
                    errorMessage = ex.Message;
                }
                else if (ex != null)
                {
                    errorMessage = ex.ToString();
                }

                log.LogMessage = ex.ToString();
                LogService.LogErrorData(log, CommonDeclarations.logger);
                return new OkObjectResult(new Tuple<object, string>(errorMessage, "Error"));
            }
        }


        /// <summary>
        /// This endpoint will use to get colour and document relationship details based on colourID.
        /// </summary>  
        /// <param name="ct"></param>
        /// <param name="RecordID"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <remarks>Code documentation Pending</remarks>
        [HttpGet, Route("GetColourDocuments")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetColourDocuments(string RecordID, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'GetColourDocuments'.";
                LogService.LogInfoData(log, _logger);

                Object result = await _colourDocumentService.GetColourDocumentsByColourID(RecordID, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LogMessage = "Colour and Document relationship fetched successfully.";
                LogService.LogInfoData(log, _logger);

                await AddRecentActivity(RecordID, "Get Colour By Id");

                return new OkObjectResult(new Tuple<object, string>(result, "Success"));
            }
            catch (Exception ex)
            {
                if (CommonDeclarations.logger == null)
                {
                    //initLogger(_configuration);
                }
                string errorMessage = "";
                log.LoggedBy = !string.IsNullOrEmpty(CommonDeclarations.userEmail) ? CommonDeclarations.userEmail : "UnAuthorized/Anonymous user";
                if (ex != null && ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMessage = ex.InnerException.Message;
                }
                else if (ex != null && !string.IsNullOrEmpty(ex.Message))
                {
                    errorMessage = ex.Message;
                }
                else if (ex != null)
                {
                    errorMessage = ex.ToString();
                }

                log.LogMessage = ex.ToString();
                LogService.LogErrorData(log, CommonDeclarations.logger);
                return new OkObjectResult(new Tuple<object, string>(errorMessage, "Error"));
            }
        }

        #region Private Methods and classes
        
        /// <summary>
        /// This method will use to get/fetch loggedin user details.
        /// </summary>                          
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>
        private async Task<UserClass> getuserdetails()
        {
            try
            {
                UserClass userObj = new UserClass();
                string userOrg = string.Empty;
                string userID = string.Empty;
                string role = string.Empty;

                //Getting user token from Httpcontext(From userlogin)
                var token = await HttpContext.GetTokenAsync("Bearer", "access_token");
                if (token == null) { throw new Exception("User token expired."); }
                CommonDeclarations.accessToken = token;

                string tokenEmail = this.User.Claims.Count() > 0 ? this.User.Claims.SingleOrDefault(p => p.Type.Contains("emailaddress")).Value : null;
                string userEmail = !string.IsNullOrEmpty(tokenEmail) ? tokenEmail : _configuration["TestEmail"];
                if (string.IsNullOrEmpty(userEmail)) { throw new Exception("User token expired."); }
                CommonDeclarations.userEmail = userEmail;

                object getUserDetails = await _plPermissionInterface.GetUserDetail(CommonDeclarations.accessToken, default);
                dynamic userDetails = (dynamic)getUserDetails;
                if (userDetails != null)
                {
                    if (userDetails.item2 == "Error") { throw new Exception(Convert.ToString(userDetails.item1)); }
                    userID = Convert.ToString(userDetails.item1.id);
                    role = Convert.ToString(userDetails.item1.userrole);
                    userOrg = Convert.ToString(userDetails.item1.org);
                }

                userObj.userId = userID;
                userObj.userOrg = userOrg;
                userObj.userEmail = userEmail;
                userObj.userRole = role;

                //NLogConfigFile nLogWrapper = new NLogConfigFile(_configuration);
                //nLogWrapper.ConfigureNLog();
                //Logger _logger = LogManager.GetCurrentClassLogger();

                //Logging Details With Local and Server in MongoDB
                CommonDeclarations.PageAccessed = Request.HttpContext.Request.Path.ToString();
                Logger _logger = LogManager.GetLogger("appLog");

                userObj.logger = _logger;
                CommonDeclarations.logger = _logger;
                CommonDeclarations.userOrg = userOrg;
                CommonDeclarations.userID = userID;              
                CommonDeclarations.userRole = role;

                return userObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This method will use to initialize the logger.
        /// </summary>        
        /// <param name="config"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>  
        //private void initLogger(IConfiguration config)
        //{
        //    try
        //    {
        //        NLogConfigFile nLogWrapper = new NLogConfigFile(config);
        //        nLogWrapper.ConfigureNLog();
        //        Logger _logger = LogManager.GetCurrentClassLogger();
        //        CommonDeclarations.logger = _logger;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// Basic User class consists of user properties (It is like hepler class for 'getuserdetails' method.
        /// </summary>       
        private class UserClass
        {
            public string userID { get; set; }
            public string userOrg { get; set; }
            public string userEmail { get; set; }
            public string userId { get; set; }
            public string userRole { get; set; }
            public Logger logger { get; set; }
        }

        /// <summary>
        /// This method will use to Add recenetly worked item into database.
        /// </summary>        
        /// <param name="config"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>  
        private async Task<object> AddRecentActivity(string RecordId, string Description)
        {
            try
            {
                List<RecentActivityHelperClass> recentActivity = new List<RecentActivityHelperClass>();
                var recentActivityData = new RecentActivityHelperClass();
                recentActivityData.Module = "Colours";
                recentActivityData.RecordId = RecordId;
                recentActivityData.Description = Description;

                recentActivity.Add(recentActivityData);

                object raResult = await _plCoreInterface.AddRecentActivity(recentActivity, default);
                dynamic updateColourResult = (dynamic)raResult;
                if (updateColourResult != null)
                {
                    if (updateColourResult.item2 == "Error")
                    {
                        throw new Exception(Convert.ToString(updateColourResult.item1));
                    }
                }
                return "Success";
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #endregion Methods
    }
}


