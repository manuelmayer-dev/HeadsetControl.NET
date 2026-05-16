using System.Collections;
using System.Runtime.InteropServices;
using HeadsetControl.NET.Native;

namespace HeadsetControl.NET;

/// <summary>
/// Snapshot of the connected headsets. Owns the underlying native handle
/// array — dispose to release it.
/// </summary>
public sealed class HeadsetCollection : IReadOnlyList<Headset>, IDisposable
{
    private readonly Headset[] _headsets;
    private IntPtr _nativeArray;
    private int _count;

    internal HeadsetCollection(IntPtr nativeArray, int count)
    {
        _nativeArray = nativeArray;
        _count = count;
        _headsets = new Headset[count];

        for (var i = 0; i < count; i++)
        {
            var handle = Marshal.ReadIntPtr(nativeArray, i * IntPtr.Size);
            _headsets[i] = new Headset(this, handle);
        }
    }

    public int Count => _headsets.Length;

    public Headset this[int index] => _headsets[index];

    public bool IsDisposed { get; private set; }

    public IEnumerator<Headset> GetEnumerator() => ((IEnumerable<Headset>)_headsets).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        if (_nativeArray == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.FreeHeadsets(_nativeArray, _count);
        _nativeArray = IntPtr.Zero;
        _count = 0;
    }
}
