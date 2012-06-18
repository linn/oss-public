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
using System.Text;

namespace Luminescence.Xiph
{
    public class FileFormatException : Exception
    {
        public FileFormatException(string message)
            : base(message)
        {
        }
    }
   /// <summary>
   /// Classe abstraite assurant la gestion des tags Vorbis Comment.
   /// </summary>
   public abstract class VorbisComment
   {
      private SortedList<string, List<string>> tags = new SortedList<string, List<string>>();
      private string vendorString;

      /// <summary>
      /// Méthode de lecture d'un fichier contenant des métadonnées au format Vorbis Comment.
      /// </summary>
      /// <param name="fileName">Chemin d'accès du fichier à analyser</param>
      public abstract void ReadMetadata(string fileName);

      #region Tags Vorbis Comment

      /// <summary>
      /// Obtient ou définit le tag "Titre".
      /// </summary>
      public string Title
      {
         get
         {
            if (tags.ContainsKey("TITLE")) return tags["TITLE"][0];
            return null;
         }
         set
         {
            tags.Remove("TITLE");
            if (value != null) tags.AddTag("TITLE", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Version".
      /// </summary>
      public string Version
      {
         get
         {
            if (tags.ContainsKey("VERSION")) return tags["VERSION"][0];
            return null;
         }
         set
         {
            tags.Remove("VERSION");
            if (value != null) tags.AddTag("VERSION", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Album".
      /// </summary>
      public string Album
      {
         get
         {
            if (tags.ContainsKey("ALBUM")) return tags["ALBUM"][0];
            return null;
         }
         set
         {
            tags.Remove("ALBUM");
            if (value != null) tags.AddTag("ALBUM", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Numéro de piste".
      /// </summary>
      public string TrackNumber
      {
         get
         {
            if (tags.ContainsKey("TRACKNUMBER")) return tags["TRACKNUMBER"][0];
            return null;
         }
         set
         {
            tags.Remove("TRACKNUMBER");
            if (value != null) tags.AddTag("TRACKNUMBER", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Artiste".
      /// </summary>
      public string Artist
      {
         get
         {
            if (tags.ContainsKey("ARTIST")) return tags["ARTIST"][0];
            return null;
         }
         set
         {
            tags.Remove("ARTIST");
            if (value != null) tags.AddTag("ARTIST", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Compositeur".
      /// </summary>
      public string Performer
      {
         get
         {
            if (tags.ContainsKey("PERFORMER")) return tags["PERFORMER"][0];
            return null;
         }
         set
         {
            tags.Remove("PERFORMER");
            if (value != null) tags.AddTag("PERFORMER", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Copyright".
      /// </summary>
      public string Copyright
      {
         get
         {
            if (tags.ContainsKey("COPYRIGHT")) return tags["COPYRIGHT"][0];
            return null;
         }
         set
         {
            tags.Remove("COPYRIGHT");
            if (value != null) tags.AddTag("COPYRIGHT", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "License".
      /// </summary>
      public string License
      {
         get
         {
            if (tags.ContainsKey("LICENSE")) return tags["LICENSE"][0];
            return null;
         }
         set
         {
            tags.Remove("LICENSE");
            if (value != null) tags.AddTag("LICENSE", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Organisation".
      /// </summary>
      public string Organization
      {
         get
         {
            if (tags.ContainsKey("ORGANIZATION")) return tags["ORGANIZATION"][0];
            return null;
         }
         set
         {
            tags.Remove("ORGANIZATION");
            if (value != null) tags.AddTag("ORGANIZATION", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Description".
      /// </summary>
      public string Description
      {
         get
         {
            if (tags.ContainsKey("DESCRIPTION")) return tags["DESCRIPTION"][0];
            return null;
         }
         set
         {
            tags.Remove("DESCRIPTION");
            if (value != null) tags.AddTag("DESCRIPTION", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Genre".
      /// </summary>
      public string Genre
      {
         get
         {
            if (tags.ContainsKey("GENRE")) return tags["GENRE"][0];
            return null;
         }
         set
         {
            tags.Remove("GENRE");
            if (value != null) tags.AddTag("GENRE", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Date".
      /// </summary>
      public string Date
      {
         get
         {
            if (tags.ContainsKey("DATE")) return tags["DATE"][0];
            return null;
         }
         set
         {
            tags.Remove("DATE");
            if (value != null) tags.AddTag("DATE", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Localisation".
      /// </summary>
      public string Location
      {
         get
         {
            if (tags.ContainsKey("LOCATION")) return tags["LOCATION"][0];
            return null;
         }
         set
         {
            tags.Remove("LOCATION");
            if (value != null) tags.AddTag("LOCATION", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "Contact".
      /// </summary>
      public string Contact
      {
         get
         {
            if (tags.ContainsKey("CONTACT")) return tags["CONTACT"][0];
            return null;
         }
         set
         {
            tags.Remove("CONTACT");
            if (value != null) tags.AddTag("CONTACT", value);
         }
      }

      /// <summary>
      /// Obtient ou définit le tag "ISRC".
      /// </summary>
      public string ISRC
      {
         get
         {
            if (tags.ContainsKey("ISRC")) return tags["ISRC"][0];
            return null;
         }
         set
         {
            tags.Remove("ISRC");
            if (value != null) tags.AddTag("ISRC", value);
         }
      }

      #endregion

      /// <summary>
      /// Retourne le "Vendor".
      /// </summary>
      public string Vendor
      {
         get { return vendorString; }
      }

      /// <summary>
      /// Supprime tous les tags non officiels.
      /// </summary>
      public void ClearNonOfficialTags()
      {
         List<string> del = new List<string>();
         foreach (KeyValuePair<string, List<string>> kvp in tags)
         {
            if (!IsOfficialVorbisTag(kvp.Key)) del.Add(kvp.Key);
         }

         foreach (string key in del)
         {
            tags.Remove(key);
         }
      }

      /// <summary>
      /// Supprime tous les tags.
      /// </summary>
      public void ClearTags()
      {
         tags.Clear();
      }

      /// <summary>
      /// Retourne le nombre total de tags.
      /// </summary>
      public int TagsCount
      {
         get
         {
            int i = 0;
            foreach (KeyValuePair<string, List<string>> kvp in tags)
            {
               i += kvp.Value.Count;
            }

            return i;
         }
      }

      /// <summary>
      /// Renvoie une collection en lecture seule des tags de même nom.
      /// </summary>
      /// <param name="key">Nom du tag</param>
      /// <returns>Collection en lecture seule contenant les valeurs du tag demandé</returns>
      public ReadOnlyCollection<string> this[string key]
      {
         get
         {
            key = CheckedVorbisCommentKey(key);
            if (tags.ContainsKey(key))
               return tags[key].AsReadOnly();
            else
               return null;
         }
      }

      /// <summary>
      /// Supprime un tag.
      /// </summary>
      /// <param name="name">Nom du tag</param>
      /// <returns>True si la suppression du tag réussi, sinon False</returns>
      public bool RemoveTag(string name)
      {
         return tags.Remove(CheckedVorbisCommentKey(name));
      }

      /// <summary>
      /// Supprime un tag.
      /// </summary>
      /// <param name="name">Nom du tag</param>
      /// <param name="index">Index du tag</param>
      /// <returns>True si la suppression du tag à l'index spécifié réussi, sinon False</returns>
      public bool RemoveTag(string name, int index)
      {
         name = CheckedVorbisCommentKey(name);
         if (tags.ContainsKey(name) && index >= 0 && index < tags[name].Count)
         {
            tags[name].RemoveAt(index);
            return true;
         }

         return false;
      }

      /// <summary>
      /// Modifie un tag.
      /// </summary>
      /// <param name="name">Nom du tag</param>
      /// <param name="content">Valeur du nouveau tag</param>
      /// <returns>True si la modification du tag réussi, sinon False</returns>
      public bool SetTag(string name, string content)
      {
         name = CheckedVorbisCommentKey(name);
         if (tags.ContainsKey(name))
         {
            tags[name] = new List<string>(new string[] { content.Trim() });
            return true;
         }

         return false;
      }

      /// <summary>
      /// Modifie un tag.
      /// </summary>
      /// <param name="name">Nom du tag</param>
      /// <param name="content">Valeur du nouveau tag</param>
      /// <param name="index">Index du tag</param>
      /// <returns>True si la modification du tag à l'index spécifié réussi, sinon False</returns>
      public bool SetTag(string name, string content, int index)
      {
         name = CheckedVorbisCommentKey(name);
         if (tags.ContainsKey(name) && index >= 0 && index < tags[name].Count)
         {
            tags[name][index] = content.Trim();
            return true;
         }

         return false;
      }

      /// <summary>
      /// Retourne une collection contenant tous les tags.
      /// </summary>
      /// <returns>Collection contenant les noms et valeurs de tous les tags</returns>
      public SortedList<string, List<string>> GetAllTags()
      {
         return new SortedList<string, List<string>>(tags);
      }

      /// <summary>
      /// Lit des tags au format Vorbis Comment dans un flux.
      /// </summary>
      /// <param name="stream">Flux à partir duquel les tags doivent être lus</param>
      /// <param name="readFramingBit">Vrai si le "framing bit" doit être lu</param>
      public void ReadVorbisComment(Stream stream, bool readFramingBit)
      {
         BinaryReader br = new BinaryReader(stream);
         int vendor_length = br.ReadInt32();
         byte[] buffer = br.ReadBytes(vendor_length);
         UTF8Encoding utf8 = new UTF8Encoding();
         vendorString = utf8.GetString(buffer);

         int comment_number = br.ReadInt32();
         for (int i = 0; i < comment_number; i++)
         {
            int length = br.ReadInt32();
            buffer = br.ReadBytes(length);
            string comment2 = utf8.GetString(buffer);
            int sepindex = comment2.IndexOf('=');
            string name = comment2.Substring(0, sepindex);
            string value = comment2.Substring(sepindex + 1);
            tags.AddTag(name, value);
         }

         if (readFramingBit)
         {
            if (br.ReadByte() != 1)
               throw new FileFormatException("The framing bit is unset in Vorbis Comment block.");
         }
      }

      /// <summary>
      /// Ecrit des tags au format Vorbis Comment à partir d'un dictionnaire les contenant.
      /// </summary>
      /// <param name="writeFramingBit">Vrai si le "framing bit" doit être ajouté à la fin du block Vorbis Comment</param>
      /// <returns>Tableau d'octets contenant les tags au fomat Vorbis Comment</returns>
      public byte[] WriteVorbisComment(bool writeFramingBit)
      {
         List<string> vorbisTags = new List<string>();
         foreach (KeyValuePair<string, List<string>> kvp in tags)
         {
            foreach (string s in kvp.Value)
            {
               vorbisTags.Add(kvp.Key + "=" + s);
            }
         }

         MemoryStream ms = new MemoryStream();
         BinaryWriter bw = new BinaryWriter(ms);
         UTF8Encoding utf8 = new UTF8Encoding();
         byte[] buffer = utf8.GetBytes(vendorString);
         bw.Write(buffer.Length);
         bw.Write(buffer);
         bw.Write(vorbisTags.Count);
         for (int i = 0; i < vorbisTags.Count; i++)
         {
            buffer = utf8.GetBytes(vorbisTags[i]);
            bw.Write(buffer.Length);
            bw.Write(buffer);
         }

         if (writeFramingBit)
            bw.Write((byte)1);

         return ms.ToArray();
      }

      /// <summary>
      /// Vérifie si le tag est officiel.
      /// </summary>
      /// <param name="tag">Nom du tag</param>
      /// <returns>True si le tag est officiel, sinon False</returns>
      public static bool IsOfficialVorbisTag(string tag)
      {
         if (tag == "TITLE" ||
             tag == "VERSION" ||
             tag == "ALBUM" ||
             tag == "TRACKNUMBER" ||
             tag == "ARTIST" ||
             tag == "PERFORMER" ||
             tag == "COPYRIGHT" ||
             tag == "LICENSE" ||
             tag == "ORGANIZATION" ||
             tag == "DESCRIPTION" ||
             tag == "GENRE" ||
             tag == "DATE" ||
             tag == "LOCATION" ||
             tag == "CONTACT" ||
             tag == "ISRC")
            return true;

         return false;
      }

      /// <summary>
      /// S'assure de la validité des clefs utilisé dans les tags Vorbis Comment.
      /// </summary>
      /// <param name="key">Clef à valider</param>
      /// <returns>Valeur du tag en majuscule</returns>
      /// <remarks>Déclenche une exception en cas de présence d'un caractère non valide dans le nom du tag</remarks>
      public static string CheckedVorbisCommentKey(string key)
      {
         foreach (char c in key)
         {
            if (c < ' ' || c > '}' || c == '=')
               throw new FormatException("The Vorbis Comment key contains this invalid character: " + c);
         }

         return key.ToUpperInvariant();
      }
   }
}