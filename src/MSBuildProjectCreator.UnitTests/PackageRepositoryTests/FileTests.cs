﻿// Copyright (c) Jeff Kluge. All rights reserved.
//
// Licensed under the MIT license.

using NuGet.Packaging;
using NuGet.Packaging.Core;
using Shouldly;
using System.IO;
using Xunit;

namespace Microsoft.Build.Utilities.ProjectCreation.UnitTests.PackageRepositoryTests
{
    public class FileTests : TestBase
    {
        [Fact]
        public void CustomFileTest()
        {
            string relativePath = Path.Combine("test", "foo.txt");
            const string contents = "798D159A4ADE45B9896EDE89FBA39C60";

            FileInfo sourceFileInfo = new FileInfo(Path.Combine(TestRootPath, "something"));

            File.WriteAllText(sourceFileInfo.FullName, contents);

            PackageRepository.Create(TestRootPath)
                .Package("PackageA", "1.0.0", out PackageIdentity packageA)
                .FileCustom(relativePath, sourceFileInfo);

            VerifyFileContents(packageA, relativePath, contents);
        }

        [Fact]
        public void TextFileTest()
        {
            string relativePath = Path.Combine("test", "foo.txt");
            const string contents = "FF6B25B727E04D9980DE3B5D7AE0FB6E";

            PackageRepository.Create(TestRootPath)
                .Package("PackageA", "1.0.0", out PackageIdentity packageA)
                .FileText(relativePath, contents);

            VerifyFileContents(packageA, relativePath, contents);
        }

        private void VerifyFileContents(PackageIdentity package, string relativePath, string contents)
        {
            DirectoryInfo packageDirectory = new DirectoryInfo(((VersionFolderPathResolver)VersionFolderPathResolver).GetInstallPath(package.Id, package.Version))
                            .ShouldExist();

            FileInfo file = new FileInfo(Path.Combine(packageDirectory.FullName, relativePath))
                .ShouldExist();

            File.ReadAllText(file.FullName).ShouldBe(contents);
        }
    }
}