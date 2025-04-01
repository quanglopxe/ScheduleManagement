using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.ViewModels
{
    public class MeetingParticipantViewModel
    {
        public int? DepartmentID { get; set; }
        public int? UserID { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
    }
}