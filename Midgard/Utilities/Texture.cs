using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace Midgard.Utilities
{
    public class Texture
    {
        public static string Hash(Bitmap image)
        {
            var sha256 = SHA256.Create();
            var width = image.Width;
            var height = image.Height;
            var buffer = new byte[width * height * 4 + 8];

            PutInt(buffer, 0, width);
            PutInt(buffer, 4, height);
            var pos = 8;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = image.GetPixel(x, y);
                    PutInt(buffer, pos, pixel.ToArgb());
                    if (buffer[pos + 0] == 0) 
                    {
                        buffer[pos + 1] = buffer[pos + 2] = buffer[pos + 3] = 0;
                    }
                    pos += 4;
                    if (pos == buffer.Length) 
                    {
                        pos = 0;
                    }
                }
            }

            var bytes = sha256.ComputeHash(buffer);
            var result = string.Empty;
            foreach (var t in bytes)
            {
                result += t.ToString("X2");
            }
            return result.ToLower();
        }
        
        private static void PutInt(byte[] array, int offset, int x) {
            array[offset + 0] = (byte) (x >> 24 & 0xff);
            array[offset + 1] = (byte) (x >> 16 & 0xff);
            array[offset + 2] = (byte) (x >> 8 & 0xff);
            array[offset + 3] = (byte) (x >> 0 & 0xff);
        }

        public static bool Check(FileStream stream)
        {
            using var image = Image.FromStream(stream, validateImageData: false, useEmbeddedColorManagement: false);
            var width = image.PhysicalDimension.Width;
            var height = image.PhysicalDimension.Height;
            return !(width >= 4096) || !(height >= 4096);
        }
    }
}