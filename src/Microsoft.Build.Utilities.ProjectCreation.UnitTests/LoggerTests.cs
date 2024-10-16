// Copyright (c) Jeff Kluge. All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Shouldly;
using System.IO;
using Xunit;

namespace Microsoft.Build.Utilities.ProjectCreation.UnitTests
{
    public class LoggerTests : TestBase
    {
        [Fact]
        public void ProjectCollectionLoggersWork()
        {
            string binLogPath = Path.Combine(TestRootPath, "test.binlog");
            string fileLogPath = Path.Combine(TestRootPath, "test.log");

            using (ProjectCollection projectCollection = new ProjectCollection())
            {
                projectCollection.RegisterLogger(new BinaryLogger
                {
                    Parameters = $"LogFile={binLogPath}",
                });
                projectCollection.RegisterLogger(new FileLogger
                {
                    Parameters = $"LogFile={fileLogPath}",
                    Verbosity = LoggerVerbosity.Normal,
                    ShowSummary = true,
                });

                ProjectCreator.Templates
                    .LogsMessage(
                        text: "$(Property1)",
                        projectCollection: projectCollection)
                    .Property("Property1", "2AE492F6EEE04255B31B088051E9AF0F")
                    .Save(GetTempFileName(".proj"))
                    .TryBuild(out bool result, out BuildOutput buildOutput);

                result.ShouldBeTrue();

                buildOutput.MessageEvents.Normal.ShouldContain(i => i.Message == "2AE492F6EEE04255B31B088051E9AF0F", buildOutput.GetConsoleLog());
            }

            File.Exists(binLogPath).ShouldBeTrue();

            File.Exists(fileLogPath).ShouldBeTrue();

            string fileLogContents = File.ReadAllText(fileLogPath);

            fileLogContents.ShouldContain("2AE492F6EEE04255B31B088051E9AF0F", Case.Sensitive, fileLogContents);
        }

        [Fact]
        public void ProjectCollectionLoggersWorkWithRestore()
        {
            string binLogPath = Path.Combine(TestRootPath, "test.binlog");
            string fileLogPath = Path.Combine(TestRootPath, "test.log");

            using (ProjectCollection projectCollection = new ProjectCollection())
            {
                projectCollection.RegisterLogger(new BinaryLogger
                {
                    Parameters = $"LogFile={binLogPath}",
                });

                projectCollection.RegisterLogger(new FileLogger
                {
                    Parameters = $"LogFile={fileLogPath}",
                    Verbosity = LoggerVerbosity.Normal,
                    ShowSummary = true,
                });

                ProjectCreator.Templates
                    .LogsMessage(
                        text: "$(Property1)",
                        projectCollection: projectCollection)
                    .Property("Property1", "B7F9A257198D4A44A06BB6146AB27440")
                    .Target("Restore")
                        .TaskMessage("38EC33B686134B3C8DE4B8E571D4FB24", MessageImportance.High)
                    .Save(GetTempFileName(".proj"))
                    .TryBuild(restore: true, out bool result, out BuildOutput buildOutput);

                result.ShouldBeTrue(buildOutput.GetConsoleLog());

                buildOutput.MessageEvents.High.ShouldContain(i => i.Message == "38EC33B686134B3C8DE4B8E571D4FB24", buildOutput.GetConsoleLog());
                buildOutput.MessageEvents.Normal.ShouldContain(i => i.Message == "B7F9A257198D4A44A06BB6146AB27440", buildOutput.GetConsoleLog());
            }

            File.Exists(binLogPath).ShouldBeTrue();

            File.Exists(fileLogPath).ShouldBeTrue();

            string fileLogContents = File.ReadAllText(fileLogPath);

            fileLogContents.ShouldContain("38EC33B686134B3C8DE4B8E571D4FB24", Case.Sensitive, fileLogContents);
            fileLogContents.ShouldContain("B7F9A257198D4A44A06BB6146AB27440", Case.Sensitive, fileLogContents);
        }
    }
}