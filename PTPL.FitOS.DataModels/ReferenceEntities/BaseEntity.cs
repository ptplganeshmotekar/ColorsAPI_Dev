using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PTPL.FitOS.DataModels
{
    /// <summary>
    /// This class method is use to declare datamodel class as BaseEntity
    /// </summary>
    /// <returns>Returns data value based on request parameters</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [MaxLength(36)]
        public string ID { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Classification { get; set; }
        public string Org { get; set; }
        public string Status { get; set; }
        public string CreatedByID { get; set; }
        public string ModifiedByID { get; set; }
        [NotMapped]
        public bool IsFavourite { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool DeleteStatus { get; set; }
    }
}
