
using System;

using Upnp;
using Linn;
using Linn.TestFramework;


namespace TestDidlLite
{
    class SuiteDidlLite : Suite
    {
        public SuiteDidlLite() : base("Tests for Didl Lite")
        {
        }

        public override void Test()
        {
            TestTypes("");
            TestTypes(".extension");
            TestTypes(".author");
            TestTypes(".actor");
            TestTypes(".artist");

            TEST_THROWS_NEW(typeof(AssertionError), typeof(DidlLite), Create("badtype"));
            TEST_THROWS_NEW(typeof(AssertionError), typeof(DidlLite), Create("object.badtype"));
        }

        private void TestTypes(string aExtension)
        {
            DidlLite didl = null;

            didl = new DidlLite(Create("object.item" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0].GetType().FullName == "Upnp.item");

            didl = new DidlLite(Create("object.item.audioItem" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is audioItem);
            TEST(didl[0].GetType().FullName == "Upnp.audioItem");

            didl = new DidlLite(Create("object.item.audioItem.musicTrack" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is audioItem);
            TEST(didl[0] is musicTrack);
            TEST(didl[0].GetType().FullName == "Upnp.musicTrack");

            didl = new DidlLite(Create("object.item.audioItem.audioBroadcast" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is audioItem);
            TEST(didl[0] is audioBroadcast);
            TEST(didl[0].GetType().FullName == "Upnp.audioBroadcast");

            didl = new DidlLite(Create("object.item.audioItem.audioBook" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is audioItem);
            TEST(didl[0] is audioBook);
            TEST(didl[0].GetType().FullName == "Upnp.audioBook");

            didl = new DidlLite(Create("object.item.videoItem" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is videoItem);
            TEST(didl[0].GetType().FullName == "Upnp.videoItem");

            didl = new DidlLite(Create("object.item.videoItem.movie" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is videoItem);
            TEST(didl[0] is movie);
            TEST(didl[0].GetType().FullName == "Upnp.movie");

            didl = new DidlLite(Create("object.item.videoItem.videoBroadcast" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is videoItem);
            TEST(didl[0] is videoBroadcast);
            TEST(didl[0].GetType().FullName == "Upnp.videoBroadcast");

            didl = new DidlLite(Create("object.item.videoItem.musicVideoClip" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is videoItem);
            TEST(didl[0] is musicVideoClip);
            TEST(didl[0].GetType().FullName == "Upnp.musicVideoClip");

            didl = new DidlLite(Create("object.item.imageItem" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is imageItem);
            TEST(didl[0].GetType().FullName == "Upnp.imageItem");

            didl = new DidlLite(Create("object.item.imageItem.photo" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is imageItem);
            TEST(didl[0] is photo);
            TEST(didl[0].GetType().FullName == "Upnp.photo");

            didl = new DidlLite(Create("object.item.playlistItem" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is playlistItem);
            TEST(didl[0].GetType().FullName == "Upnp.playlistItem");

            didl = new DidlLite(Create("object.item.textItem" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is item);
            TEST(didl[0] is textItem);
            TEST(didl[0].GetType().FullName == "Upnp.textItem");

            didl = new DidlLite(Create("object.container" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0].GetType().FullName == "Upnp.container");

            didl = new DidlLite(Create("object.container.album" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is album);
            TEST(didl[0].GetType().FullName == "Upnp.album");

            didl = new DidlLite(Create("object.container.album.musicAlbum" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is album);
            TEST(didl[0] is musicAlbum);
            TEST(didl[0].GetType().FullName == "Upnp.musicAlbum");

            didl = new DidlLite(Create("object.container.album.photoAlbum" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is album);
            TEST(didl[0] is photoAlbum);
            TEST(didl[0].GetType().FullName == "Upnp.photoAlbum");

            didl = new DidlLite(Create("object.container.genre" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is genre);
            TEST(didl[0].GetType().FullName == "Upnp.genre");

            didl = new DidlLite(Create("object.container.genre.musicGenre" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is genre);
            TEST(didl[0] is musicGenre);
            TEST(didl[0].GetType().FullName == "Upnp.musicGenre");

            didl = new DidlLite(Create("object.container.genre.movieGenre" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is genre);
            TEST(didl[0] is movieGenre);
            TEST(didl[0].GetType().FullName == "Upnp.movieGenre");

            didl = new DidlLite(Create("object.container.playlistContainer" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is playlistContainer);
            TEST(didl[0].GetType().FullName == "Upnp.playlistContainer");

            didl = new DidlLite(Create("object.container.person" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is person);
            TEST(didl[0].GetType().FullName == "Upnp.person");

            didl = new DidlLite(Create("object.container.person.musicArtist" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is person);
            TEST(didl[0] is musicArtist);
            TEST(didl[0].GetType().FullName == "Upnp.musicArtist");

            didl = new DidlLite(Create("object.container.storageSystem" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is storageSystem);
            TEST(didl[0].GetType().FullName == "Upnp.storageSystem");

            didl = new DidlLite(Create("object.container.storageVolume" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is storageVolume);
            TEST(didl[0].GetType().FullName == "Upnp.storageVolume");

            didl = new DidlLite(Create("object.container.storageFolder" + aExtension));
            TEST(didl.Count == 1);
            TEST(didl[0] is container);
            TEST(didl[0] is storageFolder);
            TEST(didl[0].GetType().FullName == "Upnp.storageFolder");
        }

        private string Create(string aUpnpClass)
        {
            string xml = String.Format(
            "<DIDL-Lite xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                "<container id=\"32\" parentID=\"0\">" +
                    "<dc:title xmlns:dc=\"http://purl.org/dc/elements/1.1/\">Title</dc:title>" +
                    "<upnp:class xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\">{0}</upnp:class>" +
                "</container>" +
            "</DIDL-Lite>", aUpnpClass);
            return xml;
        }
    }
    
    static class TestDidlLite
    {
        static void Main(string[] aArgs)
        {
            Helper helper = new Helper(aArgs);
            helper.ProcessCommandLine();

            Runner runner = new Runner("Didl Lite tests");
            runner.Add(new SuiteDidlLite());
            runner.Run();

            helper.Dispose();
        }
    }
}




