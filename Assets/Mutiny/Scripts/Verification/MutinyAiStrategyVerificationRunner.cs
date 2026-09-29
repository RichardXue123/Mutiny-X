using System;
using System.Collections;
using UnityEngine;

namespace Mutiny.Verification
{
    public sealed class MutinyAiStrategyVerificationRunner : MonoBehaviour
    {
        public Action<MutinyLevel1VerificationResult> Completed;
        private IEnumerator Start()
        {
            var result = new MutinyLevel1VerificationResult();
            IEnumerator tests = MutinyAiStrategyVerificationTest.Run(result);
            try
            {
                while (true)
                {
                    bool next;
                    try { next = tests.MoveNext(); }
                    catch (Exception error)
                    {
                        Debug.LogException(error);
                        result.Assert(false, "AI strategy verification exception: " + error);
                        break;
                    }
                    if (!next) break;
                    yield return tests.Current;
                }
            }
            finally { (tests as IDisposable)?.Dispose(); }
            Completed?.Invoke(result);
        }
    }
}
