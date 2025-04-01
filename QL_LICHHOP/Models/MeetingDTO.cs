using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Models
{
    public class MeetingDTO
    {
        public int MeetingID { get; set; }
        public string StartTime { get; set; }
        public string OldDate { get; set; }
        public string NewDate { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public List<string> Host { get; set; }
        public List<string> Participants { get; set; }
        public List<string> Departments { get; set; }
        public string RegistrationPlace { get; set; }
        public string CreatedBy { get; set; }
        public string ScheduleName { get; set; }
        public string Status { get; set; }
        public List<string> AttachmentUrls { get; set; }
        public List<string> ParticipantsOrDepartment { get; set; }
    }
}