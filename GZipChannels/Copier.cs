using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GZipChannels
{
    internal class Copier
    {
        private readonly string _source;
        private readonly string _target;
        private readonly Channel<byte[]> _channel;
        private const int BlockSize = 10 * 1024 * 1024;
        private const int ChannelSize = 100;

        public Copier(string source, string target)
        {
            _source = source;
            _target = target;
            _channel = Channel.CreateBounded<byte[]>(ChannelSize);
        }

        public async Task Run()
        {
            var fileWriter = Task.Run(WriteTargetFile);

            await ReadFileToChannel();

            await fileWriter;
        }

        private async Task ReadFileToChannel()
        {
            await using var source = new FileStream(_source, FileMode.Open, FileAccess.Read);

            var bytes = new byte[BlockSize];
            int bytesRead;

            do
            {
                bytesRead = await source.ReadAsync(bytes.AsMemory());

                if (bytesRead == 0)
                {
                    break;
                }

                if (bytesRead == BlockSize)
                {
                    await _channel.Writer.WriteAsync(bytes);
                }

                if (bytesRead < BlockSize)
                {
                    var arr = new byte[bytesRead];
                    Buffer.BlockCopy(bytes, 0, arr, 0, bytesRead);

                    await _channel.Writer.WriteAsync(arr);
                }
            } while (bytesRead > 0);

            _channel.Writer.Complete();
        }

        private async Task WriteTargetFile()
        {
            await using var target = new FileStream(_target, FileMode.Create, FileAccess.Write);

            await foreach (var bytes in _channel.Reader.ReadAllAsync())
            {
                await target.WriteAsync(bytes);
            }
        }
    }
}