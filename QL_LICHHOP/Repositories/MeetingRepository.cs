using QL_LICHHOP.Models;
using QL_LICHHOP.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public List<MeetingSchedule> GenerateWeeklySchedule(DateTime startOfWeek, DateTime endOfWeek, List<string> status)
        {
            List<MeetingSchedule> schedule = new List<MeetingSchedule>();            
            for (int i = 0; i < 7; i++)
            {
                DateTime currentDate = startOfWeek.AddDays(i).Date;

                var morningMeeting = GetMeetingByTimeOfDay(currentDate, new List<string> { "sáng", "cả ngày" }, status);
                var afternoonMeeting = GetMeetingByTimeOfDay(currentDate, new List<string> { "chiều" }, status);

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
        //private List<MeetingDTO> GetMeetingByTimeOfDay(DateTime date, List<string> scheduleTypes)
        //{
        //    return _context.Meetings
        //        .Where(m => m.StartTime.Date == date && scheduleTypes.Contains(m.ScheduleType.ToLower()))
        //        .Join(_context.ScheduleTypes,
        //            m => m.ScheduleTypeID,
        //            s => s.ScheduleTypeID,
        //            (m, s) => new { m, ScheduleName = s.Name })
        //        .GroupJoin(_context.MeetingHosts,
        //            ms => ms.m.MeetingID,
        //            mh => mh.MeetingID,
        //            (ms, hosts) => new { ms.m, ms.ScheduleName, hosts })
        //        .GroupJoin(_context.MeetingParticipants,
        //            mh => mh.m.MeetingID,
        //            mp => mp.MeetingID,
        //            (mh, participants) => new { mh.m, mh.ScheduleName, mh.hosts, participants })
        //        .Join(_context.Departments,
        //            m => m.m.DepartmentID,
        //            d => d.DepartmentID,
        //            (m, d) => new { m.m, m.ScheduleName, m.hosts, m.participants, DepartmentName = d.DepartmentName })
        //        .ToList()
        //        .Select(x => new MeetingDTO
        //        {
        //            MeetingID = x.m.MeetingID,
        //            Location = x.m.Location,
        //            StartTime = x.m.StartTime.ToString("HH:mm"),
        //            Title = x.m.Title,
        //            // Lấy Host từ MeetingHosts
        //            Host = x.hosts
        //                .Join(_context.Users,
        //                    mh => mh.UserID,
        //                    u => u.UserID,
        //                    (mh, u) => u.FullName)
        //                .ToList(),
        //            // Lấy danh sách người tham dự từ bảng MeetingParticipants
        //            Participants = x.participants
        //                .Join(_context.Users,
        //                    mp => mp.UserID,
        //                    u => u.UserID,
        //                    (mp, u) => u.FullName)
        //                .ToList(),
        //            RegistrationPlace = x.DepartmentName,
        //            CreatedBy = x.m.CreatedBy,
        //            // Lấy ScheduleName từ bảng ScheduleTypes
        //            ScheduleName = x.ScheduleName,
        //            Status = x.m.Status,
        //            AttachmentUrls = _context.MeetingAttachments
        //                .Where(a => a.MeetingID == x.m.MeetingID)
        //                .Select(a => a.FilePath)
        //                .ToList(),
        //            // Kiểm tra nếu Participants rỗng, thay thế bằng tên phòng ban
        //            ParticipantsOrDepartment = x.participants.Any()
        //                ? x.participants
        //                    .Join(_context.Users,
        //                        mp => mp.UserID,
        //                        u => u.UserID,
        //                        (mp, u) => u.FullName)
        //                    .ToList()
        //                : new List<string> { x.DepartmentName } // Nếu không có Participants, lấy DepartmentName
        //        })
        //        .ToList();
        //}

        private List<MeetingDTO> GetMeetingByTimeOfDay(DateTime date, List<string> scheduleTypes, List<string> status)
        {
            return _context.Meetings
                .Where(m => m.StartTime.Date == date &&
                            scheduleTypes.Contains(m.ScheduleType.ToLower()) &&
                            status.Contains(m.Status))
                .Join(_context.ScheduleTypes,
                    m => m.ScheduleTypeID,
                    s => s.ScheduleTypeID,
                    (m, s) => new { m, ScheduleName = s.Name })
                .GroupJoin(_context.MeetingHosts,
                    ms => ms.m.MeetingID,
                    mh => mh.MeetingID,
                    (ms, hosts) => new { ms.m, ms.ScheduleName, hosts })
                .GroupJoin(_context.MeetingParticipants,
                    mh => mh.m.MeetingID,
                    mp => mp.MeetingID,
                    (mh, participants) => new { mh.m, mh.ScheduleName, mh.hosts, participants })
                .Join(_context.Departments,
                    m => m.m.DepartmentID,
                    d => d.DepartmentID,
                    (m, d) => new { m.m, m.ScheduleName, m.hosts, m.participants, DepartmentName = d.DepartmentName, DepartmentID = d.DepartmentID })
                .ToList()
                .Select(x => new MeetingDTO
                {
                    MeetingID = x.m.MeetingID,
                    Location = x.m.Location,
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
                        .Where(mp => mp.MeetingID == x.m.MeetingID)
                        .Join(_context.Users,
                            mp => mp.UserID,
                            u => u.UserID,
                            (mp, u) => new
                            {
                                u.FullName,
                                DepartmentName = _context.Departments
                                    .Where(d => d.DepartmentID == u.DepartmentID)
                                    .Select(d => d.DepartmentName)
                                    .FirstOrDefault()
                            })
                        .ToList()
                        .Select(p => $"{p.DepartmentName} - {p.FullName}")
                        .ToList(),
                    RegistrationPlace = x.DepartmentName,
                    CreatedBy = x.m.CreatedBy,
                    // Lấy ScheduleName từ bảng ScheduleTypes
                    ScheduleName = x.ScheduleName,
                    Status = x.m.Status,
                    AttachmentUrls = _context.MeetingAttachments
                        .Where(a => a.MeetingID == x.m.MeetingID)
                        .Select(a => a.FilePath)
                        .ToList(),
                    // Thêm OldDate và NewDate
                    OldDate = x.m.OldDate?.ToString("dd/MM/yyyy HH:mm"),
                    NewDate = x.m.NewDate?.ToString("dd/MM/yyyy HH:mm"),
                    // Lấy danh sách phòng ban từ MeetingParticipants
                    Departments = x.participants
                        .Where(mp => mp.MeetingID == x.m.MeetingID)
                        .Select(mp => _context.Departments
                            .FirstOrDefault(d => d.DepartmentID == mp.DepartmentID)?.DepartmentName)
                        .Where(d => d != null)
                        .ToList()
                        .Except(
                            x.participants
                                .Where(mp => mp.MeetingID == x.m.MeetingID)
                                .Select(mp => _context.Users
                                    .Where(u => u.UserID == mp.UserID)
                                    .Select(u => u.DepartmentID)
                                    .FirstOrDefault())
                                .Distinct()
                                .Select(departmentID => _context.Departments
                                    .FirstOrDefault(d => d.DepartmentID == departmentID)?.DepartmentName)
                                .Where(d => d != null)
                                .ToList()
                        )
                        .ToList(),
                })
                .ToList();
        }


        public int GetWeekOfYear(DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        // Thêm cuộc họp mới
        public void AddMeeting(MeetingViewModel model, List<string> newParticipants)
        {
            var newMeeting = new Meeting
            {
                Title = model.Title,
                RegistrationPlace = model.RegistrationPlace,
                DepartmentID = model.DepartmentId,
                ScheduleTypeID = model.ScheduleTypeID,
                ScheduleType = model.ScheduleType,
                StartTime = (DateTime)model.StartTime,
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

            foreach (var participant in model.Participants)
            {
                _context.MeetingParticipants.InsertOnSubmit(new MeetingParticipant
                {
                    MeetingID = newMeeting.MeetingID,
                    DepartmentID = participant.DepartmentID > 0 ? participant.DepartmentID : (int?)null,
                    UserID = participant.UserID > 0 ? participant.UserID : (int?)null
                });
            }

            _context.SubmitChanges();
            

            // Nếu có newParticipants, tạo user mới và thêm vào cuộc họp
            if (newParticipants != null)
            {
                foreach (var fullName in newParticipants)
                {
                    var newUser = new User
                    {
                        FullName = fullName,
                        UserName = "guest" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        PasswordHash = "123",
                        DepartmentID = model.DepartmentId,
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

            // Lưu file đính kèm
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


        public void UpdateMeeting(MeetingViewModel updatedMeeting, List<string> newParticipants)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == updatedMeeting.MeetingID);
            if (meeting != null)
            {
                // Cập nhật thông tin cuộc họp
                meeting.Title = updatedMeeting.Title;
                meeting.RegistrationPlace = updatedMeeting.RegistrationPlace;
                meeting.DepartmentID = updatedMeeting.DepartmentId;
                meeting.ScheduleTypeID = updatedMeeting.ScheduleTypeID;
                meeting.ScheduleType = updatedMeeting.ScheduleType;
                meeting.StartTime = (DateTime)updatedMeeting.StartTime;
                meeting.DurationMinutes = updatedMeeting.DurationMinutes;
                meeting.Location = updatedMeeting.Location;
                meeting.VehicleType = updatedMeeting.VehicleType;
                meeting.Preparation = updatedMeeting.Preparation;
                meeting.UpdatedBy = "Chưa xử lý";
                meeting.UpdatedAt = DateTime.Now;

                _context.SubmitChanges();

                // Cập nhật Host của cuộc họp
                var existingHost = _context.MeetingHosts.FirstOrDefault(h => h.MeetingID == meeting.MeetingID);
                if (existingHost != null)
                {
                    existingHost.UserID = updatedMeeting.HostUserID;
                    existingHost.UpdatedAt = DateTime.Now;
                    existingHost.UpdatedBy = "Chưa xử lý";
                }
                else
                {
                    var newHost = new MeetingHost
                    {
                        MeetingID = meeting.MeetingID,
                        UserID = updatedMeeting.HostUserID,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Chưa xử lý"
                    };
                    _context.MeetingHosts.InsertOnSubmit(newHost);
                }
                _context.SubmitChanges();

                // Xóa tất cả người tham dự hiện tại
                var existingParticipants = _context.MeetingParticipants.Where(p => p.MeetingID == meeting.MeetingID).ToList();
                _context.MeetingParticipants.DeleteAllOnSubmit(existingParticipants);

                foreach (var participant in updatedMeeting.Participants)
                {
                    _context.MeetingParticipants.InsertOnSubmit(new MeetingParticipant
                    {
                        MeetingID = updatedMeeting.MeetingID,
                        DepartmentID = participant.DepartmentID > 0 ? participant.DepartmentID : (int?)null,
                        UserID = participant.UserID > 0 ? participant.UserID : (int?)null
                    });
                }

                // Thêm người tham dự mới (tạo user mới nếu cần)
                if (newParticipants != null)
                {
                    foreach (var fullName in newParticipants)
                    {
                        var newUser = new User
                        {
                            FullName = fullName,
                            UserName = "guest" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            PasswordHash = "123", // Mật khẩu mặc định
                            DepartmentID = updatedMeeting.DepartmentId,
                            Role = "Người dùng mới",
                            CreatedAt = DateTime.Now
                        };

                        _context.Users.InsertOnSubmit(newUser);
                        _context.SubmitChanges();

                        var newParticipant = new MeetingParticipant
                        {
                            MeetingID = meeting.MeetingID,
                            UserID = newUser.UserID,
                            CreatedAt = DateTime.Now
                        };

                        _context.MeetingParticipants.InsertOnSubmit(newParticipant);
                    }
                }
                _context.SubmitChanges();

                // Xử lý file đính kèm
                var existingAttachments = _context.MeetingAttachments.Where(a => a.MeetingID == meeting.MeetingID).ToList();
                List<string> newFilePaths = new List<string>();

                foreach (var file in updatedMeeting.Attachments)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads"), fileName);
                        file.SaveAs(path);

                        var attachment = new MeetingAttachment
                        {
                            MeetingID = meeting.MeetingID,
                            FilePath = fileName,
                            UploadedAt = DateTime.Now,
                            UploadedBy = "Chưa xử lý"
                        };

                        _context.MeetingAttachments.InsertOnSubmit(attachment);
                        newFilePaths.Add(fileName); // Lưu lại danh sách file mới để kiểm tra sau
                    }
                }
                _context.SubmitChanges();

                // **Chỉ xóa file cũ nếu có ít nhất một file mới được thêm thành công**
                if (newFilePaths.Count > 0)
                {
                    foreach (var attachment in existingAttachments)
                    {
                        string filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Uploads"), attachment.FilePath);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    _context.MeetingAttachments.DeleteAllOnSubmit(existingAttachments);
                    _context.SubmitChanges();
                }
            }
        }





        // Tìm cuộc họp theo ID
        public MeetingViewModel GetMeetingById(int id)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                var meetingViewModel = new MeetingViewModel
                {
                    MeetingID = meeting.MeetingID,
                    Title = meeting.Title,
                    RegistrationPlace = meeting.RegistrationPlace,
                    DepartmentId = meeting.DepartmentID,
                    ScheduleTypeID = meeting.ScheduleTypeID,
                    ScheduleType = meeting.ScheduleType,
                    StartTime = meeting.StartTime,
                    DurationMinutes = meeting.DurationMinutes,
                    Location = meeting.Location,
                    VehicleType = meeting.VehicleType,
                    Preparation = meeting.Preparation,
                    HostUserID = _context.MeetingHosts
                        .FirstOrDefault(h => h.MeetingID == meeting.MeetingID)?.UserID ?? 0,

                    // Danh sách thành phần tham dự (bao gồm cả phòng ban và cá nhân)
                    Participants = _context.MeetingParticipants
                        .Where(p => p.MeetingID == meeting.MeetingID)
                        .Select(p => new MeetingParticipantViewModel
                        {
                            DepartmentID = p.DepartmentID,
                            UserID = p.UserID,
                            FullName = _context.Users
                                .Where(u => u.UserID == p.UserID)
                                .Select(u => u.FullName)
                                .FirstOrDefault(), // Lấy tên người tham dự
                            DepartmentName = _context.Departments
                                .Where(d => d.DepartmentID == p.DepartmentID)
                                .Select(d => d.DepartmentName)
                                .FirstOrDefault() // Lấy tên phòng ban
                        })
                        .ToList(),

                    // Danh sách file đính kèm
                    AttachmentPaths = _context.MeetingAttachments
                        .Where(a => a.MeetingID == meeting.MeetingID)
                        .Select(a => a.FilePath)
                        .ToList()
                };

                return meetingViewModel;
            }
            return null; // Trả về null nếu không tìm thấy cuộc họp
        }

        public List<User> GetMeetingHosts(int meetingId)
        {
            return _context.MeetingHosts
                .Where(h => h.MeetingID == meetingId)
                .Join(_context.Users, h => h.UserID, u => u.UserID, (h, u) => u)
                .ToList();
        }
        public Meeting ApproveSchedule(int id)
        {            
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);

            if (meeting != null)
            {         
                meeting.Status = "Đã duyệt";
                _context.SubmitChanges();
            }
            return meeting;
        }
        public Meeting RejectSchedule(int id)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                meeting.Status = "Không duyệt";
                _context.SubmitChanges();
            }
            return meeting;
        }
        public Meeting CancelSchedule(int id)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                meeting.Status = "Đã hoãn";
                _context.SubmitChanges();
            }
            return meeting;
        }
        public Meeting PostponeSchedule(int id, DateTime newDate)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                meeting.OldDate = meeting.StartTime;
                meeting.NewDate = newDate;

                meeting.Status = "Đã dời";
                _context.SubmitChanges();
            }
            return meeting;
        }

    }
}