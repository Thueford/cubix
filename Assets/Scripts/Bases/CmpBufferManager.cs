using System.Collections.Generic;
using UnityEngine;

public class CmpBufferManager
{
    private static int MAXAGE = 5;
    private static Dictionary<int, CmpBufferManager> managers = 
        new Dictionary<int, CmpBufferManager>();

    private int size;
    private Queue<ComputeBuffer> buffers;
    private Queue<int> times;

    // Buffer props
    private int count, stride;
    ComputeBufferType type;

    private CmpBufferManager() { }

    private CmpBufferManager(int count, int stride, ComputeBufferType type, int size)
    {
        this.count = count;
        this.stride = stride;
        this.size = size;
        this.type = type;
        buffers = new Queue<ComputeBuffer>();
        times = new Queue<int>();
    }

    private void CheckBuffers()
    {
        while (buffers.Count > 0 && Time.time - times.Peek() > MAXAGE)
        {
            buffers.Dequeue().Release();
            times.Dequeue();
        }
    }

    ~CmpBufferManager()
    {
        while (buffers.Count > 0)
        {
            buffers.Dequeue().Release();
            times.Dequeue();
        }
    }

    public static void ReleaseManagers()
    {
        managers.Clear();
    }

    public static CmpBufferManager getManager(int count, int stride, ComputeBufferType type = 0, int size = 10)
    {
        int key = count.GetHashCode() ^ stride.GetHashCode();
        if (!managers.ContainsKey(key))
            managers[key] = new CmpBufferManager(count, stride, type, size);
        return managers[key];
    }

    public ComputeBuffer getBuffer()
    {
        if (buffers.Count == 0) return new ComputeBuffer(count, stride, type);
        else {
            times.Dequeue();
            return buffers.Dequeue();
        }
    }

    public void releaseBuffer(ComputeBuffer b)
    {
        if (buffers.Count < size)
        {
            times.Enqueue((int)Time.time);
            buffers.Enqueue(b);
        }
        else b.Release();
        CheckBuffers();
    }
}
