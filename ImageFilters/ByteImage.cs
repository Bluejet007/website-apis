using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFilters
{
    public class ByteImage
    {
        public byte[,,] PixelMap { get; set; }
        public PixelFormat Format { get; }
        public ImageFormat Type { get; }
        public int Height { get { return PixelMap.GetLength(0); } }
        public int Width { get { return PixelMap.GetLength(1); } }
        public int ChannelCount { get { return PixelMap.GetLength(2); } }
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

        public ByteImage(Stream stream)
        {
            Bitmap bmp = new Bitmap(stream);
            this.Format = bmp.PixelFormat;
            this.Type = bmp.RawFormat;

            int bpp = GetChannelCountFromFormat(this.Format);

            Rectangle rect = new(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            int byteCount = System.Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] input = new byte[byteCount];
            Marshal.Copy(bmpData.Scan0, input, 0, byteCount);
            bmp.UnlockBits(bmpData);

            this.PixelMap = new byte[bmp.Height, bmp.Width, bpp];
            ParallelOptions parallelOptions = new ParallelOptions();

            for (int y = PixelMap.GetLowerBound(0); y < PixelMap.GetUpperBound(0) + 1; y++)
            {
                Parallel.For(PixelMap.GetLowerBound(1), PixelMap.GetUpperBound(1) + 1, parallelOptions, (x) =>
                {
                    for (int z = PixelMap.GetLowerBound(2); z < PixelMap.GetUpperBound(2) + 1; z++)
                        this.PixelMap[y, x, z] = input[bmpData.Stride * y + x * bpp + (bpp - z - 1)];
                });
            }
        }

        public ByteImage(ByteImage original, bool copyData = true)
        {
            this.Format = original.Format;
            this.Type = original.Type;
            this.PixelMap = copyData ? (byte[,,])original.PixelMap.Clone() : new byte[original.Height, original.Width, original.ChannelCount];
        }

        public ByteImage(int height, int width, ImageFormat type, PixelFormat format = PixelFormat.Format24bppRgb)
        {
            this.Format = format;
            this.Type = type;
            this.PixelMap = new byte[height, width, GetChannelCountFromFormat(format)];
        }

        public MemoryStream ToMemoryStream()
        {
            PixelFormat bmpFormat = Format;

            int stride = 4 * ((this.Width * ChannelCount + 3) / 4);
            byte[] output = new byte[Height * stride];
            for (int y = PixelMap.GetLowerBound(0); y < PixelMap.GetUpperBound(0) + 1; y++)
            {
                Parallel.For(PixelMap.GetLowerBound(1), PixelMap.GetUpperBound(1) + 1, (x) =>
                {
                    for (int z = PixelMap.GetLowerBound(2); z < PixelMap.GetUpperBound(2) + 1; z++)
                        output[y * stride + x * this.ChannelCount + (this.ChannelCount - z - 1)] = PixelMap[y, x, z];
                });
            }

            Bitmap bmp = new(this.Width, this.Height, bmpFormat);
            var rect = new Rectangle(0, 0, this.Width, this.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmpFormat);
            Marshal.Copy(output, 0, bmpData.Scan0, output.Length);
            bmp.UnlockBits(bmpData);

            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, this.Type);

            return stream;
        }

        public byte[,] GetChannel(byte channelIndex)
        {
            byte[,] channel = new byte[this.Height, this.Width];

            for (int i = PixelMap.GetLowerBound(0); i < PixelMap.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(PixelMap.GetLowerBound(1), PixelMap.GetUpperBound(1) + 1, (j) =>
                    channel[i, j] = PixelMap[i, j, channelIndex]);
            }

            return channel;
        }

        public static int GetChannelCountFromFormat(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.Format16bppGrayScale => 1,
                PixelFormat.Format24bppRgb => 3,
                PixelFormat.Format32bppRgb => 4,
                PixelFormat.Format32bppArgb => 4,
                PixelFormat.Format32bppPArgb => 4,
                _ => throw new NotImplementedException($"Unsupported pixel format: {format}")
            };
        }
    }
}