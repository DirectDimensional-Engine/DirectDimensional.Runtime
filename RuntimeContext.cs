using System;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings.DXGI;
using DirectDimensional.Core;

using D3D11Texture2D = DirectDimensional.Bindings.Direct3D11.Texture2D;

namespace DirectDimensional.Runtime {
    internal static unsafe class RuntimeContext {
        private static ComArray<RenderTargetView> _outputArr = null!;

        public static RenderTargetView? RenderingOutput {
            get => _outputArr[0];
            set => _outputArr[0] = value;
        }
        public static ComArray<RenderTargetView> RenderingOutputAsArray => _outputArr;

        public static void Initialize() {
            _outputArr = new(1);
        }

        public static void Shutdown() {
            _outputArr.TrueDispose();
        }

        public static HRESULT GenerateDefaultRenderingOutput(SwapChain swapChain) {
            if (RenderingOutput.Alive()) {
                Logger.Warn("Cannot generate default rendering output in runtime context as one output is already exists");
                return HRESULTCodes.S_FALSE;
            }

            HRESULT hr = swapChain.GetBuffer<D3D11Texture2D>(0, out var backBuffer);
            if (hr.Failed) return hr;

            hr = Direct3DContext.Device.CreateRenderTargetView(backBuffer!, null, out var view);
            if (hr.Failed) {
                backBuffer.CheckAndRelease();
                return hr;
            }

            RenderingOutput = view;
            return hr;
        }
    }
}
