using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class Buffer
{
    private Stack<int> stack;
    private int capacity;
    private object lockObject = new object();

    public Buffer(int capacity)
    {
        this.capacity = capacity;
        this.stack = new Stack<int>(capacity);
    }

    // Положить данные в буфер
    public bool Put(int data, int threadId)
    {
        Monitor.Enter(lockObject);
        try
        {
            // Критическая секция
            if (stack.Count >= capacity)
            {
                return false; // Буфер полон
            }

            stack.Push(data);
            return true;
        }
        finally
        {
            Monitor.Exit(lockObject);
        }
    }

    // Взять данные из буфера
    public bool Take(out int data, int threadId)
    {
        Monitor.Enter(lockObject);
        try
        {
            // Критическая секция
            data = -1;
            if (stack.Count == 0)
            {
                return false; // Буфер пуст
            }

            data = stack.Pop();
            return true;
        }
        finally
        {
            Monitor.Exit(lockObject);
        }
    }

    // Получить текущее состояние буфера
    public string GetBufferState()
    {
        Monitor.Enter(lockObject);
        try
        {
            if (stack.Count == 0)
                return "Буфер пуст";

            var items = stack.ToArray();
            Array.Reverse(items); // Для отображения в порядке добавления
            return $"Стек: [{string.Join(", ", items)}] (Размер: {stack.Count}/{capacity})";
        }
        finally
        {
            Monitor.Exit(lockObject);
        }
    }

    public int Count
    {
        get
        {
            Monitor.Enter(lockObject);
            try
            {
                return stack.Count;
            }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }
    }

    public int Capacity
    {
        get { return capacity; }
    }
}