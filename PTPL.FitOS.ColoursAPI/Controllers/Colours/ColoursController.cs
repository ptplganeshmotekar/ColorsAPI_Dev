using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using PTPL.FitOS.DataModels;
using PTPL.FitOS.Services;
//using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.IO;
using OfficeOpenXml;

namespace PTPL.FitOS.ColoursAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Colours")]
    [ApiController]
#pragma warning disable
    public class ColoursController : ControllerBase
    {

        #region Fields

        /// <summary>
        /// Field declarations
        /// </summary>
        private readonly IColourServices _plColoursService;
        private IConfiguration _configuration;
        private readonly IPermissionInterface _plPermissionInterface;
        private readonly ICoreInterface _plCoreInterface;
        LogDTO log = new LogDTO();
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public ColoursController(IColourServices plColoursService, IConfiguration Configuration, IPermissionInterface plPermissionInterface, ICoreInterface plCoreInterface)
        {
            log.ApplicationType = CommonDeclarations.ApplicationType;
            log.Module = CommonDeclarations.module;
            log.ClassType = "Controller-Level";
            log.LoggedTime = DateTime.Now;
            _plColoursService = plColoursService;
            _configuration = Configuration;
            _plPermissionInterface = plPermissionInterface;
            _plCoreInterface = plCoreInterface;
        }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// This endpoint will use to Add colours based on parameters.
        /// </summary>
        /// <param name="plColours"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>
        [HttpPost, Route("AddColour")]
        public async Task<IActionResult> AddColour(ColoursDTO plColours, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'AddColour'.";
                LogService.LogInfoData(log, _logger);

                var result = await _plColoursService.AddColour(plColours, userEmail, ct);

                await AddRecentActivity(plColours.ID, "Added new colour");

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colour created successfully.";
                LogService.LogInfoData(log, _logger);

                //Logging With The UserActivityLog in MongoDB
                //var ActivityLog = LogService.UserActivityLog("Add", null, plColours);

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

        /// <summary>
        /// This endpoint will use to Update colours based on parameters.
        /// </summary>
        /// <param name="plColours"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>
        [HttpPost, Route("UpdateColourById")]
        public async Task<IActionResult> UpdateColourById(ColoursDTO plColours, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'UpdateColourById'.";
                LogService.LogInfoData(log, _logger);

                var result = await _plColoursService.UpdateColour(plColours, userEmail, ct);

                await AddRecentActivity(plColours.ID, "Update colour by id");

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colour updated successfully.";
                LogService.LogInfoData(log, _logger);

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

        /// <summary>
        /// This endpoint will use to update multiple colours at a time based on parameters.(Bulk-Update)
        /// </summary>
        /// <param name="plColours"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>        
        [HttpPost, Route("BulkUpdateColour")]
        public async Task<IActionResult> BulkUpdateColour(List<ColoursDTO> plColours, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'BulkUpdateColour'.";
                LogService.LogInfoData(log, _logger);

                var result = await _plColoursService.BulkUpdateColour(plColours, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Bulk colours updated successfully.";
                LogService.LogInfoData(log, _logger);
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

        /// <summary>
        /// This endpoint will use to Get/Fetch colours based on parameters.
        /// </summary>                  
        /// <param name="Type"></param>
        /// <param name="RecordCount"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>  
        [HttpGet, Route("GetAllColours")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetAllColours(string Type, string? RecordCount = null, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'GetAllColours'.";
                LogService.LogInfoData(log, _logger);

                var res = await _plColoursService.GetAllColours(userEmail, RecordCount, Type, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colours retrieved/fetched successfully.";
                LogService.LogInfoData(log, _logger);
                return new OkObjectResult(new Tuple<object, string, int>(res, "Success", CommonDeclarations.totalRocordsCount));
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
        /// This endpoint will use to delete colours based on RecordID.
        /// </summary>
        /// <param name="RecordID"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>  
        [HttpPost, Route("DeleteColourByID")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> DeleteColourByID(string RecordID, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'DeleteColourByID'.";
                LogService.LogInfoData(log, _logger);

                var result = await _plColoursService.DeleteColourByID(RecordID, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colour deleted successfully.";
                LogService.LogInfoData(log, _logger);
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

        /// <summary>
        /// This endpoint will use to delete multiple colours at a time based on parameters.(Bulk-Delete)
        /// </summary>
        /// <param name="deleteIds"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>  
        [HttpPost, Route("BulkDeleteColour")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> BulkDeleteColour(DeleteBulk deleteIds, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'BulkDeleteColour'.";
                LogService.LogInfoData(log, _logger);

                var result = await _plColoursService.BulkDeleteColours(deleteIds, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Bulk colours deleted successfully.";
                LogService.LogInfoData(log, _logger);
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

        /// <summary>
        ///This endpoint will used for advanced search of colours.
        /// </summary>
        /// <param name="AdvancedSearchObj"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>  
        [HttpPost("AdvancedSearch")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> AdvancedSearch(AdvancedSearchEntity AdvancedSearchObj, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'AdvancedSearch'.";
                LogService.LogInfoData(log, _logger);

                var res = await _plColoursService.AdvancedSearchForColours(AdvancedSearchObj, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colours fetched successfully based on search criteria.";
                LogService.LogInfoData(log, _logger);
                return new OkObjectResult(new Tuple<object, string>(res, "Success"));
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
        /// This endpoint will use to get/fetch list of colours based on colourids list.
        /// </summary>
        /// <param name="colIds"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// /// <remarks>Code documentation Pending</remarks>  
        [HttpPost, Route("GetColoursFromIds")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetColoursFromIds(List<string> colIds, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'GetColoursFromIds'.";
                LogService.LogInfoData(log, _logger);

                var res = await _plColoursService.GetColoursFromIds(colIds, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colours fetched successfully based on colourId's.";
                LogService.LogInfoData(log, _logger);
                return new OkObjectResult(new Tuple<object, string>(res, "Success"));
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
        /// This endpoint will use to get/fetch colour details based on colour ID.
        /// </summary>
        /// <param name="RecordID"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// /// <remarks>Code documentation Pending</remarks>  
        [HttpGet, Route("GetColourByID")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetColoursByID(string RecordID, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'GetColourByID'.";
                LogService.LogInfoData(log, _logger);

                var res = await _plColoursService.GetColourByID(RecordID, userEmail, ct);

                await AddRecentActivity(RecordID, "Get Colour By Id");

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colour fetched successfully based on colourId.";
                LogService.LogInfoData(log, _logger);

                return new OkObjectResult(new Tuple<object, string>(res, "Success"));
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
        /// This endpoint will use to check colour-document relationship existance based on colour Id's list.
        /// </summary>
        /// <param name="docIds"></param>             
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// /// <remarks>Code documentation Pending</remarks>  
        [HttpPost, Route("CheckColourDocuments")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> CheckColourDocuments(List<string> docIds, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'CheckColourDocuments'.";
                LogService.LogInfoData(log, _logger);

                var res = await _plColoursService.CheckColourDocuments(docIds, userEmail, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Checked colour documents relationship successfully..";
                LogService.LogInfoData(log, _logger);
                return new OkObjectResult(new Tuple<object, string>(res, "Success"));
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
        /// This endpoint will use to get/fetch shared colours based on user login.
        /// </summary>                  
        /// <param name="RecordCount"></param>
        /// <param name="ct"></param>
        /// <exception cref="Error-StatusCode-400"></exception>
        /// <exception cref="Error-StatusCode-404"></exception>
        /// <exception cref="Error-StatusCode-500"></exception>
        /// <exception cref="Too many connection"></exception>
        /// <exception cref="Connection Timed Out"></exception>
        /// <exception cref="Sql parameter mismatch"></exception>
        /// <exception cref="user token expired."></exception>
        /// <remarks>Code documentation Pending</remarks>  
        [HttpGet, Route("GetAllSharedColours")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetAllSharedColours(string? RecordCount = null, CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'GetAllSharedColours'.";
                LogService.LogInfoData(log, _logger);

                var res = await _plColoursService.GetAllSharedColours(userEmail, RecordCount, ct);

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "All shared colours retrieved/fetched successfully.";
                LogService.LogInfoData(log, _logger);
                return new OkObjectResult(new Tuple<object, string, int>(res, "Success", CommonDeclarations.totalRocordsCount));
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
            public string userOrg { get; set; }
            public string userEmail { get; set; }
            public string userId { get; set; }
            public string userRole { get; set; }
            public Logger logger { get; set; }
        }

        /// <summary>
        /// This method will use to Add recenetly worked item into database.
        /// </summary>        
        /// <param name="RecordId"></param>
        /// <param name="Description"></param>
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

        [HttpPost, Route("UploadColours")]
        //public async Task<DemoResponse<List<ColoursDTO>>> Import(IFormFile formFile, CancellationToken cancellationToken)
        public async Task<IActionResult> UploadColours(CancellationToken ct = default)
        {
            try
            {
                UserClass userObj = await getuserdetails();
                string userEmail = userObj.userEmail;
                Logger _logger = userObj.logger;

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "API call made to colour application, API name - 'Upload Excel Colour data'.";
                LogService.LogInfoData(log, _logger);

                IFormFile formFile = this.HttpContext.Request.Form.Files[0];

                #region Validation
                if (formFile == null || formFile.Length <= 0)
                {
                    return new OkObjectResult(new Tuple<object, string>("Uploaded file is empty", "Error"));
                }

                if (!Path.GetExtension(formFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase) && (!Path.GetExtension(formFile.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase)))
                {
                    return new OkObjectResult(new Tuple<object, string>("Not Support file extension", "Error"));
                }
                #endregion

                #region Loading data into clours class DTO
                var colourList = new List<ColoursDTO>();
                using (var stream = new MemoryStream())
                {
                    await formFile.CopyToAsync(stream, ct);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;
                        var colCount = worksheet.Dimension.Columns;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            for (int col = 1; col <= colCount; col++)
                            {
                                if (worksheet.Cells[row, col].Value == null)
                                {
                                    worksheet.Cells[row, col].Value = string.Empty;
                                }
                            }

                            colourList.Add(new ColoursDTO
                            {
                                Classification = worksheet.Cells[row, 1].Value.ToString().Trim(),
                                Name = worksheet.Cells[row, 2].Value.ToString().Trim(),
                                Status = worksheet.Cells[row, 3].Value.ToString().Trim(),
                                InternalRef = worksheet.Cells[row, 4].Value.ToString().Trim(),
                                Description = worksheet.Cells[row, 5].Value.ToString().Trim(),
                                Hexcode = worksheet.Cells[row, 6].Value.ToString().Trim(),
                                ColorStandard = worksheet.Cells[row, 7].Value.ToString().Trim(),
                                PantoneCode = worksheet.Cells[row, 8].Value.ToString().Trim()
                            });
                        }
                    }
                }
                #endregion

                if (colourList.Count <= 0)
                {
                    return new OkObjectResult(new Tuple<object, string>("Error occured while mapping excel objects to Datamodel class .", "Error"));
                }
                /// Service level method to save colors data into DB
                var result = await _plColoursService.AddExcelColour(colourList, userEmail, ct);

                //await AddRecentActivity(plColours.ID, "Added new colour");

                //Logging the info to aws cloudwatch
                log.LoggedBy = userEmail; log.LogMessage = "Colour excel data uploaded successfully.";
                LogService.LogInfoData(log, _logger);

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
        #endregion
    }
}
