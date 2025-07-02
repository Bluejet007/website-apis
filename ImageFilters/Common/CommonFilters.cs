using System.Drawing.Imaging;

namespace ImageFilters.Common
{
    internal class CommonFilters
    {
        public static ByteImage SrgbToLinear(ByteImage srgbMap)
        {
            ByteImage linearRgbMap = new ByteImage(srgbMap, false);

            for(int i = srgbMap.PixelMap.GetLowerBound(0); i < srgbMap.PixelMap.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(srgbMap.PixelMap.GetLowerBound(1), srgbMap.PixelMap.GetUpperBound(1) + 1, (j) =>
                {
                    for(int ch = srgbMap.PixelMap.GetLowerBound(2); ch < srgbMap.PixelMap.GetUpperBound(2) + 1; ch++)
                    {
                        float value = srgbMap.PixelMap[i, j, ch] / 255f;
                        linearRgbMap.PixelMap[i, j, ch] = (byte)(MathF.Pow(value, 2.2f) * 255);
                    }
                });
            }

            return linearRgbMap;
        }
        public static byte[,] GammaToLinear(byte[,] gammaMap)
        {
            byte[,] linearMap = new byte[gammaMap.GetLength(0), gammaMap.GetLength(1)];

            for (int i = gammaMap.GetLowerBound(0); i < gammaMap.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(gammaMap.GetLowerBound(1), gammaMap.GetUpperBound(1) + 1, (j) =>
                {
                    float value = gammaMap[i, j] / 255f;
                    linearMap[i, j] = (byte)(MathF.Pow(value, 2.2f) * 255);
                });
            }

            return linearMap;
        }

        public static ByteImage LinearToSrgb(ByteImage linearRgbMap)
        {
            ByteImage srgbMap = new ByteImage(linearRgbMap, false);

            for(int i = linearRgbMap.PixelMap.GetLowerBound(0); i < linearRgbMap.PixelMap.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linearRgbMap.PixelMap.GetLowerBound(1), linearRgbMap.PixelMap.GetUpperBound(1) + 1, (j) =>
                {
                    for(int ch = linearRgbMap.PixelMap.GetLowerBound(2); ch < linearRgbMap.PixelMap.GetUpperBound(2) + 1; ch++)
                    {
                        float value = linearRgbMap.PixelMap[i, j, ch] / 255f;
                        srgbMap.PixelMap[i, j, ch] = (byte)(MathF.Pow(value, 1 / 2.2f) * 255);
                    }
                });
            }

            return srgbMap;
        }

        public static byte[,] LinearToGamma(byte[,] linearMap)
        {
            byte[,] gammaMap = new byte[linearMap.GetLength(0), linearMap.GetLength(1)];

            for (int i = linearMap.GetLowerBound(0); i < linearMap.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linearMap.GetLowerBound(1), linearMap.GetUpperBound(1) + 1, (j) =>
                {
                    float value = linearMap[i, j] / 255f;
                    gammaMap[i, j] = (byte)(MathF.Pow(value, 1 / 2.2f) * 255);
                });
            }

            return gammaMap;
        }

        public static ByteImage GreyScale(ByteImage rgbMap)
        {
            ByteImage linearMap = SrgbToLinear(rgbMap);
            ByteImage valueMap = new ByteImage(linearMap.Height, linearMap.Width, linearMap.Type, PixelFormat.Format24bppRgb);

            for(int i = linearMap.PixelMap.GetLowerBound(0); i < linearMap.PixelMap.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linearMap.PixelMap.GetLowerBound(1), linearMap.PixelMap.GetUpperBound(1) + 1, (j) =>
                {
                    byte value = (byte)((linearMap.PixelMap[i, j, 2] * 54 + linearMap.PixelMap[i, j, 1] * 182 + linearMap.PixelMap[i, j, 0] * 19) / 255);
                    valueMap.PixelMap[i, j, 0] = valueMap.PixelMap[i, j, 1] = valueMap.PixelMap[i, j, 2] = value;
                });
            }

            return LinearToSrgb(valueMap);
        }

        public static byte[,] SimpleGreyScale(ByteImage rgbMap)
        {
            ByteImage linearMap = SrgbToLinear(rgbMap);
            byte[,] valueMap = new byte[linearMap.Height, linearMap.Width];

            for (int i = linearMap.PixelMap.GetLowerBound(0); i < linearMap.PixelMap.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linearMap.PixelMap.GetLowerBound(1), linearMap.PixelMap.GetUpperBound(1) + 1, (j) =>
                    valueMap[i, j] = (byte)((linearMap.PixelMap[i, j, 2] * 54 + linearMap.PixelMap[i, j, 1] * 182 + linearMap.PixelMap[i, j, 0] * 19) / 255));
            }

            return LinearToGamma(valueMap);
        }
    }
}
