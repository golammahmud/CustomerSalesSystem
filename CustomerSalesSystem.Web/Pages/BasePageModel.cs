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

        public class PopUpModel
        {
            public string ModalId { get; set; }      // Unique ID for the modal
            public string Title { get; set; }        // Modal title text
            public string FormId { get; set; }       // Form ID inside the modal
            public string Handler { get; set; }      // Handler method name (for form submission)
            public string Page { get; set; }         // Page or action reference
            public string ButtonTitle { get; set; }  // Submit button text
            public List<ModalField> Fields { get; set; } = new(); // Form fields collection
        }

        public class ModalField
        {
            public string Label { get; set; }
            public string Name { get; set; }
            public string Type { get; set; } // "text", "hidden", "select"
            public string Value { get; set; }
            public IEnumerable<SelectOption> Options { get; set; } // For dropdowns
        }

        public class SelectOption
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }
    }
}
