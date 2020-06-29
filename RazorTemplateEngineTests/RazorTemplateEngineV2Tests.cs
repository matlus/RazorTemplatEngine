using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazorTemplatEngine;
using RazorTemplatEngine.Exceptions;
using RazorTemplatEngine.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RazorTemplateEngineTests
{
    [TestClass]
    public class RazorTemplateEngineV2Tests
    {
        private const string TemplatesFolder = "Templates";
        private readonly RazorTemplateEngineV2 _razorTemplateEngine;
        public RazorTemplateEngineV2Tests()
        {
            _razorTemplateEngine = new RazorTemplateEngineV2();
        }

        [TestMethod]
        public async Task RenderTemplateAsync_WhenTemplateExistsAndModelProvided_ShouldRenderTemplate()
        {
            // Arrange
            var expectedStrings = new List<string>();

            var enterpeiseSystem = "Build Orbit";
            var application = "Marketing Application";
            var title = "Load Balanced Virtual Machines";
            var jobStart = new JobStart(title, enterpeiseSystem, application,
                new List<VirtualMachine>
                {
                    new VirtualMachine("A12345678", 90d, 27d),
                    new VirtualMachine("B24681012", 63d, 15.8d),
                    new VirtualMachine("C135791113", 47d, 63d)
                });

            expectedStrings.Add(enterpeiseSystem);
            expectedStrings.Add(application);
            expectedStrings.Add(title);

            foreach (var vm in jobStart.VirtualMachines)
            {
                expectedStrings.Add(vm.Name);
                expectedStrings.Add(vm.Cpu.ToString());
                expectedStrings.Add(vm.Memory.ToString());
            }

            // Act            
            var fullHtmlContent = await _razorTemplateEngine.RenderTemplateAsync(jobStart);

            // Assert
            AssertStringContains(fullHtmlContent, expectedStrings);
        }

        [TestMethod]
        public async Task RenderTemplateAsync_WhenRazorOrResourceTemplateDoNotExist_ShouldThrow()
        {
            // Arrange
            var model = new object();

            // Act
            try
            {
                _ = await _razorTemplateEngine.RenderTemplateAsync(model);
                Assert.Fail("We were expecting an Exception of type RazorTemplateEngineEmbeddedResourceNotFoundException to be thrown, but no exception was thrown");
            }
            catch (RazorTemplateNotFoundException e)
            {
                StringAssert.Contains(e.Message, "/Templates/Object.cshtml, was not found");
            }
        }

        private static void AssertStringContains(string value, IEnumerable<string> contentParts)
        {
            var exceptionMessages = new StringBuilder();

            bool somePartNotFound = false;

            foreach (var part in contentParts)
            {
                if (!value.Contains(part))
                {
                    somePartNotFound = true;
                    exceptionMessages.AppendLine($"The Expected substring: {part}, was not found in the string.");
                }
            }

            if (somePartNotFound)
            {
                throw new AssertFailedException(exceptionMessages.ToString());
            }
        }
    }
}
