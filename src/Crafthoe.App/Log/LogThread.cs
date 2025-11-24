namespace Crafthoe.App;

public class LogThread(Thread thread)
{
    private LogBuffer[] buffers = [new(), new()];
    private int bufferCount = 2;
    private int bufferIndex;

    public Thread Thread => thread;
    public ReadOnlySpan<LogBuffer> Buffers => new(buffers, 0, bufferCount);

    public void Add(LogEntry entry, ReadOnlySpan<char> chars)
    {
        var buffer = SelectBuffer(chars.Length);
        buffer.Write(entry, chars);
    }

    private LogBuffer SelectBuffer(int count)
    {
        if (buffers[bufferIndex].Capacity == 0 || buffers[bufferIndex].CharCapacity <= count)
            NextBuffer();

        return buffers[bufferIndex];
    }

    private void NextBuffer()
    {
        if (buffers.Length < 64)
        {
            int next = NextBufferIndex();
            if (next < 0)
            {
                int prevLength = buffers.Length;
                Array.Resize(ref buffers, buffers.Length * 2);
                for (int i = prevLength; i < buffers.Length; i++)
                    buffers[i] = new();

                bufferCount = buffers.Length;
                bufferIndex = prevLength;
            }
            else bufferIndex = next;
        }
        else
        {
            int next = NextBufferIndex();

            while (next < 0)
            {
                lock (this)
                {
                    System.Threading.Monitor.Wait(this);
                }

                next = NextBufferIndex();
            }

            bufferIndex = next;
        }

        buffers[bufferIndex].Clear();
    }

    private int NextBufferIndex()
    {
        int index = bufferIndex + 1;
        int count = buffers.Length - 1;

        while (count > 0)
        {
            int rindex = index % buffers.Length;
            var buffer = buffers[rindex];

            if (buffer.Synced)
                return rindex;

            index++;
            count--;
        }

        return -1;
    }
}
