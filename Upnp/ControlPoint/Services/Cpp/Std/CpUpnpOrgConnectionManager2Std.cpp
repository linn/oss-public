#include <Std/CpUpnpOrgConnectionManager2.h>
#include <CpProxy.h>
#include <CpiService.h>
#include <Thread.h>
#include <AsyncPrivate.h>
#include <Buffer.h>
#include <Std/CpDevice.h>

#include <string>

using namespace Zapp;


class SyncGetProtocolInfoUpnpOrgConnectionManager2Cpp : public SyncProxyAction
{
public:
    SyncGetProtocolInfoUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, std::string& aSource, std::string& aSink);
    virtual void CompleteRequest(IAsync& aAsync);
private:
    CpProxyUpnpOrgConnectionManager2Cpp& iService;
    std::string& iSource;
    std::string& iSink;
};

SyncGetProtocolInfoUpnpOrgConnectionManager2Cpp::SyncGetProtocolInfoUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, std::string& aSource, std::string& aSink)
    : iService(aService)
    , iSource(aSource)
    , iSink(aSink)
{
}

void SyncGetProtocolInfoUpnpOrgConnectionManager2Cpp::CompleteRequest(IAsync& aAsync)
{
    iService.EndGetProtocolInfo(aAsync, iSource, iSink);
}


class SyncPrepareForConnectionUpnpOrgConnectionManager2Cpp : public SyncProxyAction
{
public:
    SyncPrepareForConnectionUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, int32_t& aConnectionID, int32_t& aAVTransportID, int32_t& aRcsID);
    virtual void CompleteRequest(IAsync& aAsync);
private:
    CpProxyUpnpOrgConnectionManager2Cpp& iService;
    int32_t& iConnectionID;
    int32_t& iAVTransportID;
    int32_t& iRcsID;
};

SyncPrepareForConnectionUpnpOrgConnectionManager2Cpp::SyncPrepareForConnectionUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, int32_t& aConnectionID, int32_t& aAVTransportID, int32_t& aRcsID)
    : iService(aService)
    , iConnectionID(aConnectionID)
    , iAVTransportID(aAVTransportID)
    , iRcsID(aRcsID)
{
}

void SyncPrepareForConnectionUpnpOrgConnectionManager2Cpp::CompleteRequest(IAsync& aAsync)
{
    iService.EndPrepareForConnection(aAsync, iConnectionID, iAVTransportID, iRcsID);
}


class SyncConnectionCompleteUpnpOrgConnectionManager2Cpp : public SyncProxyAction
{
public:
    SyncConnectionCompleteUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService);
    virtual void CompleteRequest(IAsync& aAsync);
private:
    CpProxyUpnpOrgConnectionManager2Cpp& iService;
};

SyncConnectionCompleteUpnpOrgConnectionManager2Cpp::SyncConnectionCompleteUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService)
    : iService(aService)
{
}

void SyncConnectionCompleteUpnpOrgConnectionManager2Cpp::CompleteRequest(IAsync& aAsync)
{
    iService.EndConnectionComplete(aAsync);
}


class SyncGetCurrentConnectionIDsUpnpOrgConnectionManager2Cpp : public SyncProxyAction
{
public:
    SyncGetCurrentConnectionIDsUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, std::string& aConnectionIDs);
    virtual void CompleteRequest(IAsync& aAsync);
private:
    CpProxyUpnpOrgConnectionManager2Cpp& iService;
    std::string& iConnectionIDs;
};

SyncGetCurrentConnectionIDsUpnpOrgConnectionManager2Cpp::SyncGetCurrentConnectionIDsUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, std::string& aConnectionIDs)
    : iService(aService)
    , iConnectionIDs(aConnectionIDs)
{
}

void SyncGetCurrentConnectionIDsUpnpOrgConnectionManager2Cpp::CompleteRequest(IAsync& aAsync)
{
    iService.EndGetCurrentConnectionIDs(aAsync, iConnectionIDs);
}


class SyncGetCurrentConnectionInfoUpnpOrgConnectionManager2Cpp : public SyncProxyAction
{
public:
    SyncGetCurrentConnectionInfoUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, int32_t& aRcsID, int32_t& aAVTransportID, std::string& aProtocolInfo, std::string& aPeerConnectionManager, int32_t& aPeerConnectionID, std::string& aDirection, std::string& aStatus);
    virtual void CompleteRequest(IAsync& aAsync);
