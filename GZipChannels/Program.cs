using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommandLine;

namespace GZipChannels
{

    internal class Archiver
    {
        private readonly string _source;
        private readonly string _target;
        private readonly Channel<byte[]> _channel;
        private static int BlockSize = 10 * 1024 * 1024;
        private static int ChannelSize = 100;

        public Archiver(string source, string target)
        {
            _source = source;
            _target = target;
            _channel = Channel.CreateBounded<byte[]>(ChannelSize);
        }

        public async Task Run()
        {
            var writer = Task.Run(Writer);

            await using var source = new FileStream(_source, FileMode.Open, FileAccess.Read);

            var bytes = new byte[BlockSize];
            int readed;

            do
            {
                readed = await source.ReadAsync(bytes, 0, BlockSize);

                if (readed == 0)
                {
                    break;
                }

                if (readed == BlockSize)
                {
                    await _channel.Writer.WriteAsync(bytes);
                }

                if (readed < BlockSize)
                {
                    var arr = new byte[readed];
                    Buffer.BlockCopy(bytes, 0, arr, 0, readed);

                    await _channel.Writer.WriteAsync(arr);
                }

            } while (readed > 0);

            _channel.Writer.Complete();

            await writer;
        }

        private async Task Writer()
        {
            using var target = new FileStream(_target, FileMode.Create, FileAccess.Write);

            await foreach (var bytes in _channel.Reader.ReadAllAsync())
            {
                await target.WriteAsync(bytes);
            }
        }
    }

    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var exitCode = 0;

            try
            {
                var result = Parser.Default.ParseArguments<GZipChannelsOptions>(args);

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

        private static async Task WithParsedAsync(GZipChannelsOptions arg)
        {
            var a = new Archiver(arg.SourcePath, arg.TargetPath);
            await a.Run();
        }
    }
}
