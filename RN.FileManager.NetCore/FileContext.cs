using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RN.FileManagers.NetCore
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
            return string.Format(@"{0}\{1}\{2}", Configuration.Instance.UploadFullPath, Request.currentPath, fileName ?? Request.fileName).MatchPath();
        }

        private string GetFullThumbFilePath(string fileName = null)
        {
            return string.Format(@"{0}\{1}\{2}", Configuration.Instance.ThumbFullPath, Request.currentPath, fileName ?? Request.fileName).MatchPath();
        }

        private string GetFullUploadUrl(string fileName = null)
        {
            if (Configuration.Instance.RootUrl.IndexOf("http") == 0)
                return string.Format("{0}/{1}", Configuration.Instance.RootUrl, string.Format("{0}/{1}/{2}", Configuration.Instance.UploadPath, Request.currentPath, fileName ?? Request.fileName)).MatchUrl();
            else
                return string.Format("{0}?mode=file&fileName={1}", Configuration.Instance.RootUrl, Uri.EscapeUriString(string.Format("{0}\\{1}", Request.currentPath, fileName ?? Request.fileName)));
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
                        return GetResourceStreamString("index.html").Replace("__RNSCRIPT__", Configuration.Instance.Json);
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
                Request.fileName = Path.GetFileName(GetFullUploadFilePath(fileName));
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
                Request.fileName = Path.GetFileName(GetFullUploadFilePath(fileName));
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
                        fileItem.ThumbImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKAAAACgCAYAAACLz2ctAAAgAElEQVR4Xu2dCZRcZ3Xn73u19ypZljcMNsaExawGzBYjhHEgbBMGDGEssyTDSXLIgFkCh0AmToAEGIZ95rBMgLAabIx3g5FRy2rZlmxrs2QtLbXUUktqtXrfqrurq96ce+93v/e9V2+r7hIttaptneqlllfv/ep/1+9+FjS+GmdgEc+AtYiv3XjpxhmABoANCBb1DJxWADoOfiBuysBxSEMabBgvpmC5Oj9OwYEyVOinmdEy3U7PVWB2hQPPhQrcssvBX1nvuoX/1vg6I87AaQGgc+Sj50Cu/W8BnFXgOBeCBecAWM0A0AIAaT6TThHAmgFwHHCsUfW7SbCsEqLI/9TfLMcBsI6CY82AVR6g+9t2GSrOIUg5ZXBKR6CSKoMz3Wdd8JXJM+JKLdGDXFQAna7/kYP25TcCwBdmirOpqbEJmJ6cgtmJcZiZnIRMCqAyMwXplA3ZfBZSaRvsdBrsdBZQK9O5Ali2BalMDqyUDVYqQ38nJUyl+NaWWxssC9+uBZZte+4DjlMEyzoCALMAzkHmHY6BbU+AAyPglPshZc2A4xziB8IUzEIFLJiGtF2CSrkMdmWK/mZnJ0mpV8KUZd3Eit34Cj0Diwag49xkQ79zP1iwuvfAMXvb+q0AlRLkshbksynI5VKQy9qQydiQSduQVd8jcPif9l4t+gmA4CK8+Hv8Pd6q+9P3fgBtBjKVyQKk0mCnUmBnsvS4VD7PPKWzBLMJN4CDqotwzdGtBRVwSImR3BKA5YDllKBiOWBbk+BAN4AzC46zHyyrDFDpAguBrgxDutILJbtonf/5A2cjp4sG4OzhT740nc1vHOwfzt7zk7WQz9qQy9mQz6Ugn8PvGcBshv8xiCmw0xbYBBPjxv+7QGoAiUn19jz3QTDdx9KjA58r/PesshZYqTRYFiqvTapKHw3jb3RYKXVf/ECgKsutqDO9F3488+ucZLfCGQeACQBrGixriP9W6QcL5qBsDUPKmYaKhW7JCNj0Yegn5U0X+sFum4LlKw5Z1rsmTneoFw1A5+Q//SU49i8O7umFdXdsZACzCJ9FEGoABUK8TacglUEA7doB1DAqxdTQBoPmQh0AuVJSFl45hV4lJtBMZfa/vii2hl+pNroXqLyZDIFJsCPc+J5T+DuEGm+NS6efowBWqhXAbgVItVUg1fprSNn/BLmB/Zb1rtMyOFs0AOd6P/2WVCZ953SxZN35n2vJ50PoSP0IRFbDbNaCbCalVDAFmYwFqVT9AAwDLRxAA9h6Axik2gpiDbOoOVHKqssuB6ooKmwaIJUDyCwDq+XZYOXOnQFIrYMy3Gi1/ene000RFw3AI/e+fdW5f3Lpunxbm1WeK8NDv3sUho4PQKU0qxRQAZhBIJUPmE5BJmtBOm0EFB5zaviA/osZooB1BVCOxQ+N51j8yijugF9B1XvxPVa7HElfo3AJwLIrwcos2wd2/i1W/sVdpxOEiwZg189Xr8ovb+8459KLoWnFCnAcB2anZ2FseBwO7dwPI339DKI2waiG6Ata7AumVER7OgKojqlWE2ze3wTN9WUNdyApgHS/DMC5q8FquvQkQOoqq/BijuZPg69FBTDX3tqRby1ArrkJsi0tkCk0gZ3LkV+FmQ1My0wMDUOlVIK54iRUposAlVlKzyCAFACcjgDWAId7/CqK9wdO81RApascu5C5zoF17rVOsQh3Nf2/rW+3bjo9UkSnBYDpbJbyeXiqnApAtqkJcu1tYGcZRvqStAoAlKYmoTQxCrPjY1CaGAGn4uAjMfmh7+sJEMhNMp6Hn1AHMkFR8BnhA+pAxr2MHrU0FQ5z9JCCcbgS/e33XPCyj918Ggjg4tWC0QSLAhKAaFLxJFUw21CBSgW/cTgHhxEgIpNOw9CRk/CM1/4FwMxByt2h6XbKc1CZmyOlrMzNwlxxCsrTU3SLP3tg9KVdwtIwixIFJwlCPIGP94PpfZ/VeGF9aHLmHBgazm4Y3N9/zUv/5nsqd7l4KJ4+CigAOgBO2QWwUnEAKg4DCQC9uw7BS9d8DGDoYY7+MLdmY+TH6QqSNZVgxr8jmAjiXHGCzXhpFpxyGZxKmW6VuAbkFMPygKKmfkU185H+1E3Ac7kvzOpcg98YFYgISm56iH+DH1T8Klcs6OlOHy3NDL7ginfdwvnFRfxafABbCuBRQAEQpZDAUwDiz0EAumdcfedWQjA1QYASnJwEpgtRqYDjVEhpy9NFgrM0OQHlmSJwGZlNdHCCenEA9Kub39R6ApgYoHoPTk2PHBm//EV/d/fRRWSPP/yLdQDaBHsA5E9qoALWBKDvrWn/z2Z1tBhGUkyVQyOVwLpaaRYqpRmozOKt/JsBp1Qi1XThrFbAmqoqfgVUP+sLolRR0ub+xLZfBfU7NhPU/otLfiDA4LGxyoEdBy579Se29izW9de6sVgHIADmWvKGAlYDiAGGxwQ/2eM1wfI5cq+cWyVxrwq/TU/1QB5glPUISIRUAWqeHDLnJZibmoRycQLK5F9i2Rcrvzr6qa5+GMdlRrzeFI060AAzHGluzaS07/35lUWOEO823DcMT6zbduk1Xzh4lgPY1tKRazVNcIQCKh/wqAA4/LAh4EZzghHduhrvqpVmyiin6bqyH1KfP6nVkuMljJbAmZtTPiVAuTQHzkwRKvivNKUid/ezHgugqYIB6ZgqFQwISGIFxXFg9OQobH9gawPAbFsL5QFdH7AOABq1WY/qeUyToXpRKhkHs0BMJh1bxHIAdhM4VhbmJgagNLgfnJlRjyInCTj85tQTUPhqzK4BSO5NNQAEADTBHgApcq0XgKJ4hi9YFwCNixz4fGjC0wCZlQD2CnDAhtLwQSj1P2YIb1jEm9wMB/p7fnMcIYUjJ0dgxwMNE+wB0LYtcpBDg5AoE2yqnlRGalK2ILB8EJvP6zHVPvMvf7PbALJPByd1DsyNdsPs8fUA5ZnqyNp4Lm9Vx1cbDsr/aevuU78wMVSOICrgjj80AGQAVRSMpTX0qwIBdNw8YLUPOA//r8q0hgAYGtgkAJbASgEUXgFgt8HcRA/M9NxBfmNVVcZQ0+qKjRE8hQQdnsg51gkE8gEbAIoJJgAzYKdsho/SMAo4Ao+ohEqZ84AE4A0fA9BBSK0AxpjnKjX1mkaPX5lECSEFkLsCHCsPc0M7YabvcbCl+VT8OY8Keo+vKr8XB2FVIGUQqaJ1AnDd9kYQohUwl+GOYAEQE9AInAawApUy248zD0A86hRA5jKKiqePboLyRJ9KdJsVE1dVA02xDyx/pcP/wYhKwyCATzQAVEEI5gFz2IzACsj1YATOBBCBrBeA81Q0j2+ZwGQb8Q85txglp8+H8swITB16VOXAubHWTDa7bl28EvJLBDh8YclorYBj8ERHQwGVD7hQAM0E83zTKwl8urAgJCq69vtjdgsBVzzeBXOTo4D9zEDFGLXQKiggMYKpUF+vhggYn270ZANAnYaRSgj6RdoHNLphqBJS5QN+3PABgyoc9UjDJPUVfR8AU/lMAKUUYafBmZuF8UO4QA7rzrzISkwqpxb5tV22vYn02KAjLgoeGIOdHTsaPmC2tbkjh0FILkuOeRWA6AtS84A3Cn7ZDT4AjVVxsvRSX3ujD9DlYb5KadAVqHw++gQEow6G7xErJpPHDgNU5siAYgqK+yEl9WIudvJ+7y4/Ve8mqvYbEhGPNgBUiegoAP0+IPUHAhzd3QPxAJqgBEFTBwCrok0zGg+SQaYQo/qZqRmYGegDZ26GeyNQAckU81oXLYKe9IwfbiNo8YMW5wMOjMPO9Q0FXJVpbeY8YJACVgUhKg0jAEo/IBsro68nIVxh6RY/WEnvVwVksPygAk4MjkFpfAic0jQtMcWXSNEaF0wdIoDkHSpVNHS7yg/1Qhlmef1HQgq4/omGCa4fgBGBiAnGgspxJuhBQYt5mf1qKAsz8D4OjPQNwuzEKFiVGTK66AMigNwZxkCSGspL+hRNp2l8L+lxMQL5ZxUeIwVsAGgoYIaUgHxAWhfizwMaiehABQwCMMRfS5Jo9qhZUKktwMTW4IsNHTsJs2OjYDmzyvxakLIZRO4IEwg5OnYPuVrjAmEM8f3k12OogA/ubCggKWAzmuBMQBCC0S+25lOB2K2EnCoAQ01vgInX/PmASAjhQM9xmB4fA8sp84oChM+2CT6GkP/pzhn8ndAT8hpVEXOcAjYAXM0K2FyAlAKQQFMt8zoRraNgalWAo7sPcxBCPqCpRIZSmSrnhyWpGfaroEf0AlI0QRfcfC3dtApwfF8PTE9Mgp12SPlI/VLqVv2MUbHMwKHARL3XKg1MCL15eGiCd21oKKACMA+pgCAE2/LdUpyRhkkCYFwfn74aEcGLXxH9gY5WpKpvYgwgLqw6QGPoUjhoCcFL2bTWmaDDn5UKcmDiKmG9IGwACAC7f756VYEUMBxA8gklES1pmD2GAoZCEtIiVWXGTCVLEmQE3L9mEB3o2bYPpqeKYGdsgi+N0KVtwJE3CF0KfUD8XuUHSQmr2rFcSU4a/WofcBAVcNfZ7QMKgLlmLsVVJaKVAlZVQhIBGOC3hZXSgpQt9L4GbZ6r7vcFQ0RQJaS7H9sNxakipHJpAo8ATFnq1qYlKWKa0fxS94z6TIUGHVHHow9HRcGDE/BkA8DVqwotTVwJySKA2JBavTDd3451TAP4kGe6gWtV5UoswLz6AQxSWo9PWPVDpBnet3EbFKdnId2UgzT6fKiCqIBp0BASgCmOirl9y42GEwUcflk0qjFjg+PwZOeTDQV0AfRHweH9gFUA+uGICjJC7xsQVOgLGJXzM/8W6/rpOzy57jGYnp2DNAZgSvlwCmw6zTCib5i20TxjIKLm4NBo4WDQvXFIlEE2FLABoFLAsDQMtl9JVGykYYIBNC9MkPIZfzevVlw9N1YJfUAkdMaeuP8RmC5VINPapABk+BBCMsno/9H3EpBgekYNYgr6YASw7w+OjSAcUAF3b9zdUMB8S1N4HlBHwSoPqNaEEIBrMA3zkG/9b5BShSlbmKolTDpXmWSTgDgKHdh+70YozjmQbW+lwAPhS+MIYmWKWQk5GOE8oQKQ5l37vuJ/YTzAVcAGgDgfsK4A1qCCcWbbI2wBUHp4iwOuWp623PUgFGfKkFux3A1AMhZkETwcwikBCe4KQD6gVEmqP1C1vzrA2NBEQwExCmYA85CiIAQdHFkDEt4R7THBpomsMpcLMMV+AI3gl78NuOw1JIS33NEBUyUHcucs02kYHLxJSojwUWTMPiD/40YtGk0cKLbu8cQBiRo4PjgBux9qmGAXwEzWuygJhwep8WzVUfARVQnZGBMFx9SHg8yo3yeMBDHuUocHJVvuXA+TJQeyy9sp2CDwlA8o33sAVKU5HmVjRPnJ4x7PPcdRAR/a0/ABUQEpD7hgAOsUZFSpWESPX8JgIIgRAnDWgcyydlI3joBxGwpRQf4Z1ZAmz1FdmBPV0i/oFeLaPgwNAFUlJN+sAKRlmca6YKMbRhYqybLMY3t8CqjZCwg45CqZsMSlV4IgrLK6vgueOBDgIGDLnQ/C5IwD6eXtBBWDxxvyeBUQIeQ0jHTKoD8Y5AYEIoi/NKcSqUcSgA/vbSjgwgE0lS/k+ypTGxJU+H3IMBDjYExgFhHAidkKpNtRAdnvo+HrGfb9EDpOTHMUzK1aSgGpKBLQbxjsHPqOhmkcH5qEPQ0AV6/KNxc6cpgHzOK6YP4Mc/1XrQM2FqaTTwgAWgEHNwaMYVN0xAUkftWUy+SHLjGEVVRGYogAjs+UId2OQQgrIMKGUbCYYgYTy3IchCCAWKKTBUw1xDxVx4IKuOfhfWe5Av549ap8uwIwk1Ezog0AdROCSkirZoTje5UJHuw0lo2FmN/QRLMJqg+eoCsbeLXjzHA4gwxgBVJtrIBkgpUKiikmAHU+kDtiuDlBZmqar58sChZrPNEAEGB3LIDSjuVtSA0GMKEpDlw74gdQy6GXoFDJqRFEBwDzgGPTFbBb26jakVXlt6xOxbhBCFVDlPJJPhAPRSwGHWSoA2i+BdcZRBO8t6GASgGbeEIq0JByho1G9NK4fHM2DJ9AD4Ae/y5MBaMUznxMQhCrfMoETp/vLlvu2gCjRQWgjoJxSzIpyUmDAndGc1VEfa/nsMckyCMOiwB8pGGCV+Xb0ARzIpq3aVDT3GVKvk5My2gOB47v7eU8IJpg+fh7FCBJOS0kvRKWWolyuObhjBGAk2Ww2tsILKp84C5QKYEQu2JSXAvGIASjYFy8pJoTWAFlkx6XtKSHwgB2NXzAXFtBV0I8QUjZSESreTGShmEF/IQBYIg/lySqDQW3BjU0lSYhAQjgyOQcWK3tgJthUuSLpThVC85g7g+jYty/WDWmEoQ6CJEtkWn5XM0SjADu29QAcJULoBEF465H9C+4JV+b4AEzCPGN5xCnyK9okdGx348M8QX13Wq/8PKMW+7uhOEJBLDNE4SwCcZ8oErDkG8oaRjpD+SGBNoI3jPeLTmHDOD+hgIigFQJkTSMDKicD4Chnc1xUW6cOQ7x8hdSCUEAx+cAWlt1OxZvwogNCayGHB3jpozsHuP8RExakxlWi9bxEDzBSEIGx4cbAFIUrAH0p2EoCJFRbd7hRMf3qTQMKmBQQBBazzUUq0oJ/WY8zASHhpyGIxZDgQOw/f5NcHJoGpzmVs4BEmyyE6gkpM00DHdFU0BCszyMLU7k/dYgyBPDUw0FrAJQNis0d0qiTY0UiGpCqgvgBm/+IarR1MONCWLQVQuqMvijZY/jl1B33LshgP1DRQKQul9wVRz2A+J8c1JA9guxP1CbYKNFnxarq7owzzSqgT4AmEAF3HygYYJzbfmOXJNZCcERvcZecWqYj6Rm8BImArCqVBWVoglStbBSVxSIET6jB1EHtt+/GfoHi1BuQgVUsBGAKimN+UAEENWRwGMF5IVKbHYpEiYljCvLVX8+SAEbAKIJFgDTPKIXFyX5NytUuUGJgvvEBJ/c4FuYHqAENbVXhfl6YepSYwLa4AABPDFQhEpTi2t+ET5KRNtACWmljLzVnTs/RlqyaK2ISsf4E9FhR6wrIcNT0PVoQwEJwGxTATLZtNqSlfcyq9orDpXQMMFXYR4wCMAqnzCuncoPUQRsoVauNvOHh7j995uh72QRyoUW1QfIfp/uiskoU4wQatPrDjCShUqykVOtgQgqYNej3Q0TnGvNd2SpH9BMw/AWWLJfsG5IRV9QmWAG8MHqsWzaCkaB5Q9GwgKOGOWrnTutgQLgHAJo43oQDkJQ+TSIPgWUSgjlBWmeoErHKJNciyM6MdIAkKPgKgDVumDVEVM1msMBON7VCy6ABjxBkW1cM0KV2xZAVWDE7L/cQVCHIIFR8NrN0Nc/BbO5VshgwKEiYErBEIiqR5Da8jEPyGtDeJ0wByDcFaP2VqRvkiM4OVyErscaCsgAUi3Y8AFpx3T0Bf21YG7HOr6vF656ryigH0C/mvmvSpxJDvMDE6Rfqq6//7XdZoDtax9VAIoJ5howKSDByIEJJqbR3FJJjubGqOlZanaMdMbIECM6hCgQ1SGgAu5/7ODZbYJxr7hUwQcg7ROCAGIpzp0VaM6IJgDFBHtOdkikW+UXqqsU9NiqCxhyNROpYrgiIYDHT0zCTK6F27BI9Rg8Mx9ITamkfCoCllKcWilHUbDqjKklEzMxUmwAWAUgLft3AdT5P9+QchfA9cE+oGYmLsCIyPd5QIyQlDCIY6whAnisbxJmsq4CZrK8LJMAlHSMdEQrMyyzBGlejErBaADlmONMsQNACvj4oYYCehQQAVQ7elNHtGpA9UzHEhNMCogARpjgMJWKXXgUYMfmCVoYh9vXPgbHTkzCdKZZrQHBbmgbslnOA1JZjhpUDeXTi9QFPne4uQw6T+oHogIeONsBxHXBuXyuI4st+RnlA4oJ9kfB5A4aPqAHwDgIw/y6OP+wKkJxeQpUmTjpcR+OAB49MQnFdLNWO4TN9QEFRDbBtDCJEtFqSwelfq4SqsaEhIdAAG45yxVwYQB+DKB/va8EZfqAMcFJoF8oghqTfvHLWsKLbj5s+9rHobdPAOQoWBRQIMQ2LZoVoyohskidu6KlFszfc2XEmCUd4wI0AFTLMlEBMxQFqzwgRSAcBXs6olWLFkXBlIZRAMYGGDE5Py1yQRT5gQ66qvOgDwB2dmyDnt4xmEw3Kb9P5f9oPIcooTspQZfiZJ60DDPXSmj0B4ZFwsbyzMlRVMCes9sHFAV0AVRrWOsBYFg0GxagRCli7BJIE8xkQO56cDscOjwCkzb7gOjzYRRM+T+zJqwCEjG9PC2LfT8ZaO7NCSZ7/Uk0wVvPdgAxEd2iFJB8QD55tBC9gl0wKg/oGVIO0KcVsCO8GyYqOImLcBPlM2pL/Pq1EwE82DMKE6kmFWwIgKyEvDxTNSZQDpCT0dgZQwvU1c5Keqq+PxUTxqFSQVLArYfPcgU0AExl0tzVIQ2pntnQxrJMQACPsgk+0VHdjBBmksOgi1LESPMc5mQlUSAHdm3YAQd7RmDcwiCEVY/zgJyMJj9QDSnifkEOPnhxkquAnAdkNeRND2MS0eqwJ0enobsBoKuAdQUw1i8MiG7rDmJ0FEAAHhqBMatJp13YBLsQkgLKWmE1plcDqAIRMb9qgq/e1iEmBgFUwO6tRxoKmG3JdWSbcpBK82wYrYA6COGAhCohKg3jKuC68GaEUJ8uKLDwJ6wj0i+BcUgS1fM+EAHsPhgEIPuDOc/6YJkb7S/HmSkZpYoJa8IE4LYGgKuSAqhrwx4TrAD08BJX/TBscVRyeaGJ5zA/Us3J3dX5BAE4CgVuwcpYkCf1kwjYDUZoWJGpgDgrUFdC1N4iPERfN6fGuICAJvhgA8DVq7LNSgEzOKTcDUIoDcN7dGFnltucYALYt867Q2ZUcBE3WiMOuKorWrvqmRrIAA7DiNPEzaeqEYHNsNuUwJOygEyxxwcUE6yjYamKJMsFEoDbGwq4KtucpW6YFPUDuqPENIB66y61YQ0CuF8FIX1/iOgHjDCjNcEYllRL5uyH+WK7OnfCAQSwgj4gKOhsyGQ5AuZkNCsjd0YD9Q3q7bz0poYyvtedF5PkozE5hgD2NnxAArCAkxEynnU1NQFY75xfKHMxlzbJlVdEogIe6B6B4QqbYMkD5rAhQVqyzDQMAkgL01VKRtIwKv1CDaqSion4zMgHghRwRwNAUsBMATuiKcFFfoweSGlMx+LcICexvAroU7qgaHZeyWffVYyFK/YOHjEkAA8ygJLvQ/ByqIC6K0blAhV4ZIYlDUNzYrgVi3fVVAuUpBwXczgI4KEdRxsKWD8Ag4IL4yrEpVkik89hUXJcsiP872iC93ePwJAoYBogh/4fKaBbiuO5gcoEq4XpNLBcGlKNPKDkBHkzm+ivqfEZOL5v4NJXf3Z3T9x9T/Xfa/vo1vFosCVfAMQ8oF5Yo5LQphKaUfAJ9AHXfAyAfMCgNbEL7PNLWgmJOxf+M2vUYndtVACW0QRjxOuCh2ZY+gFplZzyAaktX0XEvJWr24wqaqgbEnynwD+ld2ZqDvr7Si++6kMPbYt7G6f674sLYBOa4JzyAdVZEwDVRFRsTI0EMBDCIEWMMqsRpyERkLVdpl0PPQkHDgzBwFyBe/9U4EE+oF6cZJTijCAEAxKpBUvqhZdqBu2mFHxcszMVGJu0v/n8NQ98pLYjr/+9FxXATBMGIXUAsC4QJvDe6wTjnk17YN++ATiJAKpgg31AoxRnBCEyN5pTMW75TRamuwCqNEzMVS2XAYqlplKuKX/d09546x31xyr5M55eAOJxU1Oqu1khr01S07IAgEww1oKPP5CwFlyD8sXlA6POaw1wagBLGISoNEzadgHMugEIKqSU4PStzgOqIEQHJO62rlGHiqttynYrZFuXDRfOaf6rE5n0fc985rdmkmNTv3ue2QBqtmqogAQKXVSgUf9TtGfzXtiLCjib1+1YHISAqoig+VULlPScaLWZte4JdEdz1KqAgk9m2fnQfN5T5vIthfstKL/fuvDfT9YPrWTPVP+zm+x1aV2wNsFVQQhmXbgdi/0/XimHX64CrvW1Y4WY0FBlUm89SZUj8CzN/9R5AWTQxARjHVivDaGhRbKLupqWauwdF2SCcXZbLUeWW7YC2p56KaQLLSfBqvwvmBj7vvX0r48kvIwLvlstx7rgFzOfoApAKaQr6GQ8G62OIwg5lvMCGAAdvaOAt5Uk1TIf0OZxBvds3kcK2D+T83XDAORxVC8FJsCzAjH3p4ZU8s7qZkOqqoTI5HxJSCc8JtnCFaPqtqddDk0rL8AAZ/v4SN/r2571vYG6XvCQJ0t4qPU/FAQwXcAgJEst+W4pRHW/SCLa6A30AHhsbXUtWA4zDMLQYCXKTzTfe31O155HXQAlES3Kh6aYl2gqE4wml9IvooAcaLhrQ9ykNPUEJsgDhl3NTHMLNJ33VJibswaK41PXXXj1j7Dr95R+1eeMzuMQvQCmecYEfbkAUvARCuDvfbVgD338w6ImoMMTgXs2d5ECnkAFNNIwaIbz+LNRDcEoORhAtxIiPqDkAWuIh6qvHO7I1LwSilNz/VP9Qy+84gMdffO4vIkfcloAyIloBaCGrqJ8P4GQfcD+A8c4Cj6GABqURUWw81LEGOVbwJlDE7xn36A2wRgJUxlObmlIkSSkeTIC7aKutu3SeUA1qEgDaJTlYgmo2kPO/QV+NzmVganx6f/z4g9u/PvY51rAHRZwGhfwqrgqzjDBCwPQp3xxqZQoGCNNdNj7rf0UogneQwqYV+s/TAA5F8hLNbErGifmy4gOGVjujuglU4yBicyNTjA4f6TUBjfvvgI+8Y7LoVKagMrsGDilMSgXT8Dc+GF6o7OzDvT1jHe88hPbXhe85eHCrn+AvarPEyZ9lt0/vnpVulDgRHQ6zfuEiPbQU2gAABkDSURBVAUm/w8V0DctSyvgRwGOxpjgxNFtAoAWZNOqz8ieR9kE901jGoYnYeWxBKeS0dIRrQHUM2JMAA1fUCohqqEj7nA39T0Vrr/v3TD1m7dWHdzc1AnoX3sdWZ+urSc6X/sv+16jthBKemlrul+Cs1/T8yW+cxWAehtSTEKrSQi0a5JKw6iWfDbBAmACExyniPopEp6KuKub4AwggFgJOR4EYIYT03p1nOycSRPyVV+guTCd0jJul7R0xkQdxqbjT4Xrf/uXgQDi447e+Rp6OAF40xIHMJPPqdEcAoACUDqiYwH0+WphypdEEQ2ea8umJaDOuMuex/YrAHNKAW3IKfDcnkBliin1ovoA1S2ZXDUNwW3LcmcGxo1oQwVcEwXgHVfT0e7b2t+5eqkrIAOY4m4YBynBBUiSgKYx+RyMqKSVq4D3h7Ql1zpgPEL54qLo2rjT997zGCrgIBwvMoCyKJ2S0SoAkc5od8dMSUiz4slAIobR6AdMkIhmAN8TroAKwD1b+zuvaQAYAOAaNMEIoDAYAlHN7fceCQzHaz4Ja+PZurZ1w86dJ6B/hn1AngsjrfmqEpJiMKkdXxRQLVLnoIPH9Oo5MebU1BhvAk3wmt+FA9h7+5/S0e55/GTn6z+/lE1wvtCRwUQ0pv0JFlZAnITlaUJQ4zrwpPR3H4OrCMDfBZTiwgCKmmRgXK3QC5fQP0yoiHu3HoQdT/TBIHXDcBAippcVkH/HI3td5eMBRSr/ZyxIqrUWvBkV8Hf/LVQBe3/DAO5+/GTntV84CwCkKFgv6+fOFzcCZgWkyIQAPG4AGABcZJolIYixQrgwIHc+2g3bd/TBuMXtWLQSDm+zYoJRFd2NrKUUJ76gngujmlIJwBoV8Ib7owB8NQO4ZaDz2rNBAV0AOeNECigpGIQRz4YfwF5UwJhqR1z9N5ajgDvEPiZeBrdv6obt2/ugmG7iJZlohrNuIKJXxRGErIC0NliCEGMtSNWotgR5wE19T4MGgD++elUqX+jI5rNAiWjdjKA6YVQDgnTDkE02FVAAjIUwTs4UUbFghfmZ8cD577HloQOwbccJKOVkPBsDiCVxDj7UPiG0ixL7gLQqTu2U5EbBUo7zLkqKyxTFAXjkNlRAB57cMtj5hqVsggVAG4cT1Qzgb9V19YERFbnGVUFMSY0FMgq8qAc78Id7d0FXbxGsbIbXgyjoxP/jnZLUtq1qw2oGEBPRRh+gygfKfnEcGcdvHYcAvvf314f6gEduexW9uV1bBjvfePYAyBeUZ1S6LVi6H9BQwJdjEHLktwGpuhpg1MKYlLQkwUq8Gg4PTcHPbt4JqVyWVrxR14tKv7Dp5WgYgcO/kfk18n/mqjgxv5KSEfhCFVCVezediAbw8K9fSW/kiccGO9/8xa6lWwlJ5bAUlwUb98XVozlUC75aC8z+oBpShCb44HF4+ZobGcDA4ZE1QlgziAJZbf4h9jU+sXsAtmzrh4liiWq8vEWDmF1letWgSj0RQU3Ix2hYcn48I9A1v4kBVIfOAK4JVUABcPujg51v/dISBzBTyFAtuApA2aY1EkADhiS5Oc99kiSg4/zHeMXDe0xNleDhx/pgx+5B2mYLVUx8O0y5UC8g/ZM94vDW9f04CpYEtDEd1TDBZitWnA+4OQ7AW19Bb2zbo0Odb/vykgYwT3lAioINH5DTMDItlaNgGtNWpYB+AGKqIIHiFWOCk0IbwuLsbBl+/ut9MDQyw6ols16oxuuaYOoDVKkXgk8BKDsk8ZRUsxFVAg/vtl1yGqM+GgTg2hvCFVABuGXTQOdffOXAUjbB+Y6MGQWrs6bzgLIWxCzFiQk+fB/fO6rakUQVA0UuqV8YHAfJxR8aKsKtdx+AsYkyb7ll9PShAoriUfRr9P+ROtJEBKkBcxOCuQbETT4bcwIloI85/DgAe255Ob2FLZsHO9/eABBbY4xasB9ArWpRZ72GBHSEi1dLg0K5XIFbf7MXDvcr5VPqhRDS5KuUVDtkWSZXQEz4TAAl9SKRbhiAceYX397mE5fA+x4IV0ABcPtjI51v+/ISroSkcrkObEbAPCB+vKkQx/ZWrYZTi5FUcppNcB8HIaKAVVY4DMQk+b755PqqH4Pw/e6+fbDrUNGt14qa4cYzqvzGJtetelAJTvuHvFMmb1IjppcVkfYJ1muB1WxAGdWRQLxjAfwVKqADWx4d6Xz7V5a0D5jryGA6Qi1KSgbgcXj59R8FOHxv8MJ0E8i4SkiiGCPBFfV9CHqPDMOv7j4Ec47a7VIBhDAxVLLu1wAwy7DRcHK6HzciIGhYBcG3wnsGu76fuThJb16d4HAJwD+8N9QHPPSrq+gdbV3yAGZRAbNgqwGVAqCMY9OLksxaMJlgBaAHoKioNsY8JwIx0jZrBPGYf/nz7XB42B2fSwlkVECLo1ttgikJzT9z1Kt8P7m/ajzAfCEKn7Tf8xwYGU7kBiFinqMCEPzbphOXwPvjAHQc2LJ5uPO/fnUJByF2NkelOFu15GsAZR2w9AWSVeZS3EkN4D0h3TAxNCVxksyOzgSKYl7wrr0n4Z77DsBMpqAVi1XMTb8IcNzxolIuNIqXVU+6XnTqRTUaUCCD64Jp/YdbftOLksJiMh+Rm/sRwPeFK+AvX0aPeHTzcOeTXz2w6ibaueXUfNV4eut3ELt/cPUqu4kVUJoRkA1zHYiejBAEYM89vpggqlIxX3UMe7/ByW489rtufxJ290yBlc1y5KuUTzpZdKVDgUdbMVDaRS1A0uPYVNpFpW60CZZymx9AUshk1ycOwIM3v5QB3DTSufvrZxOAakKqQMhDyr0NqVoBEcBAsQsDMcFnLZE6hl/kuTkHfvSDx2FwNg22GigpE61410vD99NpGJVwVpGxO4DIDDTcxlO3G9oNTKQEp5fVxHBIAK4LV0ABcNMjw517v9G9hBWwkFOJaGzJ968L5sVIenWcqmMygDcCCICR5jJYqULIDb9sCcGcnZmDb35jE1RaWnXZjHa7VPk/jHBd86t8Qe0TcssVr/1l+CTo0IuOdCu+tOQbI3prVMAPrHt/qAk++IuX0LnY9MhI523f7H7tLWyCq1YSJ9Pb6HslkIV6vEz1c5AJRgApEZ0Ci5wadzyb2YbFwQg/RzWA8txRaZYoEGN8xqC3HwLk9PQcfOnLG6Fp5XLdKi+Kxnk/o/qhAg6BktZ+KAClyYAAlBVwCj7qhjHWAMuIDjzMWkxwIgA3jXbe9o0DZwGAlG/gTD93P6tpCGpKgviCBOAhlQc8dHdE31ENMIbyV/tnc2BgCr7z3cchf+4yPUCcAhBKr7j5Pwo8aC84NyIW5XMDDwYycAGSiohN0+tvw/IfvSlfj528FN4foYDdqICOA5s3j3Z+9xvdqzsAyupzWHcVrP0s10kQXQXkZgQ9JR+fPymA5rGEmsmo4MSnnoFqlzAQwQ9H/yT88IdbIbWsnfN2ypzqxgMj5UJqaFRF0hlOpxB0quPZ7f2THkDD9JpbNUhgkvDa9JUuhhvW3gBdP3wrWLjYmGa6uRGMNsGbRju/900CUEzwEgZQjRbjXkC3JYubENyNajwK6D/hSRLPkczFfB5j/nzy5BT85D+3gdPWpmu/knyWaFdHvKKCqtrBO2ICdct4/D/a/8PfeuWrfohZrkFOSk4OCq3ngp1tAzvTDHYqD2ClAZwyTB3bQB7fw5tGN37tW92rH2cFFPjqCmENh5zw45Xwbjt+cPWqHPmAGc4DUrnWomZUbYYVeDIx32uC71KvFPIWYgOHgMdFno34UzUwMAk/++kOKDdzEMKRLysdj2FzE9G65KYS0OgrivK5PqBX+cy+P08tWLXAxB+h9+LQhz3i6+FHRn9/w7cPvgltUgNAFYVoBTx4V/z4NfLvklyWuCAlmRkeHJiEm3+xA2YLrSoHyPChugl8vBk1+4C04EhBSguOzBqvasHnoUNG65V0xRjt90krIAm1ge6Gef8H1g195O9+ePj/KgDx13VXwSRXp5bjTnxfrYC5DNgUBVNsp9YF81AiHljuzQMOSBCCAHp8wNAffPer5S2HqWvw2xwcnIRf37ITptItrIC6sYCDECnDSUDi7gHCCWuzwUDat7jea1Q9lEmms6XNc7JNCquOWlIL1W/HGRwq/eLdH3nyrw8BzCnw/PDVxRTXcjUSw5XkjgRgPtuRpkqIGs0hAOpSnHeJJj7vQI+Kgg/eabzMAvN9iVQyXgWHhqbgjtt2wpjdRB8obr3i6DcrUTBtQs0KSICqSJf3/1CLjjzRL+cDuf6rmhEEPqMDZiFvQUiysA7lWMWJydL3//vnnvzHx49p+IJM8BIAsJBdl85lrZoBvP5GAASw6uOzQBATm+xgGIeHinD3HbtgCApkTqXzhZpPDf+Po2JlmpXyCYCetR7mzBdDCaUZQadhkpZA/IdtKGBprtI5MlH54f7eyYff++WeQ0r1BLylB6BKw6xL5zIGgHyGZGckbzDCfyMFvP4jDGAtVRCCy38FajQAMTIzMlyEe+/aBYPlAvltHHyY5lfKcbIFq7/y4Rs8KZsQGn6fDJAwl2DOl79yBQZmZivrdxwY/+oNX+p5Uvl6GPH6weNUhPtPTuSCVbDGK5DEuCa7jwtg2qpaE2L4fpQTVBtXVwMorxWV60sS7dbhNFgWjI4U4ff37YbjMzkKQkTp3OjX7feT1isKPKT85vH3vK32Zu+fKJ+phMnOunuv4nT5xM0PDr/p3356rBc4zSLgIXySdhHozDygCd2ZC+BD337lNStWNP2eFRA7okWiaGEwNyHIYiSpiJACnjAUMOi0xyWeI2AL/FNyOMdGi/CH+/fCkamsjn6lBOcGIbLaTZlhKbGhD6imXpmzXvxKJ+U2fZtgIbr/LM3OVuC3G0Y/+Ikf92JHRxB8CJwJ3ZID0L73iy99wzMuab8HAZQ8IFGIsOEZM7tgggDsNrY4W+QEtFzgsdFpePCBvXBoIqvTK5Lv49Z7NyXjbrvgLlbCPKh3rQd/Lk3o6O9GEKJSgHwICSORsfEy3Hr/wJov3tmPW46K8gUpYJgPWLd0TPKPd60aH33/zK3/cuUbn//M5XcQgDQbRingfABMdPKT5vpqOCW+u46PFWHjui7oHsuwCVZJZoQQVx2ISdbzXvRSSzbD3PXsHb1hmllTDbUZTs6dviKjY2W45f7BNV++++wFMHvz/3zRn7/42St+wwooaRiZjkWrkNTaYBnPxh86MsFrPgLQfXvIhFR1nhOpQVIokwUvE2PT8PCGfbB/OKPrwOT/SSeMvuUUDUXCuvXKu9ZD5/88+T539gvBaMKX6P3y+xgbK8Mvfzd4w1fu0QAGqaBphv1R8BmvgNmffPYFr7vquSvvTGUzGW7Hkv2C1RLMOAAP3B6/KCmRMgYEMhG/ihL2ifFpeOTB/bB/JO2W4cwoWOUCZbmlagLSEPrVzjS9BJvyF/EYkg4iCjpeVMBf3Df8nq/99sSDASbYNMV+PxCfTgIT+X5BtrEGe7Og1/E/OPNXb7vosk++59kb0pnsSjcPyHeTxUhmMILRsEcBEcBAYUrwlmpQC36JBM8JABMTDOCB0WoA0f/TpldKcIb6+Ve7+XN9QT97FLCGyzM6Vq5885a+a37aOdJlBCFhKmhGwksGQOwBatv5k9W35gu515nDiQg61YpP71a3ZvEZHhQTfOA36pTHRbXJ4EnqwEdd58mJGdi0YT/sDwBQGk+lLiyjNqT9yrPYSCmcP9L1K2BNAm8c+Mhoeegfvn349Q/un+r3RcFBEC5JAPF0NL3t5edd/vVPXfkw2Kkmcz5gFIDoA74CfUANYAJ7qVM8NchEzSoJMF2chYc6MAjBYUtutUOvfMMynFRBpAlBGhDM9b5GMGLm/LQKGlFwDe9I37Wnd3brdZ/vum50BnCTaqz1mqmYWkzwgs1wQnmYz9uMfUwaAJZt+P5rb3rKyuYPGb0Ixpheo0VflY0GevrhFWs+DLAfFbDGBLR5SPOBssrke0+fBnA87c0DGu34OhJWEbC59FJMqj/arVf0q9wb59YHhr/w2Z8d/wEAlAIAlER0UC5wyZhguZSFV17Z9pSb//lPn6iAlVe9CDUAGOCfVX2kYj5jnj8v7PM4XSzBQ+u74OBEmmrBUu3IpBzuhFENCNKGJWZYmhD0dANTAaUdS6meWYqL/YgH3KGnb+bxN316/8fnAI4CwLQCUKALM8H+jugzPgo27WbLv/71s15w/Vsv/7Vtw/kUY9E8IjWonDwQd8NqUsDrlQLGVS5CeUoIWo1wIoCd6/aWv7N+dENLPmW3t6SzK5dlWpY3pZqWtdjNzYVUcyFn5bJpCzdJ51SNseiIks5Gn59uNiBpNP42j9wfnvCZkjP16f/o/cw9m8a2A8BxjJt8CugHUF0NuSp02eoGX/Lwbj4fteSPwfxryz1ffdU7rrh8OTY/5iplx+KdkVRfoNGSP3BYALzNG4REMhVlqqvscvIj990TV8V1rttXuvbzXV/D660uLt6iqZPb0kueWWh79fPaL376BdmV5y3PXHxee/pprYXUhSkbbBvHyWCa0LIs23L4xqh+SONBrS7qbKky/dHv9n577ePjW3HjeQXgsAGgXwX98NUVvASe+7yvw3weiIS0/PxzL3v1q1943k/KZefc5AD63kotqpcU2oTvaHq6BJ0dXbPXfq7rXxV0s7jzqYIPAcR/+DOaPoQSv9cX/gVPzxeed2lT01NW5lqWNUGutSnVvKI9s7Kt2T6/JZ+6sCVvXZLP2RemU9YFtmVlkhyW4ziV3Yenu751e/9v/7BtYreCDzehRgjHfCYYIZNGBBnHUde8X5UbneRN/JHugzjkn3Nu07I7v/uaH6Rt65qK42R4Nrm7KKlaAcOOTtEVa20D7hD7mODXnC7OQed6AvAzCjCBTODD26IBovwdI1H8h0BKRGq2P5lHRF36n7zugqe87NnNly1rsS5ozttPy2ftZ6Rt63zLhgIbSssZHJ87eO8jIw/971v6d5UZejS5CN24+h5f3+x88UO34G6XOHbmearjnnZBf0/9+cvPaX7XNU+77OqXnv8Vy7Feh226MqI3OYA+05ronYbcKdFjMQ1Tgs71+xHAfzAiTFE8UUJRRbwV04zfI4D4s0AhIJppEXlTAgbCiN+T+7gcIP28y1uy5SawilMTztb9BLvp15lm1A/XKYctiIqEp3ZBQM33wZisznzmA89Y+c5Vl7w7lbavas6n3tx/6EQTByHiA8736aOmpkY9Z/gpQxO8cf3+mdd/ruujPgX0QyfKZ/5eFNDMy/lbpUy/LKhBVA7OPEjThM73ZJ2yx53OAJpvGv2dTFsb5O/+wqtWv+hZbTe25FNXgmXlMTis79lJarqrXxUVcOODBxDAv/cBaJpYP3yieqKICB3eH82h3EpOTm7ZL6nuUK7vqfgjPNuZAqCcCoQtvXIlZL/xty+85HnPaH/RxSvz72xvzfyZbVlNp+Z8JY2g2QQrAD9oRJdiWkXhBEA/ePKzXwH9XSl/dD/t1JxXftYzDUDzXKCJxmpK9rPvv+yCD77p6e85f0X2bZm0/XTbts45lSct7LkxDaMA/ICvyC8QisqZEJpgRvl//srEKUmL/LHP25kMYBWMb3hlW/PrXnThyje/8vzXX35x8425TOryP+YJJR/wwW40we8NADBIAf3w1er/iSn+Y77Nur7WUgFQTgpFg0oZc+u+9oorn3NZyzWtTZnVTbnUK8ACVM1T9kV5wPXdk9d+vuv9BoAClWliJSltRr5BdVnxAcMqEosSudbzBC41AP3nBk00memPv+PSiz71vmd+akV77p2W5eQtCyfx1PdrYnwG1nV097ztywcwCpb0hwQgZiDiT7uY8Al0SSLg+r6BRXi2pQ6gqYyc1nnvxef8l9dc+vynrixce0575j3ZlH1hvc77sd4RuH1tz+0f+o8jP/IBiDCJApq3ZupFADXzdv41uqdsSFC9zkGtz3O2AGieF3zPBGN7O+Q7v3n1my+7qHlNLmNfYdv2hViHrfUk4v1xN8zHNx8ufeW2w//4q4dH90WYYFE7UwVN+MwEdBCAZ7zf578Y8znfS+UxCGP6oosg8+G3XHbua6686LlXXNr84bbm9OsBktVa5UTgZKzv3Lzn3n++9fgPy2Uqe0mNV+AKU0C/+fU3iAbl/pbK+T+j0zD1vgi0KhKV8ebPXXnpq56z/HXLWjPXNOdTV9u2dW7Ui50cKs587ad71/37Lb04LwSL/FNGIlkA80e88ntJvUTl/5ZEyiXoHJ6NJjgJuKSM+O/trzq/9Ws3Pvd9F60sfCiTts8DcDKOg81QjlOuOOUte0cO/9nHNt85Ojl3BACOAQC2OCFUqFx+f09atPyBSVTAsaRMrv/kNwCMx5FgvO4ly5v+5oY/ueTi8woXWY5lHx0qpn+x9tjU9+84jKBJw4Hf9EpZzbyttes4/gjP4Hs0AKzt4kmeUaJRCWjMerR87zeb/trtKe2zq+1tLd69GwAu3rlvvPIZXgtuXMAlcAYaCrgELuKZ/BYaAJ7JV28JHPv/B59XwiffDWcOAAAAAElFTkSuQmCC";

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
                if (fileContent.IndexOf(base64) > -1)
                    fileContent = fileContent.Substring(fileContent.IndexOf(base64) + base64.Length);

                var bytes = Convert.FromBase64String(fileContent);

                if (Request.mode.IndexOf("thumb") > -1)
                    File.WriteAllBytes(GetFullThumbFilePath(), bytes);
                else
                    File.WriteAllBytes(GetFullUploadFilePath(), bytes);

                return "true";
            }
            catch
            {
                return "true";
            }
        }

        private string GetResourceStreamString(string fileName)
        {
            var assembly = typeof(FileContext).GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(string.Format("RN.FileManager.NetCore.data.{0}", fileName)))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private byte[] GetResourceStreamBytes(string fileName)
        {
            var assembly = typeof(FileContext).GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(string.Format("RN.FileManager.NetCore.data.{0}", fileName)))
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
