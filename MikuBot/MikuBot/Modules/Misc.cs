using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace MikuBot.Modules
{
    struct RGB
    {
        public float r, g, b;
        private float total()
        {
            return r + g + b;
        }
        public RGB(float _r, float _g, float _b)
        {
            r = _r;
            g = _g;
            b = _b;
        }
        public RGB(Color color)
        {
            r = color.R;
            g = color.G;
            b = color.B;
        }
        public Color ToColor()
        {
            return Color.FromArgb(255, (int)r, (int)g, (int)b);
        }
        public static RGB operator -(RGB col1, RGB col2)
        {
            return new RGB(
                col1.r - col2.r,
                col1.g - col2.g,
                col1.b - col2.b);
        }

        public static RGB operator /(RGB col1, RGB col2)
        {
            return new RGB(
                col1.r / col2.r,
                col1.g / col2.g,
                col1.b / col2.b);
        }
        public static RGB operator +(RGB col1, RGB col2)
        {
            return new RGB(
                col1.r + col2.r,
                col1.g + col2.g,
                col1.b + col2.b);
        }

        public static bool operator >(RGB col1, RGB col2)
        {
            return col1.r > col2.r && col1.g > col2.g && col1.b > col2.b;
        }

        public static bool operator <(RGB col1, RGB col2)
        {
            return col1.total() > col2.total();
        }

    }

    class Misc : ModuleBase
    {
        private static DiscordSocketClient _client;
        public Misc(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }
        static RGB RandomColor()
        {
            Random gen = new Random();
            return new RGB(gen.Next(0, 255), gen.Next(0, 255), gen.Next(0, 255));
        }

        [Command("gradient", RunMode=RunMode.Async)]
        public async Task Gradient(int count)
        {
            Bitmap bmp = new Bitmap(600, 600);
            float size = bmp.Height * bmp.Width;
            RGB col1 = RandomColor();
            RGB col2 = RandomColor();
            RGB inc = col1 > col2 ?
                (col1 - col2) / new RGB(size, size, size) :
                (col2 - col1) / new RGB(size, size, size);
            RGB col = col1;
            for (int i = 0; i < bmp.Height; ++i)
                for (int j = 0; j < bmp.Width; ++j)
                {
                    col = col + inc;
                    bmp.SetPixel(i, j, col.ToColor());
                }
            if (!Directory.Exists("tmp"))
                Directory.CreateDirectory("tmp");
            string path = $"tmp/gradient{Context.Message.Id}.jpeg";
            bmp.Save(path);
            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }
    }
}
