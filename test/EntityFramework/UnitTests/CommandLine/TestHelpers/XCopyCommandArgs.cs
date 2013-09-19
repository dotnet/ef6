// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;
    using System;

    /// <summary>
    /// Test class to see if we can simulate processing of XCopy command lines
    /// </summary>
    /// <remarks>
    /// XCOPY source [destination] [/A | /M] [/D[:date]] [/P] [/S [/E]] [/V] [/W]
    /// [/C] [/I] [/Q] [/F] [/L] [/G] [/H] [/R] [/T] [/U]
    /// [/K] [/N] [/O] [/X] [/Y] [/-Y] [/Z] [/B]
    /// [/EXCLUDE:file1[+file2][+file3]...]
    /// 
    /// source       Specifies the file(s) to copy.
    /// destination  Specifies the location and/or name of new files.
    /// /A           Copies only files with the archive attribute set,
    /// doesn't change the attribute.
    /// /M           Copies only files with the archive attribute set,
    /// turns off the archive attribute.
    /// /D:m-d-y     Copies files changed on or after the specified date.
    /// If no date is given, copies only those files whose
    /// source time is newer than the destination time.
    /// /EXCLUDE:file1[+file2][+file3]...
    /// Specifies a list of files containing strings.  Each string
    /// should be in a separate line in the files.  When any of the
    /// strings match any part of the absolute path of the file to be
    /// copied, that file will be excluded from being copied.  For
    /// example, specifying a string like \obj\ or .obj will exclude
    /// all files underneath the directory obj or all files with the
    /// .obj extension respectively.
    /// /P           Prompts you before creating each destination file.
    /// /S           Copies directories and subdirectories except empty ones.
    /// /E           Copies directories and subdirectories, including empty ones.
    /// Same as /S /E. May be used to modify /T.
    /// /V           Verifies the size of each new file.
    /// /W           Prompts you to press a key before copying.
    /// /C           Continues copying even if errors occur.
    /// /I           If destination does not exist and copying more than one file,
    /// assumes that destination must be a directory.
    /// /Q           Does not display file names while copying.
    /// /F           Displays full source and destination file names while copying.
    /// /L           Displays files that would be copied.
    /// /G           Allows the copying of encrypted files to destination that does
    /// not support encryption.
    /// /H           Copies hidden and system files also.
    /// /R           Overwrites read-only files.
    /// /T           Creates directory structure, but does not copy files. Does not
    /// include empty directories or subdirectories. /T /E includes
    /// empty directories and subdirectories.
    /// /U           Copies only files that already exist in destination.
    /// /K           Copies attributes. Normal Xcopy will reset read-only attributes.
    /// /N           Copies using the generated short names.
    /// /O           Copies file ownership and ACL information.
    /// /X           Copies file audit settings (implies /O).
    /// /Y           Suppresses prompting to confirm you want to overwrite an
    /// existing destination file.
    /// /-Y          Causes prompting to confirm you want to overwrite an
    /// existing destination file.
    /// /Z           Copies networked files in restartable mode.
    /// /B           Copies the Symbolic Link itself versus the target of the link.
    /// /J           Copies using unbuffered I/O. Recommended for very large files.
    /// 
    /// The switch /Y may be preset in the COPYCMD environment variable.
    /// This may be overridden with /-Y on the command line.
    /// </remarks>
    [migrate::CmdLine.CommandLineArgumentsAttribute(Title = Title, Description = Description)]
    public class XCopyCommandArgs
    {
        public const string Title = "XCopy Example";

        public const string Description = "A possible implementation of XCopy as a command args class";

        /// source       Specifies the file(s) to copy.
        [migrate::CmdLine.CommandLineParameterAttribute(Name = "source", ParameterIndex = 1, Required = true,
            Description = "Specifies the file(s) to copy.")]
        public string Source { get; set; }

        /// destination  Specifies the location and/or name of new files.
        [migrate::CmdLine.CommandLineParameterAttribute(Name = "destination", ParameterIndex = 2,
            Description = "Specifies the file(s) to copy.")]
        public string Destination { get; set; }

        //   /A           Copies only files with the archive attribute set,
        [migrate::CmdLine.CommandLineParameterAttribute(Command = "A", Description = "Copies only files with the archive attribute set")]
        public bool ArchivedBit { get; set; }

        /// /I           If destination does not exist and copying more than one file,
        /// assumes that destination must be a directory.
        [migrate::CmdLine.CommandLineParameterAttribute(Command = "I",
            Description = "If destination does not exist and copying more than one file,assumes that destination must be a directory.")]
        public bool InferDirectory { get; set; }

        /// /D:m-d-y     Copies files changed on or after the specified date.
        [migrate::CmdLine.CommandLineParameterAttribute(Command = "D", ValueExample = "m-d-y",
            Description = "Copies files changed on or after the specified date.")]
        public DateTime ChangedAfterDate { get; set; }

        /// /EXCLUDE:file1[+file2][+file3]...
        [migrate::CmdLine.CommandLineParameterAttribute(Command = "EXCLUDE", ValueExample = "file1[+file2][+file3]...",
            DescriptionResourceId = "ExcludeDescription")]
        public string ExcludeFiles { get; set; }
    }
}
