using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rx.Filemanagers
{
    public static class Resourses
    {
        public const string INDEX = "index.html";
        public const string MAIN_JS = "main.js";
        public const string STYLE_CSS = "style.css";
        public const string ANGULAR_JS = "angular.js";
        public const string FONT_AWEASOME_CSS = "font-awesome.css";
        public const string FONT_AWEASOME_TTF = "font-awesome.ttf";
    }

    public class FileItem
    {
        public FileType type { get; set; } = FileType.other;

        public string fileName { get; set; } = string.Empty;

        public string fileUplPath { get; set; } = string.Empty;

        public string fileThumbPath { get; set; } = string.Empty;
    }

    public class Request
    {
        public RequestType m { get; set; } = RequestType.index;

        public string filter { get; set; } = "*.*";

        private string _fileName = string.Empty;
        public string fileName
        {
            get
            {
                switch (m)
                {
                    case RequestType.index:
                        return Resourses.INDEX;
                    case RequestType.main:
                        return Resourses.MAIN_JS;
                    case RequestType.style:
                        return Resourses.STYLE_CSS;
                    case RequestType.angular:
                        return Resourses.ANGULAR_JS;
                    case RequestType.fontcss:
                        return Resourses.FONT_AWEASOME_CSS;
                    case RequestType.fontfile:
                        return Resourses.FONT_AWEASOME_TTF;
                }

                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        public string fileData { get; set; } = string.Empty;

        public string currentPath { get; set; } = string.Empty;
    }

    public class FileContext
    {
        private Request request { get; set; } = new Request();

        public void SetRequest(Request req)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            request = req;
        }

        public string Type
        {
            get
            {
                if (request == null)
                    throw new ArgumentNullException("request");

                switch (request.m)
                {
                    case RequestType.index:
                        return "text/html";
                    case RequestType.style:
                    case RequestType.fontcss:
                        return "text/css";
                    case RequestType.main:
                    case RequestType.angular:
                        return "application/javascript";
                    case RequestType.fontfile:
                        return "application/x-font-ttf";
                    case RequestType.getfiles:
                    case RequestType.gettrees:
                        return "application/json";
                    default:
                        return "octet/stream";
                }
            }
        }

        public object Content
        {
            get
            {
                if (request == null)
                    throw new ArgumentNullException("request");

                switch (request.m)
                {
                    default:
                    case RequestType.index:
                        return GetResourceStreamString(Resourses.INDEX);
                    case RequestType.main:
                        return GetResourceStreamBytes(Resourses.MAIN_JS);
                    case RequestType.style:
                        return GetResourceStreamBytes(Resourses.STYLE_CSS);
                    case RequestType.angular:
                        return GetResourceStreamBytes(Resourses.ANGULAR_JS);
                    case RequestType.fontcss:
                        return GetResourceStreamBytes(Resourses.FONT_AWEASOME_CSS);
                    case RequestType.fontfile:
                        return GetResourceStreamBytes(Resourses.FONT_AWEASOME_TTF);
                }
            }
        }

        private string GetResourceStreamString(string fileName)
        {
            using (Stream stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(string.Format("Rx.Filemanagers.data.{0}", fileName)))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private byte[] GetResourceStreamBytes(string fileName)
        {
            using (Stream stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(string.Format("Rx.Filemanagers.data.{0}", fileName)))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }

    }

    public enum FileType
    {
        other = 0,
        image = 1,
        folder = 2
    }

    public enum RequestType
    {
        index = 1,
        main = 2,
        style = 3,
        angular = 4,
        fontcss = 5,
        fontfile = 6,
        upload = 7, // Tải lên tập tin
        download = 8, // Tải xuống tập tin
        delete = 9, // Xóa tập tin
        getfiles = 10, // Lấy thông tin file & folder thư mục hiện hành
        gettrees = 11
    }
}
