using Microsoft.Extensions.FileProviders;
using RazorTemplatEngine.Enums;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RazorTemplatEngine.Providers
{
    internal static class HtmlResourceFileProvider
    {
        private static readonly EmbeddedFileProvider s_embeddedFileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        private static readonly HashSet<string> s_embeddedResourceFiles = new HashSet<string>();

        static HtmlResourceFileProvider()
        {
            var directoryContents = s_embeddedFileProvider.GetDirectoryContents("");
            foreach (var item in directoryContents)
            {
                s_embeddedResourceFiles.Add(item.Name);
            }
        }

        public static Stream GetResourceStream(string templateFolderName, string resourceFile, ResourceType resourceType)
        {
            var defaultResourceName = GetEmbeddedResourceName(templateFolderName, "_", resourceType);
            var resourceFileName = GetEmbeddedResourceName(templateFolderName, resourceFile, resourceType);

            var resourceName = s_embeddedResourceFiles.Contains(resourceFileName) ? resourceFileName : defaultResourceName;
            return s_embeddedFileProvider.GetFileInfo(resourceName).CreateReadStream();
        }

        public static string ValidateDefaultResources(string templateFolderName)
        {
            string defaultHeaderMissing = null;
            if (!Exists(templateFolderName, "_", ResourceType.Header))
            {
                defaultHeaderMissing = "The Default Header Html Resource: \"_Header.html\", was Not found in the folder: " + templateFolderName;
            }

            string defaultFooterMissing = null;
            if (!Exists(templateFolderName, "_", ResourceType.Footer))
            {
                defaultFooterMissing = "The Default Footer Html Resource: \"_Footer.html\", was Not found in the folder: " + templateFolderName;
            }

            if (defaultHeaderMissing != null || defaultFooterMissing != null)
            {
                return defaultHeaderMissing + defaultFooterMissing;
            }
            else
            {
                return null;
            }
        }

        public static async Task LoadResource(string templateFolderName, string templateNamePrefix, ResourceType resourceType, TextWriter textWriter)
        {
            var resourceStream = GetResourceStream(templateFolderName, templateNamePrefix, resourceType);
            using var streamReader = new StreamReader(resourceStream);

            var buffer = new char[1024];
            int bytesRead = 0;
            while ((bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await textWriter.WriteAsync(buffer, 0, bytesRead);
            }
        }

        public static string GetEmbeddedResourceName(string templateFolderName, string fileName, ResourceType resourceType)
        {
            return $"{templateFolderName}.{fileName}{resourceType}.html";
        }

        private static bool Exists(string templateFolderName, string resourceFile, ResourceType resourceType)
        {
            var resourceFileName = GetEmbeddedResourceName(templateFolderName, resourceFile, resourceType);
            return s_embeddedResourceFiles.Contains(resourceFileName);
        }
    }
}
