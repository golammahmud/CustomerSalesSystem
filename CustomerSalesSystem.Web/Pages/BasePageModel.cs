using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerSalesSystem.Web.Pages
{
    public class BasePageModel : PageModel
    {
        /// <summary>
        /// Injects a JavaScript call to speak the given text using the JS window.speak function.
        /// </summary>
        public void Speak(string text)
        {
            var escapedText = System.Text.Encodings.Web.JavaScriptEncoder.Default.Encode(text);
            ViewData["SpeakScript"] = new HtmlString($"<script>window.speak('{escapedText}');</script>");
        }
    }
}
