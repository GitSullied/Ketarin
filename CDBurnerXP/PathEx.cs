using System;
using System.IO;
using System.Security;
using Microsoft.Win32;
using System.Collections.Generic;

namespace CDBurnerXP.IO
{
    public static partial class PathEx
    {
        public delegate string TempPathGetterDelegate();

        private static bool m_CancelCopy = false;
        private static TempPathGetterDelegate m_TempPathGetter = null;

        #region Properties

        /// <summary>
        /// Allows to specify a method which determines the path
        /// for temporary data.
        /// </summary>
        public static TempPathGetterDelegate TempPathGetter
        {
            get { return m_TempPathGetter; }
            set { m_TempPathGetter = value; }
        }

        /// <summary>
        /// Allows to cancel the copy process.
        /// </summary>
        public static bool CancelCopy
        {
            set { PathEx.m_CancelCopy = value; }
        }

        /// <summary>
        /// Determines whether or not hidden files are shown in Windows Explorer.
        /// </summary>
        public static bool IsShowHiddenFilesEnabled
        {
            get
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser;
                    key = key.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
                    int value = Convert.ToInt16(key.GetValue("Hidden"));
                    return (value == 1);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a path for storing data from removable media.
        /// </summary>
        public static string RemoveableMediaCache
        {
            get
            {
                return Path.Combine(GetTempPath(), "FromRemovableMedia");
            }
        }

        #endregion


        public static event EventHandler<GenericEventArgs<string>> FileCopying;

        /// <summary>
        /// Returns a random file name within the configured temp directory.
        /// </summary>
        public static string GetTempFileName()
        {
            return GetTempFileName("tmp");
        }

        /// <summary>
        /// Returns a random file name within the configured temp directory.
        /// </summary>
        public static string GetTempFileName(string extension)
        {
            return Path.Combine(GetTempPath(), Guid.NewGuid().ToString("N") + "." + extension.TrimStart('.'));
        }

        /// <summary>
        /// Returns the configured temp directory.
        /// </summary>
        public static string GetTempPath()
        {
            string path = (m_TempPathGetter == null) ? Path.GetTempPath() : m_TempPathGetter();
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                return Path.GetTempPath();
            }

            return path;
        }

        /// <summary>
        /// Determines whether a drive with the given drive letter is set in the unitmask.
        /// </summary>
        public static bool IsDriveInUnitmask(int bitmask, char driveLetter)
        {
            int pos = char.ToLower(driveLetter) - 'a';
            if (pos < 0) return false;

            return ((bitmask & (int)Math.Pow(2, pos)) > 0);
        }

        /// <summary>
        /// Determines whether or not a given drive is hidden per group policies.
        /// </summary>
        public static bool IsDriveHidden(char driveLetter)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer");
                if (key == null) return false;

                int noDrives = Convert.ToInt32(key.GetValue("NoDrives"));
                int noViewOnDrives = Convert.ToInt32(key.GetValue("NoViewOnDrives"));

                return IsDriveInUnitmask(noDrives, driveLetter) || IsDriveInUnitmask(noViewOnDrives, driveLetter);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the directory where all temporary audio data should be stored.
        /// </summary>
        public static string GetTempRipPath() {
            string path = Path.Combine(GetTempPath(), "TempRips");
            try
            {
                Directory.CreateDirectory(path);
            } catch { };
            return path;
        }

        public static string GetTempRipFile()
        {
            return GetTempRipFile(string.Empty);
        }

        /// <summary>
        /// Creates a local temp directory within %temp%/FromRemovableMedia/{GUID}
        /// and replaces the source path with the new directory.
        /// </summary>
        /// <returns>The original path on failure, otherwise the new temp dir</returns>
        public static string CacheLocally(string sourcePath)
        {
            try
            {
                string newPath = RemoveableMediaCache;
                newPath = Path.Combine(newPath, Guid.NewGuid().ToString());
                // Make sure that we still have the same directory name
                newPath = Path.Combine(newPath, Path.GetFileName(sourcePath));

                CopyFolder(sourcePath, newPath);
                return newPath;
            }
            catch
            {
                return sourcePath;
            }
        }

