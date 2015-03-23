using System;

// ReSharper disable once CheckNamespace
namespace SharpDX.Toolkit.Graphics
{
    public static class BlockExtensions
    {
        public static IDisposable Block(this SpriteBatch spriteBatch,
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null)
        {
            return new SpriteBatchBlock(spriteBatch, sortMode, blendState,
                samplerState, depthStencilState, rasterizerState, effect);
        }

        private struct SpriteBatchBlock : IDisposable
        {
            private readonly SpriteBatch _spriteBatch;

            public SpriteBatchBlock(SpriteBatch spriteBatch, SpriteSortMode sortMode,
                BlendState blendState, SamplerState samplerState,
                DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
            {
                if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
                _spriteBatch = spriteBatch;
                _spriteBatch.Begin(sortMode, blendState, samplerState, 
                    depthStencilState, rasterizerState, effect);
            }

            public void Dispose()
            {
                _spriteBatch.End();
            }
        }
    }
}