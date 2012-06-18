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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Luminescence.Xiph
{
   /// <summary>
   /// Classe de lecture / écriture des tags des fichiers Ogg Vorbis, FLAC (.ogg) et Speex (.spx).
   /// </summary>
   public sealed class OggTagger : VorbisComment
   {
      // Membres génériques
      private string filename;
      private byte channels;
      private int sample_rate;
      private OggCodec codec;
      private long length;
      private long samples;

      private List<int> pagesToRemove = new List<int>();
      private byte[] vorbisSetupHeader;

      // Membres spécifiques FLAC
      private byte bits_per_sample;
      private List<byte[]> metadataBlocks = new List<byte[]>(4);
      private Collection<ID3PictureFrame> arts = new Collection<ID3PictureFrame>();

      //Membres spécifiques Vorbis
      private int bitrate_maximum;
      private int bitrate_minimum;

      // Membres spécifiques Speex
      private bool speexVBR;
      private SpeexBand band;

      // Membres spécifiques Speex & Vorbis
      private int bitrate_nominal;

      // Identificateurs
      private static readonly byte[] vorbis1 = { 0x01, 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73 };
      private static readonly byte[] vorbis3 = { 0x03, 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73 };
      private static readonly byte[] fLaC = { 0x66, 0x4C, 0x61, 0x43 };
      private static readonly byte[] Speex___ = { 0x53, 0x70, 0x65, 0x65, 0x78, 0x20, 0x20, 0x20 };

      /// <summary>
      /// Constructeur de la classe OggTagger.
      /// </summary>
      /// <param name="fileName">Chemin d'accès du fichier Ogg à analyser</param>
      public OggTagger(string fileName)
      {
         ReadMetadata(fileName);
      }

      /// <summary>
      /// Lit les métadonnées du fichier Ogg.
      /// </summary>
      /// <param name="fileName">Chemin d'accès du fichier Ogg à analyser</param>
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

            OggPageReader page = new OggPageReader(fs);
            byte[] firstSegment = page.GetSegments()[0];
            MemoryStream ms = new MemoryStream(firstSegment);
            BinaryReader br = new BinaryReader(ms);

            if (firstSegment.ArrayEquals(vorbis1, 0, vorbis1.Length))
            {
               codec = OggCodec.Vorbis;

               ms.Position = 11;
               channels = br.ReadByte();
               sample_rate = br.ReadInt32();
               bitrate_maximum = br.ReadInt32();
               bitrate_nominal = br.ReadInt32();
               bitrate_minimum = br.ReadInt32();

               ms = new MemoryStream();
               do
               {
                  page.ReadNextPage();
                  pagesToRemove.Add(page.PageSequenceNumber);
                  byte[] data = page.GetData();
                  ms.Write(data, 0, data.Length);
               }
               while (!page.IsComplete);

               if (!ms.ToArray().ArrayEquals(vorbis3, 0, vorbis3.Length))
                  throw new FileFormatException(String.Format("Vorbis comment header not found in file '{0}'.", filename));

               ms.Position = 7;
               ReadVorbisComment(ms, true);

               while (true) // go to vorbis setup header if it is on the same page of comments and save it
               {
                  int id = ms.ReadByte();
                  if (id == -1) break;
                  if (id == 5)
                  {
                     ms.Position--;
                     int dataSizeToKeep = (int)(ms.Length - ms.Position);
                     vorbisSetupHeader = new byte[dataSizeToKeep];
                     ms.Read(vorbisSetupHeader, 0, dataSizeToKeep);
                     break;
                  }
               }

               page.ReadLastPage();
               if (page.MultiplexedStreamDetected)
                  throw new FileFormatException(String.Format("The file '{0}' contains multiplexed streams.", filename));

               samples = page.AbsoluteGranulePostion;
            }
            else if (firstSegment.ArrayEquals(fLaC, 9, fLaC.Length))
            {
               codec = OggCodec.FLAC;

               ms.Position = 27;
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

               bool last_metadata_block;
               do
               {
                  ms = new MemoryStream();
                  BinaryReader br2 = new BinaryReader(ms);
                  do
                  {
                     page.ReadNextPage();
                     pagesToRemove.Add(page.PageSequenceNumber);
                     byte[] data = page.GetData();
                     ms.Write(data, 0, data.Length);
                  }
                  while (!page.IsComplete);

                  ms.Position = 0;
                  byte startHeader = br2.ReadByte();
                  last_metadata_block = (startHeader >> 7) == 1;
                  int blockType = startHeader & 127;

                  buffer = br2.ReadBytes(3);
                  int size = buffer[0];
                  size = (size << 8) | buffer[1];
                  size = (size << 8) | buffer[2];

                  if (blockType == 4) // VORBIS_COMMENT
                  {
                     ReadVorbisComment(ms, false);
                  }
                  else if (blockType == 6) // PICTURE
                  {
                     buffer = br2.ReadBytes(size);
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

               page.ReadLastPage();
               if (page.MultiplexedStreamDetected)
                  throw new FileFormatException(String.Format("The file '{0}' contains multiplexed streams.", filename));
            }
            else if (firstSegment.ArrayEquals(Speex___, 0, Speex___.Length))
            {
               codec = OggCodec.Speex;
               ms.Position = 36;

               sample_rate = br.ReadInt32();
               band = (SpeexBand)br.ReadInt32();

               ms.Position += 4;
               channels = (byte)br.ReadInt32();
               bitrate_nominal = br.ReadInt32();
               ms.Position += 4;
               speexVBR = (br.ReadInt32() == 1);

               ms = new MemoryStream();
               do
               {
                  page.ReadNextPage();
                  pagesToRemove.Add(page.PageSequenceNumber);
                  byte[] data = page.GetData();
                  ms.Write(data, 0, data.Length);
               }
               while (!page.IsComplete);

               ms.Position = 0;
               ReadVorbisComment(ms, false);

               page.ReadLastPage();
               if (page.MultiplexedStreamDetected)
                  throw new FileFormatException(String.Format("The file '{0}' contains multiplexed streams.", filename));

               samples = page.AbsoluteGranulePostion;
            }
            else
            {
               throw new FileFormatException(String.Format("The file '{0}' is not a valid Ogg Vorbis, FLAC or Speex file, or it contains multiplexed streams.", filename));
            }
         }
         finally
         {
            if (fs != null) fs.Close();
         }
      }

      /// <summary>
      /// Collection contenant les images.
      /// </summary>
      public Collection<ID3PictureFrame> FlacArts
      {
         get { return arts; }
      }

      #region Informations techniques

      /// <summary>
      /// Retourne le type de codec utilisé dans le fichier Ogg.
      /// </summary>
      public OggCodec Codec
      {
         get { return codec; }
      }

      /// <summary>
      /// Retourne le type de largeur de bande utilisé dans le fichier Ogg Speex.
      /// </summary>
      public SpeexBand SpeexBand
      {
         get { return band; }
      }

      /// <summary>
      /// Retourne le chemin et le nom du fichier Ogg analysé.
      /// </summary>
      public string FileName
      {
         get { return filename; }
      }

      /// <summary>
      /// Retourne le nombre de canaux dans le fcihier Ogg.
      /// </summary>
      public byte Channels
      {
         get { return channels; }
      }

      /// <summary>
      /// Retourne le bitrate minimum du fichier Ogg Vorbis.
      /// </summary>
      public int MinimumVorbisBitrate
      {
         get { return bitrate_minimum; }
      }

      /// <summary>
      /// Retourne le taux d'échantillonage du fichier Ogg.
      /// </summary>
      public int SampleRate
      {
         get { return sample_rate; }
      }

      /// <summary>
      /// Retourne le bitrate maximum du fichier Ogg Vorbis.
      /// </summary>
      public int MaximumVorbisBitrate
      {
         get { return bitrate_maximum; }
      }

      /// <summary>
      /// Retourne le bitrate nominal du fichier Ogg Vorbis ou Speex.
      /// </summary>
      public int VorbisSpeexNominalBitrate
      {
         get { return bitrate_nominal; }
      }

      /// <summary>
      /// Retourne Vrai si le fichier Ogg Vorbis ou Speex est encodé avec un débit variable.
      /// </summary>
      public bool IsVariableBitrate
      {
         get
         {
            switch (codec)
            {
               case OggCodec.Vorbis:
                  return ((bitrate_maximum != bitrate_minimum) || (bitrate_minimum != bitrate_nominal));
               case OggCodec.Speex:
                  return speexVBR;
               default:
                  return false;
            }
         }
      }

      /// <summary>
      /// Retourne le niveau de qualité du fichier Ogg Vorbis (de 0.0 à 1.0).
      /// </summary>
      public float VorbisQuality
      {
         get
         {
            switch ((int)Math.Round((double)(bitrate_nominal / 1000), 0))
            {
               case 64: return 0F;
               case 72: return 0.05F;
               case 80: return 0.1F;
               case 88: return 0.15F;
               case 96: return 0.2F;
               case 104: return 0.25F;
               case 112: return 0.3F;
               case 120: return 0.35F;
               case 128: return 0.4F;
               case 144: return 0.45F;
               case 160: return 0.5F;
               case 176: return 0.55F;
               case 192: return 0.6F;
               case 208: return 0.65F;
               case 224: return 0.7F;
               case 240: return 0.75F;
               case 256: return 0.8F;
               case 288: return 0.85F;
               case 320: return 0.9F;
               case 410: return 0.95F;
               case 500: return 1F;
               default: return -1;
            }
         }
      }

      /// <summary>
      /// Retourne la version du codec utilisé.
      /// </summary>
      public string CodecVersion
      {
         get
         {
            switch (codec)
            {
               case OggCodec.Vorbis:
                  switch (Vendor)
                  {
                     case "Xiphophorus libVorbis I 20000508": return "1.0 Beta 1/2";
                     case "Xiphophorus libVorbis I 20001031": return "1.0 Beta 3";
                     case "Xiphophorus libVorbis I 20010225": return "1.0 Beta 4";
                     case "Xiphophorus libVorbis I 20010615": return "1.0 RC1";
                     case "Xiphophorus libVorbis I 20010813": return "1.0 RC2";
                     case "Xiphophorus libVorbis I 20011217":
                     case "Xiphophorus libVorbis I 20011231": return "1.0 RC3";
                     case "Xiph.Org libVorbis I 20020717": return "1.0";
                     case "Xiph.Org libVorbis I 20030308": return "Post 1.0 CVS";
                     case "Xiph.Org libVorbis I 20030909": return "1.0.1";
                     case "Xiph.Org libVorbis I 20031230 (1.0.1)": return "Post 1.0.1 CVS";
                     case "AO; aoTuV b2 [20040420] (based on Xiph.Org's 1.0.1)": return "aoTuV Beta 2";
                     case "Xiph.Org libVorbis I 20040920":
                     case "Xiph.Org libVorbis I 20040629": return "1.1";
                     case "AO; aoTuV b3 [20041120] (based on Xiph.Org's libVorbis)": return "aoTuV Beta 3";
                     case "Xiph.Org libVorbis I 20050304": return "1.1.1/1.1.2";
                     case "Xiph.Org libVorbis I 20070622": return "1.2";
                     case "AO; aoTuV b4 [20050617] (based on Xiph.Org's libVorbis)": return "aoTuV Beta 4";
                     case "AO; aoTuV b4a [20051105] (based on Xiph.Org's libVorbis)": return "aoTuV Beta 4.5";
                     case "AO; aoTuV r1 [20051117] (based on Xiph.Org's libVorbis)": return "aoTuV Beta 4.51 / R1";
                     case "AO; aoTuV b5 [20061024] (based on Xiph.Org's libVorbis)": return "aoTuV Beta 5";
                     default: return Vendor;
                  }

               case OggCodec.FLAC:
                  string[] flacBuffer = Vendor.Split(new char[] { ' ' });
                  if (flacBuffer.Length > 2)
                     return flacBuffer[2];
                  else
                     return Vendor;

               case OggCodec.Speex:
                  string[] speexBuffer = Vendor.Split(new char[] { '-' });
                  if (speexBuffer.Length > 1)
                     return speexBuffer[1];
                  else
                     return Vendor;

               default:
                  return null;
            }
         }
      }

      /// <summary>
      /// Retourne la taille du fichiers Ogg (en octets).
      /// </summary>
      public long Length
      {
         get { return length; }
      }

      /// <summary>
      /// Retourne le nombre d'échantillon contenu dans le fichier Ogg.
      /// </summary>
      public long Samples
      {
         get { return samples; }
      }

      /// <summary>
      /// Retourne la durée du fichier Ogg (en secondes).
      /// </summary>
      public float Duration
      {
         get
         {
            return (float)Math.Round((double)samples / sample_rate, 3);
         }
      }

      /// <summary>
      /// Retourne le bitrate moyen du fichier Ogg.
      /// </summary>
      public int AverageBitrate
      {
         get
         {
            return (int)Math.Round((double)length * 8 / Duration, 0);
         }
      }

      /// <summary>
      /// Retourne le taux de compression du ficher Ogg.
      /// </summary>
      public float Ratio
      {
         get
         {
            return (float)length / UncompressedFileLength;
         }
      }

      /// <summary>
      /// Retourne la taille de l'échantillon du fichier Ogg FLAC.
      /// </summary>
      public byte FlacBitsPerSample
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
            byte buffer;
            if (codec == OggCodec.FLAC)
               buffer = bits_per_sample;
            else
               buffer = 16;

            return (long)Math.Round(((double)(Duration * sample_rate * buffer * channels / 8) + 44), 0);
         }
      }

      #endregion
   }
}