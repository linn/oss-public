#ifndef HEADER_LINNCOUKPREAMP4CPP
#define HEADER_LINNCOUKPREAMP4CPP

#include <ZappTypes.h>
#include <Exception.h>
#include <Functor.h>
#include <FunctorAsync.h>
#include <CpProxy.h>

#include <string>

namespace Zapp {

class CpDeviceCpp;
class Action;
class PropertyBinary;
class PropertyBool;
class PropertyInt;
class PropertyString;
class PropertyUint;

/**
 * Proxy for linn.co.uk:Preamp:4
 */
class CpProxyLinnCoUkPreamp4Cpp : public CpProxy
{
public:
    /**
     * Constructor.
     *
     * Use CpProxy::[Un]Subscribe() to enable/disable querying of state variable
     * and reporting of their changes.
     *
     * @param[in]  aDevice   The device to use
     */
    CpProxyLinnCoUkPreamp4Cpp(CpDeviceCpp& aDevice);
    /**
     * Destructor.
     * If any asynchronous method is in progress, this will block until they complete.
     * [Note that any methods still in progress are likely to complete with an error.]
     * Clients who have called Subscribe() do not need to call Unsubscribe() before
     * calling delete.  An unsubscribe will be triggered automatically when required.
     */
    ~CpProxyLinnCoUkPreamp4Cpp();

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     */
    void SyncVolumeInc();
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndVolumeInc().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginVolumeInc(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndVolumeInc(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     */
    void SyncVolumeDec();
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndVolumeDec().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginVolumeDec(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndVolumeDec(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[in]  aaVolume
     */
    void SyncSetVolume(uint32_t aaVolume);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndSetVolume().
     *
     * @param[in] aaVolume
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginSetVolume(uint32_t aaVolume, FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndSetVolume(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[out] aaVolume
     */
    void SyncVolume(uint32_t& aaVolume);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndVolume().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginVolume(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     * @param[out] aaVolume
     */
    void EndVolume(IAsync& aAsync, uint32_t& aaVolume);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[in]  aaMute
     */
    void SyncSetMute(bool aaMute);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndSetMute().
     *
     * @param[in] aaMute
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginSetMute(bool aaMute, FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndSetMute(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[out] aaMute
     */
    void SyncMute(bool& aaMute);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndMute().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginMute(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     * @param[out] aaMute
     */
    void EndMute(IAsync& aAsync, bool& aaMute);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[in]  aaBalance
     */
    void SyncSetBalance(int32_t aaBalance);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndSetBalance().
     *
     * @param[in] aaBalance
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginSetBalance(int32_t aaBalance, FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndSetBalance(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[out] aaBalance
     */
    void SyncBalance(int32_t& aaBalance);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndBalance().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginBalance(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     * @param[out] aaBalance
     */
    void EndBalance(IAsync& aAsync, int32_t& aaBalance);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[in]  aaVolumeLimit
     */
    void SyncSetVolumeLimit(uint32_t aaVolumeLimit);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndSetVolumeLimit().
     *
     * @param[in] aaVolumeLimit
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginSetVolumeLimit(uint32_t aaVolumeLimit, FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndSetVolumeLimit(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[out] aaVolumeLimit
     */
    void SyncVolumeLimit(uint32_t& aaVolumeLimit);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndVolumeLimit().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginVolumeLimit(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     * @param[out] aaVolumeLimit
     */
    void EndVolumeLimit(IAsync& aAsync, uint32_t& aaVolumeLimit);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[in]  aaStartupVolume
     */
    void SyncSetStartupVolume(uint32_t aaStartupVolume);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndSetStartupVolume().
     *
     * @param[in] aaStartupVolume
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginSetStartupVolume(uint32_t aaStartupVolume, FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndSetStartupVolume(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[out] aaStartupVolume
     */
    void SyncStartupVolume(uint32_t& aaStartupVolume);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndStartupVolume().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginStartupVolume(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     * @param[out] aaStartupVolume
     */
    void EndStartupVolume(IAsync& aAsync, uint32_t& aaStartupVolume);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[in]  aaStartupVolumeEnabled
     */
    void SyncSetStartupVolumeEnabled(bool aaStartupVolumeEnabled);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndSetStartupVolumeEnabled().
     *
     * @param[in] aaStartupVolumeEnabled
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginSetStartupVolumeEnabled(bool aaStartupVolumeEnabled, FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     */
    void EndSetStartupVolumeEnabled(IAsync& aAsync);

    /**
     * Invoke the action synchronously.  Blocks until the action has been processed
     * on the device and sets any output arguments.
     *
     * @param[out] aaStartupVolumeEnabled
     */
    void SyncStartupVolumeEnabled(bool& aaStartupVolumeEnabled);
    /**
     * Invoke the action asynchronously.
     * Returns immediately and will run the client-specified callback when the action
     * later completes.  Any output arguments can then be retrieved by calling
     * EndStartupVolumeEnabled().
     *
     * @param[in] aFunctor   Callback to run when the action completes.
     *                       This is guaranteed to be run but may indicate an error
     */
    void BeginStartupVolumeEnabled(FunctorAsync& aFunctor);
    /**
     * Retrieve the output arguments from an asynchronously invoked action.
     * This may only be called from the callback set in the above Begin function.
     *
     * @param[in]  aAsync  Argument passed to the callback set in the above Begin function
     * @param[out] aaStartupVolumeEnabled
     */
    void EndStartupVolumeEnabled(IAsync& aAsync, bool& aaStartupVolumeEnabled);

    /**
     * Set a callback to be run when the Volume state variable changes.
     *
     * Callbacks may be run in different threads but callbacks for a
     * CpProxyLinnCoUkPreamp4Cpp instance will not overlap.
     *
     * @param[in]  aFunctor  The callback to run when the state variable changes
     */
    void SetPropertyVolumeChanged(Functor& aFunctor);
    /**
     * Set a callback to be run when the Mute state variable changes.
     *
     * Callbacks may be run in different threads but callbacks for a
     * CpProxyLinnCoUkPreamp4Cpp instance will not overlap.
     *
     * @param[in]  aFunctor  The callback to run when the state variable changes
     */
    void SetPropertyMuteChanged(Functor& aFunctor);
    /**
     * Set a callback to be run when the Balance state variable changes.
     *
     * Callbacks may be run in different threads but callbacks for a
     * CpProxyLinnCoUkPreamp4Cpp instance will not overlap.
     *
     * @param[in]  aFunctor  The callback to run when the state variable changes
     */
    void SetPropertyBalanceChanged(Functor& aFunctor);
    /**
     * Set a callback to be run when the VolumeLimit state variable changes.
     *
     * Callbacks may be run in different threads but callbacks for a
     * CpProxyLinnCoUkPreamp4Cpp instance will not overlap.
     *
     * @param[in]  aFunctor  The callback to run when the state variable changes
     */
    void SetPropertyVolumeLimitChanged(Functor& aFunctor);
    /**
     * Set a callback to be run when the StartupVolume state variable changes.
     *
     * Callbacks may be run in different threads but callbacks for a
     * CpProxyLinnCoUkPreamp4Cpp instance will not overlap.
     *
     * @param[in]  aFunctor  The callback to run when the state variable changes
     */
    void SetPropertyStartupVolumeChanged(Functor& aFunctor);
    /**
     * Set a callback to be run when the StartupVolumeEnabled state variable changes.
     *
     * Callbacks may be run in different threads but callbacks for a
     * CpProxyLinnCoUkPreamp4Cpp instance will not overlap.
     *
     * @param[in]  aFunctor  The callback to run when the state variable changes
     */
    void SetPropertyStartupVolumeEnabledChanged(Functor& aFunctor);

    /**
     * Query the value of the Volume property.
     *
     * This function is threadsafe and can only be called if Subscribe() has been
     * called and a first eventing callback received more recently than any call
	 * to Unsubscribe().
     *
     * @param[out] aVolume
     */
    void PropertyVolume(uint32_t& aVolume) const;
    /**
     * Query the value of the Mute property.
     *
     * This function is threadsafe and can only be called if Subscribe() has been
     * called and a first eventing callback received more recently than any call
	 * to Unsubscribe().
     *
     * @param[out] aMute
     */
    void PropertyMute(bool& aMute) const;
    /**
     * Query the value of the Balance property.
     *
     * This function is threadsafe and can only be called if Subscribe() has been
     * called and a first eventing callback received more recently than any call
	 * to Unsubscribe().
     *
     * @param[out] aBalance
     */
    void PropertyBalance(int32_t& aBalance) const;
    /**
     * Query the value of the VolumeLimit property.
     *
     * This function is threadsafe and can only be called if Subscribe() has been
     * called and a first eventing callback received more recently than any call
	 * to Unsubscribe().
     *
     * @param[out] aVolumeLimit
     */
    void PropertyVolumeLimit(uint32_t& aVolumeLimit) const;
    /**
     * Query the value of the StartupVolume property.
     *
     * This function is threadsafe and can only be called if Subscribe() has been
     * called and a first eventing callback received more recently than any call
	 * to Unsubscribe().
     *
     * @param[out] aStartupVolume
     */
    void PropertyStartupVolume(uint32_t& aStartupVolume) const;
    /**
     * Query the value of the StartupVolumeEnabled property.
     *
     * This function is threadsafe and can only be called if Subscribe() has been
     * called and a first eventing callback received more recently than any call
	 * to Unsubscribe().
     *
     * @param[out] aStartupVolumeEnabled
     */
    void PropertyStartupVolumeEnabled(bool& aStartupVolumeEnabled) const;
private:
    void VolumePropertyChanged();
    void MutePropertyChanged();
    void BalancePropertyChanged();
    void VolumeLimitPropertyChanged();
    void StartupVolumePropertyChanged();
    void StartupVolumeEnabledPropertyChanged();
private:
    Action* iActionVolumeInc;
    Action* iActionVolumeDec;
    Action* iActionSetVolume;
    Action* iActionVolume;
    Action* iActionSetMute;
    Action* iActionMute;
    Action* iActionSetBalance;
    Action* iActionBalance;
    Action* iActionSetVolumeLimit;
    Action* iActionVolumeLimit;
    Action* iActionSetStartupVolume;
    Action* iActionStartupVolume;
    Action* iActionSetStartupVolumeEnabled;
    Action* iActionStartupVolumeEnabled;
    PropertyUint* iVolume;
    PropertyBool* iMute;
    PropertyInt* iBalance;
    PropertyUint* iVolumeLimit;
    PropertyUint* iStartupVolume;
    PropertyBool* iStartupVolumeEnabled;
    Functor iVolumeChanged;
    Functor iMuteChanged;
    Functor iBalanceChanged;
    Functor iVolumeLimitChanged;
    Functor iStartupVolumeChanged;
    Functor iStartupVolumeEnabledChanged;
};

} // namespace Zapp

#endif // HEADER_LINNCOUKPREAMP4CPP
