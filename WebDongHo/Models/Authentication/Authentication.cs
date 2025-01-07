using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebDongHo.Models.Authentication
{
    public class Authentication : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var userRoleId = httpContext.Session.GetInt32("RoleId");

            if (userRoleId == null || userRoleId != 1)
            {
                context.Result = new ViewResult
                {
                    ViewName = "AccessDenied"
                };
            }
        }
    }
}
