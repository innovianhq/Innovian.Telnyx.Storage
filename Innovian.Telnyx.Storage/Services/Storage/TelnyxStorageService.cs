// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Innovian.Telnyx.Storage.Core;
using Innovian.Telnyx.Storage.Enums;
using Innovian.Telnyx.Storage.Exceptions;
using Innovian.Telnyx.Storage.Extensions;
using Innovian.Telnyx.Storage.Services.Storage.Requests;
using Innovian.Telnyx.Storage.Services.Storage.Responses;
using Innovian.Telnyx.Storage.Utilities;
using Innovian.Telnyx.Storage.Validation;
using Microsoft.Extensions.Options;

namespace Innovian.Telnyx.Storage.Services.Storage;

public sealed class TelnyxStorageService : ITelnyxStorageService
{
    /// <summary>
    /// Used to create the HttpClient instances.
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;
    /// <summary>
    /// The client configuration options.
    /// </summary>
    private readonly TelnyxClientOptions _options;

    /// <summary>
    /// A local cache of the endpoints to use for each bucket to avoid frequent lookups.
    /// </summary>
    private readonly Dictionary<string, string> _bucketEndpointCache = new();
    
    /// <summary>
    /// Typical means of registering the Telnyx storage service if initially provisioned with a known key.
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="options"></param>
    public TelnyxStorageService(IHttpClientFactory httpClientFactory, IOptions<TelnyxClientOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    /// <summary>
    /// A factory delegate used to instantiate this service with a key at runtime.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create the HttpClient instances.</param>
    /// <param name="apiKey">The API key as resolved at run-time.</param>
    /// <returns></returns>
    public delegate TelnyxStorageService Factory(string apiKey, IHttpClientFactory httpClientFactory);

    /// <summary>
    /// Used to instantiate this class via the DI factory.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create the HttpClient instances.</param>
    /// <param name="apiKey">The API key for the service.</param>
    public TelnyxStorageService(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _httpClientFactory = httpClientFactory;
        _options = new TelnyxClientOptions
        {
            TelnyxApiKey = apiKey
        };
    }

    #region Bucket requests

    /// <summary>
    /// Lists all buckets.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task<ConditionalValue<ListAllMyBucketsResult>> ListBucketsAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri("https://telnyxstorage.com/");

        var client = BuildHttpClient();
        var response = await client.GetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var serializer = new XmlSerializer(typeof(ListAllMyBucketsResult));
            using var stringReader = new StringReader(await response.Content.ReadAsStringAsync(cancellationToken));
            var deserializedValue = (ListAllMyBucketsResult)serializer.Deserialize(stringReader);
            return new ConditionalValue<ListAllMyBucketsResult>(deserializedValue);
        }

