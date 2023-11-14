// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Security.Cryptography;
using Innovian.Telnyx.Storage.Enums;
using Innovian.Telnyx.Storage.Exceptions;
using Innovian.Telnyx.Storage.Extensions;
using Innovian.Telnyx.Storage.Services.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Innovian.Telnyx.Storage.Tests;

[TestClass]
public class IntegrationTest
{
    private static ITelnyxStorageService BuildStorageService()
    {
        var services = new ServiceCollection();

        var apiKey = Environment.GetEnvironmentVariable("Telnyx_ApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("Telnyx API key not found");

        services.AddTelnyxClient(opt =>
        {
            opt.TelnyxApiKey = apiKey;
        });
        var app = services.BuildServiceProvider();
        return app.GetRequiredService<ITelnyxStorageService>();
    }

    /// <summary>
    /// Start with a potential clean-up task - if the bucket exists, retrieve all the keys in it, delete each
    /// one and then delete the bucket.
    /// </summary>
    /// <returns></returns>
    private async Task CleanupBucketAsync()
    {
        var storageSvc = BuildStorageService();

        //Check to see if the bucket exists
        var bucketCheck = await storageSvc.HeadBucketAsync(Constants.BucketName);
        if (!bucketCheck)
            return;

        //Check to see if there's anything in the bucket
        var objectList = await storageSvc.ListObjectsV2Async(Constants.BucketName);
        if (objectList.Contents == null || objectList.Contents.Length == 0)
            return;

        //Delete all the keys in this bucket
        await storageSvc.DeleteObjectsAsync(Constants.BucketName, objectList.Contents.Select(c => c.Key).ToList());

        //Delete the bucket
        await storageSvc.DeleteBucketAsync(Constants.BucketName);
    }

    [TestMethod]
    public async Task EndToEndTest()
    {
        var storageSvc = BuildStorageService();

        //Start with a potential clean up task
        await Assert.ThrowsExceptionAsync<ErrorException>(async () => await CleanupBucketAsync());

        const string fileName = "onemegabyte.txt";

        //Test for non-existent bucket existence
        const string nonExistentBucketName = "integration-nonexistent-test-bucket";

        //Get the bucket location
        await Assert.ThrowsExceptionAsync<ErrorException>(async () =>
            await storageSvc.HeadBucketAsync(nonExistentBucketName));
        
        //Get the MD5 has of the input file
        var originalHash = await ComputeFileHashAsync(fileName);
        
        //Create a bucket
        await storageSvc.CreateBucketAsync(Constants.BucketName, LocationConstraint.Central);

        //Get the bucket location
        var bucketLocationResult = await storageSvc.GetBucketLocationAsync(Constants.BucketName);
        Assert.IsTrue(bucketLocationResult.HasValue);
        Assert.AreEqual(bucketLocationResult.Value, LocationConstraint.Central.GetValueFromEnumMember());
        
        //Test that this bucket exists
        var existingBucketExistenceCheck = await storageSvc.HeadBucketAsync(Constants.BucketName);
        Assert.IsTrue(existingBucketExistenceCheck);

        //List the buckets on the account - the non-existent bucket name should not exist, but the created one should
        var existingBucketNames = await storageSvc.ListBucketsAsync();
        Assert.IsTrue(existingBucketNames.HasValue);

        //The non-existent bucket should not exist
        Assert.IsTrue(existingBucketNames.Value.Buckets.All(b => b.Name != nonExistentBucketName));

        //The created bucket should exist
        Assert.IsTrue(existingBucketNames.Value.Buckets.Any(b => b.Name == Constants.BucketName));

        //List the objects in the created bucket - should be none
        var objectList1 = await storageSvc.ListObjectsV2Async(Constants.BucketName);
        Assert.AreEqual(Constants.BucketName, objectList1.Name);
        Assert.IsNull(objectList1.Contents);
        
        //Check for the presence of a non-existent item in the bucket
        await Assert.ThrowsExceptionAsync<TargetNotFoundException>(async () =>
            await storageSvc.HeadObjectAsync(Constants.BucketName, "nonexistent.jpg"));

        //Upload an object to storage
        await storageSvc.PutObjectAsync(Constants.BucketName, fileName, fileName);

        //Check for the presence of this newly uploaded object in storage
        var existingObjectHeadResult = await storageSvc.HeadObjectAsync(Constants.BucketName, fileName);
        Assert.IsNotNull(existingObjectHeadResult.Date);
        Assert.IsNotNull(existingObjectHeadResult.AcceptRanges);
        Assert.IsNotNull(existingObjectHeadResult.RequestId);
        Assert.IsNotNull(existingObjectHeadResult.Etag);
        Assert.IsNotNull(existingObjectHeadResult.Server);

        //Upload the file twice more with two different names
        const string altFileName = "alt.txt";
        const string altName1 = $"test/folder/{altFileName}";
        
        //Generate the byte array comprising a second file
        const string altName2 = "test/folder/alt2.txt";
        var altFileContents = new byte[500000];
        for (var a = 0; a < altFileContents.Length; a++)
        {
            altFileContents[a] = 0x20;
        }

        //Upload from a file
        await storageSvc.PutObjectAsync(Constants.BucketName, altName1, fileName);
        //Upload from a byte array
        await storageSvc.PutObjectAsync(Constants.BucketName, altName2, "alt2.txt", altFileContents);

        //List the objects in the bucket
        var objectList2 = await storageSvc.ListObjectsV2Async(Constants.BucketName);
        Assert.IsNotNull(objectList2.Contents);
        Assert.AreEqual(3, objectList2.Contents.Length);
        Assert.IsTrue(objectList2.Contents.Any(a => a.Key == fileName));
        Assert.IsTrue(objectList2.Contents.Any(a => a.Key == altName1));
        Assert.IsTrue(objectList2.Contents.Any(a => a.Key == altName2));

        //Delete the second alt object
        await storageSvc.DeleteObjectAsync(Constants.BucketName, altName2);

        //List all the objects remaining in the bucket
        var objectList3 = await storageSvc.ListObjectsV2Async(Constants.BucketName);
        Assert.IsNotNull(objectList3.Contents);
        Assert.AreEqual(2, objectList3.Contents.Length);
        Assert.IsTrue(objectList3.Contents.Any(a => a.Key == fileName));
        Assert.IsTrue(objectList3.Contents.Any(a => a.Key == altName1));

        //Attempting to download the deleted file should fail
        await Assert.ThrowsExceptionAsync<TargetNotFoundException>(async () =>
            await storageSvc.GetObjectAsync(Constants.BucketName, altName2));

        //Download the second file and write to a local file
        var altFileBytes = await storageSvc.GetObjectAsync(Constants.BucketName, altName1);
        await File.WriteAllBytesAsync(altFileName, altFileBytes);

        //Compare the hashes of the original file to this one
        var altHash = await ComputeFileHashAsync(altFileName);
        Assert.AreEqual(originalHash, altHash);
        //Delete the local file
        File.Delete(altFileName);
        
        //Delete the remaining files from the bucket
        await storageSvc.DeleteObjectsAsync(Constants.BucketName, new List<string> {fileName, altName1});

        //List the files in the bucket once more (should be empty)
        var objectList4 = await storageSvc.ListObjectsV2Async(Constants.BucketName);
        Assert.IsNull(objectList4.Contents);

        //Delay 5 seconds just to ensure everything has been deleted on the server before attempting a bucket deletion
        await Task.Delay(TimeSpan.FromSeconds(5));

        //Delete the created bucket
        await storageSvc.DeleteBucketAsync(Constants.BucketName);
    }
    
    /// <summary>
    /// Computes an MD5 hash from the specified file.
    /// </summary>
    /// <param name="fileName">The name of the file in the local directory to compute the hash for.</param>
    /// <returns>The hex representation of the MD5 hash.</returns>
    private async Task<string> ComputeFileHashAsync(string fileName)
    {
        await using var fs = File.OpenRead(fileName);
        var md5 = MD5.Create();
        var hash = BitConverter.ToString(await md5.ComputeHashAsync(fs));
        return hash;
    }
}