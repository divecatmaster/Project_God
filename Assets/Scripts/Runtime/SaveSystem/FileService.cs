using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace DiveCat.SaveSystem
{
    public static class FileService
    {
        /// <summary>
        /// Saves data to a file using an atomic write strategy.
        /// Writes to a temporary file first, then swaps.
        /// </summary>
        public static async Task SaveAsync(string filePath, string content)
        {
            string tempPath = filePath + ".tmp";
            string backupPath = filePath + ".bak";

            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                // Write to temp file
                using (StreamWriter writer = new StreamWriter(tempPath))
                {
                    await writer.WriteAsync(content);
                }

                // Atomic swap
                if (File.Exists(filePath))
                {
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                    File.Move(filePath, backupPath);
                }

                File.Move(tempPath, filePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FileService] Failed to save file at {filePath}: {ex.Message}");
                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }
        }

        public static async Task<string> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // Try recovery from backup if main file is missing
                string backupPath = filePath + ".bak";
                if (File.Exists(backupPath))
                {
                    Debug.LogWarning($"[FileService] Primary save missing, recovering from backup: {backupPath}");
                    File.Copy(backupPath, filePath);
                }
                else
                {
                    return null;
                }
            }

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FileService] Failed to load file at {filePath}: {ex.Message}");
                return null;
            }
        }

        public static void Delete(string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            if (File.Exists(filePath + ".bak")) File.Delete(filePath + ".bak");
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(filePath) || File.Exists(filePath + ".bak");
        }
    }
}
