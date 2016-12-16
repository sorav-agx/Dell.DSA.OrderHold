using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging.IIS
{
    public class IISLogParser
    {
        private readonly string _rawText;
        private readonly Uri _fileLocation;
        private List<ILogParseExaminer> _cancelledExaminers = new List<ILogParseExaminer>();

        public IISLogParser(string rawLogText)
        {
            if (string.IsNullOrWhiteSpace(rawLogText))
                throw new ArgumentNullException("rawLogText");

            this._rawText = rawLogText;
        }
        public IISLogParser(Uri fileLocation)
        {
            if (fileLocation == null)
                throw new ArgumentNullException("fileLocation");

            _fileLocation = fileLocation;
        }

        public List<IISLog> GetFormattedIISLogs()
        {
            List<string> fieldsToParse = new List<string>();
            List<RawIISLog> logs = new List<RawIISLog>();

            if (!string.IsNullOrWhiteSpace(_rawText))
            {
                foreach (var str in _rawText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (str.StartsWith("#Fields"))
                    {
                        fieldsToParse.Clear();
                        int x = 0;
                        foreach (var field in str.Split(new char[] { ' ' }))
                        {
                            if (x == 0)
                            {
                                x += 1;
                                continue;
                            }
                            fieldsToParse.Add(field);
                            x++;
                        }
                    }
                    else
                    {
                        if (str.StartsWith("#"))
                            continue;
                        var item = ParseRow(fieldsToParse, str);
                        logs.Add(item);
                    }
                }
            }
            else if (_fileLocation != null)
            {
                foreach (var fileLocation in Directory.GetFiles(_fileLocation.AbsolutePath, "*.log"))
                {
                    fieldsToParse.Clear();
                    var file = File.ReadAllText(fileLocation);
                    foreach (var str in file.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (str.StartsWith("#Fields"))
                        {
                            fieldsToParse.Clear();
                            int x = 0;
                            foreach (var field in str.Split(new char[] { ' ' }))
                            {
                                if (x == 0)
                                {
                                    x += 1;
                                    continue;
                                }
                                fieldsToParse.Add(field);
                                x++;
                            }
                        }
                        else
                        {
                            if (str.StartsWith("#"))
                                continue;
                            var item = ParseRow(fieldsToParse, str);
                            logs.Add(item);
                        }
                    }
                }
            }
            else
                throw new Exception("There are no longs to parse.");

            return logs.Select(d => d.ToParsedLog()).ToList();
        }

        public void Parse(params ILogParseExaminer[] logParseExaminers)
        {
            if (logParseExaminers == null || logParseExaminers.Length == 0)
                throw new ArgumentNullException("logParseExaminer");
            _cancelledExaminers = new List<ILogParseExaminer>();

            List<string> fieldsToParse = new List<string>();
            if (!string.IsNullOrWhiteSpace(_rawText))
            {
                foreach (var str in _rawText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (str.StartsWith("#Fields"))
                    {
                        fieldsToParse.Clear();
                        int x = 0;
                        foreach (var field in str.Split(new char[] { ' ' }))
                        {
                            if (x == 0)
                            {
                                x += 1;
                                continue;
                            }
                            fieldsToParse.Add(field);
                            x++;
                        }
                    }
                    else
                    {
                        if (str.StartsWith("#"))
                            continue;
                        try
                        {
                            var item = ParseRow(fieldsToParse, str);
                            Parallel.ForEach(logParseExaminers, (d, state) =>
                            {
                                LogParseExecutedEventArgs args = new LogParseExecutedEventArgs(item);
                                d.LogParseExecuted(args);
                                if (args.IsParsingCanceled)
                                {
                                    _cancelledExaminers.Add(d);
                                    if (_cancelledExaminers.Count == logParseExaminers.Count())
                                        state.Break();
                                }
                            });
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            else if (_fileLocation != null)
            {
                foreach (var fileLocation in Directory.GetFiles(_fileLocation.AbsolutePath, "*.log"))
                {
                    fieldsToParse.Clear();
                    var file = File.ReadAllText(fileLocation);
                    foreach (var str in file.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (str.StartsWith("#Fields"))
                        {
                            fieldsToParse.Clear();
                            int x = 0;
                            foreach (var field in str.Split(new char[] { ' ' }))
                            {
                                if (x == 0)
                                {
                                    x += 1;
                                    continue;
                                }
                                fieldsToParse.Add(field);
                                x++;
                            }
                        }
                        else
                        {
                            if (str.StartsWith("#"))
                                continue;
                            try
                            {
                                var item = ParseRow(fieldsToParse, str);
                                Parallel.ForEach(logParseExaminers, (d, state) =>
                                {
                                    LogParseExecutedEventArgs args = new LogParseExecutedEventArgs(item);
                                    d.LogParseExecuted(args);
                                    if (args.IsParsingCanceled)
                                    {
                                        _cancelledExaminers.Add(d);
                                        if (_cancelledExaminers.Count == logParseExaminers.Count())
                                            state.Break();
                                    }
                                });
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            else
                throw new Exception("There are no longs to parse.");
        }

        private RawIISLog ParseRow(List<string> validFields, string row)
        {
            if (string.IsNullOrWhiteSpace(row))
                return null;

            RawIISLog raw = new RawIISLog();
            int i = 0;
            // date time s-ip cs-method cs-uri-stem cs-uri-query s-port cs-username c-ip cs(User-Agent) cs(Referer) sc-status sc-substatus sc-win32-status time-taken 
            foreach (var item in row.Split(new char[] { ' ' }, StringSplitOptions.None))
            {
                string key = "Unknown";
                if (validFields.Count >= i)
                    key = validFields[i];

                switch (key.ToLower())
                {
                    case "cs-uri-stem":
                        raw.BaseUri = item;
                        break;
                    case "c-ip":
                        raw.ClientIpAddress = item;
                        break;
                    case "s-ip":
                        raw.ServerIpAddress = item;
                        break;
                    case "date":
                        raw.Date = item;
                        break;
                    case "cs-method":
                        raw.HttpMethod = item;
                        break;
                    case "sc-status":
                        raw.HttpStatusCode = item;
                        break;
                    case "sc-substatus":
                        raw.HttpSubStatus = item;
                        break;
                    case "s-port":
                        raw.Port = item;
                        break;
                    case "cs(referer)":
                        raw.RefererHeader = item;
                        break;
                    case "time":
                        raw.Time = item;
                        break;
                    case "time-taken":
                        raw.TimeTaken = item;
                        break;
                    case "cs-uri-query":
                        raw.UriQuerystring = item;
                        break;
                    case "cs(user-agent)":
                        raw.UserAgentHeader = item;
                        break;
                    case "cs-username":
                        raw.UserAgentHeader = item;
                        break;
                    case "sc-win32-status":
                        raw.Win32Status = item;
                        break;
                    default:
                        raw.AdditionalValues.Add(key, item);
                        break;
                }
                i += 1;
            }

            return raw;
        }
    }
}
