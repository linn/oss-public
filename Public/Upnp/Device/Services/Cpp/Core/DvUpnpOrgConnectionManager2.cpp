#include <Core/DvUpnpOrgConnectionManager2.h>
#include <ZappTypes.h>
#include <Core/DvInvocationResponse.h>
#include <Service.h>
#include <FunctorDvInvocation.h>

using namespace Zapp;

void DvServiceUpnpOrgConnectionManager2::SetPropertySourceProtocolInfo(const Brx& aValue)
{
    SetPropertyString(*iPropertySourceProtocolInfo, aValue);
}

void DvServiceUpnpOrgConnectionManager2::GetPropertySourceProtocolInfo(Brhz& aValue)
{
    aValue.Set(iPropertySourceProtocolInfo->Value());
}

void DvServiceUpnpOrgConnectionManager2::SetPropertySinkProtocolInfo(const Brx& aValue)
{
    SetPropertyString(*iPropertySinkProtocolInfo, aValue);
}

void DvServiceUpnpOrgConnectionManager2::GetPropertySinkProtocolInfo(Brhz& aValue)
{
    aValue.Set(iPropertySinkProtocolInfo->Value());
}

void DvServiceUpnpOrgConnectionManager2::SetPropertyCurrentConnectionIDs(const Brx& aValue)
{
    SetPropertyString(*iPropertyCurrentConnectionIDs, aValue);
}

void DvServiceUpnpOrgConnectionManager2::GetPropertyCurrentConnectionIDs(Brhz& aValue)
{
    aValue.Set(iPropertyCurrentConnectionIDs->Value());
}

DvServiceUpnpOrgConnectionManager2::DvServiceUpnpOrgConnectionManager2(DvDevice& aDevice)
    : DvService(aDevice.Device(), "upnp.org", "ConnectionManager", 2)
{
    Functor empty;
    iPropertySourceProtocolInfo = new PropertyString(new ParameterString("SourceProtocolInfo"), empty);
    iService->AddProperty(iPropertySourceProtocolInfo); // passes ownership
    iPropertySinkProtocolInfo = new PropertyString(new ParameterString("SinkProtocolInfo"), empty);
    iService->AddProperty(iPropertySinkProtocolInfo); // passes ownership
    iPropertyCurrentConnectionIDs = new PropertyString(new ParameterString("CurrentConnectionIDs"), empty);
    iService->AddProperty(iPropertyCurrentConnectionIDs); // passes ownership
}

void DvServiceUpnpOrgConnectionManager2::EnableActionGetProtocolInfo()
{
    Zapp::Action* action = new Zapp::Action("GetProtocolInfo");
    action->AddOutputParameter(new ParameterRelated("Source", *iPropertySourceProtocolInfo));
    action->AddOutputParameter(new ParameterRelated("Sink", *iPropertySinkProtocolInfo));
    FunctorDvInvocation functor = MakeFunctorDvInvocation(*this, &DvServiceUpnpOrgConnectionManager2::DoGetProtocolInfo);
    iService->AddAction(action, functor);
}

void DvServiceUpnpOrgConnectionManager2::EnableActionPrepareForConnection()
{
    Zapp::Action* action = new Zapp::Action("PrepareForConnection");
    TChar** allowedValues;
    TUint index;
    action->AddInputParameter(new ParameterString("RemoteProtocolInfo"));
    action->AddInputParameter(new ParameterString("PeerConnectionManager"));
    action->AddInputParameter(new ParameterInt("PeerConnectionID"));
    index = 0;
    allowedValues = new TChar*[2];
    allowedValues[index++] = (TChar*)"Input";
    allowedValues[index++] = (TChar*)"Output";
    action->AddInputParameter(new ParameterString("Direction", allowedValues, 2));
    delete[] allowedValues;
    action->AddOutputParameter(new ParameterInt("ConnectionID"));
    action->AddOutputParameter(new ParameterInt("AVTransportID"));
    action->AddOutputParameter(new ParameterInt("RcsID"));
    FunctorDvInvocation functor = MakeFunctorDvInvocation(*this, &DvServiceUpnpOrgConnectionManager2::DoPrepareForConnection);
    iService->AddAction(action, functor);
}

