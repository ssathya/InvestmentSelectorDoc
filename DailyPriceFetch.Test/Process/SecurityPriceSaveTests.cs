using AutoMapper;
using DailyPriceFetch.Process;
using Microsoft.Extensions.Logging;
using MongoRepository.Repository;
using Moq;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;

namespace DailyPriceFetch.Test.Process
{
	[TestFixture]
	public class SecurityPriceSaveTests
	{
		private MockRepository mockRepository;

		private Mock<ILogger<SecurityPriceSave>> mockLogger;
		private Mock<IMapper> mockMapper;
		private Mock<IAppRepository<string>> mockAppRepository;
		private Mock<IHttpClientFactory> mockHttpClientFactory;

		[SetUp]
		public void SetUp()
		{
			this.mockRepository = new MockRepository(MockBehavior.Strict);

			this.mockLogger = this.mockRepository.Create<ILogger<SecurityPriceSave>>();
			this.mockMapper = this.mockRepository.Create<IMapper>();
			this.mockAppRepository = this.mockRepository.Create<IAppRepository<string>>();
			this.mockHttpClientFactory = this.mockRepository.Create<IHttpClientFactory>();
		}

		private SecurityPriceSave CreateSecurityPriceSave()
		{
			return new SecurityPriceSave(
				this.mockLogger.Object,
				this.mockMapper.Object,
				this.mockAppRepository.Object,
				this.mockHttpClientFactory.Object);
		}

		[Test]
		public async Task ComputeSecurityAnalysis_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var securityPriceSave = this.CreateSecurityPriceSave();

			// Act
			var result = await securityPriceSave.ComputeSecurityAnalysis();

			// Assert
			Assert.IsTrue(result);
			this.mockRepository.VerifyAll();
		}

		[Test]
		public async Task GetPricingData_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var securityPriceSave = this.CreateSecurityPriceSave();

			// Act
			var result = await securityPriceSave.GetPricingData();

			// Assert
			Assert.Fail();
			this.mockRepository.VerifyAll();
		}
	}
}
