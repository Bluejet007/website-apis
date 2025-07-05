using ImageFilters.Common;
using Microsoft.Extensions.Logging;

namespace ImageFilters.Kuwahara
{
    internal class KuwaharaFilters
    {
        public static ByteImage BaseKuwahara(ByteImage linRgbImage, byte subKernelSize, ILogger<JobProcessor> logger)
        {
            byte[,] valueArr = CommonFilters.ByteGreyScale(linRgbImage);

            logger.LogInformation("Creating means");
            KernelMap meanMap = new(valueArr.GetLength(0), valueArr.GetLength(1), subKernelSize, (i, j) =>
            {
                uint mean = 0;

                for (byte p = 0; p < subKernelSize; p++)
                {
                    for (byte q = 0; q < subKernelSize; q++)
                    {
                        int a = KernelMap.BoundReflected(i + p, valueArr.GetLength(0));
                        int b = KernelMap.BoundReflected(j + q, valueArr.GetLength(1));

                        mean += valueArr[a, b];
                    }
                }

                return (byte)(mean / (subKernelSize * subKernelSize));
            });
            logger.LogInformation("Means completed");

            logger.LogInformation("Creating variances");
            KernelMap varianceMap = new(valueArr.GetLength(0), valueArr.GetLength(1), subKernelSize, (i, j) =>
            {
                int mean = meanMap.Get(i, j);
                uint variance = 0;

                for (byte p = 0; p < subKernelSize; p++)
                {
                    for (byte q = 0; q < subKernelSize; q++)
                    {
                        int a = KernelMap.BoundReflected(i + p, valueArr.GetLength(0));
                        int b = KernelMap.BoundReflected(j + q, valueArr.GetLength(1));

                        int temp = valueArr[a, b] - mean;
                        variance += (uint)(temp * temp);
                    }
                }

                return (int)(variance / (subKernelSize * subKernelSize));
            });
            logger.LogInformation("Variances completed");

            logger.LogInformation("Creating final means");
            ByteImage linFinalImage = GetMeanImage(linRgbImage, varianceMap, subKernelSize);
            logger.LogInformation("Final means made");

            return linFinalImage;
        }

        public static ByteImage GetMeanImage(ByteImage linRgbImage, KernelMap varianceMap, byte subKernelSize)
        {
            ByteImage linResImage = new(linRgbImage, false);

            int[] quads =
            [
                1 - subKernelSize, 1 - subKernelSize,
                0, 0,
                1 - subKernelSize, 0,
                0, 1 - subKernelSize
            ];

            for (int i = 0; i < linResImage.Height; i++)
            {
                Parallel.For(0, linResImage.Width, (j) =>
                {
                    int ind = 0;
                    for (int v = 1; v < 4; v++)
                    {
                        if (varianceMap.Get(i + quads[ind], j + quads[ind + 1]) > varianceMap.Get(i + quads[v], j + quads[v + 1]))
                            ind = v;
                    }

                    for (byte k = 0; k < linResImage.ChannelCount; k++)
                    {
                        uint sum = GetQuadrantSum(linRgbImage, i + quads[ind], j + quads[ind + 1], k, subKernelSize);
                        linResImage.Pixels[i, j, k] = (byte)(sum / (subKernelSize * subKernelSize));
                    }
                });
            }

            return linResImage;
        }

        private static uint GetQuadrantSum(ByteImage image, int i, int j, byte channelIndex, int size)
        {
            uint sum = 0;

            for (int p = i; p < i + size; p++)
            {
                for (int q = j; q < j + size; q++)
                {
                    int a = KernelMap.BoundReflected(p, image.Height);
                    int b = KernelMap.BoundReflected(q, image.Width);

                    sum += image.Pixels[a, b, channelIndex];
                }
            }

            return sum;
        }

        private static uint GetQuadrantSum(ByteImage image, int i, int j, byte channelIndex, int height, int width)
        {
            uint sum = 0;

            for (int p = i; p < i + height; p++)
            {
                for (int q = j; q < j + width; q++)
                {
                    int a = KernelMap.BoundReflected(p, image.Height);
                    int b = KernelMap.BoundReflected(q, image.Width);

                    sum += image.Pixels[a, b, channelIndex];
                }
            }

            return sum;
        }
    }
}