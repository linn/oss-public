msbuild Vs2008\Kodegen\Kodegen.csproj /p:Configuration="Windows Release"
msbuild Vs2008\XmlUpnp\XmlUpnp.csproj /p:Configuration="Windows Release"
msbuild Vs2008\XmlDidlLite\XmlDidlLite.csproj /p:Configuration="Windows Release"

mkdir install\share\Kodegen
copy Kodegen\Kode\*.kode install\share\Kodegen
copy LibUpnpCil\DidlLite\UpnpAv\*.kode install\share\Kodegen

mkdir install\share\Services\Linn
copy LibUpnpCil\Services\Linn\*.xml install\share\Services\Linn

mkdir install\share\Services\Openhome
copy LibUpnpCil\Services\Openhome\*.xml install\share\Services\Openhome

mkdir install\share\Services\Sonos
copy LibUpnpCil\Services\Sonos\*.xml install\share\Services\Sonos

mkdir install\share\Services\UpnpAv
copy LibUpnpCil\Services\UpnpAv\*.xml install\share\Services\UpnpAv
copy LibUpnpCil\DidlLite\UpnpAv\*.xml install\share\Services\UpnpAv

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointConfiguration.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Configuration.xml linn.co.uk Configuration 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointDebug.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Debug.xml linn.co.uk Debug 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointDelay.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Delay.xml linn.co.uk Delay 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointDiagnostics.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Diagnostics.xml linn.co.uk Diagnostics 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointFlash.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Flash.xml linn.co.uk Flash 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointInfo.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Info1.xml av.openhome.org Info 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointJukebox.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Jukebox.xml linn.co.uk Jukebox 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointPlaylist.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Playlist1.xml av.openhome.org Playlist 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointPlaylistManager.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\PlaylistManager1.xml av.openhome.org PlaylistManager 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointProductV1.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\ProductV1.xml linn.co.uk Product 1 V1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointProductV2.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\ProductV2.xml linn.co.uk Product 2 V2

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointProduct.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Product.xml linn.co.uk Product 3 V3

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointProduct.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Product1.xml av.openhome.org Product 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointProxy.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Proxy.xml linn.co.uk Proxy 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointRadio.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Radio1.xml av.openhome.org Radio 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointReceiver.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Receiver1.xml av.openhome.org Receiver 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointSdp.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Sdp.xml linn.co.uk Sdp 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointSender.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Sender1.xml av.openhome.org Sender 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointSoundcard.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Soundcard.xml linn.co.uk Soundcard 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointTime.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Time1.xml av.openhome.org Time 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointVideo.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Video.xml linn.co.uk Video 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointVolkano.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Linn\Volkano.xml linn.co.uk Volkano 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointVolume.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Openhome\Volume1.xml av.openhome.org Volume 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointDeviceProperties.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\Sonos\DeviceProperties.xml upnp.org DeviceProperties 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointAVTransport.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\UpnpAv\AVTransport.xml upnp.org AVTransport 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointConnectionManager.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\UpnpAv\ConnectionManager.xml upnp.org ConnectionManager 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointContentDirectory.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\UpnpAv\ContentDirectory.xml upnp.org ContentDirectory 1

install\Windows\release\bin\Kodegen.exe build\share\Services\UpnpControlPointRenderingControl.cs install\share\Kodegen\UpnpControlPointCs.kode install\share\Services\UpnpAv\RenderingControl.xml upnp.org RenderingControl 1

install\Windows\release\bin\Kodegen.exe build\share\DidlLite\DidlLite.cs install\share\Kodegen\DidlLiteCs.kode install\share\Services\UpnpAv\DidlLiteDescription.xml

mkdir install\Windows\share\Linn\Resources\KinskyDesktop\
copy Layouts\Kinsky\Desktop\Icons\*.png install\Windows\share\Linn\Resources\KinskyDesktop\
copy Layouts\Kinsky\Desktop\Images\*.png install\Windows\share\Linn\Resources\KinskyDesktop\

mkdir install\MacOsX\share\Linn\Resources\KinskyDesktop\
copy Layouts\Kinsky\Desktop\Icons\*.png install\MacOsX\share\Linn\Resources\KinskyDesktop\
copy Layouts\Kinsky\Desktop\Images\*.png install\MacOsX\share\Linn\Resources\KinskyDesktop\

mkdir install\Linux\share\Linn\Resources\KinskyDesktop\
copy Layouts\Kinsky\Desktop\Icons\*.png install\Linux\share\Linn\Resources\KinskyDesktop\
copy Layouts\Kinsky\Desktop\Images\*.png install\Linux\share\Linn\Resources\KinskyDesktop\