private:
    CpProxyUpnpOrgConnectionManager2Cpp& iService;
    int32_t& iRcsID;
    int32_t& iAVTransportID;
    std::string& iProtocolInfo;
    std::string& iPeerConnectionManager;
    int32_t& iPeerConnectionID;
    std::string& iDirection;
    std::string& iStatus;
};

SyncGetCurrentConnectionInfoUpnpOrgConnectionManager2Cpp::SyncGetCurrentConnectionInfoUpnpOrgConnectionManager2Cpp(CpProxyUpnpOrgConnectionManager2Cpp& aService, int32_t& aRcsID, int32_t& aAVTransportID, std::string& aProtocolInfo, std::string& aPeerConnectionManager, int32_t& aPeerConnectionID, std::string& aDirection, std::string& aStatus)
    : iService(aService)
    , iRcsID(aRcsID)
    , iAVTransportID(aAVTransportID)
    , iProtocolInfo(aProtocolInfo)
    , iPeerConnectionManager(aPeerConnectionManager)
    , iPeerConnectionID(aPeerConnectionID)
    , iDirection(aDirection)
    , iStatus(aStatus)
{
}

void SyncGetCurrentConnectionInfoUpnpOrgConnectionManager2Cpp::CompleteRequest(IAsync& aAsync)
{
    iService.EndGetCurrentConnectionInfo(aAsync, iRcsID, iAVTransportID, iProtocolInfo, iPeerConnectionManager, iPeerConnectionID, iDirection, iStatus);
}


