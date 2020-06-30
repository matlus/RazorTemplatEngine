using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorTemplatEngine.Enums;
using RazorTemplatEngine.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RazorTemplateEngineTests
{
    [TestClass]
    public class HtmlResourceFileProviderTests
    {
        private const string TemplatesFolder = "Templates";

        private static HtmlResourceFileProvider CreateHtmlResourceFileProvider(IEnumerable<string> resourceNames, byte[] buffer = null)
        {
            var fileInfos = new List<IFileInfo>();

            foreach (var resourceName in resourceNames)
            {
                fileInfos.Add(new FileInfoTestDouble(resourceName, buffer));
            }

            var directoryContents = new DirectoryContentsTestDouble(fileInfos);
            var fileProvider = new FileProviderTestDouble(directoryContents);
            return new HtmlResourceFileProvider(fileProvider);
        }

        [TestMethod]
        [TestCategory("Edge case Test")]
        public void ValidateDefaultResources_WhenHeaderHtmlDoesNotExist_ShouldReturnErrorMessageIndicatingAsSuch()
        {
            // Arrange
            var expectedErrorMessage = "The Default Header Html Resource: \"_Header.html\", was Not found in the folder: Templates\r\n";
            var htmlResourceFileProvider = CreateHtmlResourceFileProvider(new string[] { $"{TemplatesFolder}._Footer.html" });
            
            // Act
            var errorMessage = htmlResourceFileProvider.ValidateDefaultResources(TemplatesFolder);

            // Assert
            Assert.AreEqual(expectedErrorMessage, errorMessage, $"Expecting to Find the {TemplatesFolder}._Footer.html and Not the {TemplatesFolder}._Header.html file");
        }

        [TestMethod]
        [TestCategory("Edge case Test")]
        public void ValidateDefaultResources_WhenFooterHtmlDoesNotExist_ShouldReturnErrorMessageIndicatingAsSuch()
        {
            // Arrange
            var expectedErrorMessage = "The Default Footer Html Resource: \"_Footer.html\", was Not found in the folder: Templates";
            var htmlResourceFileProvider = CreateHtmlResourceFileProvider(new string[] { $"{TemplatesFolder}._Header.html" });

            // Act
            var errorMessage = htmlResourceFileProvider.ValidateDefaultResources(TemplatesFolder);

            // Assert
            Assert.AreEqual(expectedErrorMessage, errorMessage, $"Expecting to Find the {TemplatesFolder}._Header.html and Not the {TemplatesFolder}._Footer.html file");
        }

        [TestMethod]
        [TestCategory("Edge case Test")]
        public void ValidateDefaultResources_WhenBothHeaderAndFooterHtmlFilesDoNotExist_ShouldReturnErrorMessageIndicatingAsSuch()
        {
            // Arrange
            var expectedErrorMessage = $"The Default Header Html Resource: \"_Header.html\", was Not found in the folder: {TemplatesFolder}\r\nThe Default Footer Html Resource: \"_Footer.html\", was Not found in the folder: {TemplatesFolder}";
            var htmlResourceFileProvider = CreateHtmlResourceFileProvider(new string[] { });

            // Act
            var errorMessage = htmlResourceFileProvider.ValidateDefaultResources(TemplatesFolder);

            // Assert
            Assert.AreEqual(expectedErrorMessage, errorMessage, $"Expecting to Find the {TemplatesFolder}._Header.html and Not the {TemplatesFolder}._Footer.html file");
        }

        [TestMethod]
        [TestCategory("Edge case Test")]
        [DataTestMethod]
        [DataRow(ResourceType.Header, "Header Resource Matching Model Type Name Exists")]
        [DataRow(ResourceType.Footer, "Footer Resource Matching Model Type Name Exists")]
        public void GetResourceStream_WhenHeaderAndFooterResourceMatchingModelTypeNameExist_ShouldReturnStreamForResource(ResourceType resourceType, string displayName)
        {
            // Arrange
            var expectedFileContent = $"This is the expected content of the {resourceType.ToString()} file";
            var model = new object();
            var headerResourceFileName = HtmlResourceFileProvider.GetEmbeddedResourceName(TemplatesFolder, model.GetType().Name, resourceType);
            
            var htmlResourceFileProvider = CreateHtmlResourceFileProvider(new string[] { headerResourceFileName }, Encoding.UTF8.GetBytes(expectedFileContent));

            // Act
            var resourceFileStream = htmlResourceFileProvider.GetResourceStream(TemplatesFolder, model.GetType().Name, resourceType);

            // Assert
            using var streamReader = new StreamReader(resourceFileStream);
            var actualFileContent = streamReader.ReadToEnd();
            Assert.AreEqual(expectedFileContent, actualFileContent, $"Expecting the content of the Resource File to be: {expectedFileContent}. But found the Resource File's to be: {actualFileContent}");
        }
    }

    internal sealed class FileProviderTestDouble : IFileProvider
    {
        private readonly IDirectoryContents _directorContents;
        public FileProviderTestDouble(IDirectoryContents directorContents)
        {
            _directorContents = directorContents;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _directorContents;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return _directorContents.Single(fi => fi.Name == subpath);
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class DirectoryContentsTestDouble : IDirectoryContents
    {
        private readonly IEnumerable<IFileInfo> _fileInfos;

        public DirectoryContentsTestDouble(IEnumerable<IFileInfo> fileInfos)
        {
            _fileInfos = fileInfos;
        }
        public bool Exists => throw new NotImplementedException();

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _fileInfos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal class FileInfoTestDouble : IFileInfo
    {
        private readonly byte[] _buffer;

        public bool Exists => throw new NotImplementedException();

        public long Length => throw new NotImplementedException();

        public string PhysicalPath => throw new NotImplementedException();

        private readonly string _name;
        public string Name => _name;

        public DateTimeOffset LastModified => throw new NotImplementedException();

        public bool IsDirectory => throw new NotImplementedException();

        public FileInfoTestDouble(string name, byte[] buffer)
        {
            _name = name;
            _buffer = buffer;
        }

        public Stream CreateReadStream()
        {
            var memorySream = new MemoryStream();
            memorySream.Write(_buffer, 0, _buffer.Length);
            memorySream.Position = 0;
            return memorySream;
        }
    }
}
