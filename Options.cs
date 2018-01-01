using CommandLine;
using System.Collections.Generic;

namespace PdfImageExtractor
{
    public class Options
    {
        [Option('p', "path", Required = true, HelpText = "PDF path")]
        public string Path { get; set; }

        [Option(longName: "page", HelpText = "PDF page number", DefaultValue = 0)]
        public int Page { get; set; }

        [Option(longName: "save-path", HelpText = "Path to save the extracted images")]
        public string SavePath { get; set; }

        [Option('f', longName: "filename-format", HelpText = "Filename format. e.g. Page_{page}_Image_{image}")]
        public string FilenameFormat { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Verbose")]
        public bool Verbose { get; set; }
    }
}
