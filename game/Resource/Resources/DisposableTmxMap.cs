using System;
using TiledSharp;

namespace game.Resource.Resources
{
    public class DisposableTmxMap : IDisposable
    {
        public TmxMap TmxMap { get; set; }

        public void Dispose()
        {
        }
    }
}