using System;
using System.Threading.Tasks;
using CommandLine;

namespace TestChannels
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var exitCode = 0;

            try
            {
                var result = Parser.Default.ParseArguments<ChannelsOptions>(args);

                var parsed = result.WithParsedAsync(WithParsedAsync);
                var failed = result.WithNotParsedAsync(errors =>
                {
                    exitCode = 1;

                    return Task.CompletedTask;
                });

                await Task.WhenAll(parsed, failed);

                return exitCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                exitCode = 1;
            }

            return exitCode;
        }

        private static async Task WithParsedAsync(ChannelsOptions arg)
        {
            var a = new Copier(arg.SourcePath, arg.TargetPath);
            await a.Run();
        }
    }
}
