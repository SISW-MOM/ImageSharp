﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xunit;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Tests.Formats.Bmp
{
    using static TestImages.Bmp;

    public class BmpEncoderTests : FileTestBase
    {
        public static readonly TheoryData<BmpBitsPerPixel> BitsPerPixel =
        new TheoryData<BmpBitsPerPixel>
        {
            BmpBitsPerPixel.Pixel24,
            BmpBitsPerPixel.Pixel32
        };

        public static readonly TheoryData<string, int, int, PixelResolutionUnit> RatioFiles =
        new TheoryData<string, int, int, PixelResolutionUnit>
        {
            { Car, 3780, 3780 , PixelResolutionUnit.PixelsPerMeter },
            { V5Header, 3780, 3780 , PixelResolutionUnit.PixelsPerMeter },
            { RLE8, 2835, 2835, PixelResolutionUnit.PixelsPerMeter }
        };

        public static readonly TheoryData<string, BmpBitsPerPixel> BmpBitsPerPixelFiles =
        new TheoryData<string, BmpBitsPerPixel>
        {
            { Car, BmpBitsPerPixel.Pixel24 },
            { Bit32Rgb, BmpBitsPerPixel.Pixel32 }
        };

        public BmpEncoderTests(ITestOutputHelper output) => this.Output = output;

        private ITestOutputHelper Output { get; }

        [Theory]
        [MemberData(nameof(RatioFiles))]
        public void Encode_PreserveRatio(string imagePath, int xResolution, int yResolution, PixelResolutionUnit resolutionUnit)
        {
            var options = new BmpEncoder();

            var testFile = TestFile.Create(imagePath);
            using (Image<Rgba32> input = testFile.CreateImage())
            {
                using (var memStream = new MemoryStream())
                {
                    input.Save(memStream, options);

                    memStream.Position = 0;
                    using (var output = Image.Load<Rgba32>(memStream))
                    {
                        ImageMetadata meta = output.Metadata;
                        Assert.Equal(xResolution, meta.HorizontalResolution);
                        Assert.Equal(yResolution, meta.VerticalResolution);
                        Assert.Equal(resolutionUnit, meta.ResolutionUnits);
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(BmpBitsPerPixelFiles))]
        public void Encode_PreserveBitsPerPixel(string imagePath, BmpBitsPerPixel bmpBitsPerPixel)
        {
            var options = new BmpEncoder();

            var testFile = TestFile.Create(imagePath);
            using (Image<Rgba32> input = testFile.CreateImage())
            {
                using (var memStream = new MemoryStream())
                {
                    input.Save(memStream, options);

                    memStream.Position = 0;
                    using (var output = Image.Load<Rgba32>(memStream))
                    {
                        BmpMetadata meta = output.Metadata.GetFormatMetadata(BmpFormat.Instance);

                        Assert.Equal(bmpBitsPerPixel, meta.BitsPerPixel);
                    }
                }
            }
        }

        [Theory]
        [WithTestPatternImages(nameof(BitsPerPixel), 24, 24, PixelTypes.Rgba32 | PixelTypes.Bgra32 | PixelTypes.Rgb24)]
        public void Encode_IsNotBoundToSinglePixelType<TPixel>(TestImageProvider<TPixel> provider, BmpBitsPerPixel bitsPerPixel)
            where TPixel : struct, IPixel<TPixel> => TestBmpEncoderCore(provider, bitsPerPixel);

        [Theory]
        [WithTestPatternImages(nameof(BitsPerPixel), 48, 24, PixelTypes.Rgba32)]
        [WithTestPatternImages(nameof(BitsPerPixel), 47, 8, PixelTypes.Rgba32)]
        [WithTestPatternImages(nameof(BitsPerPixel), 49, 7, PixelTypes.Rgba32)]
        [WithSolidFilledImages(nameof(BitsPerPixel), 1, 1, 255, 100, 50, 255, PixelTypes.Rgba32)]
        [WithTestPatternImages(nameof(BitsPerPixel), 7, 5, PixelTypes.Rgba32)]
        public void Encode_WorksWithDifferentSizes<TPixel>(TestImageProvider<TPixel> provider, BmpBitsPerPixel bitsPerPixel)
            where TPixel : struct, IPixel<TPixel> => TestBmpEncoderCore(provider, bitsPerPixel);

        [Theory]
        [WithFile(Bit32Rgb, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        [WithFile(Bit32Rgba, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        [WithFile(WinBmpv4, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        [WithFile(WinBmpv5, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        // WinBmpv3 is a 24 bits per pixel image
        [WithFile(WinBmpv3, PixelTypes.Rgb24, BmpBitsPerPixel.Pixel24)]
        [WithFile(Rgb16, PixelTypes.Bgra5551, BmpBitsPerPixel.Pixel16)]
        [WithFile(Bit16, PixelTypes.Bgra5551, BmpBitsPerPixel.Pixel16)]
        public void Encode_WithV3Header_Works<TPixel>(TestImageProvider<TPixel> provider, BmpBitsPerPixel bitsPerPixel)
            // if supportTransparency is false, a v3 bitmap header will be written
            where TPixel : struct, IPixel<TPixel> => TestBmpEncoderCore(provider, bitsPerPixel, supportTransparency: false);

        [Theory]
        [WithFile(Bit32Rgb, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        [WithFile(Bit32Rgba, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        [WithFile(WinBmpv4, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        [WithFile(WinBmpv5, PixelTypes.Rgba32 | PixelTypes.Rgb24, BmpBitsPerPixel.Pixel32)]
        [WithFile(WinBmpv3, PixelTypes.Rgb24, BmpBitsPerPixel.Pixel24)]
        [WithFile(Rgb16, PixelTypes.Bgra5551, BmpBitsPerPixel.Pixel16)]
        [WithFile(Bit16, PixelTypes.Bgra5551, BmpBitsPerPixel.Pixel16)]
        public void Encode_WithV4Header_Works<TPixel>(TestImageProvider<TPixel> provider, BmpBitsPerPixel bitsPerPixel)
            where TPixel : struct, IPixel<TPixel> => TestBmpEncoderCore(provider, bitsPerPixel, supportTransparency: true);

        [Theory]
        [WithFile(TestImages.Png.GrayAlpha2BitInterlaced, PixelTypes.Rgba32, BmpBitsPerPixel.Pixel32)]
        public void Encode_PreservesAlpha<TPixel>(TestImageProvider<TPixel> provider, BmpBitsPerPixel bitsPerPixel)
            where TPixel : struct, IPixel<TPixel> => TestBmpEncoderCore(provider, bitsPerPixel, supportTransparency: true);

        private static void TestBmpEncoderCore<TPixel>(TestImageProvider<TPixel> provider, BmpBitsPerPixel bitsPerPixel, bool supportTransparency = true)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                // There is no alpha in bmp with 24 bits per pixels, so the reference image will be made opaque.
                if (bitsPerPixel == BmpBitsPerPixel.Pixel24)
                {
                    image.Mutate(c => c.MakeOpaque());
                }

                var encoder = new BmpEncoder { BitsPerPixel = bitsPerPixel, SupportTransparency = supportTransparency };

                // Does DebugSave & load reference CompareToReferenceInput():
                image.VerifyEncoder(provider, "bmp", bitsPerPixel, encoder);
            }
        }
    }
}