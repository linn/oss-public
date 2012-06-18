/**
 * Ssdp utilities which depend on the device stack
 * Placed in a separate file to encourage the linker to drop dv code for cp builds
 */

#include <Ssdp.h>
#include <Http.h>
#include <Ascii.h>
#include <Stream.h>
#include <DviStack.h>

using namespace Zapp;

// Ssdp (fragments)

void Ssdp::WriteBootId(IWriterHttpHeader& aWriter)
{
    IWriterAscii& stream = aWriter.WriteHeaderField(Ssdp::kHeaderBootId);
    stream.WriteUint(DviStack::BootId());
    stream.WriteFlush();
}

void Ssdp::WriteNextBootId(IWriterHttpHeader& aWriter)
{
    IWriterAscii& stream = aWriter.WriteHeaderField(Ssdp::kHeaderNextBootId);
    stream.WriteUint(DviStack::NextBootId());
    stream.WriteFlush();
}


// SsdpNotifier

SsdpNotifier::SsdpNotifier(TIpAddress aInterface, TUint aConfigId)
    : iSocket(Endpoint(Ssdp::kMulticastPort, Ssdp::kMulticastAddress), kTimeToLive, aInterface)
    , iBuffer(iSocket)
    , iWriter(iBuffer)
    , iInterface(aInterface)
    , iConfigId(aConfigId)
{
}

void SsdpNotifier::SsdpNotify(const Brx& aUri, ENotificationType aNotificationType)
{
    Ssdp::WriteMethodNotify(iWriter);
    Ssdp::WriteHost(iWriter);
    Ssdp::WriteBootId(iWriter);
    Ssdp::WriteConfigId(iWriter, iConfigId);
    switch (aNotificationType)
    {
    case EAlive:
        Ssdp::WriteServer(iWriter);
        Ssdp::WriteMaxAge(iWriter);
        Ssdp::WriteLocation(iWriter, aUri);
        Ssdp::WriteSubTypeAlive(iWriter);
        // !!!! Ssdp::WriteSearchPort(iWriter, ????);
        break;
    case EByeBye:
        Ssdp::WriteSubTypeByeBye(iWriter);
        break;
    case EUpdate:
        Ssdp::WriteNextBootId(iWriter);
        break;
    }
}

void SsdpNotifier::SsdpNotifyRoot(const Brx& aUuid, const Brx& aUri, ENotificationType aNotificationType)
{
    SsdpNotify(aUri, aNotificationType);
    Ssdp::WriteNotificationTypeRoot(iWriter);
    Ssdp::WriteUsnRoot(iWriter, aUuid);
    iWriter.WriteFlush();
}

void SsdpNotifier::SsdpNotifyUuid(const Brx& aUuid, const Brx& aUri, ENotificationType aNotificationType)
{
    SsdpNotify(aUri, aNotificationType);
    Ssdp::WriteNotificationTypeUuid(iWriter, aUuid);
    Ssdp::WriteUsnUuid(iWriter, aUuid);
    iWriter.WriteFlush();
}

void SsdpNotifier::SsdpNotifyDeviceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri, ENotificationType aNotificationType)
{
    SsdpNotify(aUri, aNotificationType);
    Ssdp::WriteNotificationTypeDeviceType(iWriter, aDomain, aType, aVersion);
    Ssdp::WriteUsnDeviceType(iWriter, aDomain, aType, aVersion, aUuid);
    iWriter.WriteFlush();
}

void SsdpNotifier::SsdpNotifyServiceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri, ENotificationType aNotificationType)
{
    SsdpNotify(aUri, aNotificationType);
    Ssdp::WriteNotificationTypeServiceType(iWriter, aDomain, aType, aVersion);
    Ssdp::WriteUsnServiceType(iWriter, aDomain, aType, aVersion, aUuid);
    iWriter.WriteFlush();
}


// SsdpNotifierAlive

SsdpNotifierAlive::SsdpNotifierAlive(SsdpNotifier& aNotifier)
    : iNotifier(aNotifier)
{
}

void SsdpNotifierAlive::SsdpNotifyRoot(const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyRoot(aUuid, aUri, SsdpNotifier::EAlive);
}

void SsdpNotifierAlive::SsdpNotifyUuid(const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyUuid(aUuid, aUri, SsdpNotifier::EAlive);
}

void SsdpNotifierAlive::SsdpNotifyDeviceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyDeviceType(aDomain, aType, aVersion, aUuid, aUri, SsdpNotifier::EAlive);
}

