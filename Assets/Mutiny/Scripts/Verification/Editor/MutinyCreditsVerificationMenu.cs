using System;
using Mutiny.Presentation;
using UnityEditor;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    public static class MutinyCreditsVerificationMenu
    {
        [MenuItem("Mutiny/Parity/Validate Credits")]
        public static void Validate()
        {
            var flow = new MutinyFrontendFlow();
            Require(flow.CurrentPage == MutinyFrontendPage.Title, "starts on title");
            flow.PressCreditsBack();
            Require(flow.CurrentPage == MutinyFrontendPage.Title, "Back is gated outside Credits");
            flow.PressCredits();
            Require(flow.CurrentPage == MutinyFrontendPage.Credits, "Credits enters its production page");
            flow.PressPlay();
            Require(flow.CurrentPage == MutinyFrontendPage.Credits, "Play is gated while Credits is visible");
            flow.PressCreditsBack();
            Require(flow.CurrentPage == MutinyFrontendPage.Title, "Back returns to title");
            flow.PressCredits();
            Require(flow.CurrentPage == MutinyFrontendPage.Credits, "Credits can be opened again");

            CheckSize("credits_panel", 462, 352);
            CheckSize("credits_nitrome_logo", 99, 74);
            CheckSize("credits_copyright", 141, 11);
            Texture2D copyrightMask = CheckSize("credits_copyright_hit", 141, 11);
            Require(copyrightMask.isReadable, "copyright mask is readable for original shape hit testing");
            Texture2D mask = CheckSize("credits_nitrome_hit", 99, 74);
            Require(mask.isReadable, "Nitrome mask is readable for original shape hit testing");
            Require(mask.GetPixel(0, 0).a == 0f, "transparent logo corner does not hit");
            bool hasOpaquePixel = false;
            for (int y = 0; y < mask.height && !hasOpaquePixel; y++)
                for (int x = 0; x < mask.width; x++)
                    if (mask.GetPixel(x, y).a > 0.5f)
                    {
                        hasOpaquePixel = true;
                        break;
                    }
            Require(hasOpaquePixel, "Nitrome shape contains clickable pixels");
            Debug.Log("[Mutiny Parity] Credits navigation and five original resources passed (FRONT-CRED-01/02/03/04).");
        }

        private static Texture2D CheckSize(string name, int width, int height)
        {
            Texture2D texture = Resources.Load<Texture2D>("UI/Frontend/" + name);
            Require(texture != null && texture.width == width && texture.height == height,
                $"{name} is present at {width}x{height}");
            return texture;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("FRONT-CRED verification failed: " + message);
        }
    }
}
