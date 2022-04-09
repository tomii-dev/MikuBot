using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MikuBot.Services
{
    static class FFmpeg
    {
        public static Process CreateStream(string path)
        {
            string fileName = "ffmpeg.exe";

            if (Environment.OSVersion.ToString().Contains("Unix")) fileName = "ffmpeg";

            return Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"-hide_banner -loglevel panic -i {path} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }
    }
}
