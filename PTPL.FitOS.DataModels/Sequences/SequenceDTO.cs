using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PTPL.FitOS.DataModels
{
    /// <summary>
    /// This class method is use to declare datamodel class as ReferenceEntity
    /// </summary>
    /// <returns>Returns data value based on request parameters</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    [Table("PlSequence")]
    public class SequenceDTO : BaseEntity
    {
        [Required]
        [MaxLength(36)]
        public string Name { get; set; }

        [MaxLength(36)]
        public string Prefix { get; set; }
        public int CurrentValue { get; set; }
        public int Padding { get; set; }
    }
}
