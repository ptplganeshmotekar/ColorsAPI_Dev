using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace PTPL.FitOS.DataModels
{
    /// <summary>
    /// This class method is use to declare datamodel class as ReferenceEntity
    /// </summary>
    /// <returns>Returns data value based on request parameters</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    [Table("PlColours")]
    public class ColoursDTO : BaseEntity
    {
        #region Fields
        public string Name { get; set; }        
        public string InternalRef { get; set; }
        public string Description { get; set; }
        public string ColorStandard { get; set; }
        public string ColourSwatch { get; set; }
        public string R { get; set; }
        public string G { get; set; }
        public string B { get; set; }
        public string PantoneCode { get; set; }
        public string Hexcode { get; set; }
        public string Thumbnail { get; set; }
        public string Sequence { get; set; }
                       
        #region Relationship Properties
        public IList<ColourDocumentDTO> ColourDocuments { get; set; }

        #endregion Relationship Properties

        #region NotMapped Properties        
        [NotMapped]
        public string MaterialColourStatus { get; set; }
        [NotMapped]
        public dynamic Documents { get; set; }
        [NotMapped]
        public dynamic SupplierColours { get; set; }
        [NotMapped]
        public dynamic ThumbnailFiles { get; set; }
        [NotMapped]
        public dynamic CreatedBy { get; set; }
        [NotMapped]
        public dynamic ModifiedBy { get; set; }
        [NotMapped]
        public bool IsEdit { get; set; }
        [NotMapped]
        public bool IsDelete { get; set; }
        #endregion

        #endregion
    } 
}
