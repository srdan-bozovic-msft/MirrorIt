using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorIt.App.Model;
using System.IO;
using System.Collections;

namespace MirrorIt.App.Service
{
    public class MirrorService : IMirrorService
    {
        public MirrorResult Mirror(string sourcePath, string mirrorPath, MirrorConfiguration configuration)
        {
            var mirrorResult = new MirrorResult();
            mirrorResult.Succedeed = ProcessDirectory(sourcePath, mirrorPath, configuration, ref mirrorResult);
            return mirrorResult;
        }

        private bool ProcessDirectory(string srcDir, string destDir, MirrorConfiguration inputParams, ref MirrorResult results)
        {
            DirectoryInfo diSrc = new DirectoryInfo(srcDir);
            DirectoryInfo diDest = new DirectoryInfo(destDir);

            // create destination directory if it doesn't exist
            if (!diDest.Exists)
            {
                try
                {
                    Trace("Creating directory: {0}", diDest.FullName);

                    // create the destination directory
                    diDest.Create();
                    results.DirectoriesCreated++;
                }
                catch (Exception ex)
                {
                    Trace("Error: failed to create directory {0}. {1}", destDir, ex.Message);
                    return false;
                }
            }

            // get list of selected files from source directory
            FileInfo[] fiSrc = GetFiles(diSrc, inputParams, ref results);
            // get list of files in destination directory
            FileInfo[] fiDest = GetFiles(diDest, null, ref results);

            // put the source files and destination files into hash tables                     
            Hashtable hashSrc = new Hashtable(fiSrc.Length);
            foreach (FileInfo srcFile in fiSrc)
            {
                hashSrc.Add(srcFile.Name, srcFile);
            }
            Hashtable hashDest = new Hashtable(fiDest.Length);
            foreach (FileInfo destFile in fiDest)
            {
                hashDest.Add(destFile.Name, destFile);
            }

            // make sure all the selected source files exist in destination
            foreach (FileInfo srcFile in fiSrc)
            {
                bool isUpToDate = false;

                // look up in hash table to see if file exists in destination
                FileInfo destFile = (FileInfo)hashDest[srcFile.Name];
                // if file exists and length, write time and attributes match, it's up to date
                if ((destFile != null) && (srcFile.Length == destFile.Length) &&
                    (srcFile.LastWriteTime == destFile.LastWriteTime) &&
                    (srcFile.Attributes == destFile.Attributes))
                {
                    isUpToDate = true;
                    results.FilesUpToDate++;
                }

                // if the file doesn't exist or is different, copy the source file to destination
                if (!isUpToDate)
                {
                    string destPath = Path.Combine(destDir, srcFile.Name);
                    // make sure destination is not read-only
                    if (destFile != null && destFile.IsReadOnly)
                    {
                        destFile.IsReadOnly = false;
                    }

                    try
                    {
                        Trace("Copying: {0} -> {1}", srcFile.FullName, Path.GetFullPath(destPath));

                        // copy the file
                        srcFile.CopyTo(destPath, true);
                        // set attributes appropriately
                        File.SetAttributes(destPath, srcFile.Attributes);
                        results.FilesCopied++;
                    }
                    catch (Exception ex)
                    {
                        Trace("Error: failed to copy file from {0} to {1}. {2}", srcFile.FullName, destPath, ex.Message);
                        return false;
                    }
                }
            }

            // delete extra files in destination directory if specified
            if (inputParams.DeleteFromDest)
            {
                foreach (FileInfo destFile in fiDest)
                {
                    FileInfo srcFile = (FileInfo)hashSrc[destFile.Name];
                    if (srcFile == null)
                    {
                        // if this file is specified in exclude-from-deletion list, don't delete it
                        if (ShouldExclude(inputParams.DeleteExcludeFiles, null, destFile.Name))
                            continue;

                        try
                        {

                            Trace("Deleting: {0} ", destFile.FullName);

                            destFile.IsReadOnly = false;
                            // delete the file
                            destFile.Delete();
                            results.FilesDeleted++;
                        }
                        catch (Exception ex)
                        {
                            Trace("Error: failed to delete file from {0}. {1}", destFile.FullName, ex.Message);
                            return false;
                        }
                    }
                }
            }

            // Get list of selected subdirectories in source directory
            DirectoryInfo[] diSrcSubdirs = GetDirectories(diSrc, inputParams, ref results);
            // Get list of subdirectories in destination directory
            DirectoryInfo[] diDestSubdirs = GetDirectories(diDest, null, ref results);

            // add selected source subdirectories to hash table, and recursively process them
            Hashtable hashSrcSubdirs = new Hashtable(diSrcSubdirs.Length);
            foreach (DirectoryInfo diSrcSubdir in diSrcSubdirs)
            {
                hashSrcSubdirs.Add(diSrcSubdir.Name, diSrcSubdir);
                // recurse into this directory
                if (!ProcessDirectory(diSrcSubdir.FullName, Path.Combine(destDir, diSrcSubdir.Name), inputParams, ref results))
                    return false;
            }

            // delete extra directories in destination if specified
            if (inputParams.DeleteFromDest)
            {
                foreach (DirectoryInfo diDestSubdir in diDestSubdirs)
                {
                    // does this destination subdirectory exist in the source subdirs?
                    if (!hashSrcSubdirs.ContainsKey(diDestSubdir.Name))
                    {
                        // if this directory is specified in exclude-from-deletion list, don't delete it
                        if (ShouldExclude(inputParams.DeleteExcludeDirs, null, diDestSubdir.Name))
                            continue;

                        try
                        {
                            Trace("Deleting directory: {0} ", diDestSubdir.FullName);

                            // delete directory
                            DeleteDirectory(diDestSubdir);
                            results.DirectoriesDeleted++;
                        }
                        catch (Exception ex)
                        {
                            Trace("Error: failed to delete directory {0}. {1}", diDestSubdir.FullName, ex.Message);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public virtual FileInfo[] GetFiles(DirectoryInfo directoryInfo, MirrorConfiguration inputParams, ref MirrorResult results)
        {
            // get all files
            List<FileInfo> fileList = new List<FileInfo>(directoryInfo.GetFiles());

            // do we need to do any filtering?
            bool needFilter = (inputParams != null) && (inputParams.AreSourceFilesFiltered);

            if (needFilter)
            {
                for (int i = 0; i < fileList.Count; i++)
                {
                    FileInfo fileInfo = fileList[i];

                    // filter out any files based on hiddenness and exclude/include filespecs
                    if ((inputParams.ExcludeHidden && ((fileInfo.Attributes & FileAttributes.Hidden) > 0)) ||
                         ShouldExclude(inputParams.ExcludeFiles, inputParams.IncludeFiles, fileInfo.Name))
                    {
                        fileList.RemoveAt(i);
                        results.FilesIgnored++;
                        i--;
                    }
                }
            }

            return fileList.ToArray();
        }

        public virtual DirectoryInfo[] GetDirectories(DirectoryInfo directoryInfo, MirrorConfiguration inputParams, ref MirrorResult results)
        {
            // get all directories
            List<DirectoryInfo> directoryList = new List<DirectoryInfo>(directoryInfo.GetDirectories());

            // do we need to do any filtering?
            bool needFilter = (inputParams != null) && (inputParams.AreSourceFilesFiltered);
            if (needFilter)
            {
                for (int i = 0; i < directoryList.Count; i++)
                {
                    DirectoryInfo subdirInfo = directoryList[i];

                    // filter out directories based on hiddenness and exclude/include filespecs
                    if ((inputParams.ExcludeHidden && ((subdirInfo.Attributes & FileAttributes.Hidden) > 0)) ||
                         ShouldExclude(inputParams.ExcludeDirs, inputParams.IncludeDirs, subdirInfo.Name))
                    {
                        directoryList.RemoveAt(i);
                        results.DirectoriesIgnored++;
                        i--;
                    }
                }
            }

            return directoryList.ToArray();
        }

        public virtual void DeleteDirectory(DirectoryInfo directory)
        {
            // make sure all files are not read-only
            FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo fileInfo in files)
            {
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }
            }

            // make sure all subdirectories are not read-only
            DirectoryInfo[] directories = directory.GetDirectories("*.*", SearchOption.AllDirectories);
            foreach (DirectoryInfo subdir in directories)
            {
                if ((subdir.Attributes & FileAttributes.ReadOnly) > 0)
                {
                    subdir.Attributes &= ~FileAttributes.ReadOnly;
                }
            }

            // make sure top level directory is not read-only
            if ((directory.Attributes & FileAttributes.ReadOnly) > 0)
            {
                directory.Attributes &= ~FileAttributes.ReadOnly;
            }
            directory.Delete(true);
        }

        private bool ShouldExclude(object deleteExcludeDirs, object p, string name)
        {
            return false;
        }

        private void Trace(string message, params object[] args)
        {

        }

    }
}
