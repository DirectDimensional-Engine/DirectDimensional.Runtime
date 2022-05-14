//using DirectDimensional.Bindings.Direct3D11;
//using DirectDimensional.Bindings.D3DCompiler;
//using DirectDimensional.Bindings.DXGI;
//using DirectDimensional.Bindings;
//using System.Runtime.InteropServices;
//using System.Runtime.CompilerServices;
//using System.Numerics;
//using DirectDimensional.Core;
//using DirectDimensional.Core.Utilities;
//using DirectDimensional.Core.Miscs;

//using D3D11Buffer = DirectDimensional.Bindings.Direct3D11.Buffer;

//using DDVertexShader = DirectDimensional.Core.VertexShader;
//using DDPixelShader = DirectDimensional.Core.PixelShader;

//namespace DirectDimensional.Runtime {
//    internal static unsafe class EngineLifecycle {
//        private static InputLayout _il = null!;

//        private static ComArray<D3D11Buffer> _constantBuffers = null!;

//        private static Mesh _mesh = null!;
//        private static Material _standard3DShader = null!;

//        public static void Initialize() {
//            var device = Direct3DContext.Device;

//            _mesh = new();

//            _mesh.SetVertices(new List<Vertex>() {
//                new Vertex(new(0, 0.5f, 0), Color32.White, new(0, 0)),
//                new Vertex(new(0.5f, -0.5f, 0), Color32.White, new(0, 0)),
//                new Vertex(new(-0.5f, -0.5f, 0), Color32.White, new(0, 0)),
//            });

//            _mesh.SetIndices(new List<ushort>() {
//                0, 1, 2
//            });

//            var _vs = DDVertexShader.CompileFromRawFile(@"D:\C# Projects\DirectDimensional.Core\Resources\Standard3DVS.hlsl", null, out var pBytecode);

//            D3D11_INPUT_ELEMENT_DESC[] inputElementDescs = new D3D11_INPUT_ELEMENT_DESC[] {
//                new("Position", 0, DXGI_FORMAT.R32G32B32_FLOAT, 0, 0, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
//                new("Color", 0, DXGI_FORMAT.R8G8B8A8_UNORM, 0, 12, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
//                new("TexCoord", 0, DXGI_FORMAT.R32G32_FLOAT, 0, 16, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),

//                new("Normal", 0, DXGI_FORMAT.R32G32B32_FLOAT, 0, 24, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
//                new("Tangent", 0, DXGI_FORMAT.R32G32B32_FLOAT, 0, 36, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
//                new("Bitangent", 0, DXGI_FORMAT.R32G32B32_FLOAT, 0, 48, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
//            };

//            device.CreateInputLayout(inputElementDescs, pBytecode!, out _il!).ThrowExceptionIfError();

//            for (int i = 0; i < inputElementDescs.Length; i++) {
//                inputElementDescs[i].Dispose();
//            }

//            pBytecode.CheckAndRelease();

//            var _ps = DDPixelShader.CompileFromRawFile(@"D:\C# Projects\DirectDimensional.Core\Resources\Standard3DPS.hlsl", null);

//            _standard3DShader = new(_vs!, _ps!);

//            pBytecode.CheckAndRelease();

//            _constantBuffers = new(RenderingConstants.SystemBufferCount);

//            device.CreateConstantBuffer(16u, null, out var cbuf);
//            _constantBuffers[RenderingConstants.PerFrameBufferIndex] = cbuf!;

//            Matrix4x4 view = DDMath.LookToLH(-Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitY);
//            Span<Matrix4x4> _perCam = new Span<Matrix4x4>(new Matrix4x4[3]);

//            _perCam[0] = DDMath.LookToLH(-Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitY);
//            _perCam[1] = DDMath.PerspectiveFovLH(1.5f, Window.ClientAspectRatio, 0.3f, 1000f);
//            _perCam[2] = _perCam[0] * _perCam[1];

//            fixed (void* ptr = _perCam) {
//                device.CreateConstantBuffer(192, ptr, out cbuf);
//                _constantBuffers[RenderingConstants.PerCameraBufferIndex] = cbuf!;
//            }

//            Matrix4x4 model = Matrix4x4.Identity;
//            device.CreateConstantBuffer(64u, &model, out cbuf);
//            _constantBuffers[RenderingConstants.PerObjectBuiltInIndex] = cbuf!;
//        }

//        static float color = 0;
//        public static void Cycle() {
//            if (!RuntimeContext.RenderingOutput.Alive()) {
//                return;
//            }

//            var d3dctx = Direct3DContext.DevCtx;

//            d3dctx.ClearRenderTargetView(RuntimeContext.RenderingOutput, new Vector4(color, color, color, 1));
//            color = (color + 0.01f) % 1;

//            d3dctx.OMSetRenderTargets(RuntimeContext.RenderingOutputAsArray, null);

//            d3dctx.IASetInputLayout(_il);
//            d3dctx.IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY.TriangleList);
//            d3dctx.IASetVertexBuffers(0u, _mesh.VertexBuffers!, new[] { (uint)Vertex.MemorySize }, new[] { 0u });
//            d3dctx.IASetIndexBuffer(_mesh.IndexBuffer, DXGI_FORMAT.R16_UINT, 0);

//            d3dctx.VSSetShader(_standard3DShader.VertexShader!.Shader);
//            d3dctx.PSSetShader(_standard3DShader.PixelShader!.Shader);

//            d3dctx.VSSetConstantBuffers(0u, _constantBuffers!);
//            d3dctx.PSSetConstantBuffers(0u, _constantBuffers!);

//            d3dctx.DrawIndexed(3, 0, 0);
//        }

//#if DEBUG
//        private static void DebugWriter(IntPtr pMessage) {
//            D3D11_MESSAGE* pMsg = (D3D11_MESSAGE*)pMessage;

//            if (pMsg->Category != D3D11_MESSAGE_CATEGORY.D3D11_MESSAGE_CATEGORY_STATE_CREATION) {
//                Console.WriteLine(pMsg->Description);
//            }
//        }
//#endif

//        public static void Shutdown() {
//            _il.CheckAndRelease();
//            _standard3DShader.Destroy();
//            _mesh.Destroy();

//            _constantBuffers.TrueDispose();

//#if DEBUG
//            Direct3DContext.ReportLiveObjects();
//            Direct3DContext.FlushD3D11DebugMessages(DebugWriter);
//#endif

//            Direct3DContext.Shutdown();
//        }
//    }
//}
