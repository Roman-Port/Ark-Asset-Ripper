using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkUassetReader
{
    public static class TgaConverter
    {
        public static MemoryStream ConvertTga(FileStream tga, out MemoryStream thumbOutput)
        {
            //Read in the TGA header.
            ReadTgaHeader(ReadBytes(tga, 18), out ushort imageWidth, out ushort imageHeight, out byte colorDepth);

            //Create the image and begin reading data.
            Image<Rgba32> image = new Image<Rgba32>(imageWidth, imageHeight);

            //Begin unpacking
            byte[] channels;
            for(int y = 0; y<imageHeight; y++)
            {
                for(int x = 0; x<imageWidth; x++)
                {
                    if(colorDepth == 32)
                    {
                        //Read four channels
                        channels = ReadBytes(tga, 4);

                        //Set pixel
                        image[x, imageWidth - y - 1] = new Rgba32(channels[2], channels[1], channels[0], channels[3]);
                    } else if (colorDepth == 24)
                    {
                        //Read three channels
                        channels = ReadBytes(tga, 3);

                        //Set pixel
                        image[x, imageWidth - y - 1] = new Rgba32(channels[2], channels[1], channels[0]);
                    }
                }
            }

            //Open a MemoryStream and write it
            MemoryStream ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Position = 0;

            //Open a MemoryStream and write a thumbnail version at a lower resolution
            image.Mutate(x => x.Resize(64, 64));
            thumbOutput = new MemoryStream();
            image.SaveAsPng(thumbOutput);
            thumbOutput.Position = 0;
            return ms;
        }

        public static MemoryStream ConvertTga(string filePath, out MemoryStream thumbOut)
        {
            MemoryStream ms;

            //Open FileStream
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
                ms = ConvertTga(fs, out thumbOut);

            return ms;
        }

        static byte[] ReadBytes(Stream s, int length)
        {
            byte[] buf = new byte[length];
            s.Read(buf, 0, length);
            return buf;
        }

        /// <summary>
        /// Read the 18 byte header
        /// </summary>
        /// <param name="data"></param>
        static void ReadTgaHeader(byte[] data, out ushort imageWidth, out ushort imageHeight, out byte colorDepth)
        {
            //Read as of https://en.wikipedia.org/wiki/Truevision_TGA
            //Ensure the first byte is 0x00
            if (data[0] != 0x00)
                throw new Exception("Failed to read TGA image; ID length was not 0x00.");

            //ensure the color map type is 0x00
            if (data[1] != 0x00)
                throw new Exception("Failed to read TGA image; Color map type was not 0x00.");

            //Ensure this is the correct type of image
            if (data[2] != 0x02)
                throw new Exception("Failed to read TGA image; Image type was not 'uncompressed true-color image' (0x02)");

            //Skip the next 5 bytes because we don't have a color map

            //Read width and height
            imageWidth = ReadUShort(new byte[] { data[12], data[13] });
            imageHeight = ReadUShort(new byte[] { data[14], data[15] });

            //Ensure each pixel is the same depth
            if(data[16] != 32 && data[16] != 24)
                throw new Exception($"Failed to read TGA image; Only images with 32-bit or 24-bit color depth are supported. This one is {data[16]}.");
            colorDepth = data[16];
        }

        static ushort ReadUShort(byte[] d)
        {
            return BitConverter.ToUInt16(d, 0);
        }
    }
}
