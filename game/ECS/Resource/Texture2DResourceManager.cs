﻿using System.Collections.Generic;
using DefaultEcs;
using DefaultEcs.Resource;
using game.ECS.Components;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Resource
{
    public class Texture2DResourceManager : AResourceManager<string, DisposableDummy<Texture2D>>
    {
        private readonly ContentManager contentManager;
        
        public Texture2DResourceManager(ContentManager contentManager)
        {
            this.contentManager = contentManager;
        }

        protected override DisposableDummy<Texture2D> Load(string info)
        {
            return new DisposableDummy<Texture2D>(contentManager.Load<Texture2D>(info));
        }

        protected override void OnResourceLoaded(in Entity entity, string info, DisposableDummy<Texture2D> resource)
        {
            ref var texture2DList = ref entity.Get<Texture2DResources>();
            if (texture2DList.textures == null)
                texture2DList.textures = new Dictionary<string, Texture2D>();
            
            texture2DList.textures[info] = resource.Data;
        }
    }
}