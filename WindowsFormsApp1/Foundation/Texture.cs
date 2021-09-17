using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.DXGI;
using System.Runtime.InteropServices;
using SharpDX.WIC;

namespace WindowsFormsApp1.Foundation
{
    public class Texture                   // 31 lines
    {
        // Propertues
        public ShaderResourceView TextureResource { get; private set; }

        // Methods.
        public bool Init(SharpDX.Direct3D11.Device device, string fileName)
        {
            try
            {
                using (var texture = LoadFromFile(device, new SharpDX.WIC.ImagingFactory(), fileName))
                {
                    ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription()
                    {
                        Format = texture.Description.Format,
                        Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                    };
                    srvDesc.Texture2D.MostDetailedMip = 0;
                    srvDesc.Texture2D.MipLevels = -1;

                    TextureResource = new ShaderResourceView(device, texture, srvDesc);
                    device.ImmediateContext.GenerateMips(TextureResource);
                }
                // TextureResource = ShaderResourceView.FromFile(device, fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Destroy()
        {
            TextureResource?.Dispose();
            TextureResource = null;
        }

        public Texture2D LoadFromFile(SharpDX.Direct3D11.Device device, ImagingFactory factory, string fileName)
        {
            using (var bs = LoadBitmap(factory, fileName))
                return CreateTexture2DFromBitmap(device, bs);
        }
        public BitmapSource LoadBitmap(ImagingFactory factory, string filename)
        {
            var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(
                factory,
                filename,
                SharpDX.WIC.DecodeOptions.CacheOnDemand
                );

            var result = new SharpDX.WIC.FormatConverter(factory);

            result.Initialize(
                bitmapDecoder.GetFrame(0),
                SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                SharpDX.WIC.BitmapDitherType.None,
                null,
                0.0,
                SharpDX.WIC.BitmapPaletteType.Custom);

            return result;
        }
        public Texture2D CreateTexture2DFromBitmap(SharpDX.Direct3D11.Device device, BitmapSource bitmapSource)
        {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmapSource.Size.Width * 4;
            using (var buffer = new SharpDX.DataStream(bitmapSource.Size.Height * stride, true, true))
            {
                // Copy the content of the WIC to the buffer
                bitmapSource.CopyPixels(stride, buffer);
                return new SharpDX.Direct3D11.Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = bitmapSource.Size.Width,
                    Height = bitmapSource.Size.Height,
                    ArraySize = 1,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps, // ResourceOptionFlags.GenerateMipMap
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                }, new SharpDX.DataRectangle(buffer.DataPointer, stride));
            }
        }
    }
}
