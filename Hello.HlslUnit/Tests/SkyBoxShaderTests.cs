using System.Runtime.InteropServices;
using HlslUnit;
using NUnit.Framework;
using SharpDX;

namespace Hello.HlslUnit.Tests
{
    [TestFixture]
    class SkyBoxShaderTests
    {
        private static readonly string ShaderFile = @"Content\SkyBoxEffect.fx".Locate();
        private readonly Shader _vertexShader = new Shader(ShaderFile.GetByteCode("VS", "vs_4_0"));
        private readonly Shader _pixelShader = new Shader(ShaderFile.GetByteCode("PS", "ps_4_0"));

        // ReSharper disable InconsistentNaming
        [Test]
        public void VertexShader_should_mult_by_ViewProj_and_copy_TexCoords_from_local_position()
        {
            // Arrange.
            _vertexShader.SetConstantBuffer("$Globals", new ConstantBufferGlobals
            {
                ViewProj = Matrix.LookAtLH(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY) *
                Matrix.PerspectiveFovRH(MathUtil.PiOverFour, 1, 0.01f, 1.0f)
            });

            var pos = new Vector3(3, 0, 2);
            var projPos = new Vector4(-7.24264f, 0, 3.020202f, -3.040404f);
            var expected = new VertexShaderOutput { Position = projPos, TexCoords = pos };

            // Act.
            var input = new VertexShaderInput { Position = pos };
            var actual = _vertexShader.Execute<VertexShaderInput, VertexShaderOutput>(input);

            // Assert.
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Description("TODO: Fix the sampling approach for TextureCube. This does not work")]
        public void PixelShader_should_take_texcoords_to_sample_cube_map()
        {
            // Arrange.
            var input = new VertexShaderOutput { TexCoords = new Vector3(1,2,3) };

            var expectedColor = Vector4.One;
            var anyOtherColor = Vector4.Zero;
            ResourceCallback<Vector4> cubeSample = (u, v, w, i) =>
                {
                    var texCoords = new Vector3(u, v, w);
                    return texCoords.Equals(input.TexCoords) ? expectedColor : anyOtherColor;
                };

            _pixelShader.SetResource("CubeSampler", cubeSample);
                //(u, v, w, i) => expectedColor);// );


            // Act.
            var actual = _pixelShader.Execute<VertexShaderOutput, Vector4>(input);

            // Assert.
            Assert.That(actual, Is.EqualTo(expectedColor));
        }
        // ReSharper restore InconsistentNaming

        [StructLayout(LayoutKind.Sequential)]
        public struct ConstantBufferGlobals
        {
            public Matrix ViewProj;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexShaderInput
        {
            public Vector3 Position;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexShaderOutput
        {
            public Vector4 Position;
            public Vector3 TexCoords;
        }
    }
}
