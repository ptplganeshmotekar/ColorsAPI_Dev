//using MongoDB.Bson.Serialization.Attributes;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace PTPL.FitOS.DataModels
//{
//    public class UserActivityLogDTO
//    {
//        [BsonId]
//        public Guid UserActivityLogId { get; set; } = Guid.NewGuid();

//        public string UserId { get; set; }
//        public string UserEmail { get; set; }
//        public string ControllerName { get; set; }
//        public string ActionName { get; set; }
//        public string RoleId { get; set; }
//        public string IpAddress { get; set; }
//        public DateTime LoggedInAt { get; set; }
//        public DateTime LoggedOutAt { get; set; }
//        public string PageAccessed { get; set; }
//        public string OldData { get; set; }
//        public string NewData { get; set; }

//    }
//}
