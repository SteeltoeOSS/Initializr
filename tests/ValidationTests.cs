using Steeltoe.Initializr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Steeltoe.Initializr.Tests
{
    public class ValidationTests
    {
        [Fact]
        public void ProjectNameValidationTest()
        {
            var attrib = new ProjectNameValidationAttribute();
            var value = "123";
            var result = attrib.IsValid(value);

            Assert.False(result, "ProjectName cannot start with numbers");
        }

        [Fact]
        public void ProjectNameValidation_TestSegments()
        {
            var attrib = new ProjectNameValidationAttribute();
            var value = "Test.123";
            var result = attrib.IsValid(value);

            Assert.False(result, "No segment of ProjectName can start with numbers");
        }

        [Fact]
        public void ProjectNameValidation_TestHyphens()
        {
            var attrib = new ProjectNameValidationAttribute();
            var value = "Test-result.Foo";
            var result = attrib.IsValid(value);

            Assert.False(result, "No segment of ProjectName can contain hyphens");
        }

        [Fact]
        public void ProjectNameValidation_TestColons()
        {
            var attrib = new ProjectNameValidationAttribute();
            var value = "Test-result.Foo:boo";
            var result = attrib.IsValid(value);

            Assert.False(result, "No segment of ProjectName can contain :");
        }
    }

}
