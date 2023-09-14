using System;
using System.Collections.Generic;
using System.Text;

namespace PTPL.FitOS.DataModels
{
    /// <summary>
    /// This class method is use to declare datamodel class as AdvancedSearchEntity
    /// </summary>
    /// <returns>Returns data value based on request parameters</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    public class AdvancedSearchEntity
    {        
        public string Keyword { get; set; }
        public string Classification { get; set; }
        public string[] Status { get; set; }
        public DateTime? CreatedFrom { get; set; }        
        public DateTime? CreatedTo { get; set; }        
        public DateTime? ModifiedFrom { get; set; }        
        public DateTime? ModifiedTo { get; set; }        
        public string createdBy { get; set; }
        public string Sequence { get; set; }
    }
}
