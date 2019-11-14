using System;
using TiledSharp;

namespace game.ECS.Resource
{
    public class DisposableTmxMap : IDisposable
    {
        public TmxMap TmxMap { get; set; }

        public void Dispose()
        {
        }
    }
}