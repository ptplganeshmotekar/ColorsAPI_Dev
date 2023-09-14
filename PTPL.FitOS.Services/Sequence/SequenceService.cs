using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using PTPL.FitOS.DataContext;
using PTPL.FitOS.DataModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PTPL.FitOS.Services
{
    /// <summary>
    /// This class consists of sequence related service methods.
    /// </summary>   
    public class SequenceService : ISequenceService
    {
        #region Fields      

        /// <summary>
        /// Field declarations
        /// </summary>
        private readonly DbSet<SequenceDTO> _plSequence;
        private readonly GetDataDBContext _dbContext;
        private readonly InsertUpdateDBContext _context;
        LogDTO log = new LogDTO();
        NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor of the class
        /// </summary>   
        public SequenceService(GetDataDBContext dbContext, InsertUpdateDBContext context, IConfiguration configuration)
        {
            log.ApplicationType = CommonDeclarations.ApplicationType;
            log.Module = CommonDeclarations.module;
            log.ClassType = "Service-Level";
            log.LoggedTime = DateTime.Now;            
            _context = context;
            _dbContext = dbContext;
            _plSequence = _context.SequenceDBSet;
        }
        #endregion

        #region Methods

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
        public string GetNextSequence(string sequenceName, string userOrg, CancellationToken ct = default)
        {
            try
            {
                SequenceDTO result = _dbContext.SequenceDBSet.FirstOrDefault(s => s.Name == sequenceName && s.Org == userOrg);
                if (result == null)
                {                    
                    string tempPrefix = sequenceName.Substring(0, 3).ToUpper();
                    SequenceDTO sequence = new SequenceDTO();
                    sequence.Org = userOrg;
                    sequence.Name = sequenceName;
                    sequence.Padding = 5;
                    sequence.Prefix = tempPrefix;
                    sequence.CurrentValue = 1;

                    _context.SequenceDBSet.Add(sequence);
                    _context.SaveChanges();

                    //Logging the info to aws cloudwatch                    
                    log.LogMessage = $"Sequence is created successfully with sequence id - {tempPrefix + "00001"}.";
                    LogService.LogInfoData(log, _logger);

                    return tempPrefix = tempPrefix + "00001";
                }
                else
                {
                    int currentValue = result.CurrentValue;
                    int nextValue = currentValue + 1;
                    result.CurrentValue = nextValue;
                    _context.Update(result);
                    _context.SaveChanges();
                    string nextNumber = "";
                    if (result.Padding > 0)
                    {
                        nextNumber = nextValue.ToString("D" + result.Padding);
                    }
                    else
                    {
                        nextNumber = nextValue.ToString();
                    }
                    string nextSequence = result.Prefix + nextNumber;

                    //Logging the info to aws cloudwatch                    
                    log.LogMessage = $"Sequence is created successfully with sequence id - {nextSequence}.";
                    LogService.LogInfoData(log, _logger);

                    return nextSequence;
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }

        #endregion Methods
    }
}
