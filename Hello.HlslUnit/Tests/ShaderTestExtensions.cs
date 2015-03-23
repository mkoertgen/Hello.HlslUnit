using System;
using System.IO;
using SharpDX.D3DCompiler;

namespace Hello.HlslUnit.Tests
{
    static class ShaderTestExtensions
    {
        public static byte[] GetByteCode(this string fileName, string entryPoint, string profile,
            ShaderFlags shaderFlags = ShaderFlags.None)
        {
            var compiledShader = ShaderBytecode.CompileFromFile(fileName, entryPoint, profile, shaderFlags);
            return compiledShader.Bytecode.Data;
        }

        public static string Locate(this string fileProjectRelativePath)
        {
            var filePath = fileProjectRelativePath.Replace("/", @"\");

            var environmentDir = new DirectoryInfo(Environment.CurrentDirectory); // --> $(TargetDir) aka $(ProjectDir)bin\Release
            var itemPathUri = new Uri(Path.Combine(environmentDir.Parent.Parent.FullName, filePath)); // --> $(ProjectDir)

            var itemPath = itemPathUri.LocalPath;
            if (!File.Exists(itemPath))
                throw new FileNotFoundException("Could not locate deployment item", itemPath);
            return itemPath;
        }
    }
}