// Based on Luminescence.Xiph:

/*
This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

Copyright (C) 2005-2008  Cyber Sinh (http://www.luminescence-software.org/)
*/

using System;
using System.IO;
using System.Text;

namespace Luminescence.Xiph
{
   /// <summary>
   /// Classe de lecture / écriture des images dans les fichiers FLAC.
   /// </summary>
   public class ID3PictureFrame
   {
      private ID3PictureType pictureType = ID3PictureType.FrontCover;
      private string description;
      byte[] bitmap;

      /// <summary>
      /// Constructeur de la classe ID3Picture.
      /// </summary>
      /// <param name="data">Données METADATA_BLOCK_PICTURE sous la forme d'un tableau d'octets</param>
      public ID3PictureFrame(byte[] data)
      {
         MemoryStream ms = new MemoryStream(data, 0, data.Length, false);
         BinaryReader br = new BinaryReader(ms);

         pictureType = (ID3PictureType)br.ReadBigEndianInt32();

         int mimeSize = br.ReadBigEndianInt32();
         ms.Position += mimeSize;

         int descSize = br.ReadBigEndianInt32();
         if (descSize > 0)
         {
            bitmap = br.ReadBytes(descSize);
            UTF8Encoding utf8 = new UTF8Encoding();
            description = utf8.GetString(bitmap);
         }

         ms.Position += 16;
         int pictSize = br.ReadBigEndianInt32();
         bitmap = br.ReadBytes(pictSize);
      }

      /// <summary>
      /// Type d'image selon la norme ID3v2 (frame APIC).
      /// </summary>
      public ID3PictureType PictureType
      {
         get { return pictureType; }
      }

      /// <summary>
      /// Description de l'image.
      /// </summary>
      public string Description
      {
         get { return description; }
      }

      /// <summary>
      /// Bitmap constituant l'image.
      /// </summary>
      public byte[] Picture
      {
         get { return bitmap; }
      }
   }
}