using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFilters
{
    public class ByteImage
    {
        public byte[,,] PixelMap {  get; set; }
        public PixelFormat Format { get; }
        public ImageFormat Type { get; }
        public bool IsAlpha
        {
            get
            {
                return Format switch
                {
                    PixelFormat.Format16bppArgb1555 => true,
                    PixelFormat.Format32bppArgb => true,
                    PixelFormat.Format32bppPArgb => true,
                    PixelFormat.Format64bppArgb => true,
                    PixelFormat.Format64bppPArgb => true,
                    PixelFormat.Alpha => true,
                    PixelFormat.PAlpha => true,
                    _ => false
                };
            }
        }
        public int ChannelCount { get { return PixelMap.GetLength(2); } }

        public ByteImage(Stream stream)
        {
            Bitmap bmp = new Bitmap(stream);
            this.Format = bmp.PixelFormat;
            this.Type = bmp.RawFormat;

            int bpp = this.Format switch
            {
                PixelFormat.Format24bppRgb => 3,
                PixelFormat.Format32bppRgb => 4,
                PixelFormat.Format32bppArgb => 4,
                PixelFormat.Format32bppPArgb => 4,
                _ => throw new NotImplementedException($"Unsupported pixel format: {bmp.PixelFormat}")
            };

            Rectangle rect = new(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            int byteCount = System.Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] input = new byte[byteCount];
            Marshal.Copy(bmpData.Scan0, input, 0, byteCount);
            bmp.UnlockBits(bmpData);

            this.PixelMap = new byte[bmp.Height, bmp.Width, bpp];
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    for (int z = 0; z < bpp; z++)
                        this.PixelMap[y, x, z] = input[bmpData.Stride * y + x * bpp + (bpp - z - 1)];
        }

        public ByteImage(ByteImage original)
        {
            this.PixelMap = original.PixelMap;
            this.Format = original.Format;
            this.Type = original.Type;
        }

        public MemoryStream ToMemoryStream()
        {
            int height = PixelMap.GetLength(0);
            int width = PixelMap.GetLength(1);
            int bpp = PixelMap.GetLength(2);

            PixelFormat bmpFormat = Format;

            int stride = 4 * ((width * bpp + 3) / 4);
            byte[] output = new byte[height * stride];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    for (int z = 0; z < bpp; z++)
                        output[y * stride + x * bpp + (bpp - z - 1)] = PixelMap[y, x, z];

            Bitmap bmp = new(width, height, bmpFormat);
            var rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmpFormat);
            Marshal.Copy(output, 0, bmpData.Scan0, output.Length);
            bmp.UnlockBits(bmpData);

            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, this.Type);

            return stream;
        }
    }
}
