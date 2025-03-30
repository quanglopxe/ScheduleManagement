using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Models
{
    public class MeetingDTO
    {
        public string StartTime { get; set; }
        public string Title { get; set; }
        public List<string> Host { get; set; }
        public List<string> Participants { get; set; }
        public string RegistrationPlace { get; set; }
        public string CreatedBy { get; set; }
        public string ScheduleName { get; set; }
        public string Status { get; set; }
        public List<string> AttachmentUrls { get; set; }
    }
}