CpProxyUpnpOrgConnectionManager2Cpp::CpProxyUpnpOrgConnectionManager2Cpp(CpDeviceCpp& aDevice)
{
    iService = new CpiService("schemas-upnp-org", "ConnectionManager", 2, aDevice.Device());
    Zapp::Parameter* param;
    TChar** allowedValues;
    TUint index;

    iActionGetProtocolInfo = new Action("GetProtocolInfo");
    param = new Zapp::ParameterString("Source");
    iActionGetProtocolInfo->AddOutputParameter(param);
    param = new Zapp::ParameterString("Sink");
    iActionGetProtocolInfo->AddOutputParameter(param);

    iActionPrepareForConnection = new Action("PrepareForConnection");
    param = new Zapp::ParameterString("RemoteProtocolInfo");
    iActionPrepareForConnection->AddInputParameter(param);
    param = new Zapp::ParameterString("PeerConnectionManager");
    iActionPrepareForConnection->AddInputParameter(param);
    param = new Zapp::ParameterInt("PeerConnectionID");
    iActionPrepareForConnection->AddInputParameter(param);
    index = 0;
    allowedValues = new TChar*[2];
    allowedValues[index++] = (TChar*)"Input";
    allowedValues[index++] = (TChar*)"Output";
    param = new Zapp::ParameterString("Direction", allowedValues, 2);
    iActionPrepareForConnection->AddInputParameter(param);
    delete[] allowedValues;
    param = new Zapp::ParameterInt("ConnectionID");
    iActionPrepareForConnection->AddOutputParameter(param);
    param = new Zapp::ParameterInt("AVTransportID");
    iActionPrepareForConnection->AddOutputParameter(param);
    param = new Zapp::ParameterInt("RcsID");
    iActionPrepareForConnection->AddOutputParameter(param);

    iActionConnectionComplete = new Action("ConnectionComplete");
    param = new Zapp::ParameterInt("ConnectionID");
    iActionConnectionComplete->AddInputParameter(param);

    iActionGetCurrentConnectionIDs = new Action("GetCurrentConnectionIDs");
    param = new Zapp::ParameterString("ConnectionIDs");
    iActionGetCurrentConnectionIDs->AddOutputParameter(param);

    iActionGetCurrentConnectionInfo = new Action("GetCurrentConnectionInfo");
    param = new Zapp::ParameterInt("ConnectionID");
    iActionGetCurrentConnectionInfo->AddInputParameter(param);
    param = new Zapp::ParameterInt("RcsID");
    iActionGetCurrentConnectionInfo->AddOutputParameter(param);
    param = new Zapp::ParameterInt("AVTransportID");
    iActionGetCurrentConnectionInfo->AddOutputParameter(param);
    param = new Zapp::ParameterString("ProtocolInfo");
    iActionGetCurrentConnectionInfo->AddOutputParameter(param);
    param = new Zapp::ParameterString("PeerConnectionManager");
    iActionGetCurrentConnectionInfo->AddOutputParameter(param);
    param = new Zapp::ParameterInt("PeerConnectionID");
    iActionGetCurrentConnectionInfo->AddOutputParameter(param);
    index = 0;
    allowedValues = new TChar*[2];
    allowedValues[index++] = (TChar*)"Input";
    allowedValues[index++] = (TChar*)"Output";
    param = new Zapp::ParameterString("Direction", allowedValues, 2);
    iActionGetCurrentConnectionInfo->AddOutputParameter(param);
    delete[] allowedValues;
    index = 0;
    allowedValues = new TChar*[5];
    allowedValues[index++] = (TChar*)"OK";
    allowedValues[index++] = (TChar*)"ContentFormatMismatch";
    allowedValues[index++] = (TChar*)"InsufficientBandwidth";
    allowedValues[index++] = (TChar*)"UnreliableChannel";
    allowedValues[index++] = (TChar*)"Unknown";
    param = new Zapp::ParameterString("Status", allowedValues, 5);
    iActionGetCurrentConnectionInfo->AddOutputParameter(param);
    delete[] allowedValues;

    Functor functor;
    functor = MakeFunctor(*this, &CpProxyUpnpOrgConnectionManager2Cpp::SourceProtocolInfoPropertyChanged);
    iSourceProtocolInfo = new PropertyString("SourceProtocolInfo", functor);
    iService->AddProperty(iSourceProtocolInfo);
    functor = MakeFunctor(*this, &CpProxyUpnpOrgConnectionManager2Cpp::SinkProtocolInfoPropertyChanged);
    iSinkProtocolInfo = new PropertyString("SinkProtocolInfo", functor);
    iService->AddProperty(iSinkProtocolInfo);
    functor = MakeFunctor(*this, &CpProxyUpnpOrgConnectionManager2Cpp::CurrentConnectionIDsPropertyChanged);
    iCurrentConnectionIDs = new PropertyString("CurrentConnectionIDs", functor);
    iService->AddProperty(iCurrentConnectionIDs);
}

CpProxyUpnpOrgConnectionManager2Cpp::~CpProxyUpnpOrgConnectionManager2Cpp()
{
    delete iService;
    delete iActionGetProtocolInfo;
    delete iActionPrepareForConnection;
    delete iActionConnectionComplete;
    delete iActionGetCurrentConnectionIDs;
    delete iActionGetCurrentConnectionInfo;
}

void CpProxyUpnpOrgConnectionManager2Cpp::SyncGetProtocolInfo(std::string& aSource, std::string& aSink)
{
    SyncGetProtocolInfoUpnpOrgConnectionManager2Cpp sync(*this, aSource, aSink);
    BeginGetProtocolInfo(sync.Functor());
    sync.Wait();
}

