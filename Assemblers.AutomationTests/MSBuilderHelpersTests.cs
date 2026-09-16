
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.CICD.Assemblers.Automation;

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
            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath);
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

            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath);

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
        public void CreateSyntheticPackageAssemblyReference_QAOpsApiCommon_ReturnsReference()
        {
            var csprojPath = @"C:\Users\SenaidVD\Desktop\Skyline-QAOps\Dxm\QAOps.Api.Common\QAOps.Api.Common.csproj";

            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath);

            Assert.IsNotNull(info);

            var reference = MSBuildHelpers.CreateSyntheticPackageAssembyReference(info);

            Assert.IsNotNull(reference);
        }
        [TestMethod]
        public void CreateSyntheticPackageAssemblyReference_QAOpsApi_ReturnsReference()
        {
            var csprojPath = @"C:\Users\SenaidVD\Desktop\Skyline-QAOps\Dxm\QAOps.Api\QAOps.Api.csproj";

            var info = MSBuildHelpers.EvaluateReferenceProject(csprojPath);

            Assert.IsNotNull(info);

            var reference = MSBuildHelpers.CreateSyntheticPackageAssembyReference(info);

            Assert.IsNotNull(reference);

            Console.WriteLine($"DllImport = {reference.DllImport}");
            Console.WriteLine($"AssemblyPath = {reference.AssemblyPath}");
            Console.WriteLine($"IsFilesPackage = {reference.IsFilesPackage}");
        }
    }
    
    }
