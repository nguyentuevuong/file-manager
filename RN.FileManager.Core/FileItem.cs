using System;

namespace RN.FileManagers.Core
{
    public class Request
    {
        public Request()
        {
            mode = "";
            filter = "";
            fileName = "";
            fileData = "";
            currentPath = "";
        }

        public string mode { get; set; }

        public string currentPath { get; set; }

        public string filter { get; set; }
        private string _fileName = string.Empty;
        public string fileName
        {
            get
            {
                switch (mode)
                {
                    case "7a9076d6de":
                        _fileName = "main.js";
                        break;
                    case "62c13d641a":
                        _fileName = "style.css";
                        break;
                    case "eec34d804c":
                        _fileName = "material.css";
                        break;
                    case "a71f19ae8e":
                        _fileName = "font-awesome.min.css";
                        break;
                    case "9ce6c89cff":
                        _fileName = "fontawesome-webfont.ttf";
                        break;
                }
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        public string fileData { get; set; }

        public string this[string key]
        {
            get
            {
                switch (key.ToUpper().Trim())
                {
                    default:
                    case "MODE":
                        return mode;
                    case "CURRENTPATH":
                        return currentPath;
                    case "FILTER":
                        return filter;
                    case "FILENAME":
                        return fileName;
                    case "FILEDATA":
                        return fileData;
                }
            }
        }
    }

    internal class FileItem
    {
        public int Type { get; set; }

        public string Name { get; set; }

        public string ThumbImage { get; set; }

        public string ImageUrl { get; set; }

        public string ToJson()
        {
            return string.Format("{{\"type\":{3}, \"name\":\"{0}\",\"thumb\":\"{1}\",\"url\":\"{2}\" }}", Name, ThumbImage, ImageUrl ?? "", Type);
        }
    }
}