void SsdpNotifierAlive::SsdpNotifyServiceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyServiceType(aDomain, aType, aVersion, aUuid, aUri, SsdpNotifier::EAlive);
}


// SsdpNotifierByeBye

SsdpNotifierByeBye::SsdpNotifierByeBye(SsdpNotifier& aNotifier)
    : iNotifier(aNotifier)
{
}

void SsdpNotifierByeBye::SsdpNotifyRoot(const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyRoot(aUuid, aUri, SsdpNotifier::EByeBye);
}

void SsdpNotifierByeBye::SsdpNotifyUuid(const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyUuid(aUuid, aUri, SsdpNotifier::EByeBye);
}

void SsdpNotifierByeBye::SsdpNotifyDeviceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyDeviceType(aDomain, aType, aVersion, aUuid, aUri, SsdpNotifier::EByeBye);
}

void SsdpNotifierByeBye::SsdpNotifyServiceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyServiceType(aDomain, aType, aVersion, aUuid, aUri, SsdpNotifier::EByeBye);
}


// SsdpNotifierUpdate

SsdpNotifierUpdate::SsdpNotifierUpdate(SsdpNotifier& aNotifier)
    : iNotifier(aNotifier)
{
}

void SsdpNotifierUpdate::SsdpNotifyRoot(const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyRoot(aUuid, aUri, SsdpNotifier::EUpdate);
}

void SsdpNotifierUpdate::SsdpNotifyUuid(const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyUuid(aUuid, aUri, SsdpNotifier::EUpdate);
}

void SsdpNotifierUpdate::SsdpNotifyDeviceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyDeviceType(aDomain, aType, aVersion, aUuid, aUri, SsdpNotifier::EUpdate);
}

void SsdpNotifierUpdate::SsdpNotifyServiceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    iNotifier.SsdpNotifyServiceType(aDomain, aType, aVersion, aUuid, aUri, SsdpNotifier::EUpdate);
}


// SsdpMsearchResponder

SsdpMsearchResponder::SsdpMsearchResponder(TUint aConfigId)
    : iBuffer(iResponse)
    , iWriter(iBuffer)
    , iConfigId(aConfigId)
{
}

void SsdpMsearchResponder::SetRemote(const Endpoint& aEndpoint)
{
	iRemote.Replace(aEndpoint);
}

void SsdpMsearchResponder::SsdpNotify(const Brx& aUri)
{
    Ssdp::WriteStatus(iWriter);
    Ssdp::WriteServer(iWriter);
    Ssdp::WriteMaxAge(iWriter);
    Ssdp::WriteExt(iWriter);
    Ssdp::WriteLocation(iWriter, aUri);
    Ssdp::WriteBootId(iWriter);
    Ssdp::WriteConfigId(iWriter, iConfigId);
    // !!!! Ssdp::WriteSearchPort(iWriter, ????);
}

void SsdpMsearchResponder::Flush()
{
    iWriter.WriteFlush();
    SocketUdpClient socket(iRemote);
    socket.Send(iResponse);
    iBuffer.Flush();
}

void SsdpMsearchResponder::SsdpNotifyRoot(const Brx& aUuid, const Brx& aUri)
{
    SsdpNotify(aUri);
    Ssdp::WriteSearchTypeRoot(iWriter);
    Ssdp::WriteUsnRoot(iWriter, aUuid);
    Flush();
}

void SsdpMsearchResponder::SsdpNotifyUuid(const Brx& aUuid, const Brx& aUri)
{
    SsdpNotify(aUri);
    Ssdp::WriteSearchTypeUuid(iWriter, aUuid);
    Ssdp::WriteUsnUuid(iWriter, aUuid);
    Flush();
}

void SsdpMsearchResponder::SsdpNotifyDeviceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    SsdpNotify(aUri);
    Ssdp::WriteSearchTypeDeviceType(iWriter, aDomain, aType, aVersion);
    Ssdp::WriteUsnDeviceType(iWriter, aDomain, aType, aVersion, aUuid);
    Flush();
}

void SsdpMsearchResponder::SsdpNotifyServiceType(const Brx& aDomain, const Brx& aType, TUint aVersion, const Brx& aUuid, const Brx& aUri)
{
    SsdpNotify(aUri);
    Ssdp::WriteSearchTypeServiceType(iWriter, aDomain, aType, aVersion);
    Ssdp::WriteUsnServiceType(iWriter, aDomain, aType, aVersion, aUuid);
    Flush();
}
