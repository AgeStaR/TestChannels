using System;
using CommandLine;

namespace GZipChannels
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var exitCode = 0;

            try
            {

                Parser.Default.ParseArguments<GZipChannelsOptions>(args)
                    .WithParsed<GZipChannelsOptions>(o =>
                    {

                    }).WithNotParsed(errors =>
                    {
                        exitCode = 1;
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                exitCode = 1;
            }

            return exitCode;
        }
    }
}
