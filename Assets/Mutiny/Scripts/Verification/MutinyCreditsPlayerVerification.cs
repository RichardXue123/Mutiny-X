using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Mutiny.Presentation;
using UnityEngine;

namespace Mutiny.Verification
{
    // Opt-in Development Player regression: -mutiny-verify-credits <absolute PNG path>.
    public sealed class MutinyCreditsPlayerVerification : MonoBehaviour
    {
        private string m_CapturePath;

        public static void ValidateAvatar(MutinyFrontendController frontend)
        {
            Texture2D bundled = Resources.Load<Texture2D>("UI/Frontend/XingTong");
            if (bundled == null || frontend.CreditsAvatar != bundled ||
                bundled.width != 1254 || bundled.height != 1254)
            {
                throw new InvalidOperationException(
                    "EXT-CRED-01: production Credits avatar must use the bundled 1254x1254 XingTong resource.");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Application.isEditor || !Debug.isDebugBuild)
                return;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++)
            {
                if (args[i] != "-mutiny-verify-credits")
                    continue;
                var host = new GameObject("MutinyCreditsPlayerVerification");
                var verifier = host.AddComponent<MutinyCreditsPlayerVerification>();
                verifier.m_CapturePath = Path.GetFullPath(args[i + 1]);
                Application.runInBackground = true;
                verifier.StartCoroutine(verifier.Verify());
                return;
            }
        }

        private IEnumerator Verify()
        {
            // Let all production AfterSceneLoad initializers complete first.
            yield return null;
            float splashDeadline = Time.realtimeSinceStartup + 20f;
            while (!UnityEngine.Rendering.SplashScreen.isFinished && Time.realtimeSinceStartup < splashDeadline)
                yield return null;
            if (!UnityEngine.Rendering.SplashScreen.isFinished)
            {
                Debug.LogError("EXT-CRED-01: Player splash screen did not finish before visual capture.");
                Application.Quit(1);
                yield break;
            }
            MutinyFrontendController frontend = FindAnyObjectByType<MutinyFrontendController>();
            try
            {
                if (frontend == null || frontend.CurrentPage != MutinyFrontendPage.Title)
                    throw new InvalidOperationException("EXT-CRED-01: Main must initialize the production title page.");
                ValidateAvatar(frontend);
                typeof(MutinyFrontendController).GetMethod("OpenCredits",
                    BindingFlags.Instance | BindingFlags.NonPublic).Invoke(frontend, null);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Application.Quit(1);
                yield break;
            }

            float deadline = Time.realtimeSinceStartup + 10f;
            while ((frontend.CurrentPage != MutinyFrontendPage.Credits ||
                MutinyTransitionManager.IsTransitionActive) && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (frontend.CurrentPage != MutinyFrontendPage.Credits || MutinyTransitionManager.IsTransitionActive)
            {
                Debug.LogError("EXT-CRED-01: production Credits transition did not complete.");
                Application.Quit(1);
                yield break;
            }

            // Capture the real OnGUI result including the avatar and both author labels.
            yield return new WaitForEndOfFrame();
            Texture2D screenshot = null;
            try
            {
                screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                if (screenshot == null)
                    throw new InvalidOperationException("EXT-CRED-01: Credits backbuffer capture failed.");
                Directory.CreateDirectory(Path.GetDirectoryName(m_CapturePath));
                File.WriteAllBytes(m_CapturePath, screenshot.EncodeToPNG());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Application.Quit(1);
                yield break;
            }
            finally
            {
                if (screenshot != null)
                    Destroy(screenshot);
            }
            Debug.Log("[Mutiny Credits] EXT-CRED-01 resource/navigation checks passed; screenshot requires visual inspection: " + m_CapturePath);
            Application.Quit(0);
        }
    }
}
