using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mutiny.Simulation
{
    // Serializes only small DTOs on the main thread. The worker receives strings,
    // never GameObjects, Random.state, or Unity APIs. Bounded backpressure lets the
    // decision coroutine yield instead of blocking on slow storage.
    internal sealed class MutinyAITraceWriter
    {
        private const int ChunkRecords = 64;
        private sealed class Chunk { public bool Candidates; public List<string> Records; }
        private readonly BlockingCollection<Chunk> m_Queue = new BlockingCollection<Chunk>(16);
        private List<string> m_Draws = new List<string>(ChunkRecords);
        private List<string> m_Candidates = new List<string>(ChunkRecords);
        private volatile bool m_Aborted;
        private volatile Exception m_Error;
        private string m_Header;
        private bool m_Closed;
        public string Path { get; }
        public Task Completion { get; }
        public Exception Error => m_Error;

        public MutinyAITraceWriter(string path)
        {
            Path = path;
            Completion = Task.Run(Write);
        }

        public bool CanAcceptRecords => m_Error != null ||
            (Flush(ref m_Draws, false, false) && Flush(ref m_Candidates, true, false));

        public void Record(MutinyAIRandomDraw draw)
        {
            if (!m_Aborted && m_Error == null) m_Draws.Add(JsonUtility.ToJson(draw));
        }

        public void Record(MutinyAICandidateRecord candidate)
        {
            if (!m_Aborted && m_Error == null) m_Candidates.Add(JsonUtility.ToJson(candidate));
        }

        private bool Flush(ref List<string> buffer, bool candidates, bool all)
        {
            if (buffer.Count == 0 || (!all && buffer.Count < ChunkRecords)) return true;
            if (!m_Queue.TryAdd(new Chunk { Candidates = candidates, Records = buffer })) return false;
            buffer = new List<string>(ChunkRecords);
            return true;
        }

        public bool TryComplete(MutinyAIDecisionTrace trace)
        {
            if (m_Closed || m_Error != null) return true;
            if (!Flush(ref m_Draws, false, true) || !Flush(ref m_Candidates, true, true)) return false;
            // The header contains no candidate/random arrays, regardless of search size.
            string header = JsonUtility.ToJson(new Header
            {
                DecisionId = trace.DecisionId, TeamNumber = trace.TeamNumber,
                Seed = trace.Seed, Phase = trace.Phase, Winner = trace.Winner
            });
            m_Header = header.Substring(0, header.Length - 1);
            m_Closed = true;
            m_Queue.CompleteAdding();
            return true;
        }

        public void Abort()
        {
            if (m_Closed) return; // A published decision may finish saving after the turn ends.
            m_Aborted = true;
            m_Closed = true;
            m_Queue.CompleteAdding();
        }

        [Serializable]
        private sealed class Header
        {
            public int DecisionId, TeamNumber, Seed;
            public string Phase, Winner;
        }

        private void Write()
        {
            string unique = Path + "." + Guid.NewGuid().ToString("N");
            string draws = unique + ".draws.tmp", candidates = unique + ".candidates.tmp", staged = unique + ".json.tmp";
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                bool firstDraw = true, firstCandidate = true;
                using (var drawFile = new StreamWriter(draws, false, new UTF8Encoding(false)))
                using (var candidateFile = new StreamWriter(candidates, false, new UTF8Encoding(false)))
                    foreach (Chunk chunk in m_Queue.GetConsumingEnumerable())
                    {
                        if (m_Aborted) break;
                        StreamWriter file = chunk.Candidates ? candidateFile : drawFile;
                        bool first = chunk.Candidates ? firstCandidate : firstDraw;
                        foreach (string record in chunk.Records)
                        {
                            if (!first) file.Write(',');
                            file.Write(record);
                            first = false;
                        }
                        if (chunk.Candidates) firstCandidate = first;
                        else firstDraw = first;
                    }
                if (m_Aborted) return;
                using (var output = new FileStream(staged, FileMode.CreateNew))
                {
                    WriteText(output, m_Header + ",\"Draws\":[");
                    using (var input = File.OpenRead(draws)) input.CopyTo(output);
                    WriteText(output, "],\"Candidates\":[");
                    using (var input = File.OpenRead(candidates)) input.CopyTo(output);
                    WriteText(output, "]}");
                }
                File.Move(staged, Path); // Publish only a complete JSON, with a unique decision filename.
            }
            catch (Exception error) { m_Error = error; }
            finally
            {
                // Only this writer's explicitly named temporary files are removed.
                foreach (string temporary in new[] { draws, candidates, staged })
                    try { if (File.Exists(temporary)) File.Delete(temporary); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                // Do not dispose the queue while the main thread can still call TryComplete/Abort.
            }
        }

        private static void WriteText(Stream file, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            file.Write(bytes, 0, bytes.Length);
        }
    }
}
