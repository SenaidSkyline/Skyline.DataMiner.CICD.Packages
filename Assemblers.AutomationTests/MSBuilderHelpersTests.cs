
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.CICD.Assemblers.Automation;
using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;

namespace Assemblers.AutomationTests
{
    [TestClass]
    public class MSBuilderHelpersTests
    {
        [TestMethod]
        public void EvaluateReferenceProject_BasicPackableProject_ReadsCoreProperties()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            var csprojPath = Path.Combine(dir, "Lib.csproj");
            File.WriteAllText(csprojPath, """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <AssemblyName>LibA</AssemblyName>
                    <PackageId>Pkg.LibA</PackageId>
                    <PackageVersion>1.0.5</PackageVersion>
                    <IsPackable>true</IsPackable>
                  </PropertyGroup>
                </Project>
                """);
            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath, "net48");
            Assert.IsNotNull(info);
            Assert.AreEqual("Pkg.LibA", info.PackageId);
            Assert.AreEqual("netstandard2.0", info.TargetFramework);
            Assert.AreEqual("1.0.5", info.PackageVersion);
            Assert.IsTrue(info.ShouldHarvestAsNuGetAssemblies());
        }
        [TestMethod]
        public void EvaluateReferenceProject_QAOpsApi_ReadsCoreProperties()
        {
            var csprojPath = @"C:\Users\SenaidVD\Desktop\Skyline-QAOps\Dxm\QAOps.Api\QAOps.Api.csproj";

            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath, "net48");

            Assert.IsNotNull(info);
            Assert.AreEqual("Skyline.DataMiner.QAOps.Api", info.PackageId);
            Assert.AreEqual("netstandard2.0", info.TargetFramework);
            Assert.AreEqual("0.0.2-local12", info.PackageVersion);
            Assert.IsTrue(info.IsPackable);
            Assert.IsTrue(info.GeneratePackageOnBuild);
            Assert.IsTrue(string.IsNullOrWhiteSpace(info.DataMinerType));
            Assert.IsTrue(info.ShouldHarvestAsNuGetAssemblies());
        }
        [TestMethod]
        public void EvaluateReferenceProject_DataMinerProject_IsNotHarvested()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            var csprojPath = Path.Combine(dir, "Lib.csproj");

            File.WriteAllText(csprojPath, """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>netstandard2.0</TargetFramework>
                    <AssemblyName>DataMinerLib</AssemblyName>
                    <PackageId>DataMiner.Lib</PackageId>
                    <PackageVersion>1.0.0</PackageVersion>
                    <IsPackable>true</IsPackable>
                    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
                    <DataMinerType>AutomationScript</DataMinerType>
                  </PropertyGroup>
                </Project>
                """);

            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath, "net48");

            Assert.IsNotNull(info);
            Assert.IsTrue(info.IsDataMinerProject);
            Assert.IsFalse(info.ShouldHarvestAsNuGetAssemblies());
        }
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetLibrary_ReadsTargetFramework()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            try
            {
                var csprojPath = Path.Combine(dir, "Lib.csproj");

                File.WriteAllText(csprojPath, """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFrameworks>net48;net10.0</TargetFrameworks>
                        <OutputType>Library</OutputType>
                        <AssemblyName>Lib</AssemblyName>
                      </PropertyGroup>
                    </Project>
                    """);

                var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath, "net48");

                Assert.IsNotNull(info);
                Assert.AreEqual("Library", info.OutputType);
                Assert.AreEqual("net48", info.TargetFramework);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetLibrary_ReadsTargetPath()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            try
            {
                var csprojPath = Path.Combine(dir, "Lib.csproj");

                File.WriteAllText(csprojPath, """
                    <Project Sdk="Microsoft.NET.Sdk">
                      <PropertyGroup>
                        <TargetFrameworks>net48;net10.0</TargetFrameworks>
                        <OutputType>Library</OutputType>
                        <AssemblyName>Lib</AssemblyName>
                      </PropertyGroup>
                    </Project>
                    """);
                var projectCollection = new ProjectCollection();

                var project = projectCollection.LoadProject(
                    csprojPath,
                    new Dictionary<string, string>
                    {
                        ["TargetFramework"] = "net48",
                    },
                    null);

                Console.WriteLine($"TargetPath: {project.GetPropertyValue("TargetPath")}");
                var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath, "net48");
                Console.WriteLine($"TargetFramework: {info.TargetFramework}");
                Console.WriteLine($"TargetPath: {info.TargetPath}");
                Assert.IsNotNull(info);
                Assert.IsFalse(string.IsNullOrWhiteSpace(info.TargetPath));

               
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
        [TestMethod]
        public void EvaluateReferenceProject_MultiTargetLibrary_RecursiveReference_UsesSameTargetFramework()
        {
       
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            try
            {
                var libraryBPath = Path.Combine(dir, "LibraryB.csproj");
                File.WriteAllText(libraryBPath, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net48;net10.0</TargetFrameworks>
                <OutputType>Library</OutputType>
                <AssemblyName>LibraryB</AssemblyName>
              </PropertyGroup>
            </Project>
            """);

                var libraryAPath = Path.Combine(dir, "LibraryA.csproj");
                File.WriteAllText(libraryAPath, $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net48;net10.0</TargetFrameworks>
                <OutputType>Library</OutputType>
                <AssemblyName>LibraryA</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="{libraryBPath}" />
              </ItemGroup>
            </Project>
            """);

                var rootPath = Path.Combine(dir, "Root.csproj");
                File.WriteAllText(rootPath, $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <OutputType>Library</OutputType>
                <AssemblyName>Root</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="{libraryAPath}" />
              </ItemGroup>
            </Project>
            """);

                var projectCollection = new ProjectCollection();

                var rootProject = projectCollection.LoadProject(
                    rootPath,
                    new Dictionary<string, string>
                    {
                        ["TargetFramework"] = "net48",
                    },
                    null);

                var rootTfm = rootProject.GetPropertyValue("TargetFrameworkMoniker");

                var libraryA = MSBuildHelpers.EvaluateReferenceProject(
                    libraryAPath,
                    rootTfm);

                Assert.IsNotNull(libraryA);
                Assert.AreEqual("net48", libraryA.TargetFramework);
                StringAssert.EndsWith(libraryA.TargetPath, @"bin\Debug\net48\LibraryA.dll");
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
    
    }
