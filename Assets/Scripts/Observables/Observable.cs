using System;

public abstract class Observable<T>
{
    public event Action<T> ValueChanged;

    private T value = default;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            ValueChanged.Invoke(value);
        }
    }
}
