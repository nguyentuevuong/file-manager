using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RN.FileManagers.Core
{
    public class FileContext
    {
        /// <summary>
        /// Hàm khởi tạo ẩn
        /// </summary>
        private FileContext() { }

        private const string base64 = ";base64,";

        private string UserName { get; set; }
        public Request Request { get; protected set; }

        private Configuration config = new Configuration();
        public void SetAppPath(string appPath)
        {
            config = new Configuration(appPath);
        }

        public void SetRequest(Request request)
        {
            Request = request ?? new Request();

            switch (Request.mode)
            {
                default:
                case "html":
                    {
                        Type = "text/html";
                        break;
                    }
                case "css":
                case "62c13d641a":
                case "eec34d804c":
                case "a71f19ae8e":
                    {
                        Type = "text/css";
                        break;
                    }
                case "js":
                case "7a9076d6de":
                    {
                        Type = "text/javascript";
                        break;
                    }
                case "font":
                case "9ce6c89cff":
                    {
                        Type = "application/x-font-ttf";
                        break;
                    }
                case "folder":
                case "addfolder":
                case "get":
                case "getfiles":
                case "delete":
                case "upload":
                case "uploadthumb":
                    {
                        Type = "application/json";
                        break;
                    }
                case "file":
                case "download":
                    {
                        Type = "octet/stream";
                        break;
                    }
            }
        }

        public void SetUserName(string userName)
        {
            UserName = (userName ?? string.Empty).Trim();
        }

        public string Type { get; private set; }

        private string GetFullUploadFilePath(string fileName = null)
        {
            return string.Format(@"{0}\{1}\{2}", config.UploadFullPath, Request.currentPath, fileName ?? Request.fileName).MatchPath();
        }

        private string GetFullThumbFilePath(string fileName = null)
        {
            return string.Format(@"{0}\{1}\{2}", config.ThumbFullPath, Request.currentPath, fileName ?? Request.fileName).MatchPath();
        }

        private string GetFullUploadUrl(string fileName = null)
        {
            if (config.RootUrl.IndexOf("http") == -1)
            {
                return string.Format("?mode=file&fileName={0}", Uri.EscapeUriString(string.Format("{0}\\{1}", Request.currentPath, fileName ?? Request.fileName)));
            }
            else
            {
                return string.Format("{0}/{1}", config.RootUrl, string.Format("{0}/{1}/{2}", config.UploadPath, Request.currentPath, fileName ?? Request.fileName)).MatchUrl();
            }
        }

        public object Content
        {
            get
            {
                if (Request == null)
                    throw new ArgumentNullException("request");

                switch (Request.mode)
                {
                    default:
                    case "html":
                        return GetResourceStreamString("index.html").Replace("__RNSCRIPT__", config.Json);
                    case "js":
                    case "css":
                    case "7a9076d6de":
                    case "62c13d641a":
                    case "eec34d804c":
                    case "a71f19ae8e":
                        return GetResourceStreamString(Request.fileName);
                    case "font":
                    case "9ce6c89cff":
                        return GetResourceStreamBytes(Request.fileName);
                    case "folder":
                    case "addfolder":
                        return CreateFolder();
                    case "get":
                    case "getfiles":
                        return GetFiles();
                    case "delete":
                        return Delete();
                    case "file":
                        return GetFile(Request.fileName);
                    case "download":
                        return GetFile(Request.fileName, false);
                    case "upload":
                    case "uploadthumb":
                        return Upload();
                }
            }
        }

        private byte[] GetFile(string fileName = null)
        {
            try
            {
                return File.ReadAllBytes(GetFullUploadFilePath(fileName));
            }
            catch
            {
                return new byte[] { };
            }
        }

        private string GetFile(string fileName, bool thumb)
        {
            try
            {
                if (thumb)
                    return Convert.ToBase64String(File.ReadAllBytes(GetFullThumbFilePath(fileName)));
                else
                    return Convert.ToBase64String(File.ReadAllBytes(GetFullUploadFilePath(fileName)));
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetFiles()
        {
            string filter = Request.filter ?? "*.*";

            if (String.IsNullOrEmpty(filter))
                filter = "*.*";

            if (filter.IndexOf(".") == -1)
                filter = String.Format("*{0}*.*", filter);

            if (!filter.StartsWith("*"))
                filter = String.Format("*{0}", filter);

            if (!filter.EndsWith("*"))
                filter = String.Format("{0}*", filter);


            var objs = new List<string>();
            try
            {
                foreach (var fileName in Directory.GetDirectories(GetFullUploadFilePath(), filter))
                {
                    try
                    {
                        FileItem fileItem = new FileItem();

                        fileItem.Type = 0;
                        fileItem.Name = Path.GetFileName(fileName).ToLower();
                        fileItem.ThumbImage = Configuration.FolderImage;

                        objs.Add(fileItem.ToJson());
                    }
                    catch
                    {
                        continue;
                    }
                }

                foreach (var fileName in Directory.GetFiles(GetFullUploadFilePath(), filter))
                {
                    try
                    {
                        FileItem fileItem = new FileItem();

                        fileItem.Type = 1;
                        fileItem.Name = Path.GetFileName(fileName).ToLower();
                        fileItem.ThumbImage = string.Format("data:image/png;base64,{0}", GetFile(fileItem.Name, true));
                        fileItem.ImageUrl = GetFullUploadUrl(Path.GetFileName(fileName)).ToLower();
                        objs.Add(fileItem.ToJson());
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {
            }

            return string.Format("[{0}]", string.Join(",", objs));
        }

        private string Delete()
        {
            try
            {
                string fullThumbPath = GetFullThumbFilePath(),
                       fullUploadPath = GetFullUploadFilePath();

                if (Request.fileName.Contains("."))
                {
                    File.Delete(fullThumbPath);
                    File.Delete(fullUploadPath);
                }
                else
                {
                    Directory.Delete(fullThumbPath);
                    Directory.Delete(fullUploadPath);
                }

                return "true";
            }
            catch
            {
                return "false";
            }
        }

        private string CreateFolder()
        {
            try
            {
                Request.fileName = Request.fileName.Replace(".", "");
                string fullThumbPath = GetFullThumbFilePath(),
                       fullUploadPath = GetFullUploadFilePath();

                if (!Directory.Exists(fullThumbPath))
                    Directory.CreateDirectory(fullThumbPath);

                if (!Directory.Exists(fullUploadPath))
                    Directory.CreateDirectory(fullUploadPath);

                return "true";
            }
            catch
            {
                return "false";
            }
        }

        private string Upload()
        {
            try
            {
                string fileContent = Request.fileData;
                if (fileContent.IndexOf(base64) == -1)
                    return string.Format("{{\"status\": false, \"url\": \"{0}\"}}", "");

                fileContent = fileContent.Substring(fileContent.IndexOf(base64) + base64.Length);

                var bytes = Convert.FromBase64String(fileContent);

                if (Request.mode.IndexOf("thumb") > -1)
                    File.WriteAllBytes(GetFullThumbFilePath(), bytes);
                else
                    File.WriteAllBytes(GetFullUploadFilePath(), bytes);

                return string.Format("{{\"status\": true, \"url\": \"{0}\"}}", GetFullUploadUrl().ToLower());
            }
            catch
            {
                return string.Format("{{\"status\": false, \"url\": \"{0}\"}}", "");
            }
        }

        private string GetResourceStreamString(string fileName)
        {
            using (Stream stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(string.Format("RN.FileManager.Core.data.{0}", fileName)))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private byte[] GetResourceStreamBytes(string fileName)
        {
            using (Stream stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(string.Format("RN.FileManager.Core.data.{0}", fileName)))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }

        public static FileContext Current
        {
            get
            {
                return new FileContext();
            }
        }
    }
}
