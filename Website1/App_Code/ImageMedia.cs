﻿using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class ImageMedia
{
    private Image _source;
    private byte[] _data;

    public int Width { get { return _source.Width; } }
    public int Height { get { return _source.Height; } }

    /// <summary>
    /// Resizes image to fit within the specified with and height, aspect ratio is maintained.
    /// </summary>
    /// <param name="width">The maximum width the image has to fit within, set to null to not restrict width.</param>
    /// <param name="height">The maximum height the image has to fit within, set to null to not restrict height.</param>
    /// <returns>A reference to this object to allow chaining operations together.</returns>
    public ImageMedia Resize(int? width, int? height)
    {
        if (width == null && height == null || width == 0 && height == 0)
            return this;

        int w = (width == null || width == 0) ? _source.Width : width.Value;
        int h = (height == null || height == 0) ? _source.Height : height.Value;
        float desiredRatio = (float)w / h;
        float scale, posx, posy;
        float ratio = (float)_source.Width / _source.Height;

        if (_source.Width < w && _source.Height < h)
        {
            scale = 1f;
            posy = (h - _source.Height) / 2f;
            posx = (w - _source.Width) / 2f;
        }
        else if (ratio > desiredRatio)
        {
            scale = (float)w / _source.Width;
            posy = (h - (_source.Height * scale)) / 2f;
            posx = 0f;
        }
        else
        {
            scale = (float)h / _source.Height;
            posx = (w - (_source.Width * scale)) / 2f;
            posy = 0f;
        }

        //if (!background.HasValue)
        //{
        //    w = (int)(_source.Width * scale);
        //    h = (int)(_source.Height * scale);
        //    posx = 0f;
        //    posy = 0f;
        //}

        Image resizedImage = new Bitmap(w, h);
        Graphics g = Graphics.FromImage(resizedImage);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawImage(_source, posx, posy, _source.Width * scale, _source.Height * scale);

        foreach (PropertyItem item in _source.PropertyItems)
            resizedImage.SetPropertyItem(item);

        _source = resizedImage;
		_data = null;
        return this;
    }

    /// <summary>
    /// Crops the image in the middle resizing it down to fit as much of the image as possible. Note, image is not enlarged to fit desired width and height.
    /// </summary>
    /// <param name="width">The desired width to crop the image to, set to null to not perform horizontal cropping.</param>
    /// <param name="height">The desired height to crop the image to, set to null to not perform vertical cropping.</param>
    /// <returns>A reference to this object to allow chaining operations together.</returns>
    public ImageMedia Crop(int? width, int? height)
    {
        if (width == null && height == null)
            return this;

        int w = (width == null) ? _source.Width : width.Value;
        int h = (height == null) ? _source.Height : height.Value;

        float scaleX = (float)w / (float)_source.Width;
        float scaleY = (float)h / (float)_source.Height;
        float scale = Math.Max(scaleX, scaleY);
        int marginX = -((int)(_source.Width * scale - w)) / 2;
        int marginY = -((int)(_source.Height * scale - h)) / 2;

        Image resizedImage = new Bitmap(w, h);
        Graphics g = Graphics.FromImage(resizedImage);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, w, h);
        g.DrawImage(_source, marginX, marginY, _source.Width * scale, _source.Height * scale);

        foreach (PropertyItem item in _source.PropertyItems)
            resizedImage.SetPropertyItem(item);

        _source = resizedImage;
		_data = null;
        return this;
    }

    /// <summary>
    /// Returns the binary data of the image in jpg format.
    /// </summary>
    public byte[] ToByteArray()
    {
        return ToByteArray("jpg");
    }

    /// <summary>
    /// Returns the binary data of the image in the specifed format.
    /// </summary>
    /// <param name="format">The format of the bitmap image specified as "gif", "jpg", "png", "bmp", "jxr" or "tiff".</param>
    public byte[] ToByteArray(string format)
    {
		//if image not altered return orig data
        if (_data != null)
            return _data;

        using (MemoryStream resized = new MemoryStream())
		{
			_source.Save(resized, GetEncoder(format));
			return resized.ToArray();
		}
    }

    private ImageFormat GetEncoder(string format)
    {
        if (format == "jpg")
            return ImageFormat.Jpeg;
        if (format == "png")
            return ImageFormat.Png;
        if (format == "gif")
            return ImageFormat.Gif;
        if (format == "tiff")
            return ImageFormat.Tiff;
        if (format == "bmp")
            return ImageFormat.Bmp;
        throw new Exception("Unrecognised image format: " + format);
    }

    /// <summary>
    /// Create a image object that can be interacted with from binary data.
    /// </summary>
    /// <exception cref="System.ArgumentException">If the supplied binary data is not a valid image, the ArgumentException will be thrown.</exception>
    public static ImageMedia Create(byte[] data)
    {
        ImageMedia result = new ImageMedia();
        result._source = Image.FromStream(new MemoryStream(data));
        result._data = data;
        return result;
    }
}
