using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PTPL.FitOS.DataModels
{
    public class FileDTOHelperClass
    {
        public string Name { get; set; }
        public string ParentRef { get; set; }
        public string FileType { get; set; }
        public string Thumbnail { get; set; }
        [NotMapped]
        public string Base64URL { get; set; }

        [NotMapped]
        public List<string> ThumbnailList { get; set; }
    }
}
