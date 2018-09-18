using System;
using System.IO;
using UnityEngine;

namespace Zenject.ReflectionBaking
{
    public class ReflectionBakingSaveData
    {
        static string _savePath;

        static ReflectionBakingSaveData()
        {
            _savePath = Path.Combine(
                Application.dataPath, "../Library/ZenjectReflectionBakingRunner.dat");
        }

        public static void ClearLastUpdateTime()
        {
            if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
            }
        }

        public static void SaveUpdateTime(long time)
        {
            File.WriteAllText(_savePath, time.ToString());
        }

        public static long GetLastUpdateTime()
        {
            if (!File.Exists(_savePath))
            {
                return 0;
            }

            return long.Parse(File.ReadAllText(_savePath));
        }
    }
}
