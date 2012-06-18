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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Luminescence.Xiph
{
   /// <summary>
   /// Classe de lecture / écriture des tags des fichiers FLAC natifs.
   /// </summary>
   public sealed class FlacTagger : VorbisComment
   {
      // Pictures
      private Collection<ID3PictureFrame> arts = new Collection<ID3PictureFrame>();

      // Informations techniques
      private string filename;
      private byte channels;
      private int sample_rate;
      private long length;
      private long samples;
      private byte bits_per_sample;

      // Identificateurs
      private static readonly byte[] fLaC = { 0x66, 0x4C, 0x61, 0x43 };

      // METADATA_BLOCK
      private List<byte[]> metadataBlocks = new List<byte[]>(4);

      /// <summary>
      /// Constructeur de la classe FlacTagger.
      /// </summary>
      /// <param name="fileName">Chemin d'accès du fichier FLAC à analyser</param>
      public FlacTagger(string fileName)
      {
         ReadMetadata(fileName);
      }

      /// <summary>
      /// Lit les métadonnées du fichier FLAC.
      /// </summary>
      /// <param name="fileName">Chemin d'accès du fichier FLAC à analyser</param>
      public override void ReadMetadata(string fileName)
      {
         filename = fileName;
         ClearTags();
         arts.Clear();
         metadataBlocks.Clear();

         FileStream fs = null;
         try
         {
            fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            length = fs.Length;

            BinaryReader br = new BinaryReader(fs);
            if (!fLaC.ArrayEquals(br.ReadBytes(4)))
               throw new FileFormatException(String.Format("The file '{0}' is not a valid FLAC file.", filename));

            fs.Position = 18;
            byte[] buffer = br.ReadBytes(8);

            sample_rate = buffer[0];
            sample_rate = (sample_rate << 8) | buffer[1];
            sample_rate = (sample_rate << 4) | (buffer[2] >> 4);

            channels = (byte)(((buffer[2] >> 1) & 7) + 1);

            bits_per_sample = (byte)((((buffer[2] & 1) << 4) | (buffer[3] >> 4)) + 1);

            samples = (buffer[3] & 15);
            samples = (samples << 8) | buffer[4];
            samples = (samples << 8) | buffer[5];
            samples = (samples << 8) | buffer[6];
            samples = (samples << 8) | buffer[7];

            fs.Position = 42; // first metadata block following STREAM_INFO
            bool last_metadata_block;
            do
            {
               byte startHeader = br.ReadByte();
               last_metadata_block = (startHeader >> 7) == 1;
               int blockType = startHeader & 127;

               buffer = br.ReadBytes(3);
               int size = buffer[0];
               size = (size << 8) | buffer[1];
               size = (size << 8) | buffer[2];

               if (blockType == 4) // VORBIS_COMMENT
               {
                  ReadVorbisComment(fs, false);
               }
               else if (blockType == 6) // PICTURE
               {
                  buffer = br.ReadBytes(size);
                  arts.Add(new ID3PictureFrame(buffer));
               }
               else // Other METADATA BLOCK
               {
                  byte[] metadata = new byte[size + 4];
                  metadata[0] = (byte)blockType;
                  buffer.CopyTo(metadata, 1);
                  buffer = br.ReadBytes(size);
                  buffer.CopyTo(metadata, 4);
                  metadataBlocks.Add(metadata);
               }
            }
            while (!last_metadata_block);

            if (Vendor == null)
               throw new FileFormatException(String.Format("Vorbis comment header not found in file '{0}'.", filename));
         }
         finally
         {
            if (fs != null) fs.Close();
         }
      }

      /// <summary>
      /// Collection contenant les images.
      /// </summary>
      public Collection<ID3PictureFrame> Arts
      {
         get { return arts; }
      }

      #region Informations techniques

      /// <summary>
      /// Retourne le chemin et le nom du fichier FLAC analysé.
      /// </summary>
      public string FileName
      {
         get { return filename; }
      }

      /// <summary>
      /// Retourne le nombre de canaux dans le fichier FLAC.
      /// </summary>
      public byte Channels
      {
         get { return channels; }
      }

      /// <summary>
      /// Retourne le taux d'échantillonage du fichier FLAC.
      /// </summary>
      public int SampleRate
      {
         get { return sample_rate; }
      }

      /// <summary>
      /// Retourne la version du codec utilisé.
      /// </summary>
      public string CodecVersion
      {
         get
         {
            string[] buffer = Vendor.Split(new char[] { ' ' });
            return buffer[2];
         }
      }

      /// <summary>
      /// Retourne la taille du fichiers FLAC (en octets).
      /// </summary>
      public long Length
      {
         get { return length; }
      }

      /// <summary>
      /// Retourne le nombre d'échantillon contenu dans le fichier FLAC.
      /// </summary>
      public long Samples
      {
         get { return samples; }
      }

      /// <summary>
      /// Retourne la durée du fichier FLAC (en secondes).
      /// </summary>
      public float Duration
      {
         get
         {
            return (float)Math.Round((double)samples / sample_rate, 3);
         }
      }

      /// <summary>
      /// Retourne le bitrate moyen du fichier FLAC.
      /// </summary>
      public int BitrateAverage
      {
         get
         {
            return (int)Math.Round((double)length * 8 / Duration, 0);
         }
      }

      /// <summary>
      /// Retourne le taux de compression du ficher FLAC.
      /// </summary>
      public float Ratio
      {
         get
         {
            return (float)length / UncompressedFileLength;
         }
      }

      /// <summary>
      /// Retourne la taille de l'échantillon du fichier FLAC.
      /// </summary>
      public byte BitsPerSample
      {
         get { return bits_per_sample; }
      }

      /// <summary>
      /// Retourne la taille du fichier décompressée (en octets).
      /// </summary>
      public long UncompressedFileLength
      {
         get
         {
            return (long)Math.Round(((double)(Duration * sample_rate * bits_per_sample * channels / 8) + 44), 0);
         }
      }

      #endregion
   }
}