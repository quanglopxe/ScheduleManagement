using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.ViewModels
{
    public class MeetingViewModel
    {
        public int MeetingID { get; set; }
        [Required]
        public string Title { get; set; }        
        public string RegistrationPlace { get; set; }
        public int DepartmentId { get; set; }
        [Required]
        public int ScheduleTypeID { get; set; }
        [Required]
        public string ScheduleType { get; set; }
        [Required]
        public DateTime? StartTime { get; set; }
        [Required]
        public int DurationMinutes { get; set; }
        public string Location { get; set; }
        public string VehicleType { get; set; }
        public string Preparation { get; set; }
        //public string Status { get; set; }        

        public int HostUserID { get; set; }  // Người chủ trì cuộc họp
        public List<MeetingParticipantViewModel> Participants { get; set; } = new List<MeetingParticipantViewModel>();

        public List<HttpPostedFileBase> Attachments { get; set; }  // Danh sách file đính kèm
        public List<string> AttachmentPaths { get; set; }
    }
}