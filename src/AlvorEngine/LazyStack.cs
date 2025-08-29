namespace AlvorEngine;

public struct LazyStack<T>
{
    private Stack<T> stack;

    public readonly int Count => stack == null ? 0 : stack.Count;

    public T Peek()
    {
        EnsureNotNull();
        return stack.Peek();
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        if (stack == null)
        {
            result = default;
            return false;
        }

        return stack.TryPeek(out result);
    }

    public T Pop()
    {
        EnsureNotNull();
        return stack.Pop();
    }

    public readonly bool TryPop([MaybeNullWhen(false)] out T result)
    {
        if (stack == null)
        {
            result = default;
            return false;
        }

        return stack.TryPop(out result);
    }

    public void Push(T item)
    {
        EnsureNotNull();
        stack.Push(item);
    }

    private void EnsureNotNull() => stack ??= [];
}
