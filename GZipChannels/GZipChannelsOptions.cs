using System.Collections.Generic;
using CommandLine;

namespace GZipChannels
{
   internal  class GZipChannelsOptions
    {
        [Option('m', "mode", Required = true, HelpText = "Select mode. (Compress/Decompress)")]
        public string Mode { get; set; }

        [Option('s', "source", Required = true, HelpText = "Source file path.")]
        public string SourcePath { get; set; }

        [Option('t', "target", Required = true, HelpText = "Target file path.")]
        public string TargetPath { get; set; }
    }
}
