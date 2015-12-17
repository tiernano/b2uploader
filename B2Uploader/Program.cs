using B2Classes;
using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace B2Uploader
{
    class CmdLineOptions
    {
        [Option('i', "accountid", HelpText = "Account ID", Required=true)]
        public string AccountId { get; set; }

        [Option('a', "appkey", HelpText = "Application Key", Required=true)]
        public string ApplicationKey { get; set; }

        [Option('d', "directory", HelpText="Directory you want to upload", Required=true)]
        public string Directory {get;set;}

        [Option('v', "verbose", HelpText="Verbose Output")]
        public bool Verbose{get;set;}
    }

    class Program
    {
        static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<CmdLineOptions>(args);

            var existCode = result.MapResult(options => {
                if (!Directory.Exists(options.Directory))
                {
                    Console.WriteLine("Directory to upload MUST EXIST!");
                    return 0;
                }

                var auth = AuthorizeUser(options.AccountId, options.ApplicationKey);
                var buckets = ListBuckets(new ListBucketsRequest() { accountId = auth.accountId }, auth.authorizationToken, auth.apiUrl);

                var bucket = buckets.buckets.First();

                string[] FilesToProcess = Directory.GetFiles(options.Directory);

                Parallel.ForEach(FilesToProcess, s =>
                {
                    //check if file already exists

                    string fileName = s.Replace('\\', '_');

                    var existingFiles = ListFileNames(new ListFileNamesRequest() { bucketId = bucket.bucketId, startFileName = fileName }, auth.apiUrl, auth.authorizationToken);
                    bool found = false;
                    foreach (var x in existingFiles.files)
                    {
                        if (x.fileName == fileName)
                        {
                            //check the file size
                            System.IO.FileInfo fi = new System.IO.FileInfo(s);
                            if (fi.Length == x.size)
                            {
                                found = true;
                                break;
                            }
                            else
                            {
                                //delete old file? could just be an older version... going to upload again...
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        Console.WriteLine("File {0} exists already, skipping", fileName);
                    }
                    else
                    {
                        var uploadURL = GetUploadURL(new GetUploadURLRequest { bucketId = bucket.bucketId }, auth.apiUrl, auth.authorizationToken);
                        var response = UploadFile(uploadURL.authorizationToken, "b2/x-auto", s, uploadURL.uploadUrl);
                    }
                });
                return 1;
            },
            errors =>{
                Console.WriteLine(errors);
                return 1;
            });
        }

        static AuthorizeResponse AuthorizeUser(string accountId, string applicationKey)
        {            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://api.backblaze.com/b2api/v1/b2_authorize_account");
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(accountId + ":" + applicationKey));
            webRequest.Headers.Add("Authorization", "Basic " + credentials);
            webRequest.ContentType = "application/json; charset=utf-8";
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            return JsonConvert.DeserializeObject<AuthorizeResponse>(responseString);
        }

        static ListBucketsResponse ListBuckets(ListBucketsRequest request, string authToken, string apiURL)
        {            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiURL + "/b2api/v1/b2_list_buckets");
            string body = JsonConvert.SerializeObject(request);
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();

            return JsonConvert.DeserializeObject<ListBucketsResponse>(responseString);
        }

        static GetUploadURLResponse GetUploadURL(GetUploadURLRequest request, string apiUrl, string authToken)
        {          
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_get_upload_url");
            string body = JsonConvert.SerializeObject(request);
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            return JsonConvert.DeserializeObject<GetUploadURLResponse>(responseString);
        }

        static UploadFileResponse UploadFile(string authToken, string contentType, string filePath, string uploadUrl)
        {

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            String sha1 = GetSha1(bytes);
            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uploadUrl);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authToken);
            webRequest.Headers.Add("X-Bz-File-Name", filePath.Replace('\\','_'));
            webRequest.Headers.Add("X-Bz-Content-Sha1", sha1);
            webRequest.ContentType = contentType;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            var resp = JsonConvert.DeserializeObject<UploadFileResponse>(responseString);

            if(resp.contentSha1 == sha1)
            {
                Console.WriteLine(responseString);
                return resp;
            }
            else
            {
                //something went wrong!
                return null;
            }
        }

        static ListFileNamesResponse ListFileNames(ListFileNamesRequest request, string apiUrl, string authToken)
        {
            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_list_file_names");
            string body = JsonConvert.SerializeObject(request);
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            return JsonConvert.DeserializeObject<ListFileNamesResponse>(responseString);
        }

        private static string GetSha1(byte[] bytes)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }

    
}
