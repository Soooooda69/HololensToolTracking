using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Slice
{
    private byte[] _bitmap;
    private Texture2D _tex;
    private int[] _size;
    private TextureFormat _format;
    private float[] _extrinsic;
    public Slice(byte[] Bitmap, float[] Extrinsic, int[] Size = null, string Type = "DXT1")
    {
        _size = Size ?? new int[] { 1024,1024 };
        _extrinsic = Extrinsic;
        if (Type == "DXT1")
        {
            /*_bitmap = new byte[EstimateDXT1Size(Size[0],Size[1],4)];*/
            _bitmap = Bitmap;
            _format = TextureFormat.DXT1;
            _tex = new Texture2D(_size[0], _size[1], _format, false);
            _tex.LoadRawTextureData(_bitmap);
        }
    }

    public static int EstimateDXT1Size(int width, int height, int depth)
    {
        int roundedWidth = (width + 3) & ~3;
        int roundedHeight = (height + 3) & ~3;
        int numBlocks = (roundedWidth / 4) * (roundedHeight / 4);
        int sizePerFrame = numBlocks * 8; 
        return sizePerFrame * depth;
    }

    public Texture2D GetDecodedTexture => _tex;
    public byte[] GetEncodedData => _bitmap;

}