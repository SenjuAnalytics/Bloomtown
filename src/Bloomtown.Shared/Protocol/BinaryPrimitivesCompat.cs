using System.Buffers.Binary;

namespace Bloomtown.Shared.Protocol;

/// <summary>
/// Polyfill for netstandard2.1: <see cref="BinaryPrimitives.WriteSingleLittleEndian"/> and
/// <see cref="BinaryPrimitives.ReadSingleLittleEndian"/> were only added to the BCL in .NET 6
/// (the integer overloads like WriteUInt32LittleEndian have always existed and don't need this).
///
/// On net8.0 this just forwards to the real BCL method. On netstandard2.1 it reimplements the
/// same little-endian byte layout using only APIs that have existed since .NET Framework 1.1,
/// so there's no risk of this hitting yet another missing-API wall.
/// </summary>
internal static class BinaryPrimitivesCompat
{
#if NET6_0_OR_GREATER
    public static void WriteSingleLittleEndian(Span<byte> destination, float value) =>
        BinaryPrimitives.WriteSingleLittleEndian(destination, value);

    public static float ReadSingleLittleEndian(ReadOnlySpan<byte> source) =>
        BinaryPrimitives.ReadSingleLittleEndian(source);
#else
    public static void WriteSingleLittleEndian(Span<byte> destination, float value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        for (var i = 0; i < 4; i++)
            destination[i] = bytes[i];
    }

    public static float ReadSingleLittleEndian(ReadOnlySpan<byte> source)
    {
        var bytes = new byte[4];
        for (var i = 0; i < 4; i++)
            bytes[i] = source[i];

        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return BitConverter.ToSingle(bytes, 0);
    }
#endif
}
