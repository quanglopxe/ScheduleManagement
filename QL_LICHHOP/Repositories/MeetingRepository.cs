using QL_LICHHOP.Models;
using QL_LICHHOP.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace QL_LICHHOP.Repositories
{
    public class MeetingRepository
    {
        private QL_LICHHOPDataContext _context;
        public MeetingRepository()
        {
            _context = new QL_LICHHOPDataContext();
        }
        // Lấy danh sách cuộc họp
        public List<Meeting> GetAllMeetings()
        {
            return _context.Meetings.ToList();
        }
        public List<MeetingSchedule> GenerateWeeklySchedule(DateTime startOfWeek, DateTime endOfWeek)
        {
            List<MeetingSchedule> schedule = new List<MeetingSchedule>();

            for (int i = 0; i < 7; i++)
            {
                DateTime currentDate = startOfWeek.AddDays(i).Date;

                var morningMeeting = GetMeetingByTimeOfDay(currentDate, new List<string> { "sáng", "cả ngày" });
                var afternoonMeeting = GetMeetingByTimeOfDay(currentDate, new List<string> { "chiều" });

                schedule.Add(new MeetingSchedule
                {
                    DayOfWeek = CultureInfo.GetCultureInfo("vi-VN").DateTimeFormat.GetDayName(currentDate.DayOfWeek),
                    Date = currentDate,
                    MorningSession = morningMeeting,
                    AfternoonSession = afternoonMeeting
                });
            }

            return schedule;
        }
        private List<MeetingDTO> GetMeetingByTimeOfDay(DateTime date, List<string> scheduleTypes)
        {
            return _context.Meetings
                .Where(m => m.StartTime.Date == date && scheduleTypes.Contains(m.ScheduleType.ToLower())) // Lọc theo sáng/chiều/cả ngày
                // Kết nối với bảng ScheduleTypes để lấy Name từ ScheduleTypeID
                .Join(_context.ScheduleTypes,
                    m => m.ScheduleTypeID,
                    s => s.ScheduleTypeID,
                    (m, s) => new { m, ScheduleName = s.Name }) // Lấy ScheduleName từ bảng ScheduleTypes
                // Lấy danh sách Host từ bảng MeetingHosts
                .GroupJoin(_context.MeetingHosts,
                    ms => ms.m.MeetingID,
                    mh => mh.MeetingID,
                    (ms, hosts) => new { ms.m, ms.ScheduleName, hosts })
                // Lấy danh sách Participants từ MeetingParticipants
                .GroupJoin(_context.MeetingParticipants,
                    mh => mh.m.MeetingID,
                    mp => mp.MeetingID,
                    (mh, participants) => new { mh.m, mh.ScheduleName, mh.hosts, participants })
                // Kết nối với bảng Departments để lấy tên phòng ban
                .Join(_context.Departments,
                    m => m.m.DepartmentID,
                    d => d.DepartmentID,
                    (m, d) => new { m.m, m.ScheduleName, m.hosts, m.participants, DepartmentName = d.DepartmentName })
                .ToList()
                .Select(x => new MeetingDTO
                {
                    StartTime = x.m.StartTime.ToString("HH:mm"),
                    Title = x.m.Title,
                    // Lấy Host từ MeetingHosts
                    Host = x.hosts
                        .Join(_context.Users,
                            mh => mh.UserID,
                            u => u.UserID,
                            (mh, u) => u.FullName)
                        .ToList(),
                    // Lấy danh sách người tham dự từ bảng MeetingParticipants
                    Participants = x.participants
                        .Join(_context.Users,
                            mp => mp.UserID,
                            u => u.UserID,
                            (mp, u) => u.FullName)
                        .ToList(),
                    RegistrationPlace = x.DepartmentName,
                    CreatedBy = x.m.CreatedBy,
                    // Lấy ScheduleName từ bảng ScheduleTypes
                    ScheduleName = x.ScheduleName,
                    Status = x.m.Status,
                    AttachmentUrls = _context.MeetingAttachments
                        .Where(a => a.MeetingID == x.m.MeetingID)
                        .Select(a => a.FilePath)
                        .ToList()
                })
                .ToList();
        }




        public int GetWeekOfYear(DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        // Thêm cuộc họp mới
        public void AddMeeting(MeetingViewModel model, List<string> newParticipants, List<int> selectedUsers, int selectedDepartment)
        {
            var newMeeting = new Meeting
            {
                Title = model.Title,
                RegistrationPlace = model.RegistrationPlace,
                DepartmentID = model.DepartmentId,
                ScheduleTypeID = model.ScheduleTypeID,
                ScheduleType = model.ScheduleType,
                StartTime = DateTime.Parse(model.StartTime),
                DurationMinutes = model.DurationMinutes,
                Location = model.Location,
                VehicleType = model.VehicleType,
                Preparation = model.Preparation,
                Status = "Chờ duyệt",
                CreatedBy = "Chưa xử lý",
                CreatedAt = DateTime.Now,
            };
            _context.Meetings.InsertOnSubmit(newMeeting);
            _context.SubmitChanges();
            
            int meetingId = newMeeting.MeetingID;
            var host = new MeetingHost
            {
                MeetingID = meetingId,
                UserID = model.HostUserID,
                CreatedAt = DateTime.Now,
                CreatedBy = "Chưa xử lý",
            };
            _context.MeetingHosts.InsertOnSubmit(host);
            _context.SubmitChanges();

            // Lưu nhân viên đã chọn
            if (selectedUsers != null)
            {
                foreach (var userId in selectedUsers)
                {
                    var meetingParticipant = new MeetingParticipant
                    {
                        MeetingID = newMeeting.MeetingID,
                        UserID = userId,
                        CreatedAt = DateTime.Now
                    };
                    _context.MeetingParticipants.InsertOnSubmit(meetingParticipant);
                }
            }

            // Tạo mới nhân viên nếu có
            if (newParticipants != null)
            {
                foreach (var fullName in newParticipants)
                {
                    var newUser = new User
                    {
                        FullName = fullName,
                        UserName = "guest" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        PasswordHash = "123",                        
                        DepartmentID = selectedDepartment,
                        Role = "Người dùng mới",
                        CreatedAt = DateTime.Now
                    };

                    _context.Users.InsertOnSubmit(newUser);
                    _context.SubmitChanges();

                    var meetingParticipant = new MeetingParticipant
                    {
                        MeetingID = newMeeting.MeetingID,
                        UserID = newUser.UserID,
                        CreatedAt = DateTime.Now
                    };

                    _context.MeetingParticipants.InsertOnSubmit(meetingParticipant);
                }
            }

            _context.SubmitChanges();

            foreach (var file in model.Attachments)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads"), fileName);
                    file.SaveAs(path);

                    var attachment = new MeetingAttachment
                    {
                        MeetingID = meetingId,                        
                        FilePath = fileName,
                        UploadedAt = DateTime.Now,
                        UploadedBy = "Chưa xử lý",
                    };
                    _context.MeetingAttachments.InsertOnSubmit(attachment);
                }
            }
            _context.SubmitChanges();
        }

        // Tìm cuộc họp theo ID
        public Meeting GetMeetingById(int id)
        {
            return _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
        }
    }
}