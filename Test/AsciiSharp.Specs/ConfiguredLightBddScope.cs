using LightBDD.Core.Configuration;
using LightBDD.Framework.Configuration;
using LightBDD.Framework.Reporting.Formatters;
using LightBDD.MsTest4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Specs;

/// <summary>
/// LightBDD のアセンブリ レベルの初期化・クリーンアップを行うクラスです。
/// Markdown レポートの出力設定もここで構成します。
/// </summary>
[TestClass]
public class ConfiguredLightBddScope
{
    [AssemblyInitialize]
    public static void Setup(TestContext testContext)
    {
        LightBddScope.Initialize(cfg => cfg
            .ReportWritersConfiguration()
            .Clear()
            .AddFileWriter<MarkdownReportFormatter>("~/Reports/FeaturesReport.md"));
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        LightBddScope.Cleanup();
    }
}
