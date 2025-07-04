using System.Drawing.Imaging;

namespace ImageFilters.Common
{
    internal class CommonFilters
    {
        public static ByteImage GreyScale(ByteImage linRgbImage)
        {
            ByteImage valueImage = new ByteImage(linRgbImage.Height, linRgbImage.Width, linRgbImage.Type, PixelFormat.Format24bppRgb);

            for(int i = linRgbImage.Pixels.GetLowerBound(0); i < linRgbImage.Pixels.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linRgbImage.Pixels.GetLowerBound(1), linRgbImage.Pixels.GetUpperBound(1) + 1, (j) =>
                {
                    byte value = (byte)((linRgbImage.Pixels[i, j, 2] * 54 + linRgbImage.Pixels[i, j, 1] * 182 + linRgbImage.Pixels[i, j, 0] * 19) / 255);
                    valueImage.Pixels[i, j, 0] = valueImage.Pixels[i, j, 1] = valueImage.Pixels[i, j, 2] = value;
                });
            }

            return valueImage;
        }

        public static byte[,] ByteGreyScale(ByteImage linRgbImage)
        {
            byte[,] valueArr = new byte[linRgbImage.Height, linRgbImage.Width];

            for (int i = linRgbImage.Pixels.GetLowerBound(0); i < linRgbImage.Pixels.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linRgbImage.Pixels.GetLowerBound(1), linRgbImage.Pixels.GetUpperBound(1) + 1, (j) =>
                    valueArr[i, j] = (byte)((linRgbImage.Pixels[i, j, 2] * 54 + linRgbImage.Pixels[i, j, 1] * 182 + linRgbImage.Pixels[i, j, 0] * 19) / 255));
            }

            return valueArr;
        }
    }
}
