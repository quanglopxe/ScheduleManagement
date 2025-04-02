using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_LICHHOP.FilterAttribute
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra nếu session UserID bị mất (hết hạn)
            if (HttpContext.Current.Session["UserID"] == null)
            {
                // Nếu là Ajax request, trả về lỗi JSON
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        //Data = new { success = false, message = "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại." },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    // Chuyển hướng về trang đăng nhập
                    //filterContext.Controller.TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                    filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                {
                    { "controller", "Account" },
                    { "action", "Login" }
                });
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}