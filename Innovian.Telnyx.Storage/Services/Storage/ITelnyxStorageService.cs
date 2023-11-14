// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using Innovian.Telnyx.Storage.Core;
using Innovian.Telnyx.Storage.Enums;
using Innovian.Telnyx.Storage.Services.Storage.Responses;

namespace Innovian.Telnyx.Storage.Services.Storage;

public interface ITelnyxStorageService
{
    /// <summary>
    /// Lists all buckets.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<ConditionalValue<ListAllMyBucketsResult>> ListBucketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific bucket's location.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<ConditionalValue<GetBucketLocationResult>> GetBucketLocationAsync(string bucketName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="locationConstraint">The region to create the bucket in.</param>
    /// <param name="isPublic">Indicates the visibility of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task CreateBucketAsync(string bucketName, LocationConstraint locationConstraint, bool isPublic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bucket.
    /// </summary>
    /// <remarks>
    /// The bucket must be empty for it to be deleted.
    /// </remarks>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a bucket exists and you have permission to access it.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<bool> HeadBucketAsync(string bucketName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an object from a given bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task DeleteObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an object from a given bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="uploadId"></param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bytes comprising the object.</returns>
    Task<byte[]> GetObjectAsync(string bucketName, string objectName, string? uploadId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves metadata from an object without returning the object itself.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<HeadObjectResponse> HeadObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an object to a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="filePath">The path to the file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an object to a bucket.
    /// </summary>
    /// <param name="bucketName">The name of the Telnyx bucket.</param>
    /// <param name="objectName">The name of the object as stored in the Telnyx container.</param>
    /// <param name="fileName">The name of the originating file absent the path.</param>
    /// <param name="content">The bytes comprising the object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task PutObjectAsync(string bucketName, string objectName, string fileName, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes one or more objects from a given bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="keys">The object names to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task DeleteObjectsAsync(string bucketName, List<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all objects contained in a given bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<ListBucketResult> ListObjectsV2Async(string bucketName, CancellationToken cancellationToken = default);
}