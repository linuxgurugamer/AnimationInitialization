using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace ModuleAnimationInit
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    namespace YourNamespace
    {
        public class ModuleAnimationInitialization : PartModule
        {
            [KSPField]
            public string animationNames = string.Empty;

            [KSPField]
            public string animationTimes = string.Empty;

            [KSPField]
            public float defaultNormalizedTime = 0f;

            [KSPField(isPersistant = true)]
            public float normalizedTime = 0f;

            [KSPField(isPersistant = true)]
            public bool hasInitialized = false;

            [KSPField]
            public bool freezeAnimation = true;

            [KSPField]
            public bool applyInEditor = true;

            [KSPField]
            public bool applyInFlight = true;

            [KSPField]
            public bool useCooperativeMode = true;

            [KSPField]
            public bool debugLogging = false;

            private string[] parsedAnimationNames = new string[0];
            private readonly Dictionary<string, float> configuredAnimationTimes = new Dictionary<string, float>();

            public override void OnStart(StartState state)
            {
                base.OnStart(state);

                if ((HighLogic.LoadedSceneIsEditor && !applyInEditor) ||
                    (HighLogic.LoadedSceneIsFlight && !applyInFlight))
                    return;

                ParseAnimationNames();
                ParseAnimationTimes();

                if (!hasInitialized)
                {
                    normalizedTime = Mathf.Clamp01(defaultNormalizedTime);
                    hasInitialized = true;
                }

                StartCoroutine(ApplyAnimationsNextFrame());
            }

            private void ParseAnimationNames()
            {
                parsedAnimationNames = (animationNames ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
            }

            private void ParseAnimationTimes()
            {
                configuredAnimationTimes.Clear();

                if (string.IsNullOrEmpty(animationTimes))
                    return;

                var pairs = animationTimes.Split(',');
                foreach (var pair in pairs)
                {
                    var kv = pair.Split(':');
                    if (kv.Length != 2) continue;

                    string name = kv[0].Trim();
                    if (float.TryParse(kv[1], out float value))
                    {
                        configuredAnimationTimes[name] = Mathf.Clamp01(value);
                    }
                }
            }

            private IEnumerator ApplyAnimationsNextFrame()
            {
                yield return null;
                ApplyAnimations();
            }

            private float GetTargetTime(string animName)
            {
                // Per-animation override
                if (configuredAnimationTimes.TryGetValue(animName, out float t))
                    return t;

                if (useCooperativeMode)
                {
                    var mag = part.FindModuleImplementing<ModuleAnimateGeneric>();
                    if (mag != null)
                        return Mathf.Clamp01(mag.animTime);

                    var solar = part.FindModuleImplementing<ModuleDeployableSolarPanel>();
                    if (solar != null)
                        return solar.deployState.ToString().Contains("EXTENDED") ? 1f : 0f;
                }

                return Mathf.Clamp01(normalizedTime);
            }

            private void ApplyAnimations()
            {
                foreach (string animName in parsedAnimationNames)
                {
                    float t = GetTargetTime(animName);

                    var animators = part.FindModelAnimators(animName);
                    if (animators == null || animators.Length == 0)
                        continue;

                    foreach (var anim in animators)
                    {
                        if (anim == null) continue;

                        var state = anim[animName];
                        if (state == null) continue;

                        state.enabled = true;
                        state.normalizedTime = t;
                        state.speed = 0f;

                        anim.Play(animName);
                        anim.Sample();

                        if (freezeAnimation)
                            state.speed = 0f;
                    }
                }
            }

            private void Log(string msg)
            {
                if (debugLogging)
                    Debug.Log("[ModuleAnimationInitialization] " + part.partInfo.name + ": " + msg);
            }
        }
    }
}