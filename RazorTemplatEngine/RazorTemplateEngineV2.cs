using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using RazorTemplatEngine.Enums;
using RazorTemplatEngine.Exceptions;
using RazorTemplatEngine.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("RazorTemplateEngineTests")]

namespace RazorTemplatEngine
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/core/razor-pages/sdk?view=aspnetcore-3.1#properties
    /// </summary>
    public sealed class RazorTemplateEngineV2
    {
        private const string TemplateFolderName = "Templates";
        private readonly Dictionary<string, RazorCompiledItem> _razorCompiledItems = new Dictionary<string, RazorCompiledItem>();
        private readonly HtmlResourceFileProvider _htmlResourceFileProvider = new HtmlResourceFileProvider();

        public RazorTemplateEngineV2()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var viewAssembly = RelatedAssemblyAttribute.GetRelatedAssemblies(thisAssembly, false).Single();
            var razorCompiledItems = new RazorCompiledItemLoader().LoadItems(viewAssembly);

            foreach (var item in razorCompiledItems)
            {
                _razorCompiledItems.Add(item.Identifier, item);
            }
        }

        public async Task<string> RenderTemplateAsync<TModel>(TModel model)
        {
            var templateNamePrefix = model.GetType().Name;
            EnsureAllTemplatesExist(TemplateFolderName, templateNamePrefix);

            using var stringWriter = new StringWriter();
            await _htmlResourceFileProvider.LoadResource(TemplateFolderName, templateNamePrefix, ResourceType.Header, stringWriter);
            await stringWriter.WriteAsync(await RenderTemplateAsync(TemplateFolderName, templateNamePrefix, model));
            await _htmlResourceFileProvider.LoadResource(TemplateFolderName, templateNamePrefix, ResourceType.Footer, stringWriter);

            stringWriter.Flush();
            return stringWriter.ToString();
        }

        private void EnsureAllTemplatesExist(string templateFolderName, string templateNamePrefix)
        {
            var razorTemplate = GetRazorTemplateName(templateFolderName, templateNamePrefix);

            var errorMessages = new StringBuilder();
            if (!_razorCompiledItems.TryGetValue(razorTemplate, out var razorCompiledItem))
            {
                errorMessages.AppendLine($"The Razor Template file: {razorTemplate}, was not found.");
            }

            errorMessages.AppendLine(_htmlResourceFileProvider.ValidateDefaultResources(templateFolderName));

            if (errorMessages.Length > 2)
            {
                throw new RazorTemplateNotFoundException(errorMessages.ToString());
            }
        }

        private async Task<string> RenderTemplateAsync<TModel>(string templateFolderName, string templateName, TModel model)
        {
            var razorTemplate = GetRazorTemplateName(templateFolderName, templateName);
            var razorCompiledItem = _razorCompiledItems[razorTemplate];
            return await GetRenderedOutput(razorCompiledItem, model);
        }

        private static string GetRazorTemplateName(string templateFolderName, string templateName)
        {
            return $"/{templateFolderName}/{templateName}.cshtml";
        }

        private static async Task<string> GetRenderedOutput<TModel>(RazorCompiledItem razorCompiledItem, TModel model)
        {
            using var stringWriter = new StringWriter();
            var razorPage = GetRazorPageInstance(razorCompiledItem, model, stringWriter);
            await razorPage.ExecuteAsync();
            return stringWriter.ToString();
        }

        private static RazorPage GetRazorPageInstance<TModel>(RazorCompiledItem razorCompiledItem, TModel model, TextWriter textWriter)
        {
            var razorPage = (RazorPage<TModel>)Activator.CreateInstance(razorCompiledItem.Type);

            razorPage.ViewData = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            razorPage.ViewContext = new ViewContext
            {
                Writer = textWriter
            };

            razorPage.HtmlEncoder = HtmlEncoder.Default;
            return razorPage;
        }        
    }
}