void CpProxyUpnpOrgConnectionManager2Cpp::BeginGetProtocolInfo(FunctorAsync& aFunctor)
{
    Invocation* invocation = iService->Invocation(*iActionGetProtocolInfo, aFunctor);
    TUint outIndex = 0;
    const Action::VectorParameters& outParams = iActionGetProtocolInfo->OutputParameters();
    invocation->AddOutput(new ArgumentString(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentString(*outParams[outIndex++]));
    invocation->Invoke();
}

void CpProxyUpnpOrgConnectionManager2Cpp::EndGetProtocolInfo(IAsync& aAsync, std::string& aSource, std::string& aSink)
{
    ASSERT(((Async&)aAsync).Type() == Async::eInvocation);
    Invocation& invocation = (Invocation&)aAsync;
    ASSERT(invocation.Action().Name() == Brn("GetProtocolInfo"));

    if (invocation.Error()) {
        THROW(ProxyError);
    }
    TUint index = 0;
    {
        const Brx& val = ((ArgumentString*)invocation.OutputArguments()[index++])->Value();
        aSource.assign((const char*)val.Ptr(), val.Bytes());
    }
    {
        const Brx& val = ((ArgumentString*)invocation.OutputArguments()[index++])->Value();
        aSink.assign((const char*)val.Ptr(), val.Bytes());
    }
}

void CpProxyUpnpOrgConnectionManager2Cpp::SyncPrepareForConnection(const std::string& aRemoteProtocolInfo, const std::string& aPeerConnectionManager, int32_t aPeerConnectionID, const std::string& aDirection, int32_t& aConnectionID, int32_t& aAVTransportID, int32_t& aRcsID)
{
    SyncPrepareForConnectionUpnpOrgConnectionManager2Cpp sync(*this, aConnectionID, aAVTransportID, aRcsID);
    BeginPrepareForConnection(aRemoteProtocolInfo, aPeerConnectionManager, aPeerConnectionID, aDirection, sync.Functor());
    sync.Wait();
}

void CpProxyUpnpOrgConnectionManager2Cpp::BeginPrepareForConnection(const std::string& aRemoteProtocolInfo, const std::string& aPeerConnectionManager, int32_t aPeerConnectionID, const std::string& aDirection, FunctorAsync& aFunctor)
{
    Invocation* invocation = iService->Invocation(*iActionPrepareForConnection, aFunctor);
    TUint inIndex = 0;
    const Action::VectorParameters& inParams = iActionPrepareForConnection->InputParameters();
    {
        Brn buf((const TByte*)aRemoteProtocolInfo.c_str(), aRemoteProtocolInfo.length());
        invocation->AddInput(new ArgumentString(*inParams[inIndex++], buf));
    }
    {
        Brn buf((const TByte*)aPeerConnectionManager.c_str(), aPeerConnectionManager.length());
        invocation->AddInput(new ArgumentString(*inParams[inIndex++], buf));
    }
    invocation->AddInput(new ArgumentInt(*inParams[inIndex++], aPeerConnectionID));
    {
        Brn buf((const TByte*)aDirection.c_str(), aDirection.length());
        invocation->AddInput(new ArgumentString(*inParams[inIndex++], buf));
    }
    TUint outIndex = 0;
    const Action::VectorParameters& outParams = iActionPrepareForConnection->OutputParameters();
    invocation->AddOutput(new ArgumentInt(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentInt(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentInt(*outParams[outIndex++]));
    invocation->Invoke();
}

void CpProxyUpnpOrgConnectionManager2Cpp::EndPrepareForConnection(IAsync& aAsync, int32_t& aConnectionID, int32_t& aAVTransportID, int32_t& aRcsID)
{
    ASSERT(((Async&)aAsync).Type() == Async::eInvocation);
    Invocation& invocation = (Invocation&)aAsync;
    ASSERT(invocation.Action().Name() == Brn("PrepareForConnection"));

    if (invocation.Error()) {
        THROW(ProxyError);
    }
    TUint index = 0;
    aConnectionID = ((ArgumentInt*)invocation.OutputArguments()[index++])->Value();
    aAVTransportID = ((ArgumentInt*)invocation.OutputArguments()[index++])->Value();
    aRcsID = ((ArgumentInt*)invocation.OutputArguments()[index++])->Value();
}

void CpProxyUpnpOrgConnectionManager2Cpp::SyncConnectionComplete(int32_t aConnectionID)
{
    SyncConnectionCompleteUpnpOrgConnectionManager2Cpp sync(*this);
    BeginConnectionComplete(aConnectionID, sync.Functor());
    sync.Wait();
}

void CpProxyUpnpOrgConnectionManager2Cpp::BeginConnectionComplete(int32_t aConnectionID, FunctorAsync& aFunctor)
{
    Invocation* invocation = iService->Invocation(*iActionConnectionComplete, aFunctor);
    TUint inIndex = 0;
    const Action::VectorParameters& inParams = iActionConnectionComplete->InputParameters();
    invocation->AddInput(new ArgumentInt(*inParams[inIndex++], aConnectionID));
    invocation->Invoke();
}

void CpProxyUpnpOrgConnectionManager2Cpp::EndConnectionComplete(IAsync& aAsync)
{
    ASSERT(((Async&)aAsync).Type() == Async::eInvocation);
    Invocation& invocation = (Invocation&)aAsync;
    ASSERT(invocation.Action().Name() == Brn("ConnectionComplete"));

    if (invocation.Error()) {
        THROW(ProxyError);
    }
}

void CpProxyUpnpOrgConnectionManager2Cpp::SyncGetCurrentConnectionIDs(std::string& aConnectionIDs)
{
    SyncGetCurrentConnectionIDsUpnpOrgConnectionManager2Cpp sync(*this, aConnectionIDs);
    BeginGetCurrentConnectionIDs(sync.Functor());
    sync.Wait();
}

void CpProxyUpnpOrgConnectionManager2Cpp::BeginGetCurrentConnectionIDs(FunctorAsync& aFunctor)
{
    Invocation* invocation = iService->Invocation(*iActionGetCurrentConnectionIDs, aFunctor);
    TUint outIndex = 0;
    const Action::VectorParameters& outParams = iActionGetCurrentConnectionIDs->OutputParameters();
    invocation->AddOutput(new ArgumentString(*outParams[outIndex++]));
    invocation->Invoke();
}

void CpProxyUpnpOrgConnectionManager2Cpp::EndGetCurrentConnectionIDs(IAsync& aAsync, std::string& aConnectionIDs)
{
    ASSERT(((Async&)aAsync).Type() == Async::eInvocation);
    Invocation& invocation = (Invocation&)aAsync;
    ASSERT(invocation.Action().Name() == Brn("GetCurrentConnectionIDs"));

    if (invocation.Error()) {
        THROW(ProxyError);
    }
    TUint index = 0;
    {
        const Brx& val = ((ArgumentString*)invocation.OutputArguments()[index++])->Value();
        aConnectionIDs.assign((const char*)val.Ptr(), val.Bytes());
    }
}

void CpProxyUpnpOrgConnectionManager2Cpp::SyncGetCurrentConnectionInfo(int32_t aConnectionID, int32_t& aRcsID, int32_t& aAVTransportID, std::string& aProtocolInfo, std::string& aPeerConnectionManager, int32_t& aPeerConnectionID, std::string& aDirection, std::string& aStatus)
{
    SyncGetCurrentConnectionInfoUpnpOrgConnectionManager2Cpp sync(*this, aRcsID, aAVTransportID, aProtocolInfo, aPeerConnectionManager, aPeerConnectionID, aDirection, aStatus);
    BeginGetCurrentConnectionInfo(aConnectionID, sync.Functor());
    sync.Wait();
}

void CpProxyUpnpOrgConnectionManager2Cpp::BeginGetCurrentConnectionInfo(int32_t aConnectionID, FunctorAsync& aFunctor)
{
    Invocation* invocation = iService->Invocation(*iActionGetCurrentConnectionInfo, aFunctor);
    TUint inIndex = 0;
    const Action::VectorParameters& inParams = iActionGetCurrentConnectionInfo->InputParameters();
    invocation->AddInput(new ArgumentInt(*inParams[inIndex++], aConnectionID));
    TUint outIndex = 0;
    const Action::VectorParameters& outParams = iActionGetCurrentConnectionInfo->OutputParameters();
    invocation->AddOutput(new ArgumentInt(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentInt(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentString(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentString(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentInt(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentString(*outParams[outIndex++]));
    invocation->AddOutput(new ArgumentString(*outParams[outIndex++]));
    invocation->Invoke();
}

void CpProxyUpnpOrgConnectionManager2Cpp::EndGetCurrentConnectionInfo(IAsync& aAsync, int32_t& aRcsID, int32_t& aAVTransportID, std::string& aProtocolInfo, std::string& aPeerConnectionManager, int32_t& aPeerConnectionID, std::string& aDirection, std::string& aStatus)
{
    ASSERT(((Async&)aAsync).Type() == Async::eInvocation);
    Invocation& invocation = (Invocation&)aAsync;
    ASSERT(invocation.Action().Name() == Brn("GetCurrentConnectionInfo"));

    if (invocation.Error()) {
        THROW(ProxyError);
    }
    TUint index = 0;
    aRcsID = ((ArgumentInt*)invocation.OutputArguments()[index++])->Value();
    aAVTransportID = ((ArgumentInt*)invocation.OutputArguments()[index++])->Value();
    {
        const Brx& val = ((ArgumentString*)invocation.OutputArguments()[index++])->Value();
        aProtocolInfo.assign((const char*)val.Ptr(), val.Bytes());
    }
    {
        const Brx& val = ((ArgumentString*)invocation.OutputArguments()[index++])->Value();
        aPeerConnectionManager.assign((const char*)val.Ptr(), val.Bytes());
    }
    aPeerConnectionID = ((ArgumentInt*)invocation.OutputArguments()[index++])->Value();
    {
        const Brx& val = ((ArgumentString*)invocation.OutputArguments()[index++])->Value();
        aDirection.assign((const char*)val.Ptr(), val.Bytes());
    }
    {
        const Brx& val = ((ArgumentString*)invocation.OutputArguments()[index++])->Value();
        aStatus.assign((const char*)val.Ptr(), val.Bytes());
    }
}

void CpProxyUpnpOrgConnectionManager2Cpp::SetPropertySourceProtocolInfoChanged(Functor& aFunctor)
{
    iLock->Wait();
    iSourceProtocolInfoChanged = aFunctor;
    iLock->Signal();
}

void CpProxyUpnpOrgConnectionManager2Cpp::SetPropertySinkProtocolInfoChanged(Functor& aFunctor)
{
    iLock->Wait();
    iSinkProtocolInfoChanged = aFunctor;
    iLock->Signal();
}

void CpProxyUpnpOrgConnectionManager2Cpp::SetPropertyCurrentConnectionIDsChanged(Functor& aFunctor)
{
    iLock->Wait();
    iCurrentConnectionIDsChanged = aFunctor;
    iLock->Signal();
}

void CpProxyUpnpOrgConnectionManager2Cpp::PropertySourceProtocolInfo(std::string& aSourceProtocolInfo) const
{
    ASSERT(iCpSubscriptionStatus == CpProxy::eSubscribed);
    const Brx& val = iSourceProtocolInfo->Value();
    aSourceProtocolInfo.assign((const char*)val.Ptr(), val.Bytes());
}

void CpProxyUpnpOrgConnectionManager2Cpp::PropertySinkProtocolInfo(std::string& aSinkProtocolInfo) const
{
    ASSERT(iCpSubscriptionStatus == CpProxy::eSubscribed);
    const Brx& val = iSinkProtocolInfo->Value();
    aSinkProtocolInfo.assign((const char*)val.Ptr(), val.Bytes());
}

void CpProxyUpnpOrgConnectionManager2Cpp::PropertyCurrentConnectionIDs(std::string& aCurrentConnectionIDs) const
{
    ASSERT(iCpSubscriptionStatus == CpProxy::eSubscribed);
    const Brx& val = iCurrentConnectionIDs->Value();
    aCurrentConnectionIDs.assign((const char*)val.Ptr(), val.Bytes());
}

void CpProxyUpnpOrgConnectionManager2Cpp::SourceProtocolInfoPropertyChanged()
{
    if (!ReportEvent()) {
        return;
    }
    AutoMutex a(*iLock);
    if (iCpSubscriptionStatus == CpProxy::eSubscribed && iSourceProtocolInfoChanged != NULL) {
        iSourceProtocolInfoChanged();
    }
}

void CpProxyUpnpOrgConnectionManager2Cpp::SinkProtocolInfoPropertyChanged()
{
    if (!ReportEvent()) {
        return;
    }
    AutoMutex a(*iLock);
    if (iCpSubscriptionStatus == CpProxy::eSubscribed && iSinkProtocolInfoChanged != NULL) {
        iSinkProtocolInfoChanged();
    }
}

void CpProxyUpnpOrgConnectionManager2Cpp::CurrentConnectionIDsPropertyChanged()
{
    if (!ReportEvent()) {
        return;
    }
    AutoMutex a(*iLock);
    if (iCpSubscriptionStatus == CpProxy::eSubscribed && iCurrentConnectionIDsChanged != NULL) {
        iCurrentConnectionIDsChanged();
    }
}
