using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.FileProviders;
using RazorTemplatEngine.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace RazorTemplatEngine
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/core/razor-pages/sdk?view=aspnetcore-3.1#properties
    /// </summary>
    public sealed class RazorTemplateEngineV1
    {
        private readonly Dictionary<string, RazorCompiledItem> _razorCompiledItems = new Dictionary<string, RazorCompiledItem>();
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        public RazorTemplateEngineV1()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var viewAssembly = RelatedAssemblyAttribute.GetRelatedAssemblies(thisAssembly, false).Single();
            var razorCompiledItems = new RazorCompiledItemLoader().LoadItems(viewAssembly);

            foreach (var item in razorCompiledItems)
            {
                _razorCompiledItems.Add(item.Identifier, item);
            }

            _embeddedFileProvider = new EmbeddedFileProvider(thisAssembly);
        }

        public async Task<string> RenderTemplateAsync<TModel>(string folderName, string templateName, string headerResourceName, string footerResourceName, TModel model)
        {
            var headerResource = $"{folderName}.{headerResourceName}";
            var footerResource = $"{folderName}.{footerResourceName}";
            var razorTemplate = $"/{folderName}/{templateName}";

            using var stringWriter = new StringWriter();
            await LoadResource(headerResource, stringWriter);
            await stringWriter.WriteAsync(await RenderTemplateAsync(razorTemplate, model));
            await LoadResource(footerResource, stringWriter);

            stringWriter.Flush();
            return stringWriter.ToString();
        }

        private async Task LoadResource(string resourceName, TextWriter textWriter)
        {
            StreamReader streamReader = null;
            try
            {
                var resourceStream = _embeddedFileProvider.GetFileInfo(resourceName).CreateReadStream();
                streamReader = new StreamReader(resourceStream);

                var buffer = new char[1024];
                int bytesRead = 0;
                while ((bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await textWriter.WriteAsync(buffer, 0, bytesRead);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new RazorTemplateNotFoundException(e.Message, e);
            }
            finally
            {
                streamReader?.Close();
            }
        }

        private async Task<string> RenderTemplateAsync<TModel>(string templateName, TModel model)
        {
            if (_razorCompiledItems.TryGetValue(templateName, out var razorCompiledItem))
            {
                return await GetRenderedOutput(razorCompiledItem, model);
            }

            throw new RazorTemplateNotFoundException($"The Razor Page/Template: {templateName}, was not found in assembly: {Assembly.GetExecutingAssembly().FullName}");
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
