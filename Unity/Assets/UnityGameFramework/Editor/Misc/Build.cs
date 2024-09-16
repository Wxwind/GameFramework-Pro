using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace UnityGameFramework.Editor
{
    public static class Build
    {
        private static void BuildInternal(BuildTarget buildTarget)
        {
            Debug.Log($"开始构建 : {buildTarget}");

            var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

            // 构建参数
            var buildParameters = new BuiltinBuildParameters();
            buildParameters.BuildOutputRoot = buildoutputRoot;
            buildParameters.BuildinFileRoot = streamingAssetsRoot;
            buildParameters.BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString();
            buildParameters.BuildTarget = buildTarget;
            buildParameters.BuildMode = EBuildMode.ForceRebuild;
            buildParameters.PackageName = "DefaultPackage";
            buildParameters.PackageVersion = "1.0";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.EnableSharePackRule = true; //启用共享资源构建模式，兼容1.5x版本
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            buildParameters.BuildinFileCopyParams = string.Empty;
            // buildParameters.EncryptionServices = CreateEncryptionInstance();
            buildParameters.CompressOption = ECompressOption.LZ4;

            // 执行构建
            var pipeline = new BuiltinBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
            }
            else
            {
                Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
            }
        }
    }
}