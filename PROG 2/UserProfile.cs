using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CybersecurityBotWPF
{
    public static class AudioManager
    {
        public static void PlayGreeting()
        {
            try
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return;

                string audioPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

                if (File.Exists(audioPath))
                {
                    using var player = new System.Media.UserProfile(audioPath);
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AudioManager.PlayGreeting error: {ex}");
            }
            catch { }
        }
    }
}