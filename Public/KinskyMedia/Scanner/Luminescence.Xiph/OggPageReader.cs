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

namespace Luminescence.Xiph
{
   /// <summary>
   /// Classe permettant le lecture séquencielle de pages Ogg dans un flux.
   /// </summary>
   public class OggPageReader : OggPage, IDisposable
   {
      private BinaryReader br;

      private bool isMultiplexedStream;
      /// <summary>
      /// Indique si un flux multiplexé a été détecté à ce stade du parcours dans le flux Ogg.
      /// </summary>
      public bool MultiplexedStreamDetected
      {
         get { return isMultiplexedStream; }
      }

      private long offset;
      /// <summary>
      /// Position dans le flux.
      /// </summary>
      public long Offset
      {
         get { return offset; }
         set { offset = value; }
      }

      /// <summary>
      /// Constructeur de la classe OggPageReader.
      /// </summary>
      /// <param name="stream">Flux contenant les pages Ogg à lire</param>
      public OggPageReader(Stream stream)
      {
         br = new BinaryReader(stream);
         ReadFirstPage();
      }

      /// <summary>
      /// Lit la première page Ogg du flux.
      /// </summary>
      public void ReadFirstPage()
      {
         offset = 0;
         ReadNextPage();
      }

      /// <summary>
      /// Lit la dernière page Ogg du flux.
      /// </summary>
      public void ReadLastPage()
      {
         while (ReadNextPage(true)) { }
      }

      /// <summary>
      /// Lit la page suivante dans le flux Ogg.
      /// </summary>
      /// <returns>True si la fin du flux a été atteinte, sinon False</returns>
      public bool ReadNextPage()
      {
         return ReadNextPage(false);
      }

      private bool ReadNextPage(bool onlyLastPageRequired)
      {
         if (offset >= br.BaseStream.Length)
            return false;

         br.BaseStream.Position = offset;

         if (!OggS.ArrayEquals(br.ReadBytes(4)))
            throw new FileFormatException("Ogg page not found.");

         if (br.ReadByte() != 0)
            throw new FileFormatException("Stream structure version unsupported.");

         HeaderType = (OggHeaderType)br.ReadByte();

         AbsoluteGranulePostion = br.ReadInt64();
         int serial = br.ReadInt32();
         if (!isMultiplexedStream && StreamSerialNumber != 0 && StreamSerialNumber != serial)
            isMultiplexedStream = true;
         StreamSerialNumber = serial;
         PageSequenceNumber = br.ReadInt32();
         Checksum = br.ReadUInt32();

         byte nbSegments = br.ReadByte();
         Segments = new byte[nbSegments][];
         byte[] sizeSegments = new byte[nbSegments];
         for (int i = 0; i < nbSegments; i++)
         {
            sizeSegments[i] = br.ReadByte();
         }

         for (int j = 0; j < sizeSegments.Length; j++)
         {
            if (!onlyLastPageRequired || IsLastInLogicalBitstream)
               Segments[j] = br.ReadBytes(sizeSegments[j]);
            else
               br.BaseStream.Position += sizeSegments[j];
         }

         offset = br.BaseStream.Position;

         return true;
      }

      #region Membres IDisposable

      /// <summary>
      /// Nettoyage des ressources utilisées par la classe.
      /// </summary>
      /// <param name="disposing">True si les ressources managées doivent être libérées, sinon False</param>
      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
            br.Close();
      }

      /// <summary>
      /// Nettoyage des ressources utilisées par la classe.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      #endregion
   }
}