        /// <summary>
        /// Tries to determine whether or not a source path
        /// is on a removable device. On failure, it will return
        /// false.
        /// </summary>
        public static bool IsRemovableSource(string sourcePath)
        {
            try
            {
                DriveInfo info = new DriveInfo(Path.GetPathRoot(sourcePath));
                return (info.DriveType == DriveType.Removable || info.DriveType == DriveType.CDRom);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to determine whether or not a source path
        /// is a network location. On failure, it will return
        /// false.
        /// </summary>
        public static bool IsNetworkLocation(string sourcePath)
        {
            try
            {
                if (sourcePath.StartsWith("\\\\")) return true;
                DriveInfo info = new DriveInfo(Path.GetPathRoot(sourcePath));
                return (info.DriveType == DriveType.Network);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Copies files or directories recursively to a
        /// new location. Both arguments must either be of 
        /// type directory or file.
        /// </summary>
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            // Only a file?
            if (File.Exists(sourceFolder))
            {
                string targetDir = Path.GetDirectoryName(destFolder);
                Directory.CreateDirectory(targetDir);
                if (FileCopying != null) FileCopying(null, new GenericEventArgs<string>(sourceFolder));
                File.Copy(sourceFolder, destFolder, true);
                return;
            }

            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                if (m_CancelCopy) break;

                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                if (FileCopying != null) FileCopying(null, new GenericEventArgs<string>(file));
                File.Copy(file, dest);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                if (m_CancelCopy) break;

                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        public static string GetTempRipFile(string desiredFilename)
        {
            try
            {
                string newTempPath = Path.Combine(GetTempRipPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(newTempPath);

                string fileName = string.IsNullOrEmpty(desiredFilename) ? "Track" : desiredFilename;
                fileName += ".wav";

                return Path.Combine(newTempPath, fileName);
            }
            catch
            {
                return Path.GetTempFileName();
            }
        }

        /// <summary>
        /// Returns the full absolute path for a given location and file.
        /// </summary>
        /// <param name="basePath">File or folder to use as base</param>
        /// <param name="file">Absolute or relative path to a file</param>
        public static string GetFullPath(string basePath, string file)
        {
            if (Path.IsPathRooted(file))
            {
                return file;
            }
            else
            {
                return Path.Combine(Path.GetDirectoryName(basePath), file);
            }
        }

        public static bool IsFolderEmpty(string sPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(sPath);
            if (dirInfo.GetFiles().Length == 0 & dirInfo.GetDirectories().Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string AppendToFileName(string append, string filename)
        {
            string path = Path.GetDirectoryName(filename);
            string ext = Path.GetExtension(filename);
            filename = Path.GetFileNameWithoutExtension(filename);
            return Path.Combine(path, filename + append + ext);
        }

        public static string ReplaceInvalidFileNameChars(string text)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char check in invalidChars)
            {
                text = text.Replace(check, '_');
            }
            return text;
        }

        public static bool IsPathDirectory(string strPath)
        {
            try
            {
                FileInfo FileProps = new FileInfo(strPath);
                if (((int)FileProps.Attributes > -1) && (FileProps.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether or not the file has the given extension.
        /// </summary>
        public static bool IsExtension(string fileName, string ext)
        {
            return string.Compare(Path.GetExtension(fileName), "." + ext.TrimStart('.'), true) == 0;
        }

        public static string GetLastPathItem(string strPath)
        {
            return strPath.Substring(strPath.LastIndexOf("\\") + 1);
        }

        public static string QualifyPath(string sPath)
        {
            if (sPath.EndsWith("\\"))
            {
                return sPath;
            }
            else
            {
                return sPath + "\\";
            }
        }

        public static bool FileBusy(string sPath)
        {
            FileStream oFileStream;
            try
            {
                oFileStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    return true;
                }
                else if (ex is UnauthorizedAccessException)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            oFileStream.Close();
            return false;
        }

        public static bool FileBusy(string sPath, ref string sInfo)
        {
            FileStream oFileStream;
            try
            {
                oFileStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);
            }
            catch (IOException ex)
            {
                sInfo = ex.Message;
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                sInfo = ex.Message;
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            oFileStream.Close();
            return false;
        }

        /// <summary>
        /// Simple function to check for some invalid chars in a file name
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>No path!</returns>
        public static bool IsInvalidFileName(string filename)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char check in invalidChars)
            {
                if (filename.Contains(check.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetFileTypeFromExt(string ext)
        {
            RegistryKey rk = Registry.ClassesRoot;
            RegistryKey exta;
            RegistryKey exta2;
            try
            {
                exta = rk.OpenSubKey(ext);
                if (exta == null)
                {
                    return "";
                }
                if (exta.GetValue("") == null)
                {
                    return "";
                }
                exta2 = rk.OpenSubKey((string)exta.GetValue(""));
                if (exta2 == null)
                {
                    return "";
                }
                return exta2.GetValue("") as string;
            }
            catch (SecurityException)
            {
                return "";
            }
        }

        public static void TryDeleteDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path)) return;

                foreach (string subDir in Directory.GetDirectories(path))
                {
                    TryDeleteDirectory(subDir);
                }

                TryDeleteFiles(Directory.GetFiles(path));

                Directory.Delete(path, true);
            }
            catch
            {
                // ignore errors
            }
        }

        public static void TryDeleteFiles(string[] sTempFilePaths)
        {
            if (sTempFilePaths == null) return;

            foreach (string file in sTempFilePaths)
            {
                TryDeleteFiles(file);
            }
        }

        public static void TryDeleteFiles(string file)
        {
            try
            {
                if (string.IsNullOrEmpty(file))
                {
                    return;
                }

                FileInfo info = new FileInfo(file);
                    
                if (info.Exists)
                {
                    info.IsReadOnly = false;
                    File.Delete(file);
                }
            }
            catch
            {
                // don't care
            }
        }

        public static string TryGetFolderPath(System.Environment.SpecialFolder folder)
        {
            // the .NET function fails, if the path is for example set to e: instead of e:\
            string result = string.Empty;

            try
            {
                result = Environment.GetFolderPath(folder);
            }
            catch (ArgumentException)
            {
                // Not so nice. Let's check the registry ourselves.
                string folderName = folder.ToString();
                try
                {
                    RegistryKey key = Registry.CurrentUser;
                    key = key.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders");
                    if ((key != null) & (key.GetValue(folderName) != null))
                    {
                        result = key.GetValue(folderName).ToString().TrimEnd('\\');
                    }
                }
                catch (Exception)
                {
                    // if we can't access registry, we have no choice but to return an empty string
                }
            }

            return result;
        }

        public static long TryGetFileSize(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;

            try
            {
                if (!File.Exists(path)) return 0;

                FileInfo info = new FileInfo(path);
                return info.Length;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
