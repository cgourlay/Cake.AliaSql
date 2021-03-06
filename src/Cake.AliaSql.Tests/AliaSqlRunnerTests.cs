﻿using System;
using System.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace Cake.AliaSql.Tests
{
    public sealed class AliaSqlRunnerTests
    {
        public sealed class TheRunMethod
        {

            [Fact]
            public void Should_Throw_If_AliaSql_Runner_Was_Not_Found()
            {
                // Given
                var fixture = new AliaSqlFixture(defaultToolExist: false);
                var runner = fixture.CreateRunner();

                // When
                var result = Record.Exception(() => runner.Run(GetDefault()));

                // Then
                Assert.IsType<CakeException>(result);
                Assert.Equal("AliaSql: Could not locate executable.", result.Message);
            }

            [Theory]
            [InlineData("C:/AliaSql/AliaSql.exe", "C:/AliaSql/AliaSql.exe")]
            [InlineData("./tools/AliaSQL/tools/AliaSQL.exe", "/Working/tools/AliaSQL/tools/AliaSQL.exe")]
            public void Should_Use_AliaSql_Runner_From_Tool_Path_If_Provided(string toolPath, string expected)
            {
                // Given
                var fixture = new AliaSqlFixture(expected);
                var runner = fixture.CreateRunner();

                var settings = GetDefault();
                settings.ToolPath = toolPath;

                // When
                runner.Run(settings);

                // Then
                fixture.ProcessRunner.Received(1).Start(Arg.Is<FilePath>(
                    fp => fp.FullPath == expected),
                    Arg.Any<ProcessSettings>());
            }

            [Fact]
            public void Should_Find_AliaSql_Runner_If_Tool_Path_Not_Provided()
            {
                // Given
                var fixture = new AliaSqlFixture();
                var runner = fixture.CreateRunner();

                // When
                runner.Run(GetDefault());

                // Then
                fixture.ProcessRunner.Received(1).Start(Arg.Is<FilePath>(
                    fp => fp.FullPath == "/Working/tools/AliaSQL/tools/AliaSQL.exe"),
                    Arg.Any<ProcessSettings>());
            }

            [Fact]
            public void Should_Set_Working_Directory()
            {
                // Given
                var fixture = new AliaSqlFixture();
                var runner = fixture.CreateRunner();

                // When
                runner.Run(GetDefault());

                // Then
                fixture.ProcessRunner.Received(1).Start(
                    Arg.Any<FilePath>(),
                    Arg.Is<ProcessSettings>(ps => ps.WorkingDirectory.FullPath == "/Working"));
            }

            [Fact]
            public void Should_Throw_If_Process_Was_Not_Started()
            {
                // Given
                var fixture = new AliaSqlFixture();
                fixture.ProcessRunner.Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>()).Returns((IProcess)null);
                var runner = fixture.CreateRunner();

                // When
                var result = Record.Exception(() => runner.Run(GetDefault()));

                // Then
                Assert.IsType<CakeException>(result);
                Assert.Equal("AliaSql: Process was not started.", result.Message);
            }

            [Fact]
            public void Should_Throw_If_Process_Has_A_Non_Zero_Exit_Code()
            {
                // Given
                var fixture = new AliaSqlFixture();
                fixture.Process.GetExitCode().Returns(1);
                var runner = fixture.CreateRunner();

                // When
                var result = Record.Exception(() => runner.Run(GetDefault()));

                // Then
                Assert.IsType<CakeException>(result);
                Assert.Equal("AliaSql: Process returned an error.", result.Message);
            }

            [Fact]
            public void Should_Throw_If_No_Arguments_Folder()
            {
                // Given
                var fixture = new AliaSqlFixture();
                fixture.Process.GetExitCode().Returns(1);
                var runner = fixture.CreateRunner();

                // When
                var result = Record.Exception(() => runner.Run(new AliaSqlSettings()));

                // Then
                Assert.IsType<NullReferenceException>(result);
            }

            private AliaSqlSettings GetDefault()
            {
                return new AliaSqlSettings
                {
                    Command = "",
                    ConnectionString = "",
                    DatabaseName = "",
                    ScriptsFolder = new DirectoryPath("/Working/scripts/")
                };
            }
        }
    }
}
