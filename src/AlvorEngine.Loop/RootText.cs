namespace AlvorEngine.Loop;

[Root]
public class RootText
{
    static RootText()
    {
        Fmt((ReadOnlyMemory<char> val, Span<char> dst, out int w, ReadOnlySpan<char> fmt) =>
        {
            val.Span.CopyTo(dst);
            w = val.Length;
            return true;
        });

        Fmt((Vector2 val, Span<char> dst, out int w, ReadOnlySpan<char> fmt) =>
            Vector((val.X, val.Y, 0, 0), 2, dst, out w, fmt));

        Fmt((Vector2i val, Span<char> dst, out int w, ReadOnlySpan<char> fmt) =>
            Vector((val.X, val.Y, 0, 0), 2, dst, out w, fmt));

        Fmt((Vector3 val, Span<char> dst, out int w, ReadOnlySpan<char> fmt) =>
            Vector((val.X, val.Y, val.Z, 0), 3, dst, out w, fmt));

        Fmt((Vector3i val, Span<char> dst, out int w, ReadOnlySpan<char> fmt) =>
            Vector((val.X, val.Y, val.Z, 0), 3, dst, out w, fmt));

        Fmt((Vector4 val, Span<char> dst, out int w, ReadOnlySpan<char> fmt) =>
            Vector((val.X, val.Y, val.Z, val.W), 4, dst, out w, fmt));

        Fmt((Vector4i val, Span<char> dst, out int w, ReadOnlySpan<char> fmt) =>
            Vector((val.X, val.Y, val.Z, val.W), 4, dst, out w, fmt));

        static bool Vector<T>((T, T, T, T) val, int count, Span<char> dst,
            out int w, ReadOnlySpan<char> fmt) where T : ISpanFormattable
        {
            w = 0;
            dst[w++] = '(';

            if (count > 0)
            {
                if (!val.Item1.TryFormat(dst[w..], out int wx, fmt, null)) return false;
                w += wx;
            }

            if (count > 1)
            {
                dst[w++] = ',';
                dst[w++] = ' ';

                if (!val.Item2.TryFormat(dst[w..], out int wy, fmt, null)) return false;
                w += wy;
            }

            if (count > 2)
            {
                dst[w++] = ',';
                dst[w++] = ' ';

                if (!val.Item3.TryFormat(dst[w..], out int wz, fmt, null)) return false;
                w += wz;
            }

            if (count > 3)
            {
                dst[w++] = ',';
                dst[w++] = ' ';

                if (!val.Item4.TryFormat(dst[w..], out int ww, fmt, null)) return false;
                w += ww;
            }

            dst[w++] = ')';

            return true;
        }

        static void Fmt<T>(Utf16ValueStringBuilder.TryFormat<T> formatMethod) =>
            Utf16ValueStringBuilder.RegisterTryFormat(formatMethod);
    }

    private Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();

    public ReadOnlySpan<char> Format<T1>(string format, T1 arg1)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1);
        return sb.AsSpan()[start..];
    }

    public ReadOnlySpan<char> Format<T1, T2>(string format, T1 arg1, T2 arg2)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1, arg2);
        return sb.AsSpan()[start..];
    }

    public ReadOnlySpan<char> Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3);
        return sb.AsSpan()[start..];
    }

    public ReadOnlySpan<char> Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4);
        return sb.AsSpan()[start..];
    }

    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5);
        return sb.AsSpan()[start..];
    }

    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6>(
        string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6);
        return sb.AsSpan()[start..];
    }

    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6, T7>(
        string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        return sb.AsSpan()[start..];
    }

    public ReadOnlySpan<char> Format<T1, T2, T3, T4, T5, T6, T7, T8>(
        string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        int start = sb.Length;
        sb.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        return sb.AsSpan()[start..];
    }

    internal void Clear() => sb.Clear();
}
