//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

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
using Innovian.Telnyx.Storage.Validation;
using Microsoft.Extensions.Options;

namespace Innovian.Telnyx.Storage.Services.Storage;

public sealed class TelnyxStorageService
{
    /// <summary>
    /// Used to create the HttpClient instances.
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;
    /// <summary>
    /// The base address to make the requests to.
    /// </summary>
    private const string BaseAddress = "https://storage.telnyx.com";
    /// <summary>
    /// The client configuration options.
    /// </summary>
    private readonly TelnyxClientOptions _options;
    
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
    public delegate TelnyxStorageService Factory(IHttpClientFactory httpClientFactory, string apiKey);

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
        var uri = new Uri($"{BaseAddress}/");

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
    /// Creates a bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="locationConstraint">The region to create the bucket in.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task CreateBucketAsync(string bucketName, LocationConstraint locationConstraint = LocationConstraint.Dallas, CancellationToken cancellationToken = default)
    {
        if (!Validators.IsBucketNameValid(bucketName))
            throw new ValidityFailureException(Validators.BucketNameValidityMessage);

        var uri = new Uri($"{BaseAddress}/{bucketName}");
        
        var serializer = new XmlSerializer(typeof(CreateBucketConfiguration));
        await using var stream = new StringWriter();
        serializer.Serialize(stream, new CreateBucketConfiguration
        {
            LocationConstraint = locationConstraint.GetValueFromEnumMember()
        });
        var xml = stream.ToString();
        var requestBody = new StringContent(xml, Encoding.UTF8, "application/xml");

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
        var uri = new Uri($"{BaseAddress}/{bucketName}");

        var client = BuildHttpClient();
        await client.DeleteAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Determines if a bucket exists and you have permission to access it.
    /// </summary>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public async Task<bool> HeadBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{BaseAddress}/{bucketName}");

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
        var uri = new Uri($"{BaseAddress}/{bucketName}/{objectName}");

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
        var uri = new Uri($"{BaseAddress}/{bucketName}/{objectName}{(uploadId == null ? "" : $"?uploadId={uploadId}")}");

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
        var uri = new Uri($"{BaseAddress}/{bucketName}/{objectName}");

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
        var uri = new Uri($"{BaseAddress}/{bucketName}/{objectName}");

        var client = BuildHttpClient();
        client.DefaultRequestHeaders.Add("x-amz-server-side-encryption", "1");

        
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
        var uri = new Uri($"{BaseAddress}/{bucketName}/{objectName}");

        var client = BuildHttpClient();
        client.DefaultRequestHeaders.Add("x-amz-server-side-encryption", "1");

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
        var uri = new Uri($"{BaseAddress}/{bucketName}?delete=true");
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
        var uri = new Uri($"{BaseAddress}/{bucketName}?list-type=2");

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