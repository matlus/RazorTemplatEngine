using Microsoft.Extensions.FileProviders;
using RazorTemplatEngine.Enums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RazorTemplatEngine.Providers
{
    internal sealed class HtmlResourceFileProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly HashSet<string> _embeddedResourceFiles = new HashSet<string>();

        [ExcludeFromCodeCoverage]
        public HtmlResourceFileProvider()
            :this(new EmbeddedFileProvider(Assembly.GetExecutingAssembly()))
        {
        }

        public HtmlResourceFileProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            var directoryContents = _fileProvider.GetDirectoryContents("");
            foreach (var item in directoryContents)
            {
                _embeddedResourceFiles.Add(item.Name);
            }
        }

        public Stream GetResourceStream(string templateFolderName, string resourceFile, ResourceType resourceType)
        {
            var defaultResourceName = GetEmbeddedResourceName(templateFolderName, "_", resourceType);
            var resourceFileName = GetEmbeddedResourceName(templateFolderName, resourceFile, resourceType);

            var resourceName = _embeddedResourceFiles.Contains(resourceFileName) ? resourceFileName : defaultResourceName;
            return _fileProvider.GetFileInfo(resourceName).CreateReadStream();
        }

        public string ValidateDefaultResources(string templateFolderName)
        {
            string defaultHeaderMissing = null;
            if (!Exists(templateFolderName, "_", ResourceType.Header))
            {
                defaultHeaderMissing = "The Default Header Html Resource: \"_Header.html\", was Not found in the folder: " + templateFolderName + "\r\n";
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

        public async Task LoadResource(string templateFolderName, string templateNamePrefix, ResourceType resourceType, TextWriter textWriter)
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

        private bool Exists(string templateFolderName, string resourceFile, ResourceType resourceType)
        {
            var resourceFileName = GetEmbeddedResourceName(templateFolderName, resourceFile, resourceType);
            return _embeddedResourceFiles.Contains(resourceFileName);
        }
    }
}
