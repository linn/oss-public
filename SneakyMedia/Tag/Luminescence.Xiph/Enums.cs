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

namespace Luminescence.Xiph
{
   /// <summary>
   /// Type de codec utilisé.
   /// </summary>
   public enum OggCodec
   {
      /// <summary>
      /// Codec Vorbis
      /// </summary>
      Vorbis = 0,
      /// <summary>
      /// Codec FLAC
      /// </summary>
      FLAC = 1,
      /// <summary>
      /// Codec Speex
      /// </summary>
      Speex = 2
   }

   /// <summary>
   /// Largeur de bande utilisée pour le fichier Speex.
   /// </summary>
   public enum SpeexBand
   {
      /// <summary>
      /// NarrowBand (8 000 Hertz)
      /// </summary>
      NarrowBand = 0,
      /// <summary>
      /// WideBand (16 000 Hertz)
      /// </summary>
      WideBand = 1,
      /// <summary>
      /// UltraWideBand (32 000 Hertz)
      /// </summary>
      UltraWideband = 2
   }

   /// <summary>
   /// Type d'image selon la norme ID3v2 (frame APIC).
   /// </summary>
   public enum ID3PictureType : int
   {
      /// <summary>
      /// Other
      /// </summary>
      /// <remarks>
      /// 'Others' are reserved and should not be used.
      /// </remarks>
      Other = 0,
      /// <summary>
      /// 32x32 pixels 'file icon' (PNG only)
      /// </summary>
      /// <remarks>
      /// There may only be one of this picture type in a file.
      /// </remarks>
      PNG32PixelsFileIcon = 1,
      /// <summary>
      /// Other file icon
      /// </summary>
      /// <remarks>
      /// There may only be one of this picture type in a file.
      /// </remarks>
      OtherFileIcon = 2,
      /// <summary>
      /// Cover (front)
      /// </summary>
      FrontCover = 3,
      /// <summary>
      /// Cover (back)
      /// </summary>
      BackCover = 4,
      /// <summary>
      /// Leaflet page
      /// </summary>
      LeafletPage = 5,
      /// <summary>
      /// Media (e.g. label side of CD)
      /// </summary>
      Media = 6,
      /// <summary>
      /// Lead artist, lead performer or soloist
      /// </summary>
      LeadArtist = 7,
      /// <summary>
      /// Artist or performer
      /// </summary>
      Artist = 8,
      /// <summary>
      /// Conductor
      /// </summary>
      Conductor = 9,
      /// <summary>
      /// Band or orchestra
      /// </summary>
      Orchestra = 10,
      /// <summary>
      /// Composer
      /// </summary>
      Composer = 11,
      /// <summary>
      /// Lyricist or text writer
      /// </summary>
      Lyricist = 12,
      /// <summary>
      /// Recording location
      /// </summary>
      RecordingLocation = 13,
      /// <summary>
      /// During recording
      /// </summary>
      DuringRecording = 14,
      /// <summary>
      /// During performance
      /// </summary>
      DuringPerformance = 15,
      /// <summary>
      /// Movie or video screen capture
      /// </summary>
      VideoScreenCapture = 16,
      /// <summary>
      /// A bright coloured fish
      /// </summary>
      BrightColouredFish = 17,
      /// <summary>
      /// Illustration
      /// </summary>
      Illustration = 18,
      /// <summary>
      /// Band or artist logotype
      /// </summary>
      ArtistLogo = 19,
      /// <summary>
      /// Publisher or studio logotype
      /// </summary>
      StudioLogo = 20
   }

   /// <summary>
   /// Type de la page Ogg.
   /// </summary>
   public enum OggHeaderType : byte
   {
      /// <summary>
      /// On est au milieu d'un flux binaire, et on commence un nouveau paquet avec cette page
      /// </summary>
      FreshPaquetInContinuedLogicalBitstream = 0,

      /// <summary>
      /// On est au milieu d'un flux binaire, et on continue un paquet de la page précédente
      /// </summary>
      ContinuedPaquetInContinuedLogicalBitstream = 1,

      /// <summary>
      /// On commence un flux binaire, et on démarre avec un nouveau paquet
      /// </summary>
      FreshPaquetInNewLogicalBitstream = 2,

      /// <summary>
      /// On termine un flux binaire, mais on commence cette page avec un nouveau paquet
      /// </summary>
      FreshPaquetInLastLogicalBitstream = 4,

      /// <summary>
      /// On termine un flux binaire en continuant un paquet de la page précédente
      /// </summary>
      ContinuedPaquetInLastLogicalBitstream = 5,

      /// <summary>
      /// On commence et termine un flux binaire : cela se produit quand le flux rentre entièrement dans une seule page
      /// </summary>
      StartAndEndLogicalBitstream = 6
   }
}