namespace Github_Scraper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json;

    public class Program
    {
        public static void Main(string[] args)
        {
            var lines = File.ReadAllLines(@"C:\Users\Ewout van der Wal\Documents\ANTLR Extractor\Github Scraper\git-urls-normalized.txt");
            var item = 357;
            lines = lines.Skip(item).ToArray();
            var fileNames = new List<string>();

            foreach (var line in lines)
            {
                var contents = Get(line).Content;
                string name;

                try
                {
                    var nameLine = contents.Split("\n").Select(l => l.Trim(' ')).First(l => l.StartsWith("parser") || l.StartsWith("lexer") || l.StartsWith("grammar")).Split(';').First();
                    name = nameLine.Trim(' ').Split(" ").Last().Trim('\r').Trim(';');
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"Getting {item + 1} ({item}): {e.GetType()}");
                    item++;
                    continue;
                }

                Console.Write($"Getting {item + 1} ({item}): {name}.g4 ({line})");

                if (fileNames.Contains(name))
                {
                    Console.WriteLine(" --- Existing, skipped");
                    item++;
                    continue;
                }

                fileNames.Add(name);
                File.WriteAllText($"C:\\Users\\Ewout van der Wal\\Documents\\ANTLR Extractor\\Github Scraper\\Github Grammars\\{name}.g4", contents, new UTF8Encoding(false));
                Console.WriteLine(" --- Done");
                item++;
            }
        }

        private static GithubObject Get(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers[HttpRequestHeader.Authorization] = "token 1a8cee937d5b64222dbe5c66d21d6784e4d6d1aa";
            request.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:83.0) Gecko/20100101 Firefox/83.0";
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using var response = (HttpWebResponse)request.GetResponse();
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream ?? throw new InvalidOperationException());
            var serializer = JsonSerializer.Create();
            return serializer.Deserialize<GithubObject>(new JsonTextReader(reader));
        }
    }

    public class GithubObject
    {
        private string _content;

        public string Content
        {
            get => this._content;
            set => this._content = Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }
}