void DvServiceUpnpOrgConnectionManager2::EnableActionConnectionComplete()
{
    Zapp::Action* action = new Zapp::Action("ConnectionComplete");
    action->AddInputParameter(new ParameterInt("ConnectionID"));
    FunctorDvInvocation functor = MakeFunctorDvInvocation(*this, &DvServiceUpnpOrgConnectionManager2::DoConnectionComplete);
    iService->AddAction(action, functor);
}

void DvServiceUpnpOrgConnectionManager2::EnableActionGetCurrentConnectionIDs()
{
    Zapp::Action* action = new Zapp::Action("GetCurrentConnectionIDs");
    action->AddOutputParameter(new ParameterRelated("ConnectionIDs", *iPropertyCurrentConnectionIDs));
    FunctorDvInvocation functor = MakeFunctorDvInvocation(*this, &DvServiceUpnpOrgConnectionManager2::DoGetCurrentConnectionIDs);
    iService->AddAction(action, functor);
}

void DvServiceUpnpOrgConnectionManager2::EnableActionGetCurrentConnectionInfo()
{
    Zapp::Action* action = new Zapp::Action("GetCurrentConnectionInfo");
    TChar** allowedValues;
    TUint index;
    action->AddInputParameter(new ParameterInt("ConnectionID"));
    action->AddOutputParameter(new ParameterInt("RcsID"));
    action->AddOutputParameter(new ParameterInt("AVTransportID"));
    action->AddOutputParameter(new ParameterString("ProtocolInfo"));
    action->AddOutputParameter(new ParameterString("PeerConnectionManager"));
    action->AddOutputParameter(new ParameterInt("PeerConnectionID"));
    index = 0;
    allowedValues = new TChar*[2];
    allowedValues[index++] = (TChar*)"Input";
    allowedValues[index++] = (TChar*)"Output";
    action->AddOutputParameter(new ParameterString("Direction", allowedValues, 2));
    delete[] allowedValues;
    index = 0;
    allowedValues = new TChar*[5];
    allowedValues[index++] = (TChar*)"OK";
    allowedValues[index++] = (TChar*)"ContentFormatMismatch";
    allowedValues[index++] = (TChar*)"InsufficientBandwidth";
    allowedValues[index++] = (TChar*)"UnreliableChannel";
    allowedValues[index++] = (TChar*)"Unknown";
    action->AddOutputParameter(new ParameterString("Status", allowedValues, 5));
    delete[] allowedValues;
    FunctorDvInvocation functor = MakeFunctorDvInvocation(*this, &DvServiceUpnpOrgConnectionManager2::DoGetCurrentConnectionInfo);
    iService->AddAction(action, functor);
}

void DvServiceUpnpOrgConnectionManager2::DoGetProtocolInfo(IDvInvocation& aInvocation, TUint aVersion)
{
    aInvocation.InvocationReadStart();
    aInvocation.InvocationReadEnd();
    InvocationResponse resp(aInvocation);
    InvocationResponseString respSource(aInvocation, "Source");
    InvocationResponseString respSink(aInvocation, "Sink");
    GetProtocolInfo(resp, aVersion, respSource, respSink);
}

void DvServiceUpnpOrgConnectionManager2::DoPrepareForConnection(IDvInvocation& aInvocation, TUint aVersion)
{
    aInvocation.InvocationReadStart();
    Brhz RemoteProtocolInfo;
    aInvocation.InvocationReadString("RemoteProtocolInfo", RemoteProtocolInfo);
    Brhz PeerConnectionManager;
    aInvocation.InvocationReadString("PeerConnectionManager", PeerConnectionManager);
    TInt PeerConnectionID = aInvocation.InvocationReadInt("PeerConnectionID");
    Brhz Direction;
    aInvocation.InvocationReadString("Direction", Direction);
    aInvocation.InvocationReadEnd();
    InvocationResponse resp(aInvocation);
    InvocationResponseInt respConnectionID(aInvocation, "ConnectionID");
    InvocationResponseInt respAVTransportID(aInvocation, "AVTransportID");
    InvocationResponseInt respRcsID(aInvocation, "RcsID");
    PrepareForConnection(resp, aVersion, RemoteProtocolInfo, PeerConnectionManager, PeerConnectionID, Direction, respConnectionID, respAVTransportID, respRcsID);
}

