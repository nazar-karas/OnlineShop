using Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class LogData
    {
        public string UserEmail { get; set; }
        public DateTime Date { get; set; }
        public string ObjectType { get; set; }
        public Guid ObjectId { get; set; }
        public string Action { get; set; }
        public string Error { get; set; }

        public LogData(string userEmail, DateTime date, ObjectType objectType, Guid objectId, ActionType action, string error)
        {
            UserEmail = userEmail;
            Date = date;
            ObjectType = objectType.ToString();
            ObjectId = objectId;
            Action = action.ToString();
            Error = error;
        }
    }
}
