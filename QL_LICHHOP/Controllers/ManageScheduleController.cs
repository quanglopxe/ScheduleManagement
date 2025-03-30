using QL_LICHHOP.Models;
using QL_LICHHOP.Repositories;
using QL_LICHHOP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult Create()
        {
            ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
            ViewBag.Departments = departmentRepository.GetDepartments();
            ViewBag.Hosts = userRepository.GetHosts();
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MeetingViewModel newMeeting, string selectedUsers, string newParticipants, int selectedDepartment)
        {
            if (ModelState.IsValid)
            {
                // Chuyển đổi selectedUsers từ string sang danh sách
                var userIds = !string.IsNullOrEmpty(selectedUsers) ? selectedUsers.Split(',').Select(int.Parse).ToList() : new List<int>();

                // Chuyển đổi newParticipants từ string sang danh sách
                var newParticipantsList = !string.IsNullOrEmpty(newParticipants) ? newParticipants.Split(',').ToList() : new List<string>();

                meetingRepository.AddMeeting(newMeeting, newParticipantsList, userIds, selectedDepartment);
                return RedirectToAction("Index");
            }

            ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
            return View(newMeeting);
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

        //public ActionResult Edit(int id)
        //{
        //    var meeting = meetingRepository.GetMeetingById(id);
        //    if (meeting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
        //    return View(meeting);
        //}

        //// Xử lý chỉnh sửa lịch họp
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Meeting updatedMeeting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        meetingRepository.UpdateMeeting(updatedMeeting);
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ScheduleTypes = scheduleTypeRepository.GetScheduleTypes();
        //    return View(updatedMeeting);
        //}
        //public ActionResult Delete(int id)
        //{
        //    var meeting = meetingRepository.GetMeetingById(id);
        //    if (meeting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(meeting);
        //}

        //// Xử lý xóa lịch họp
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult ConfirmDelete(int id)
        //{
        //    meetingRepository.DeleteMeeting(id);
        //    return RedirectToAction("Index");
        //}
    }
}