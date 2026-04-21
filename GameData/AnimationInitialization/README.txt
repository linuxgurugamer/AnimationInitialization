ModuleAnimationInitialization

Built to solve a classic KSP problem:

“Why is this thing deployed again?”

A flexible KSP1 PartModule that initializes animations while remaining compatible with stock animation systems.

This module solves incorrect startup states without fighting stock modules like ModuleAnimateGeneric or ModuleDeployableSolarPanel.

-------------------------------------------
Features

    Supports multiple animations (comma-separated)
    Supports per-animation normalizedTime values
    Works in:
        Editor (VAB/SPH)
        Flight (launch, vessel load, vessel switching)
    Cooperative mode (default):
    Syncs with ModuleAnimateGeneric
    Syncs with ModuleDeployableSolarPanel
    Prevents animation snap-back issues
    Persistent settings and UI state

-------------------------------------------
Usage

    Example Config
    MODULE
    {
        name = ModuleAnimationInitialization
        animationNames = deploy, rotate, extend
        animationTimes = deploy:0, rotate:0.5, extend:1
        defaultNormalizedTime = 0
        freezeAnimation = true
        applyInEditor = true
        applyInFlight = true
        useCooperativeMode = true
    }

-------------------------------------------
Fields

Field	                Description
animationNames	        Comma-separated list of animation names
animationTimes	        Optional per-animation values (name:value)
defaultNormalizedTime	Default fallback value
normalizedTime	        Persisted shared value
hasInitialized	        Internal initialization flag
freezeAnimation	        Stops animation playback after applying
applyInEditor	        Apply in VAB/SPH
applyInFlight	        Apply in flight
useCooperativeMode	    Sync with stock modules
debugLogging	        Enable debug logs

-------------------------------------------
Live Sync Display

Shows the actual current animation state:

    Reads from Unity AnimationState.normalizedTime
    Updates every frame
    Useful for debugging:
        Stock module overrides
        Animation drift
        Timing issues

-------------------------------------------
Cooperative Mode (Default)

The module detects and follows stock animation owners.

ModuleAnimateGeneric

    Uses:
        animTime (0–1)
    Supports partial animation states
    Treated as a continuous deploy system

ModuleDeployableSolarPanel
    Uses deploy state:
        Retracted → 0
        Extended → 1
    No partial states

-------------------------------------------
Behavior Priority

For each animation:

    ModuleAnimateGeneric.animTime
    Solar panel deploy state
    Per-animation config (animationTimes)
    Shared normalizedTime

-------------------------------------------
Behavior Summary

Scenario	        Result
Editor placement	Uses default or configured values
Launch	            Syncs with stock module
Vessel switch	    Restores correct state
Solar panels	    Fully retracted or deployed
MAG animations	    Full precision

-------------------------------------------
Important Notes

    Works with Unity legacy Animation system only
    Do not use Animation.Stop() (resets animation)
    Best animation setup:
        Frame 0 = retracted
        Frame 1 = deployed
    Max supported animations in UI: 8

-------------------------------------------
Limitations

    Solar panels cannot represent partial deployment
    Cooperative mode limits full manual override
    Multiple animations share same timing unless overridden
    No dynamic PAW field creation (KSP limitation)

-------------------------------------------

