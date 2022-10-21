using System.Text.Encodings.Web;
using CovidLetter.Frontend.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CovidLetter.Frontend.WebApp.TagHelpers
{
    [HtmlTargetElement("input", Attributes = Attribute.ErrorClassToggle, TagStructure = TagStructure.WithoutEndTag)]
    public class ErrorClassToggleTagHelper: TagHelper
    {
        [HtmlAttributeName(Attribute.ErrorClassToggle)]
        public string? Class { get; set; }

        [HtmlAttributeName(Attribute.ErrorOr)]
        public bool? Or { get; set; }

        [HtmlAttributeName(Attribute.AspFor)]
        public ModelExpression? For { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext? ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Class != null && (Or == true || For != null && ViewContext!.ViewData.ModelState.HasError(For.Name)))
            {
                output.AddClass(Class, HtmlEncoder.Default);
            }
        }
    }
}