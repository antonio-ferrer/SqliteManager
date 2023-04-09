using System;
using System.IO;
using System.Threading;

namespace FileHelper
{
    public static class FileOperations
    {

        public static bool CheckFileIsLocked(string path)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
            return false;
        }

        public static bool CheckFileIsLocked(this FileInfo file)
        {
            return CheckFileIsLocked(file.FullName);
        }

        public static void WaitForReleaseFile(string path, TimeSpan timeout, bool throwExceptionWhenIsLocked = false)
        {
            DateTime maxTime = DateTime.Now.Add(timeout);
            bool isLocked;
            do
            {
                isLocked = CheckFileIsLocked(path);
                Thread.Sleep(250);
            }
            while (isLocked && DateTime.Now < maxTime);
            if (throwExceptionWhenIsLocked && DateTime.Now > maxTime)
                throw new TimeoutException($"timout waiting for {path}");
        }

    }
}
