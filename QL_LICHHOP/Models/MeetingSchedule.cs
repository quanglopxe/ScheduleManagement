using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Models
{
    public class MeetingSchedule
    {
        public string DayOfWeek { get; set; }  // Thứ trong tuần
        public DateTime Date { get; set; }  // Ngày
        public List<MeetingDTO> MorningSession { get; set; }  // Nội dung buổi sáng
        public List<MeetingDTO> AfternoonSession { get; set; }  // Nội dung buổi chiều
    }
}