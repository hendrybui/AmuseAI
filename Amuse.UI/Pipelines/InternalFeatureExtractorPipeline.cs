using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime.Tensors;
using OnnxStack.Core.Image;
using OnnxStack.Core.Video;
using OnnxStack.FeatureExtractor.Common;
using OnnxStack.FeatureExtractor.Pipelines;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Size = OpenCvSharp.Size;

namespace Amuse.UI.Pipelines
{
    public class InternalFeatureExtractorPipeline : FeatureExtractorPipeline
    {
        public InternalFeatureExtractorPipeline(string name, FeatureExtractorModel featureExtractorModel, ILogger logger = null)
            : base(name, featureExtractorModel, logger)
        {
        }

        public override Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task UnloadAsync()
        {
            return Task.CompletedTask;
        }

        protected override async Task<OnnxImage> ExtractImageAsync(OnnxImage inputImage, FeatureExtractorOptions options, CancellationToken cancellationToken = default)
        {
            if (Name.Equals("Canny"))
                return await ExtractCannyAsync(inputImage, options);
            if (Name.Equals("SoftEdge"))
                return await ExtractSoftEdgeAsync(inputImage, options);
            return inputImage;
        }

        protected override Task<DenseTensor<float>> ExtractTensorAsync(DenseTensor<float> imageTensor, FeatureExtractorOptions options, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                using (var inputImage = new OnnxImage(imageTensor))
                using (var imageResult = await ExtractImageAsync(inputImage, options, cancellationToken))
                {
                    return await imageResult.GetImageTensorAsync();
                }
            });
        }


        private Task<OnnxImage> ExtractCannyAsync(OnnxImage source, FeatureExtractorOptions options)
        {
            return Task.Run(() =>
            {
                double lower = 0.33 * Math.Max(1, options.Value);
                double upper = 0.66 * Math.Max(1, options.Value);
                using (var input = VideoHelper.ImageToMat(source))
                {
                    using (var gray = new Mat())
                    {
                        Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY); // Convert to grayscale
                        using (var blurred = new Mat())
                        {
                            Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 1.4); // Apply Gaussian blur
                            using (var edges = new Mat())
                            {
                                Cv2.Canny(blurred, edges, lower, upper);  // Apply Canny edge detection

                                using (var result = new Image<Rgba32>(edges.Width, edges.Height))
                                {
                                    edges.GetArray(out byte[] edgeBytes);

                                    result.ProcessPixelRows(accessor =>
                                    {
                                        for (int y = 0; y < result.Height; y++)
                                        {
                                            Span<Rgba32> row = accessor.GetRowSpan(y);
                                            for (int x = 0; x < result.Width; x++)
                                            {
                                                byte val = edgeBytes[y * result.Width + x];
                                                row[x] = new Rgba32(val, val, val, 255);
                                            }
                                        }
                                    });

                                    return Task.FromResult(new OnnxImage(result));
                                }
                            }
                        }
                    }
                }
            });
        }


        private Task<OnnxImage> ExtractSoftEdgeAsync(OnnxImage source, FeatureExtractorOptions options)
        {
            var input = source.Clone();
            var edgeDetector = GetEdgeDetector(options);
            input.GetImage().Mutate(edgeDetector);
            return Task.FromResult(input);
        }


        private Action<IImageProcessingContext> GetEdgeDetector(FeatureExtractorOptions options)
        {
            var value = (int)options.Value;
            return value switch
            {
                1 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Kayyali),
                2 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Kirsch),
                3 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Laplacian3x3),
                4 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Laplacian5x5),
                5 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.LaplacianOfGaussian),
                6 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Prewitt),
                7 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.RobertsCross),
                8 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Robinson),
                9 => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Scharr),
                _ => (ctx) => ctx.DetectEdges(KnownEdgeDetectorKernels.Sobel),
            };
        }
    }
}
