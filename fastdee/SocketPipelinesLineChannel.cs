using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fastdee
{
    /// <summary>
    /// Read and write a socket automagically.
    /// You create the socket and pass it to me, I will take care of forming the lines for you,
    /// as well as collecting the received data.
    /// </summary>
    class SocketPipelinesLineChannel : ILineChannel, IDisposable
    {
        readonly Socket lowlevel;
        readonly CancellationTokenSource farewell = new CancellationTokenSource();
        public event EventHandler<GottaLineArgs>? GottaLine;

        internal SocketPipelinesLineChannel(Socket lowlevel)
        {
            this.lowlevel = lowlevel;
            var pipe = new Pipe();
            Task = Task.WhenAll(SocketToPipeAsync(pipe.Writer), PipeToLinesAsync(pipe.Reader));
        }

        protected virtual void OnGottaLine(GottaLineArgs args) => GottaLine?.Invoke(this, args);

        public void Dispose()
        {
            farewell.Cancel();
        }

        /// <summary>
        /// Feeding the pipeline with data taken from socket.
        /// This code is basically from pipeline manual + the cancellation token.
        /// </summary>
        async Task SocketToPipeAsync(PipeWriter writer)
        {
            const int minimumBufferSize = 512;
            while (true)
            {
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    int bytesRead = await lowlevel.ReceiveAsync(memory, SocketFlags.None, farewell.Token);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    writer.Advance(bytesRead);
                }
                catch (Exception)
                {
                    // Todo: maybe today I'll decide to log something, for the time being I throw everything away.
                    break;
                }

                FlushResult result = await writer.FlushAsync(farewell.Token);
                if (result.IsCompleted)
                {
                    break;
                }
            }

            await writer.CompleteAsync();
        }

        /// <summary>
        /// Search for newlines in the data returned from the pipeline.
        /// Again, this is basically by the book.
        /// </summary>
        async Task PipeToLinesAsync(PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync(farewell.Token);
                ReadOnlySequence<byte> buffer = result.Buffer;
                while (TryReadLine(ref buffer, out var stringy))
                {
                    if (stringy.Length != 0) OnGottaLine(new GottaLineArgs(stringy));
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted) break;
            }
            await reader.CompleteAsync();
        }

        /// <summary>
        /// Not quite the same as the book as I mangle into a proper <see cref="string"/> directly but
        /// it's very close. Returned strings are trimmed, but they can be empty.
        /// </summary>
        static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out string gotcha)
        {
            SequencePosition? position = buffer.PositionOf((byte)'\n');
            if (position == null)
            {
                gotcha = string.Empty;
                return false;
            }
            var mangle = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            // Note: if you get a newline, it is a valid UTF8 'character'... 'codepoint'? 
            // Anyway, it must decode correctly at least there, if not, everything is garbled up big way so BOOM!
            gotcha = Encoding.UTF8.GetString(mangle).Trim();
            return true;
        }

        public async Task WriteAsync(string raw)
        {
            raw += '\n';
            var buffer = Encoding.UTF8.GetBytes(raw);
            var sent = 0;
            while (sent < buffer.Length)
            {
                var wrap = new ReadOnlyMemory<byte>(buffer, sent, buffer.Length - sent);
                sent += await lowlevel.SendAsync(wrap, SocketFlags.None); // most of the time it will go in a single call
            }
        }

        internal Task Task { get; }
    }
}
