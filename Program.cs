using System;
using System.IO;

namespace PdfImageExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var options = new Options();
                var isValid = CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

                if (isValid)
                {
                    var service = new ImageExtractor();

                    if (!string.IsNullOrEmpty(options.FilenameFormat))
                        service.FilenameFormat = options.FilenameFormat;

                    var images = service.ExtractImages(options.Path, options.Page);

                    if (images.Count > 0)
                    {
                        string savePath = options.SavePath;
                        if (string.IsNullOrEmpty(savePath))
                        {
                            savePath = new FileInfo(options.Path).DirectoryName;
                        }

                        if (!Directory.Exists(savePath))
                            Directory.CreateDirectory(savePath);

                        foreach (var name in images.Keys)
                        {
                            images[name].Save(Path.Combine(savePath, name));
                        }

                        if (options.Verbose)
                        {
                            Console.WriteLine("Images successfully extracted");
                        }
                    }
                    else
                    {
                        if (options.Verbose)
                        {
                            Console.WriteLine("No images found");
                        }
                    }
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }


        }
    }
}
