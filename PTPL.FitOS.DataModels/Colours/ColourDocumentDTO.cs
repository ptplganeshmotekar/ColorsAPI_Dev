using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace PTPL.FitOS.DataModels
{
    /// <summary>
    /// This class method is use to declare datamodel class as ReferenceEntity
    /// </summary>
    /// <returns>Returns data value based on request parameters</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    [Table("PlColourDocuments")]
    public class ColourDocumentDTO : BaseEntity
    {
        #region Fields
        public string ColourId { get; set; }
        
        public ColoursDTO Colour { get; set; }
        public string DocumentId { get; set; }
        
        [NotMapped]
        public dynamic Document { get; set; }
        #endregion
    }
}