void DvServiceUpnpOrgConnectionManager2::DoConnectionComplete(IDvInvocation& aInvocation, TUint aVersion)
{
    aInvocation.InvocationReadStart();
    TInt ConnectionID = aInvocation.InvocationReadInt("ConnectionID");
    aInvocation.InvocationReadEnd();
    InvocationResponse resp(aInvocation);
    ConnectionComplete(resp, aVersion, ConnectionID);
}

void DvServiceUpnpOrgConnectionManager2::DoGetCurrentConnectionIDs(IDvInvocation& aInvocation, TUint aVersion)
{
    aInvocation.InvocationReadStart();
    aInvocation.InvocationReadEnd();
    InvocationResponse resp(aInvocation);
    InvocationResponseString respConnectionIDs(aInvocation, "ConnectionIDs");
    GetCurrentConnectionIDs(resp, aVersion, respConnectionIDs);
}

void DvServiceUpnpOrgConnectionManager2::DoGetCurrentConnectionInfo(IDvInvocation& aInvocation, TUint aVersion)
{
    aInvocation.InvocationReadStart();
    TInt ConnectionID = aInvocation.InvocationReadInt("ConnectionID");
    aInvocation.InvocationReadEnd();
    InvocationResponse resp(aInvocation);
    InvocationResponseInt respRcsID(aInvocation, "RcsID");
    InvocationResponseInt respAVTransportID(aInvocation, "AVTransportID");
    InvocationResponseString respProtocolInfo(aInvocation, "ProtocolInfo");
    InvocationResponseString respPeerConnectionManager(aInvocation, "PeerConnectionManager");
    InvocationResponseInt respPeerConnectionID(aInvocation, "PeerConnectionID");
    InvocationResponseString respDirection(aInvocation, "Direction");
    InvocationResponseString respStatus(aInvocation, "Status");
    GetCurrentConnectionInfo(resp, aVersion, ConnectionID, respRcsID, respAVTransportID, respProtocolInfo, respPeerConnectionManager, respPeerConnectionID, respDirection, respStatus);
}

void DvServiceUpnpOrgConnectionManager2::GetProtocolInfo(IInvocationResponse& /*aResponse*/, TUint /*aVersion*/, IInvocationResponseString& /*aSource*/, IInvocationResponseString& /*aSink*/)
{
    ASSERTS();
}

void DvServiceUpnpOrgConnectionManager2::PrepareForConnection(IInvocationResponse& /*aResponse*/, TUint /*aVersion*/, const Brx& /*aRemoteProtocolInfo*/, const Brx& /*aPeerConnectionManager*/, TInt /*aPeerConnectionID*/, const Brx& /*aDirection*/, IInvocationResponseInt& /*aConnectionID*/, IInvocationResponseInt& /*aAVTransportID*/, IInvocationResponseInt& /*aRcsID*/)
{
    ASSERTS();
}

void DvServiceUpnpOrgConnectionManager2::ConnectionComplete(IInvocationResponse& /*aResponse*/, TUint /*aVersion*/, TInt /*aConnectionID*/)
{
    ASSERTS();
}

void DvServiceUpnpOrgConnectionManager2::GetCurrentConnectionIDs(IInvocationResponse& /*aResponse*/, TUint /*aVersion*/, IInvocationResponseString& /*aConnectionIDs*/)
{
    ASSERTS();
}

void DvServiceUpnpOrgConnectionManager2::GetCurrentConnectionInfo(IInvocationResponse& /*aResponse*/, TUint /*aVersion*/, TInt /*aConnectionID*/, IInvocationResponseInt& /*aRcsID*/, IInvocationResponseInt& /*aAVTransportID*/, IInvocationResponseString& /*aProtocolInfo*/, IInvocationResponseString& /*aPeerConnectionManager*/, IInvocationResponseInt& /*aPeerConnectionID*/, IInvocationResponseString& /*aDirection*/, IInvocationResponseString& /*aStatus*/)
{
    ASSERTS();
}
