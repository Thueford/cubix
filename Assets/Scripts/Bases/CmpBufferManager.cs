using System.Collections.Generic;
using UnityEngine;

public class CmpBufferManager
{
    private static int MAXAGE = 5;
    private static Dictionary<int, CmpBufferManager> managers = 
        new Dictionary<int, CmpBufferManager>();

    private int count, stride, size;
    ComputeBufferType type;

    private Queue<ComputeBuffer> buffers;
    private Queue<int> times;

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
        if (buffers.Count == 0) return;
        while (Time.time - times.Peek() > MAXAGE)
        {
            buffers.Dequeue().Release();
            times.Dequeue();
        }
    }

    ~CmpBufferManager()
    {
        // foreach (ComputeBuffer b in buffers) b.Release();
        while (buffers.Count > 0)
        {
            buffers.Dequeue().Release();
            times.Dequeue();
        }
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
        // Debug.Log((buffers.Count == 0 ? "Get" : "Pop") + "Buffer " + (type) + " " + count + " " + stride);
        if (buffers.Count == 0) return new ComputeBuffer(count, stride, type);
        else {
            times.Dequeue();
            return buffers.Dequeue();
        }
    }

    public void releaseBuffer(ComputeBuffer b)
    {
        // Debug.Log((buffers.Count < size ? "Push" : "Release") + "Buffer " + (type) + " " + count + " " + stride);
        if (buffers.Count >= size) Debug.Log("ReleaseBuffer " + (type) + " " + count + " " + stride);
        if (buffers.Count < size)
        {
            times.Enqueue((int)Time.time);
            buffers.Enqueue(b);
        }
        else b.Release();
        CheckBuffers();
    }

}
