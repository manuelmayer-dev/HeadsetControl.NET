using System.Collections;
using System.Runtime.InteropServices;
using HeadsetControl.NET.Native;

namespace HeadsetControl.NET;

/// <summary>
/// A snapshot of currently-discovered headsets that owns the underlying native
/// array of handles. Dispose this collection (preferably via <c>using</c>) to
/// release the native allocation; the contained <see cref="Headset"/>
/// instances become unusable afterwards.
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

        for (int i = 0; i < count; i++)
        {
            IntPtr handle = Marshal.ReadIntPtr(nativeArray, i * IntPtr.Size);
            _headsets[i] = new Headset(this, handle);
        }
    }

    /// <inheritdoc />
    public int Count => _headsets.Length;

    /// <inheritdoc />
    public Headset this[int index] => _headsets[index];

    /// <summary>Whether the underlying native array has been released.</summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public IEnumerator<Headset> GetEnumerator()
    {
        return ((IEnumerable<Headset>)_headsets).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        if (_nativeArray != IntPtr.Zero)
        {
            NativeMethods.FreeHeadsets(_nativeArray, _count);
            _nativeArray = IntPtr.Zero;
            _count = 0;
        }
    }
}
