using System;
using System.Collections.Generic;
using System.Text;

namespace PTPL.FitOS.DataModels
{
    /// <summary>
    /// This class method is use to declare datamodel class as LogDTO
    /// </summary>
    /// <returns>Returns data value based on request parameters</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    public class LogDTO
    {
        public string ApplicationType { get; set; }
        public string LoggedBy { get; set; }
        public string Organization { get; set; }
        public string LogType { get; set; }
        public string Module { get; set; }
        public string ClassType { get; set; }
        public DateTime LoggedTime { get; set; }
        public string LogMessage { get; set; }
    }
}
