// filepath: c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\MySimpleSdk\src\MySimpleSdk.Tests\Client\SdkClientTests.cs
using System;
using System.Threading.Tasks;
using Xunit;

namespace MySimpleSdk.Tests.Client
{
    public class SdkClientTests
    {
        private readonly SdkClient _sdkClient;

        public SdkClientTests()
        {
            _sdkClient = new SdkClient();
        }

        [Fact]
        public async Task GetData_ShouldReturnData_WhenCalled()
        {
            // Arrange
            var expectedData = new SdkModel { Id = 1, Name = "Test", Description = "Test Description" };

            // Act
            var result = await _sdkClient.GetData(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedData.Id, result.Id);
            Assert.Equal(expectedData.Name, result.Name);
            Assert.Equal(expectedData.Description, result.Description);
        }

        [Fact]
        public async Task PostData_ShouldReturnSuccess_WhenCalledWithValidData()
        {
            // Arrange
            var newData = new SdkModel { Id = 2, Name = "New Test", Description = "New Test Description" };

            // Act
            var result = await _sdkClient.PostData(newData);

            // Assert
            Assert.True(result);
        }
    }
}