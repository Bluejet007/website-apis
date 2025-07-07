namespace ImageFilters.Common
{
    internal class ColourSpace
    {
        public static ByteImage SrgbToLinear(ByteImage srgbImage)
        {
            ByteImage linRgbImage = new(srgbImage, false);

            for (int i = srgbImage.Pixels.GetLowerBound(0); i < srgbImage.Pixels.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(srgbImage.Pixels.GetLowerBound(1), srgbImage.Pixels.GetUpperBound(1) + 1, (j) =>
                {
                    for (int ch = srgbImage.Pixels.GetLowerBound(2); ch < srgbImage.Pixels.GetUpperBound(2) + 1; ch++)
                    {
                        float value = srgbImage.Pixels[i, j, ch] / 255f;
                        linRgbImage.Pixels[i, j, ch] = (byte)(MathF.Pow(value, 2.2f) * 255);
                    }
                });
            }

            return linRgbImage;
        }
        public static byte[,] GammaToLinear(byte[,] gammaArr)
        {
            byte[,] linArr = new byte[gammaArr.GetLength(0), gammaArr.GetLength(1)];

            for (int i = gammaArr.GetLowerBound(0); i < gammaArr.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(gammaArr.GetLowerBound(1), gammaArr.GetUpperBound(1) + 1, (j) =>
                {
                    float value = gammaArr[i, j] / 255f;
                    linArr[i, j] = (byte)(MathF.Pow(value, 2.2f) * 255);
                });
            }

            return linArr;
        }

        public static ByteImage LinearToSrgb(ByteImage linRgbImage)
        {
            ByteImage srgbImage = new(linRgbImage, false);

            for (int i = linRgbImage.Pixels.GetLowerBound(0); i < linRgbImage.Pixels.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linRgbImage.Pixels.GetLowerBound(1), linRgbImage.Pixels.GetUpperBound(1) + 1, (j) =>
                {
                    for (int ch = linRgbImage.Pixels.GetLowerBound(2); ch < linRgbImage.Pixels.GetUpperBound(2) + 1; ch++)
                    {
                        float value = linRgbImage.Pixels[i, j, ch] / 255f;
                        srgbImage.Pixels[i, j, ch] = (byte)(MathF.Pow(value, 1 / 2.2f) * 255);
                    }
                });
            }

            return srgbImage;
        }

        public static byte[,] LinearToGamma(byte[,] linArr)
        {
            byte[,] gammaArr = new byte[linArr.GetLength(0), linArr.GetLength(1)];

            for (int i = linArr.GetLowerBound(0); i < linArr.GetUpperBound(0) + 1; i++)
            {
                Parallel.For(linArr.GetLowerBound(1), linArr.GetUpperBound(1) + 1, (j) =>
                {
                    float value = linArr[i, j] / 255f;
                    gammaArr[i, j] = (byte)(MathF.Pow(value, 1 / 2.2f) * 255);
                });
            }

            return gammaArr;
        }
    }
}