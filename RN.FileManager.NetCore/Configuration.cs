using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RN.FileManagers.NetCore
{
    public class Configuration
    {
        private Configuration() { ReadData(); }

        public string BasePath
        {
            get
            {
                var assembly = typeof(Configuration).GetTypeInfo().Assembly;
                if (assembly.Location.LastIndexOf("\\bin\\") > -1)
                    return assembly.Location.Substring(0, assembly.Location.LastIndexOf("\\bin\\"));
                else if (assembly.CodeBase.IndexOf("file:///") == 0)
                {
                    var basePath = assembly.CodeBase.Replace("file:///", "").Replace("/", "\\");

                    return basePath.Substring(0, basePath.LastIndexOf("\\bin\\"));
                }
                else
                {
                    return "C:\\";
                }
            }
        }

        public string RootPath
        {
            get
            {
                var rootPath = this["RootPath"].Replace('/', '\\');
                if (rootPath == "\\" || string.IsNullOrEmpty(rootPath))
                    return BasePath;

                return this["RootPath"];
            }
        }

        public string RootUrl
        {
            get
            {
                return this["RootUrl"];
            }
        }

        public string UploadPath
        {
            get
            {
                return this["UploadPath"];
            }
        }

        public string ThumbPath
        {
            get
            {
                return this["ThumbPath"];
            }
        }

        public string ThumbFullPath
        {
            get
            {
                var configPath = this["ThumbPath"];
                if (configPath.IndexOf(":\\") == -1)
                    configPath = Regex.Replace(string.Format("{0}\\{1}", RootPath, configPath), "\\+|\\/+", "\\");

                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                return configPath;
            }
        }

        public string UploadFullPath
        {
            get
            {
                var configPath = this["UploadPath"];
                if (configPath.IndexOf(":\\") == -1)
                    configPath = string.Format("{0}\\{1}", RootPath, configPath).MatchPath();

                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                return configPath;
            }
        }

        public bool PrivateUser
        {
            get
            {
                try
                {
                    return bool.Parse(this["PrivateUser"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        public float MaxUploadSize
        {
            get
            {
                try
                {
                    return float.Parse(this["MaxUploadSize"]);
                }
                catch
                {
                    return 2;
                }
            }
        }

        public Size ImageSize
        {
            get
            {
                try
                {
                    return new Size(int.Parse(this["ImageSizeWidth"]), int.Parse(this["ImageSizeHeight"]));
                }
                catch
                {
                    return new Size(800, 600);
                }
            }
        }

        public bool AcceptUpload
        {
            get
            {
                try
                {
                    return bool.Parse(this["AcceptUpload"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool AcceptDelete
        {
            get
            {
                try
                {
                    return bool.Parse(this["AcceptDelete"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool AutoUpload
        {
            get
            {
                try
                {
                    return bool.Parse(this["AutoUpload"]);
                }
                catch
                {
                    return false;
                }
            }
        }

        //public string[] MusicExtensions { get; set; }

        //public string[] VideoExtensions { get; set; }

        //public string[] ImageExtensions { get; set; }

        //public string[] OtherExtensions { get; set; }

        //public string[] AllExtensions
        //{
        //    get
        //    {
        //        return ImageExtensions
        //            .Concat(MusicExtensions)
        //            .Concat(VideoExtensions)
        //            .Concat(OtherExtensions)
        //            .ToArray();
        //    }
        //}

        private Dictionary<string, string> dict = new Dictionary<string, string>();

        private string this[string key]
        {
            get
            {
                if (!dict.ContainsKey(key))
                    dict.Add(key, "");

                return dict[key];
            }
        }

        private void ReadData()
        {
            var assembly = typeof(Configuration).GetTypeInfo().Assembly;
            string dbPath = string.Format("{0}\\App_data", assembly.Location.Substring(0, assembly.Location.LastIndexOf("\\bin\\")));
            string dbFile = string.Format("{0}\\filemanagers.cfg", dbPath);

            if (!File.Exists(dbFile))
                WriteData();

            var data = File.ReadLines(dbFile);
            foreach (var line in data)
            {
                var _line = line.Replace("',", "").Replace("'", "");
                if (_line.IndexOf(':') > -1)
                {
                    var _json = _line.Split(new[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (_json.Length >= 2)
                    {
                        dict[_json[0].Trim()] = string.Join(":", _json.Skip(1)).Trim();
                    }
                }
            }
        }

        private void WriteData()
        {
            var assembly = typeof(Configuration).GetTypeInfo().Assembly;
            string dbPath = string.Format("{0}\\App_data", assembly.Location.Substring(0, assembly.Location.LastIndexOf("\\bin\\")));
            string dbFile = string.Format("{0}\\filemanagers.cfg", dbPath);
            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);

            if (!File.Exists(dbFile))
            {
                dict["RootPath"] = "\\";
                dict["RootUrl"] = "/";
                dict["UploadPath"] = "\\uploads\\files";
                dict["ThumbPath"] = "\\uploads\\thumbs";
                dict["PrivateUser"] = "true";
                dict["MaxUploadSize"] = "2mb";
                dict["ImageSizeWidth"] = "800";
                dict["ImageSizeHeight"] = "600";
                dict["AcceptUpload"] = "true";
                dict["AcceptDelete"] = "true";
                dict["AutoUpload"] = "true";
            }

            File.WriteAllText(dbFile, ToJson());
        }

        private string ToJson()
        {
            string json = string.Join(",\n", dict.Select(m => string.Format("\t'{0}':'{1}'", m.Key, m.Value)));

            return string.Format("{{\n{0}\n}}", json);
        }

        public string Json
        {
            get
            {
                return string.Format("rn.autoupload = {0}; rn.filesize = {{ width: {1}, height: {2} }};", AutoUpload, ImageSize.Width, ImageSize.Height).ToLower();
            }
        }

        private static Configuration _Instance = null;

        public static Configuration Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = Nested.Initialize;

                return _Instance;
            }
        }

        private class Nested
        {
            internal static readonly Configuration Initialize = new Configuration();
        }
    }

    public class Size
    {
        public Size() { }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; } = 800;

        public int Height { get; set; } = 600;
    }
}
