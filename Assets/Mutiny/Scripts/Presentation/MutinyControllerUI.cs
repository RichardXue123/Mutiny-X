using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Presentation
{
    /// <summary>Adapts the actual IMGUI controls, retaining their native click callbacks and gates.</summary>
    public static class MutinyControllerUI
    {
        private sealed class Target
        {
            public string Context, Id;
            public Rect ScreenRect;
            public bool Enabled, Back;
            public int SeenFrame;
        }

        private static readonly List<Target> Targets = new List<Target>();
        private static string s_Scope, s_Focus, s_FocusContext, s_Activation, s_ActivationContext;
        private static int s_ActivationFrame;

        public static void BeginScope(string context) => s_Scope = context;

        public static bool Button(Rect rect, GUIContent content, GUIStyle style,
            string id = null, bool isBack = false, bool controllerEnabled = true)
        {
            MutinyInputHub hub = MutinyInputHub.Instance;
            bool nativeClick = GUI.Button(rect, content, style);
            if (hub == null) return nativeClick;
            if (id == null)
                id = content.tooltip + ":" + rect.x.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) +
                     ":" + rect.y.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            Vector2 min = GUI.matrix.MultiplyPoint3x4(rect.min);
            Vector2 max = GUI.matrix.MultiplyPoint3x4(rect.max);
            bool enabled = GUI.enabled && controllerEnabled &&
                           !MutinyTransitionManager.IsTransitionActive && s_Scope == hub.CurrentContext;
            Register(s_Scope, id, Rect.MinMaxRect(min.x, min.y, max.x, max.y), enabled, isBack);
            bool controllerClick = enabled && hub.IsControllerActive &&
                                   s_ActivationFrame == Time.frameCount && s_Activation == id &&
                                   s_ActivationContext == s_Scope;
            if (controllerClick) ClearActivation();
            return controllerClick || (!hub.IsControllerActive && nativeClick);
        }

        private static void Register(string context, string id, Rect rect, bool enabled, bool back)
        {
            if (context == null) return;
            Target target = Targets.Find(t => t.Context == context && t.Id == id);
            if (target == null)
            {
                target = new Target { Context = context, Id = id };
                Targets.Add(target);
            }
            target.ScreenRect = rect;
            target.Enabled = enabled;
            target.Back = back;
            target.SeenFrame = Time.frameCount;
        }

        internal static void RouteFrame(MutinyInputHub hub, Vector2 navigation, bool confirm, bool back)
        {
            string context = hub.CapturedContext;
            Targets.RemoveAll(t => t.SeenFrame < Time.frameCount - 2);
            List<Target> available = Targets.FindAll(t => t.Context == context && t.Enabled);
            if (available.Count == 0) return;
            Target focused = s_FocusContext == context ? available.Find(t => t.Id == s_Focus) : null;
            if (navigation != Vector2.zero)
            {
                if (focused == null) focused = available[0];
                else
                {
                    Vector2 screenDirection = new Vector2(navigation.x, -navigation.y);
                    float best = float.PositiveInfinity;
                    Target next = null;
                    foreach (Target candidate in available)
                    {
                        Vector2 delta = candidate.ScreenRect.center - focused.ScreenRect.center;
                        float forward = Vector2.Dot(delta, screenDirection);
                        if (forward <= 0.1f) continue;
                        float lateral = Mathf.Abs(delta.x * screenDirection.y - delta.y * screenDirection.x);
                        float score = forward + lateral * 4f;
                        if (score < best) { best = score; next = candidate; }
                    }
                    if (next != null) focused = next;
                }
                s_FocusContext = context;
                s_Focus = focused.Id;
                hub.SetPointerFromGui(focused.ScreenRect.center);
            }
            if (back) focused = available.Find(t => t.Back);
            else if (confirm && focused == null)
            {
                Vector2 pointer = new Vector2(hub.PointerPosition.x, Screen.height - hub.PointerPosition.y);
                focused = available.Find(t => t.ScreenRect.Contains(pointer));
                // A may initialize the first focus, but may not execute an invisible/default selection.
                if (focused == null)
                {
                    focused = available[0];
                    s_FocusContext = context;
                    s_Focus = focused.Id;
                    hub.SetPointerFromGui(focused.ScreenRect.center);
                    return;
                }
            }
            if ((confirm || back) && focused != null)
            {
                s_Activation = focused.Id;
                s_ActivationContext = context;
                s_ActivationFrame = Time.frameCount;
            }
        }

        internal static bool RouteBoardPointer(MutinyInputHub hub)
        {
            Vector2 pointer = new Vector2(hub.PointerPosition.x, Screen.height - hub.PointerPosition.y);
            Target target = Targets.Find(t => t.Context == "board" && t.Enabled &&
                t.SeenFrame >= Time.frameCount - 2 && t.ScreenRect.Contains(pointer));
            if (target == null) return false;
            s_Activation = target.Id;
            s_ActivationContext = "board";
            s_ActivationFrame = Time.frameCount;
            return true;
        }

        public static void ClearActivation() { s_Activation = null; s_ActivationContext = null; }
        public static void ClearFocus() { s_Focus = null; s_FocusContext = null; }
        public static void Clear() { Targets.Clear(); ClearActivation(); ClearFocus(); s_Scope = null; }
    }
}
