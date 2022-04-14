using System;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;

namespace DirectDimensional.Runtime {
    internal static class RuntimeContext {
        private static readonly ComArray<RenderTargetView> _renderingOutput;

        public static RenderTargetView? RenderingOutput {
            get => _renderingOutput[0];
            set => _renderingOutput[0] = value;
        }

        static RuntimeContext() {
            _renderingOutput = new(1);
        }

        public static ComArray<RenderTargetView> RenderOutputArr => _renderingOutput;
    }
}