        return new ConditionalValue<ListAllMyBucketsResult>();
    }

    /// <summary>
    /// Gets a specific bucket's location.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task<ConditionalValue<string>> GetBucketLocationAsync(string bucketName,
        CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"https://telnyxstorage.com/{bucketName}?location=null"); //Intended endpoint per email notification 10/9/2023

        var client = BuildHttpClient();
        var response = await client.GetAsync(uri, cancellationToken);

        var responseStr = await response.Content.ReadAsStringAsync(cancellationToken);
        using var stringReader = new StringReader(responseStr);
        var parserResponse = await stringReader.ReadToEndAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            //Parse the error
            var serializer = new XmlSerializer(typeof(ErrorResponse));
            using var sr = new StringReader(responseStr);
            var result = (ErrorResponse) serializer.Deserialize(sr);
            throw new ErrorException(result.Code)
            {
                HostId = result.HostId,
                RequestId = result.RequestId
            };
        }

        var parser = new RegexResultParser();
        var deserializedValue = parser.ParseBucketLocationResult(parserResponse);

        if (deserializedValue != null)
        {
            //Update the cache with the identified value
            _bucketEndpointCache[bucketName.ToLowerInvariant()] = deserializedValue;

            return new ConditionalValue<string>(deserializedValue);
        }

        return new ConditionalValue<string>();
    }

    /// <summary>
    /// Creates a bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="locationConstraint">The location to create the bucket in and submit all future requests to.</param>
    /// <param name="isPublic">Indicates the visibility of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task CreateBucketAsync(string bucketName, LocationConstraint locationConstraint, bool isPublic = false, CancellationToken cancellationToken = default)
    {
        if (!Validators.IsBucketNameValid(bucketName))
            throw new ValidityFailureException(Validators.BucketNameValidityMessage);

        var uri = new Uri($"https://{locationConstraint.GetValueFromEnumMember()}.telnyxstorage.com/{bucketName}");
        
        var serializer = new XmlSerializer(typeof(CreateBucketConfiguration));
        await using var stream = new Utf8StringWriter();
        serializer.Serialize(stream, new CreateBucketConfiguration
        {
            LocationConstraint = locationConstraint.GetValueFromEnumMember()
        });

        var xml = stream.ToString();
        var requestBody = new StringContent(xml, Encoding.UTF8, "application/xml");
        requestBody.Headers.Add("x-amz-acl", isPublic ? "public-read" : "private");

        var client = BuildHttpClient();
        var response = await client.PutAsync(uri, requestBody, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
            return;

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new TargetAlreadyExistsException();

        throw new Exception(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <summary>
    /// Deletes a bucket.
    /// </summary>
    /// <remarks>
    /// The bucket must be empty for it to be deleted.
    /// </remarks>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}/{bucketName}");

        //Delete the bucket from Telnyx
        var client = BuildHttpClient();
        await client.DeleteAsync(uri, cancellationToken);

        //Delete the bucket location from the cache
        _bucketEndpointCache.Remove(bucketName.ToLowerInvariant());
    }

    /// <summary>
    /// Determines if a bucket exists and you have permission to access it.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task<bool> HeadBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            return false;

        var uri = new Uri($"{baseAddress.Value}");

        var client = BuildHttpClient();
        var msg = new HttpRequestMessage(HttpMethod.Head, uri);
        var response = await client.SendAsync(msg, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    #endregion

    #region Object requests

    /// <summary>
    /// Deletes an object from a given bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task DeleteObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        //In this case, we don't put the bucket name in the subdomain - just the 
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken, false);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}/{bucketName}/{objectName}");

        var client = BuildHttpClient();
        await client.DeleteAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Retrieves an object from a given bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="uploadId"></param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bytes comprising the object.</returns>
    public async Task<byte[]> GetObjectAsync(string bucketName, string objectName, string? uploadId = null, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}/{objectName}{(uploadId == null ? "" : $"?uploadId={uploadId}")}");

        var client = BuildHttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
        var response = await client.GetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new TargetNotFoundException();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            using var ms = new MemoryStream();

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await stream.CopyToAsync(ms, cancellationToken);
            return ms.ToArray();
        }

        throw new Exception(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <summary>
    /// Retrieves metadata from an object without returning the object itself.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task<HeadObjectResponse> HeadObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}/{objectName}");

        var client = BuildHttpClient();
        var msg = new HttpRequestMessage(HttpMethod.Head, uri);
        var response = await client.SendAsync(msg, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new TargetNotFoundException();

        var anticipatedHeaders = new List<string>
        {
            "Accept-Ranges",
            "ETag",
            "x-amz-request-id",
            "Date",
            "Server"
        };

        var headerValues = new Dictionary<string, string?>();
        foreach (var anticipatedHeader in anticipatedHeaders)
        {
            if (response.Headers.TryGetValues(anticipatedHeader, out var values) && values.Any())
            {
                headerValues.Add(anticipatedHeader, values.First());
            }
            else
            {
                headerValues.Add(anticipatedHeader, null);
            }
        }

        DateTime? dateValue = null;
        if (headerValues["Date"] != null)
        {
            if (DateTime.TryParse(headerValues["Date"], out var parsedDate))
            {
                dateValue = parsedDate;
            }
        }

        return new HeadObjectResponse(headerValues["Accept-Ranges"], headerValues["ETag"],
            headerValues["x-amz-request-id"], dateValue, headerValues["Server"]);
    }

    /// <summary>
    /// Adds an object to a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="objectName">The object name.</param>
    /// <param name="filePath">The path to the file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}/{objectName}");

        var client = BuildHttpClient();
        
        var streamContent = new StreamContent(File.OpenRead(filePath));

        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        //using var ms = new MemoryStream();
        //await streamContent.CopyToAsync(ms, null, default);
        //var str = Encoding.UTF8.GetString(ms.ToArray());
        
        var response = await client.PutAsync(uri, streamContent, cancellationToken);
        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new TargetAlreadyExistsException();

        throw new Exception(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <summary>
    /// Adds an object to a bucket.
    /// </summary>
    /// <param name="bucketName">The name of the Telnyx bucket.</param>
    /// <param name="objectName">The name of the object as stored in the Telnyx container.</param>
    /// <param name="fileName">The name of the originating file absent the path.</param>
    /// <param name="content">The bytes comprising the object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task PutObjectAsync(string bucketName, string objectName, string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}/{objectName}");

        var client = BuildHttpClient();

        using var ms = new MemoryStream(content);
        var streamContent = new StreamContent(ms);

        using var formContent = new MultipartFormDataContent();
        formContent.Add(streamContent, fileName);
        formContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var response = await client.PutAsync(uri, formContent, cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new TargetAlreadyExistsException();

        throw new Exception(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <summary>
    /// Deletes one or more objects from a given bucket.
    /// </summary>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="keys">The object names to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task DeleteObjectsAsync(string bucketName, List<string> keys, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}?delete=true");
        var req = new DeleteObjectsRequest
        {
            Objects = keys.Select(k => new DeleteObject
            {
                Key = k
            }).ToList()
        };
        var payload = Serialize(req);
        var requestBody = new StringContent(payload, Encoding.UTF8, "application/xml");

        var client = BuildHttpClient();
        var response = await client.PostAsync(uri, requestBody, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
            return;

        throw new Exception(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <summary>
    /// Lists all objects contained in a given bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task<ListBucketResult> ListObjectsV2Async(string bucketName, CancellationToken cancellationToken = default)
    {
        //Retrieve the base address from the cache if available, but otherwise fall back to the location endpoint.
        var baseAddress = await GetBaseAddress(bucketName, cancellationToken);
        if (!baseAddress.HasValue)
            throw new TargetNotFoundException();

        var uri = new Uri($"{baseAddress.Value}/?list-type=2");

        var client = BuildHttpClient();
        var response = await client.GetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var serializer = new XmlSerializer(typeof(ListBucketResult));
            var strContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using var sr = new StringReader(strContent);
            return (ListBucketResult)serializer.Deserialize(sr);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new TargetNotFoundException();

        throw new Exception(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    #endregion

    #region Utility methods

    /// <summary>
    /// Builds and configures the HttpClient instance with default headers.
    /// </summary>
    /// <returns></returns>
    private HttpClient BuildHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization",
            $"AWS4-HMAC-SHA256 Credential={_options.TelnyxApiKey}/useast-1-execute-api-aws2_request");

        return httpClient;
    }

    /// <summary>
    /// Calculates the base address for a given bucket name.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="includeBucketInSubdomain">True if the bucket name should be placed in the subdomain value.</param>
    /// <returns></returns>
    private async Task<ConditionalValue<string>> GetBaseAddress(string bucketName, CancellationToken cancellationToken, bool includeBucketInSubdomain = true)
    {
        //Determine if the cache contains the bucket name
        var cacheKey = bucketName.ToLowerInvariant();
        if (_bucketEndpointCache.ContainsKey(cacheKey))
        {
            //Retrieve the location of the bucket
            var bucketLocation = await GetBucketLocationAsync(bucketName, cancellationToken);

            if (!bucketLocation.HasValue)
                return new ConditionalValue<string>();

            //Update the cache
            _bucketEndpointCache[cacheKey] = bucketLocation.Value;

            //We don't always want the bucket name in here
            var bucketNameToAdd = includeBucketInSubdomain ? bucketName + '.' : string.Empty;

            return new ConditionalValue<string>($"https://{bucketNameToAdd}{bucketLocation.Value}.telnyxstorage.com");
        }

        return new ConditionalValue<string>();
    }

    /// <summary>
    /// Serializes arbitrary data to XML.
    /// </summary>
    /// <param name="dataToSerialize">The object to serialize.</param>
    /// <returns></returns>
    private static string Serialize(object dataToSerialize)
    {
        if (dataToSerialize == null)
            return null;

        var serializer = new XmlSerializer(dataToSerialize.GetType());
        var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);

        serializer.Serialize(writer, dataToSerialize);
        return sw.ToString();
    }

    #endregion
}