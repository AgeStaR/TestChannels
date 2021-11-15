using CommandLine;

namespace TestChannels
{
   internal  class ChannelsOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source file path.")]
        public string SourcePath { get; set; }

        [Option('t', "target", Required = true, HelpText = "Target file path.")]
        public string TargetPath { get; set; }
    }
}
