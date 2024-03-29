﻿using System;
using Microsoft.Xna.Framework.Content;

namespace WickedCrush.Utility
{
    public class SynchronizedContentManager : ContentManager
    {
        private object syncRoot = new object();

        public SynchronizedContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override T Load<T>(string assetName)
        {
            lock(syncRoot)
            {
                return base.Load<T>(assetName);
            }
        }
    }
}
