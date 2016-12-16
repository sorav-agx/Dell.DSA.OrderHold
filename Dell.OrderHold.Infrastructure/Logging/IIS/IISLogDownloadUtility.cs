using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.IIS
{
    public class IISLogDownloadUtility
    {
        public List<CopyFileRecord> ScanAndCopyFiles(string searchDirectory, string createAtDirectory, params string[] logFileNamesToExclude)
        {
            if (string.IsNullOrWhiteSpace(searchDirectory))
                throw new ArgumentNullException("searchDirectory");
            if (string.IsNullOrWhiteSpace(createAtDirectory))
                throw new ArgumentNullException("createAtDirectory");

            if (!Directory.Exists(searchDirectory))
                throw new ArgumentException("searchDirectory does not exist.");
            if (!Directory.Exists(createAtDirectory))
                throw new ArgumentException("createAtDirectory does not exist.");

            searchDirectory = searchDirectory.Replace(@"\", @"/").TrimEnd(new char[] { '/' });
            createAtDirectory = createAtDirectory.Replace(@"\", @"/").TrimEnd(new char[] { '/' });
            string tempZipDirectory = createAtDirectory + "/" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(tempZipDirectory);

            List<CopyFileRecord> fileNames = new List<CopyFileRecord>();

            DirectoryInfo dinfo = new DirectoryInfo(searchDirectory);

            foreach (var file in dinfo.GetFiles("*.zip"))
            {
                ZipFile.ExtractToDirectory(file.FullName, tempZipDirectory);
            }

            dinfo = new DirectoryInfo(tempZipDirectory);
            foreach (var file in dinfo.GetFiles("*.log"))
            {
                if (logFileNamesToExclude == null || !logFileNamesToExclude.Any(d => d.ToLower().Equals(file.Name.ToLower().Replace(file.Extension.ToLower(), ""))))
                {
                    string fileName = createAtDirectory + "/" + file.Name;
                    File.Copy(file.FullName, fileName, true);
                    fileNames.Add(new CopyFileRecord()
                    {
                        FromLocation = searchDirectory + "/" + file.Name,
                        ToLocation = fileName
                    });
                }
            }

            Directory.Delete(tempZipDirectory, true);
            dinfo = new DirectoryInfo(searchDirectory);
            foreach (var file in dinfo.GetFiles("*.log"))
            {
                if (logFileNamesToExclude == null || !logFileNamesToExclude.Any(d => d.ToLower().Equals(file.Name.ToLower().Replace(file.Extension.ToLower(), ""))))
                {
                    string fileName = createAtDirectory + "/" + file.Name;
                    File.Copy(file.FullName, fileName, true);
                    fileNames.Add(new CopyFileRecord()
                    {
                        FromLocation = searchDirectory + "/" + file.Name,
                        ToLocation = fileName
                    });
                }
            }

            return fileNames;
        }
    }
}
