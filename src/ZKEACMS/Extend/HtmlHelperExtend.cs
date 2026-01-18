/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.Extend;
using Easy.Mvc;
using Easy.Mvc.Extend;
using Easy.Mvc.RazorPages;
using Easy.RepositoryPattern;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ZKEACMS.HtmlComponent;
using ZKEACMS.Widget;

namespace ZKEACMS
{
    public static class HtmlHelperExtend
    {
        public static async Task<IHtmlContent> DisplayWidget(this IHtmlHelper html, WidgetViewModelPart widget)
        {
            if (widget.ViewModel != null)
            {
                var logger = html.ViewContext.HttpContext.RequestServices.GetService<ILogger<WidgetViewModelPart>>();
                DateTime startTime = DateTime.Now;
                var widgetResult = await html.PartialAsync("DisplayWidget", widget);
                logger.LogInformation("Render Widget [{0}]: {1}ms", widget.Widget.ServiceTypeName, (DateTime.Now - startTime).TotalMilliseconds);
                return widgetResult;
            }
            return await html.WidgetError();
        }
        public static async Task<IHtmlContent> DisplayWidgetPart(this IHtmlHelper html, WidgetViewModelPart widget)
        {
            if (widget.ViewModel != null)
            {
                return await html.PartialAsync(widget.Widget.PartialView, widget.ViewModel);
            }
            return await html.WidgetError();
        }
        public static async Task<IHtmlContent> DesignWidget(this IHtmlHelper html, DesignWidgetViewModel viewModel)
        {
            return await html.PartialAsync("DesignWidget", viewModel);
        }
        public static IHtmlContent SmartLink(this IHtmlHelper html, string link, string text, string cssClass = null)
        {
            if (link.IsNullOrEmpty())
            {
                link = "/";
            }
            bool self = IsOpenSelf(link);
            HtmlContentBuilder builder = new HtmlContentBuilder();
            builder.AppendHtmlLine("<a " + (cssClass.IsNullOrWhiteSpace() ? "" : "class=\"" + cssClass + "\"") + " target=\"" + (self ? "_self" : "_blank") + "\" href=\"" + link + "\">" + text + "</a>");
            return builder;
        }

        public static IHtmlContent SmartLinkTarget(this IHtmlHelper html, string link)
        {
            HtmlContentBuilder builder = new HtmlContentBuilder();
            if (link.IsNullOrEmpty())
            {
                builder.Append("_self");
                return builder;
            }
            bool self = IsOpenSelf(link);
            builder.Append(self ? "_self" : "_blank");
            return builder;
        }

        private static bool IsOpenSelf(string link)
        {
            return !link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !link.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<IHtmlContent> WidgetError(this IHtmlHelper html)
        {
            return await html.PartialAsync("Widget.Error");
        }

        public static async Task Pagin(this IHtmlHelper html, Pagination pagin)
        {
            await html.RenderPartialAsync("Partial_Pagination", pagin);
        }
        public static async Task Pagin(this IHtmlHelper html, Pagin pagin)
        {
            await html.RenderPartialAsync("Partial_RegularPagination", pagin);
        }
        public static IHtmlContent SearchTerms(this IHtmlHelper html, bool createAble)
        {
            return html.SearchTerms(createAble, "Create");
        }
        public static IHtmlContent SearchTerms(this IHtmlHelper html, bool createAble, string createAction)
        {
            return html.Editor(string.Empty, "Search-Terms", new { CreateAble = createAble, CreateAction = createAction });
        }
        public static IDisposable SearchTermsWithActions(this IHtmlHelper html, RazorPage page)
        {
            return new InjectEditorViewRender(page, html, "Search-Terms");
        }
        public static IHtmlContent SearchItem(this IHtmlHelper html, ModelMetadata item)
        {
            var descriptor = item.GetViewDescriptor();
            if (descriptor is Easy.ViewPort.Descriptor.DropDownListDescriptor)
            {
                return html.Editor(item.PropertyName, descriptor.TemplateName);
            }
            else
            {
                if (!descriptor.Classes.Contains("form-control"))
                {
                    descriptor.Classes.Add("form-control");
                }
                Type modelType = descriptor.DataType;

                if (modelType == typeof(DateTime))
                {
                    return html.Editor(item.PropertyName, "DateTime");
                }
                else if (modelType == typeof(bool))
                {
                    return html.Editor(item.PropertyName, "DropdownBoolen");
                }
                else if (modelType == typeof(decimal))
                {
                    return html.Editor(item.PropertyName, "Decimal");
                }
                else if (modelType == typeof(int))
                {
                    return html.Editor(item.PropertyName, "Int32");
                }
                else
                {
                    return html.Editor(item.PropertyName, "String");
                }
            }
        }

        public static IHtmlContent EmailLinkButton(this IHtmlHelper html, string link, string text)
        {
            return html.EmailLinkButton(link, text, false);
        }
        public static IHtmlContent EmailLinkButton(this IHtmlHelper html, string link, string text, bool center)
        {
            return html.Partial("EmailLinkButton", new Tuple<string, string, bool>(link, text, center));
        }
        public static IHtmlContent HiddenForCurrentPagePath(this IHtmlHelper html)
        {
            var request = html.ViewContext.HttpContext.Request;
            if (request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var pagePath = request.Query["CurrentPagePath"];
                return html.Hidden("CurrentPagePath", pagePath.Count > 0 ? pagePath : request.Path);
            }
            else if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return html.Hidden("CurrentPagePath", request.Form["CurrentPagePath"]);
            }
            return html.Hidden("CurrentPagePath", request.Path);
        }

        public static string CurrencySymbol(this IHtmlHelper html)
        {
            return html.ViewContext.HttpContext.RequestServices.GetService<IApplicationContextAccessor>().Current.Currency.Symbol;
        }
        public static string CurrencyCode(this IHtmlHelper html)
        {
            return html.ViewContext.HttpContext.RequestServices.GetService<IApplicationContextAccessor>().Current.Currency.Code;
        }
        public static HtmlPanel BeginPanel(this IHtmlHelper html, string title)
        {
            return new HtmlPanel(html.ViewContext.Writer, title);
        }
        public static HtmlPanel BeginPanel(this IHtmlHelper html, string title, string link, string linkText)
        {
            return new HtmlPanel(html.ViewContext.Writer, title, link, linkText);
        }

        public static IHtmlContent ChangeHistoryBtn(this IHtmlHelper html, object entity)
        {
            if (entity == null) return null;

            var btnText = html.ViewContext.HttpContext.RequestServices.GetService<ILocalize>().Get("Change History");

            var keyProperties = entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (!keyProperties.Any())
            {
                throw new InvalidOperationException($"Entity type {entity.GetType().FullName} does not have a property marked with [Key] attribute.");
            }

            var keyValue = string.Join(":", keyProperties.Select(p => p.GetValue(entity).ToString()));
            var entityType = WebEncoders.Base64UrlEncode(entity.GetType().FullName.ToByte());
            string url = $"/admin/audittrail/history?entityType={entityType}&entityID={keyValue}";
            TagBuilder button = new TagBuilder("input");
            button.AddCssClass("btn btn-info open-dialog");
            button.Attributes.Add("type", "button");
            button.Attributes.Add("value", btnText);
            button.Attributes.Add("data-url", url);
            return button;
        }
    }

}
