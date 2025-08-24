using CustomerSalesSystem.Domain;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CustomerSalesSystem.Web.Helper
{
    public static class Toast
    {
        public static void Show(string message, ToastType type = ToastType.Info, string? title = null)
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            if (httpContext == null) return;

            var tempData = httpContext.RequestServices
                .GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
            if (tempData == null) return;

            var temp = tempData.GetTempData(httpContext);
            temp["ToastMessage"] = message;
            temp["ToastType"] = type.ToString().ToLower();
            if (!string.IsNullOrEmpty(title))
                temp["ToastTitle"] = title;
        }
    }
}
