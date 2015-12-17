using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2Classes
{
    public class AuthorizeResponse
    {
        public string accountId { get; set; }
        public string apiUrl { get; set; }
        public string authorizationToken { get; set; }
        public string downloadUrl { get; set; }
    }

    public class CreateBucketResponse
    {
        public string bucketId { get; set; }
        public string accountId { get; set; }
        public string bucketName { get; set; }
        public string bucketType { get; set; }
    }

    public class CreateBucketRequest
    {
        public string accountId { get; set; }
        public string bucketName { get; set; }
        public string bucketType { get; set; }
    }

    public class GetUploadURLRequest
    {
        public string bucketId { get; set; }
    }

    public class GetUploadURLResponse
    {
        public string bucketId { get; set; }
        public string uploadUrl { get; set; }
        public string authorizationToken { get; set; }
    }

    public class ListBucketsRequest
    {
        public string accountId { get; set; }
    }

    public class ListBucketsResponse
    {
        public List<Bucket> buckets { get; set; }
    }

    public class Bucket
    {
        public string bucketId { get; set; }
        public string accountId { get; set; }
        public string bucketName { get; set; }
        public string bucketType { get; set; }
    }

    public class FileInfo
    {
        public string author { get; set; }
    }

    public class UploadFileResponse
    {
        public string fileId { get; set; }
        public string fileName { get; set; }
        public string accountId { get; set; }
        public string bucketId { get; set; }
        public int contentLength { get; set; }
        public string contentSha1 { get; set; }
        public string contentType { get; set; }
        public FileInfo fileInfo { get; set; }
    }

    public class ListFileNamesRequest
    {
        public string bucketId { get; set; }
        public string startFileName { get; set; }
        public int maxFileCount { get; set; }
    }

    public class File
    {
        public string action { get; set; }
        public string fileId { get; set; }
        public string fileName { get; set; }
        public int size { get; set; }
        public object uploadTimestamp { get; set; }
    }

    public class ListFileNamesResponse 
    {
        public List<File> files { get; set; }
        public string nextFileName { get; set; }
    }
}
