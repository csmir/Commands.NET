using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Commands;

/// <summary>
/// A cheaper single-use alternative to <see cref="System.Text.StringBuilder"/>, where the intitial backing store is a <see cref="Span{T}"/> of type <see cref="char"/>.<br/>
/// The stack-allocated buffer will be used until it overflows, at which point a larger array will be rented from <see cref="ArrayPool{T}.Shared"/> to minimize allocation.<br/>
/// When <see cref="ToString"/> is called, the builder will return the rented array to the pool, destroying the builder automatically.
/// </summary>
internal ref struct ValueStringBuilder(Span<char> initialBuffer)
{
    private Span<char> _span = initialBuffer;
    private int _position;
    private char[]? _rentedArray;

    /// <summary>
    /// Appends a single character to the builder.
    /// </summary>
    /// <param name="c">The character to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        if (_position >= _span.Length)
            Grow(1);

        // Skip bounds check, Grow(1) will always succeed.
        Unsafe.Add(ref MemoryMarshal.GetReference(_span), _position++) = c;
    }

    /// <summary>
    /// Appends a string to the builder.
    /// </summary>
    /// <param name="s">The string to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string s) => Append(s.AsSpan());

    /// <summary>
    /// Appends a span to the builder.
    /// </summary>
    /// <param name="s">The span to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> s)
    {
        int length = s.Length;

        if (_position + length > _span.Length)
            Grow(length);

        s.CopyTo(_span.Slice(_position));

        _position += length;
    }

    /// <summary>
    /// Grows the internal buffer to accommodate additional characters.
    /// </summary>
    /// <param name="requested">The minimum number of additional characters required.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int requested)
    {
        int newCapacity = _span.Length * 2;

        if (newCapacity < _position + requested)
            newCapacity = _position + requested;

        char[] newArray = ArrayPool<char>.Shared.Rent(newCapacity);

        _span.CopyTo(newArray.AsSpan(0, _position));

        if (_rentedArray != null)
            ArrayPool<char>.Shared.Return(_rentedArray);

        _span = _rentedArray = newArray;
    }

    /// <summary>
    /// Converts the current contents of the builder to a string, and destroys the builder by returning the rented array to the pool.<br/>
    /// Only call at the end of the builder's lifetime, as it will no longer be usable after this call.<br/>
    /// </summary>
    /// <returns>The string representation of the builder's contents.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString()
    {
        string s = _span.Slice(0, _span.IndexOf('\0')).ToString();

        if (_rentedArray != null)
            ArrayPool<char>.Shared.Return(_rentedArray);

        return s;
    }
}
