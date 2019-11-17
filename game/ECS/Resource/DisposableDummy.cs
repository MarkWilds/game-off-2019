using System;

namespace game.ECS.Resource
{
    public class DisposableDummy<T> : IDisposable
    {
        public T Data { get; set; }

        public DisposableDummy(T data)
        {
            Data = data;
        }

        public void Dispose()
        {
        }
    }
}