using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using Utilities.StringHelpers;

namespace Utilities.AWSS3
{
	public class ReadWriteS3Objects
	{
		private const int BufferSize = 32768;
		private readonly string _bucketName = @"talk2control-1";
		private readonly RegionEndpoint _region = RegionEndpoint.USEast1;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadWriteS3Objects"/> class.
		/// </summary>
		/// <param name="bucketName">Name of the bucket.</param>
		/// <param name="region">The region.</param>
		public ReadWriteS3Objects(string bucketName, RegionEndpoint region)
		{
			_bucketName = bucketName;
			_region = region;
			if (_region == null)
			{
				_region = RegionEndpoint.USEast1;
			}
		}

		public ReadWriteS3Objects()
		{
		}

		/// <summary>
		/// Gets the data from s3.
		/// Useful to read ASCII files.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <returns></returns>
		public async Task<string> GetDataFromS3(string objectName)
		{
			var responseBody = "";
			AmazonS3Client client;
			try
			{
				var awsKey = Environment.GetEnvironmentVariable("AccesssKeyId");
				var awsSecret = Environment.GetEnvironmentVariable("SecureAccessKey");
				if (!awsKey.IsNullOrWhiteSpace() && !awsSecret.IsNullOrWhiteSpace())
				{
					client = new AmazonS3Client(awsKey, awsSecret, _region);
				}
				else
				{
					client = new AmazonS3Client(_region);
				}
				var request = new GetObjectRequest
				{
					BucketName = _bucketName,
					Key = objectName
				};
				using var response = await client.GetObjectAsync(request);
				using var responseStream = response.ResponseStream;
				using var reader = new StreamReader(responseStream);
				responseBody = await reader.ReadToEndAsync();

				return responseBody;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Something went wrong while reading S3");
				Console.WriteLine(ex.Message);
				Console.WriteLine("Terminating application");
				throw;
			}
		}

		public async Task<string> GetEncryptedDataFromS3(string objectName)
		{
			var returnString = await GetDataFromS3(objectName);
			returnString = returnString.Decrypt();
			return returnString;
		}

		public async Task<string> ReadExcelFile(string objectName)
		{
			var client = new AmazonS3Client(_region);
			var request = new GetObjectRequest
			{
				BucketName = _bucketName,
				Key = objectName
			};
			var fileName = GetTempFileName();
			using var fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, BufferSize,
				FileOptions.RandomAccess);
			using GetObjectResponse response = await client.GetObjectAsync(request);
			using Stream responseStream = response.ResponseStream;
			var data = new byte[BufferSize];
			int bytesRead = 0;
			do
			{
				bytesRead = responseStream.Read(data, 0, BufferSize);
				fs.Write(data, 0, bytesRead);
			}
			while (bytesRead > 0);
			return fileName;
		}

		public async Task<bool> UploadObject(FileInfo fileInfo)
		{
			try
			{
				using var client = new AmazonS3Client(_region);
				var putRequest = new PutObjectRequest
				{
					BucketName = _bucketName,
					Key = fileInfo.Name,
					FilePath = fileInfo.FullName,
					ContentType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
				};
				putRequest.Metadata.Add("UploadDate", DateTime.Now.ToString());
				await client.PutObjectAsync(putRequest);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private string GetTempFileName()
		{
			return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");
		}
	}
}