using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using QL_LICHHOP.Models;
using QL_LICHHOP.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Configuration;

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
                    StartDate = x.m.StartTime.ToString("dd/MM/yyyy"),
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
                    ScheduleType = x.m.ScheduleType,
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
        public void GenerateWeekScheduleDocument(DateTime startOfWeek, DateTime endOfWeek, string filePath, List<string> status)
        {
            // Lấy thông tin cuộc họp theo từng ngày trong tuần
            var meetingsByDay = new List<MeetingDTO>();

            for (var date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                var scheduleTypes = new List<string> { "sáng", "chiều", "cả ngày" };            
                var dailyMeetings = GetMeetingByTimeOfDay(date, scheduleTypes, status);
                meetingsByDay.AddRange(dailyMeetings);
            }

            // Tạo tài liệu Word
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();

                // Tiêu đề
                Paragraph title = new Paragraph(new Run(new Text("ỦY BAN NHÂN DÂN THÀNH PHỐ HỒ CHÍ MINH")));
                title.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                body.Append(title);

                Paragraph subtitle = new Paragraph(new Run(new Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")));
                subtitle.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                body.Append(subtitle);

                Paragraph date = new Paragraph(new Run(new Text("Độc lập - Tự do - Hạnh phúc")));
                date.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                body.Append(date);

                body.Append(new Paragraph(new Run(new Text("")))); // Thêm một dòng trống

                // Tiêu đề lịch công tác
                Paragraph scheduleTitle = new Paragraph(new Run(new Text("LỊCH HỌP VÀ LỊCH LÀM VIỆC")));
                scheduleTitle.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                body.Append(scheduleTitle);

                // Thêm thông tin tuần
                string weekInfoText = $"Tuần từ: {startOfWeek.ToString("dd/MM/yyyy")} đến {endOfWeek.ToString("dd/MM/yyyy")}";
                Paragraph weekInfo = new Paragraph(new Run(new Text(weekInfoText)));
                weekInfo.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                body.Append(weekInfo);

                body.Append(new Paragraph(new Run(new Text("")))); // Thêm một dòng trống

                // Tạo bảng
                Table table = new Table();
                TableProperties tblProps = new TableProperties(new TableBorders(
                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }));

                table.AppendChild(tblProps);

                // Thêm hàng tiêu đề
                TableRow headerRow = new TableRow();
                string[] headers = { "NGÀY", "BUỔI", "GIỜ", "NỘI DUNG", "THÀNH PHẦN", "ĐỊA ĐIỂM", "CHUẨN BỊ" };
                foreach (string header in headers)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(header))));
                    cell.Append(new TableCellProperties(new Shading() { Fill = "CCCCCC" }));
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                // Lưu trữ số lượng lịch họp cho mỗi ngày
                Dictionary<string, int> meetingCount = new Dictionary<string, int>();

                // Thêm dữ liệu cuộc họp
                foreach (var meeting in meetingsByDay)
                {
                    TableRow dataRow = new TableRow();

                    DateTime startDate;
                    DateTime startTime;

                    // Kiểm tra và chuyển đổi StartDate
                    if (DateTime.TryParseExact(meeting.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                    {
                        // Kiểm tra và chuyển đổi StartTime
                        if (DateTime.TryParse(meeting.StartTime, out startTime))
                        {
                            // Kết hợp ngày và giờ
                            DateTime combinedDateTime = startDate.Date + startTime.TimeOfDay;

                            // Lấy tên thứ bằng tiếng Việt
                            CultureInfo vietnameseCulture = new CultureInfo("vi-VN");
                            string dayOfWeek = startDate.ToString("dddd", vietnameseCulture); // Lấy tên thứ
                            string dateKey = $"{dayOfWeek} - {startDate.ToString("dd/MM/yyyy")}";

                            // Tăng số lượng lịch họp cho ngày này
                            if (!meetingCount.ContainsKey(dateKey))
                            {
                                meetingCount[dateKey] = 0;
                            }
                            meetingCount[dateKey]++;

                            // Thêm ô cho ngày
                            if (meetingCount[dateKey] == 1) // Nếu đây là lần đầu tiên gặp ngày này
                            {
                                string dateDisplay = $"{dayOfWeek}\n{startDate.ToString("dd/MM/yyyy")}";
                                dataRow.Append(new TableCell(new Paragraph(new Run(new Text(dateDisplay))))); // NGÀY
                            }
                            else // Nếu đã có ô cho ngày này
                            {
                                dataRow.Append(new TableCell(new Paragraph(new Run(new Text(""))))); // Ô trống
                            }

                            // Thêm các thông tin khác vào dataRow
                            dataRow.Append(new TableCell(new Paragraph(new Run(new Text(meeting.ScheduleType))))); // BUỔI
                            dataRow.Append(new TableCell(new Paragraph(new Run(new Text(combinedDateTime.ToString("HH:mm")))))); // GIỜ
                        }
                        else
                        {
                            // Xử lý trường hợp nếu không thể chuyển đổi StartTime
                            dataRow.Append(new TableCell(new Paragraph(new Run(new Text("Không xác định"))))); // NGÀY
                            dataRow.Append(new TableCell(new Paragraph(new Run(new Text(meeting.ScheduleType))))); // BUỔI
                            dataRow.Append(new TableCell(new Paragraph(new Run(new Text("Không xác định"))))); // GIỜ
                        }
                    }
                    else
                    {
                        // Xử lý trường hợp nếu không thể chuyển đổi StartDate
                        dataRow.Append(new TableCell(new Paragraph(new Run(new Text("Không xác định"))))); // NGÀY
                        dataRow.Append(new TableCell(new Paragraph(new Run(new Text(meeting.ScheduleType))))); // BUỔI
                        dataRow.Append(new TableCell(new Paragraph(new Run(new Text("Không xác định"))))); // GIỜ
                    }

                    dataRow.Append(new TableCell(new Paragraph(new Run(new Text(meeting.Title))))); // NỘI DUNG

                    // Xử lý cột THÀNH PHẦN
                    var participants = meeting.Participants;
                    var departments = meeting.Departments;

                    // Tạo chuỗi cho THÀNH PHẦN
                    string participantsText = string.Join(", ", participants);
                    string departmentsText = string.Join(", ", departments);

                    string combinedText = string.IsNullOrEmpty(participantsText) ? departmentsText : participantsText;

                    dataRow.Append(new TableCell(new Paragraph(new Run(new Text(combinedText))))); // THÀNH PHẦN
                    dataRow.Append(new TableCell(new Paragraph(new Run(new Text(meeting.Location))))); // ĐỊA ĐIỂM
                    dataRow.Append(new TableCell(new Paragraph(new Run(new Text(meeting.RegistrationPlace))))); // CHUẨN BỊ

                    table.Append(dataRow);
                }

                body.Append(table);
                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
        }

        // Thêm cuộc họp mới
        public void AddMeeting(MeetingViewModel model, List<string> newParticipants, string createdBy)
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
                CreatedBy = createdBy,
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
                CreatedBy = createdBy,
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
                        UploadedBy = createdBy,
                    };
                    _context.MeetingAttachments.InsertOnSubmit(attachment);
                }
            }
            _context.SubmitChanges();
        }


        public void UpdateMeeting(MeetingViewModel updatedMeeting, List<string> newParticipants, string updatedBy)
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
                meeting.UpdatedBy = updatedBy;
                meeting.UpdatedAt = DateTime.Now;

                _context.SubmitChanges();

                // Cập nhật Host của cuộc họp
                var existingHost = _context.MeetingHosts.FirstOrDefault(h => h.MeetingID == meeting.MeetingID);
                if (existingHost != null)
                {
                    existingHost.UserID = updatedMeeting.HostUserID;
                    existingHost.UpdatedAt = DateTime.Now;
                    existingHost.UpdatedBy = updatedBy;
                }
                else
                {
                    var newHost = new MeetingHost
                    {
                        MeetingID = meeting.MeetingID,
                        UserID = updatedMeeting.HostUserID,
                        CreatedAt = DateTime.Now,
                        CreatedBy = updatedBy
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
                            UploadedBy = updatedBy
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
                        .ToList(),
                    CreatedBy = meeting.CreatedBy
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
        public Meeting ApproveSchedule(int id, string updatedStatusBy)
        {            
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);

            if (meeting != null)
            {         
                meeting.Status = "Đã duyệt";
                meeting.UpdateStatusBy = updatedStatusBy;
                _context.SubmitChanges();
            }
            return meeting;
        }
        public List<Meeting> ApproveAllScheduleInWeek(DateTime startOfWeek, DateTime endOfWeek, string updatedStatusBy)
        {
            var meetings = _context.Meetings.Where(m => m.StartTime >= startOfWeek && m.StartTime <= endOfWeek).ToList();
            foreach (var meeting in meetings)
            {
                meeting.Status = "Đã duyệt";
                meeting.UpdateStatusBy = updatedStatusBy;
            }
            _context.SubmitChanges();
            return meetings;
        }
        public Meeting RejectSchedule(int id, string updatedStatusBy)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                meeting.Status = "Không duyệt";
                meeting.UpdateStatusBy = updatedStatusBy;
                _context.SubmitChanges();
            }
            return meeting;
        }
        public Meeting CancelSchedule(int id, string updatedStatusBy)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                meeting.Status = "Đã hoãn";
                meeting.UpdateStatusBy = updatedStatusBy;
                _context.SubmitChanges();
            }
            return meeting;
        }
        public Meeting PostponeSchedule(int id, DateTime newDate, string updatedStatusBy)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                meeting.OldDate = meeting.StartTime;
                meeting.NewDate = newDate;

                meeting.Status = "Đã dời";
                meeting.UpdateStatusBy = updatedStatusBy;
                _context.SubmitChanges();
            }
            return meeting;
        }
        public Meeting DeleteMeeting(int id, string updatedStatusBy)
        {
            var meeting = _context.Meetings.FirstOrDefault(m => m.MeetingID == id);
            if (meeting != null)
            {
                meeting.Status = "Đã xóa";
                meeting.UpdateStatusBy = updatedStatusBy;
                _context.SubmitChanges();
            }
            return meeting;
        }
    }
}