using QL_LICHHOP.Models;
using QL_LICHHOP.Repositories;
using QL_LICHHOP.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace QL_LICHHOP.Controllers
{
    public class ManageScheduleController : Controller
    {
        private readonly MeetingRepository meetingRepository = new MeetingRepository();
        private readonly UserRepository userRepository = new UserRepository();
        private readonly ScheduleTypeRepository scheduleTypeRepository = new ScheduleTypeRepository();      
        private readonly DepartmentRepository departmentRepository = new DepartmentRepository();

        // GET: ManageSchedule
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(DateTime? date)
        {
            ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
            ViewBag.Departments = departmentRepository.GetDepartments();
            ViewBag.Hosts = userRepository.GetHosts();
            MeetingViewModel model = new MeetingViewModel
            {
                ScheduleType = "Sáng",
                StartTime = date,
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MeetingViewModel newMeeting, string newParticipants)
        {            
            if (ModelState.IsValid)
            {                
                // Chuyển đổi newParticipants từ string sang List<string>
                var newParticipantsList = !string.IsNullOrEmpty(newParticipants)
                    ? newParticipants.Split(',').ToList()
                    : new List<string>();

                // Gửi dữ liệu vào repository
                meetingRepository.AddMeeting(newMeeting, newParticipantsList);

                return RedirectToAction("Index","ExpectedSchedule");
            }

            // Ghi log lỗi nếu có
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            // Load lại dữ liệu cho dropdowns
            ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
            ViewBag.Departments = departmentRepository.GetDepartments();
            ViewBag.Hosts = userRepository.GetHosts();

            return View(newMeeting);
        }

        public JsonResult SearchEmployees(int departmentId, string query)
        {
            var employees = userRepository.SearchUser(departmentId, query)
                .Select(e => new { e.UserID, e.FullName }) // Chọn các trường cần thiết
                .ToList();

            return Json(employees, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetEmployeesByDepartment(int departmentId)
        {
            var employees = userRepository.GetAllUsers() // Phương thức của bạn để lấy danh sách nhân viên
                .Where(e => e.DepartmentID == departmentId)
                .Select(e => new { e.UserID, e.FullName }) // Chọn các trường cần thiết
                .ToList();

            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreateNewHost(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return Json(new { success = false, message = "Tên không hợp lệ." });
            }

            var existingUser = userRepository.GetUserByName(fullName);

            if (existingUser != null)
            {
                // Nếu đã tồn tại, chỉ cần cập nhật RoleInMeeting
                userRepository.SetUserToHost(existingUser);
                return Json(new { success = true, newUserID = existingUser.UserID, message = "Đã cập nhật quyền chủ trì." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Nếu chưa tồn tại, tạo mới
                var newUser = userRepository.AddNewHost(fullName);

                return Json(new { success = true, newUserID = newUser.UserID, message = "Đã thêm người chủ trì mới." }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Edit(int id, string currentController, DateTime? selectedDates)
        {
            var meeting = meetingRepository.GetMeetingById(id);
            if (meeting == null)
            {
                return HttpNotFound();
            }
            var users = userRepository.GetAllUsers();
            ViewBag.Users = users;

            ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
            ViewBag.Departments = departmentRepository.GetDepartments();
            ViewBag.Hosts = userRepository.GetHosts();            
            ViewBag.MeetingHosts = meetingRepository.GetMeetingHosts(id);
            ViewBag.CurrentController = currentController;
            ViewBag.SelectedDates = selectedDates;
            return View(meeting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MeetingViewModel updatedMeeting, string newParticipants, string ExistingAttachments, string currentController, DateTime? selectedDates)
        {
            if (ModelState.IsValid)
            {                
                var newParticipantsList = !string.IsNullOrEmpty(newParticipants) ? newParticipants.Split(',').ToList() : new List<string>();

                // Lấy file mới từ Request.Files
                List<HttpPostedFileBase> uploadedFiles = new List<HttpPostedFileBase>();
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    if (file != null && file.ContentLength > 0)
                    {
                        uploadedFiles.Add(file);
                    }
                }

                // Nếu không có file mới, giữ lại file cũ
                if (uploadedFiles.Count == 0 && !string.IsNullOrEmpty(ExistingAttachments))
                {
                    updatedMeeting.AttachmentPaths = ExistingAttachments.Split(',').ToList();
                }

                updatedMeeting.Attachments = uploadedFiles;

                meetingRepository.UpdateMeeting(updatedMeeting, newParticipantsList);

                return RedirectToAction("Index", currentController, new { selectedDate = selectedDates });
            }

            ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
            ViewBag.Departments = departmentRepository.GetDepartments();
            return View(updatedMeeting);
        }

        public ActionResult DownloadFile(string fileName)
        {
            string filePath = Path.Combine(Server.MapPath("~/Uploads/"), fileName); // Đường dẫn tới file
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound(); // Trả về lỗi nếu file không tồn tại
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        
        public ActionResult ApproveMeeting(int id, DateTime? selectedDates, string currentAction, string currentController)
        {
            var meeting = meetingRepository.ApproveSchedule(id);
            if (meeting == null)
            {
                return HttpNotFound();
            }            
            // Lưu thông báo vào TempData
            TempData["SuccessMessage"] = "Duyệt thành công!";
            
            // Chuyển hướng về trang hiện tại với các tham số
            return RedirectToAction(currentAction, currentController, new { selectedDate = selectedDates });
        }
        public ActionResult ApproveAllMeetingInWeek(DateTime startOfWeek, DateTime endOfWeek, DateTime? selectedDates, string currentAction, string currentController)
        {
            var meetings = meetingRepository.ApproveAllScheduleInWeek(startOfWeek, endOfWeek);
            if (!meetings.Any())
            {
                return HttpNotFound();
            }
            TempData["SuccessMessage"] = "Duyệt tất cả lịch họp trong tuần thành công!";
            return RedirectToAction(currentAction, currentController, new { selectedDate = selectedDates });
        }
        public ActionResult RejectMeeting(int id, DateTime? selectedDates, string currentAction, string currentController)
        {
            var meeting = meetingRepository.RejectSchedule(id);
            if (meeting == null)
            {
                return HttpNotFound();
            }
            TempData["SuccessMessage"] = "Không duyệt!";
            return RedirectToAction(currentAction, currentController, new { selectedDate = selectedDates });
        }
        public ActionResult PostponeMeeting(int id, DateTime? selectedDates, string currentAction, string currentController, string newDate)
        {
            DateTime parsedDate;
            if (!DateTime.TryParseExact(newDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return new HttpStatusCodeResult(400, "Định dạng ngày không hợp lệ.");
            }
            var meeting = meetingRepository.PostponeSchedule(id, parsedDate);
            if (meeting == null)
            {
                return HttpNotFound();
            }
            TempData["SuccessMessage"] = "Dời lịch họp thành công!";
            return RedirectToAction(currentAction, currentController, new { selectedDate = selectedDates });
        }
        public ActionResult CancelMeeting(int id, DateTime? selectedDates, string currentAction, string currentController)
        {
            var meeting = meetingRepository.CancelSchedule(id);
            if (meeting == null)
            {
                return HttpNotFound();
            }
            TempData["SuccessMessage"] = "Hoãn lịch họp thành công!";
            return RedirectToAction(currentAction, currentController, new { selectedDate = selectedDates });
        }
        public ActionResult Delete(int id, DateTime? selectedDates, string currentAction, string currentController)
        {
            var meeting = meetingRepository.DeleteMeeting(id);
            if (meeting == null)
            {
                return HttpNotFound();
            }
            TempData["SuccessMessage"] = "Hủy lịch họp thành công!";
            return RedirectToAction(currentAction, currentController, new { selectedDate = selectedDates });
        }
        public ActionResult PrintSchedule(DateTime startOfWeek, DateTime endOfWeek, string status)
        {
            var statusList = status.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string filePath = Server.MapPath("~/Files/LichCongTac_Tuan.docx");
            meetingRepository.GenerateWeekScheduleDocument(startOfWeek, endOfWeek, filePath, statusList);         

            return File(filePath, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "LichCongTac_Tuan.docx");
        }
    }
}