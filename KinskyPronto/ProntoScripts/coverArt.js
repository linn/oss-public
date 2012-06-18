/*

// V2 API - modified to be more effecient & handle uiHeader & difference between
// Radio & Playlist.

.setWidget() - define the widget to hold the cover art. (radio or playlist home page)
.clear - don't display any cover art
.noCover - display a default cover art
.fromUrl(url) - display cover art from the URL specified.

We now store the ???

Why not just have a 1/2 second task to copy the cover art data over to the uiHeader widget?
I know it's not that elegant but it would work & is not much code.

com.philips.HttpLibrary.showHTTPImage
//
// "GALLERY" has the following images we are interested in
//
// "CACHE" - a copy of what we are currently showing
// "TRANSPARENT" - a 1x1 transparent image
// "NoAlbumArt" - A image to represent no album art.
//
*/

////
//
// coverArt,js
//
// Cover Art Support
//
//
// By necessity this module is unfortunatly very Pronto specific.
// Because we can't simply use http & do a <img src="whatever"> we have to create a socket & do things the hard way.
// Also default cover art can't be easily done so we have a separate function to handle that for cases when
// we don't have any cover art.
//
////
//
//
// .refresh()
// redraw whatever we currently have.
//
// .clear():
//  Don't display any cover art.
//  i.e. cover art for no track being played (or paused etc == playlist is empty)
//
// .noCover():
//   Display default cover art as if no cover art preset for album.
//
// .fromURL(url)
//   Set the cover art to be from the URL.
//
////
//
// The image displayed is either the current cover art (if any) or suitable graphics held on the (hidden) resource page.
//
// The sort of API we need could be along these lines:
//
//image.displayCoverArt();
//image.setCoverArt(url);
//image.clearCoverArt(); // Track with no cover art
//image.displaySource(src); "cd", "tv", etc implementation TBD
//
////

////
//
// Usage: com.philips.HttpLibrary.showHTTPImage(aUrl, CF.page("NowPlaying").widget("CoverArt"), 0);
//
////

var coverArt = {};

coverArt._gallery = "GALLERY";  

coverArt.galleryPage = CF.page("GALLERY"); // Name of the Pronto Page containing the locally held images

coverArt.transparentImageData = coverArt.galleryPage.widget("TRANSPARENT").getImage();
coverArt.noImageData          = coverArt.galleryPage.widget("NoAlbumArt").getImage();
coverArt.currentImageData     = coverArt.galleryPage.widget("TRANSPARENT").getImage();

#if (VARIANT_DS)
  coverArt.widget = CF.page("NowPlaying").widget("CoverArt");
#elif (VARIANT_RADIO)
  coverArt.widget = CF.page("NowListening").widget("CoverArt");
#elif (VARIANT_PREAMP)
  coverArt.widget = CF.page("PreAmpOnly").widget("CoverArt");
#endif  


coverArt.setCoverArtFromData = function(data)
{
  coverArt.widget.visible = true;
  coverArt.widget.stretchImage = true;
  coverArt.widget.setImage(data);
  coverArt.currentImageData = data;
  uiHeader.setCoverArtData(coverArt.currentImageData);
};

coverArt.setCoverArtFromUrl = function(aUrl)
{
  coverArt.widget.visible = true;
  coverArt.widget.stretchImage = true;
  coverArt.widget.setImage(coverArt.noImageData); // In case of failure
  com.philips.HttpLibrary.showHTTPImage(aUrl, coverArt.widget, 0);
  coverArt.currentImageData = coverArt.widget.getImage();
  uiHeader.setCoverArtData(coverArt.currentImageData);
};

// External functions
//
////
//
// .refresh()
// redraw whatever we currently have.
//
coverArt.refresh = function()
{
  coverArt.setCoverArtFromData(coverArt.currentImageData);
}
//
// .clear():
//  Don't display any cover art.
//  i.e. cover art for no track being played (or paused etc == playlist is empty)
//
coverArt.clear = function()
{
  coverArt.setCoverArtFromData(coverArt.transparentImageData);
};
//
////
//
// .noCover():
//   Display default cover art as if no cover art preset for album.
//
coverArt.noCover = function()
{
  coverArt.setCoverArtFromData(coverArt.noImageData);
};
//
////
//
// .fromURL(url)
//   Set the cover art to be from the URL.
//
coverArt.fromURL = function(url)
{
  if (url == "")
  {
    coverArt.noCover();
    return;
  };
  coverArt.setCoverArtFromUrl(url);
};
//
////
//
coverArt.setDefault = coverArt.noCover;
coverArt.setDefaultCoverArt = coverArt.setDefault;
//

coverArt.tick = function()
{
  coverArt.currentImageData = coverArt.widget.getImage();
  uiHeader.setCoverArtData(coverArt.currentImageData);
  Activity.scheduleAfter(250, coverArt.tick); 
};

coverArt._start = function()
{
  coverArt.noCover();
  coverArt.tick();
};

elab.add('coverArt', null, coverArt._start);
