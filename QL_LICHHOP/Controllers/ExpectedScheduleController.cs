using QL_LICHHOP.Models;
using QL_LICHHOP.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_LICHHOP.Controllers
{
    public class ExpectedScheduleController : Controller
    {
        MeetingRepository meetingRepository = new MeetingRepository();
        DepartmentRepository departmentRepository = new DepartmentRepository();
        ScheduleTypeRepository scheduleTypeRepository = new ScheduleTypeRepository();
        // GET: ExpectedSchedule
        public ActionResult Index(DateTime? selectedDate, string scheduleName, string registerer, string host, string status)
        {
            DateTime currentDate = selectedDate ?? DateTime.Today;

            // Xác định ngày đầu tuần (Thứ Hai) và cuối tuần (Chủ Nhật)
            DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            // Lấy số tuần theo chuẩn ISO 8601
            int weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                startOfWeek, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday
            );

            // Điều chỉnh tuần 1 nếu tuần có ít nhất một ngày trong tuần đầu tiên của năm
            if (startOfWeek.Year != endOfWeek.Year || (endOfWeek.Month == 1 && endOfWeek.Day <= 7))
            {
                weekNumber = 1;
            }
            else if (weekNumber == 1 && startOfWeek.Month == 1 && startOfWeek.Day > 7)
            {
                weekNumber = 2;
            }

            ViewBag.SelectedDate = currentDate;
            ViewBag.StartOfWeek = startOfWeek;
            ViewBag.EndOfWeek = endOfWeek;
            ViewBag.WeekNumber = weekNumber;

            ViewBag.ScheduleType = scheduleTypeRepository.GetScheduleTypes();
            // Lấy danh sách lịch họp trong tuần
            var schedules = meetingRepository.GenerateWeeklySchedule(startOfWeek, endOfWeek);

            // Lọc theo các tiêu chí tìm kiếm nếu có
            if (!string.IsNullOrEmpty(scheduleName))
            {
                schedules = schedules.Where(s =>
                    (s.MorningSession != null && s.MorningSession.Any(ms => ms.ScheduleName.IndexOf(scheduleName, StringComparison.OrdinalIgnoreCase) >= 0)) ||
                    (s.AfternoonSession != null && s.AfternoonSession.Any(afs => afs.ScheduleName.IndexOf(scheduleName, StringComparison.OrdinalIgnoreCase) >= 0))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(host))
            {
                schedules = schedules.Where(s =>
                    (s.MorningSession != null && s.MorningSession.Any(ms => ms.Host.Any(h => h.IndexOf(host, StringComparison.OrdinalIgnoreCase) >= 0))) ||
                    (s.AfternoonSession != null && s.AfternoonSession.Any(afs => afs.Host.Any(h => h.IndexOf(host, StringComparison.OrdinalIgnoreCase) >= 0)))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(registerer))
            {
                schedules = schedules.Where(s =>
                    (s.MorningSession != null && s.MorningSession.Any(ms => ms.CreatedBy.IndexOf(registerer, StringComparison.OrdinalIgnoreCase) >= 0)) ||
                    (s.AfternoonSession != null && s.AfternoonSession.Any(afs => afs.CreatedBy.IndexOf(registerer, StringComparison.OrdinalIgnoreCase) >= 0))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                schedules = schedules.Where(s =>
                    (s.MorningSession != null && s.MorningSession.Any(ms => ms.Status.Contains(status))) ||
                    (s.AfternoonSession != null && s.AfternoonSession.Any(afs => afs.Status.Contains(status)))
                ).ToList();
            }

            return View(schedules);
        }





    }
}