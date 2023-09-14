using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PTPL.FitOS.DataModels
{
    /// <summary>
    /// This class method is use to declare datamodel class as FavouritesDTO
    /// </summary>
    /// <returns>Returns data value based on request parameters</returns>
    /// <remarks>Code documentation and Review is pending</remarks>
    [Table("PlFavourites")]
    public class FavouritesDTO : BaseEntity
    {
        #region Fields
        [MaxLength(50)]
        public string Module { get; set; }
        [MaxLength(36)]
        public string RecordId { get; set; }
        [MaxLength(50)]
        public string FavouriteTo { get; set; }

        #endregion
    }
}
