using System;
using System.Collections;
using System.Collections.Generic;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Verification
{
    // Real Update/25 Hz physics, Mine countdown, Explosion animation and health
    // display drive these cases. Never assign death/commit/rest flags in tests.
    [DefaultExecutionOrder(10000)] // Observe after production input/body LateUpdate.
    public sealed class MutinySelectedDeathVerificationRunner : MonoBehaviour
    {
        public Action<MutinyLevel1VerificationResult> Completed;
        private Action m_ObserveAimFrame;

        private void LateUpdate() => m_ObserveAimFrame?.Invoke();

        private IEnumerator Start()
        {
            var result = new MutinyLevel1VerificationResult();
            var tests = new Stack<IEnumerator>();
            tests.Push(Run(result));
            try
            {
                while (tests.Count > 0)
                {
                    bool next;
                    try { next = tests.Peek().MoveNext(); }
                    catch (Exception error)
                    {
                        Debug.LogException(error);
                        result.Assert(false, "Selected death verification exception: " + error);
                        break;
                    }
                    if (!next)
                    {
                        (tests.Pop() as IDisposable)?.Dispose();
                        continue;
                    }
                    object current = tests.Peek().Current;
                    if (current is IEnumerator nested)
                        tests.Push(nested);
                    else
                        yield return current;
                }
            }
            finally
            {
                m_ObserveAimFrame = null;
                while (tests.Count > 0) (tests.Pop() as IDisposable)?.Dispose();
            }
            Completed?.Invoke(result);
        }

        private IEnumerator Run(MutinyLevel1VerificationResult result)
        {
            foreach (string mode in new[] { "hold", "release-after-death", "weapon-aim", "last-actor" })
            {
                Debug.Log("[TURN-DEATH-STAGE] " + mode);
                using (var f = new Fixture(mode != "last-actor"))
                {
                    yield return null; // Real Start binds physics and resets input.
                    f.Actor.TakeDamage(60f); // Set up low health through damage, not death flags.
                    yield return WaitUntil(() => !f.Actor.IsResolvingHealthDisplay && f.Turn.CheckAllBodiesAtRest(), 8f);
                    MutinyMine mine = f.PlaceMine();
                    yield return WaitUntil(() => mine.IsStored && mine.PhysicsBody.SimulationTickCount >= 12, 3f);
                    result.Assert(mine.IsStored && !mine.IsActive, mode + ": actual mine settles without triggering a stationary character");

                    Vector2 origin = ActorPixels(f.Actor);
                    bool selected = f.Input.TrySelectCharacterForVerification(f.Team, origin);
                    bool ready = f.Input.SelectCharacterThrow();
                    if (mode == "weapon-aim")
                    {
                        f.Actor.AddWeapon("cherryBomb");
                        ready = f.Input.SelectWeapon("cherryBomb");
                        f.Input.BeginAimForVerification(f.Actor);
                        // Nearby physical motion is another original Mine trigger.
                        f.Actor.ApplyImpulse(new Vector2(8f / MutinyPhysics.PixelsPerUnit, 0f));
                    }
                    else
                        ready &= f.Input.TryBeginAimFromPrimaryPointerForVerification(f.Actor, origin);
                    f.Input.AdvanceAimPointerForVerification(f.Actor, origin + new Vector2(0f, 120f), true, false);
                    result.Assert(selected && ready && f.Input.IsAiming && !f.Turn.ActionCommittedThisTurn,
                        mode + ": production selection and held aim do not fabricate a committed action");
                    yield return WaitUntil(() => !f.Actor.IsAlive, 5f);
                    result.Assert(!f.Actor.IsAlive, mode + ": actual countdown/explosion kills the selected actor");
                    yield return null; // PlayerInput.Update must release even without a pointer release.
                    // The manager owns a 25 Hz clock; a render frame need not
                    // include a simulation tick. Observe the next real tick.
                    yield return WaitUntil(() => f.Turn.CurrentPhase == TurnPhase.Settling, 1f);
                    result.Assert(!f.Input.IsAiming && !f.Input.IsActionMenuOpen && f.Input.EquippedWeapon == null &&
                                  !f.Input.TrajectoryRenderer.GetComponent<LineRenderer>().enabled &&
                                  f.Team.SelectedCharacter == f.Actor && !f.Turn.ActionCommittedThisTurn &&
                                  !f.Input.CanProcessCurrentTurnInputForVerification(),
                        mode + ": death releases aim/trajectory/input but retains the dead turn actor and does not commit a jump");
                    result.Assert(f.Turn.CurrentTeam == f.Team && f.Turn.CurrentPhase == TurnPhase.Settling &&
                                  !f.Turn.CheckAllBodiesAtRest(),
                        mode + ": explosion/health still block settlement; no immediate team switch");
                    if (f.Teammate != null)
                        result.Assert(!f.Input.TrySelectCharacterForVerification(f.Team, ActorPixels(f.Teammate)),
                            mode + ": cannot replace the dead selected actor during settlement");
                    if (mode == "weapon-aim")
                        result.Assert(f.Actor.WeaponInventory["cherryBomb"] == 1,
                            "weapon-aim: death does not spend an unfired weapon");
                    if (mode == "release-after-death")
                    {
                        f.Input.ResolveAimReleaseForVerification(f.Actor, origin + new Vector2(0f, 120f));
                        result.Assert(!f.Actor.IsSelfThrown && !f.Turn.ActionCommittedThisTurn,
                            "release-after-death: stale release cannot launch the corpse");
                    }

                    bool blockedCorrectly = true;
                    float deadline = Time.realtimeSinceStartup + 10f;
                    while (!f.Turn.CheckAllBodiesAtRest() && Time.realtimeSinceStartup < deadline)
                    {
                        blockedCorrectly &= f.Turn.CurrentTeam == f.Team && f.Turn.InactivityTicks == 0;
                        yield return null;
                    }
                    result.Assert(blockedCorrectly && f.Turn.CheckAllBodiesAtRest() && f.Actor.ShownHealth == 0f,
                        mode + ": real physics/health settle before the usual inactivity gate starts");
                    // Stop only the autonomous timer after real settlement, then
                    // drive its production tick entry to check the exact 11-tick gate.
                    f.Turn.enabled = false;
                    int remaining = MutinyTurnManager.InactivitySettlingThreshold + 1 - f.Turn.InactivityTicks;
                    for (int i = 1; i < remaining; i++) f.Turn.AdvanceSimulationTick();
                    result.Assert(remaining > 0 && f.Turn.CurrentTeam == f.Team && f.Turn.CurrentPhase != TurnPhase.GameOver,
                        mode + ": inactivity <= 10 still cannot finish the turn");
                    f.Turn.AdvanceSimulationTick();
                    f.Turn.enabled = true;
                    if (mode == "last-actor")
                        result.Assert(f.Turn.CurrentPhase == TurnPhase.GameOver && f.Turn.GameResult == GameOverResult.Team2Wins &&
                                      f.Turn.TurnCount == 0 && f.Started == 0,
                            "last-actor: production settlement enters defeat, not another playable turn");
                    else
                    {
                        result.Assert(f.Turn.CurrentTeam == f.EnemyTeam && f.Turn.TurnCount == 1 && f.Started == 1 && f.Ended == 1,
                            mode + ": uncommitted death switches exactly once through normal turn events");
                        yield return new WaitForSeconds(0.6f);
                        result.Assert(f.Turn.TurnCount == 1 && f.Started == 1 && f.Team.SelectedCharacter == null,
                            mode + ": new unselected turn does not auto-pass or retain the dead actor");
                    }
                }
                yield return null;
            }

            Debug.Log("[TURN-DEATH-STAGE] nonlethal");
            using (var f = new Fixture(true))
            {
                yield return null;
                MutinyMine mine = f.PlaceMine();
                yield return WaitUntil(() => mine.IsStored && mine.PhysicsBody.SimulationTickCount >= 12, 3f);
                Vector2 origin = ActorPixels(f.Actor);
                f.Input.TrySelectCharacterForVerification(f.Team, origin);
                f.Input.SelectCharacterThrow();
                f.Input.TryBeginAimFromPrimaryPointerForVerification(f.Actor, origin);
                Vector2 fixedPointer = origin + new Vector2(32f, 40f);
                f.Input.AdvanceAimPointerForVerification(f.Actor, fixedPointer, true, false);
                yield return WaitUntil(() => f.Actor.Health < f.Actor.MaxHealth, 5f);
                result.Assert(f.Actor.IsAlive && f.Input.IsAiming && !f.Actor.PhysicsBody.IsAtRest && f.Turn.TurnCount == 0,
                    "MIN-AIM-01 nonlethal actual Mine explosion leaves the airborne actor aiming");

                // No further pointer frames: the held pointer is stationary.
                // Observe real LateUpdate while real Mine impulse/physics moves.
                int frames = 0;
                bool attached = true, currentDirection = true;
                Vector2 firstDisplay = MutinyPhysics.UnityToPixel(f.Actor.PhysicsBody.PresentationPosition);
                Vector2 lastDisplay = firstDisplay;
                m_ObserveAimFrame = () =>
                {
                    frames++;
                    Vector3 display = f.Actor.PhysicsBody.PresentationPosition;
                    lastDisplay = MutinyPhysics.UnityToPixel(display);
                    LineRenderer pull = f.Input.TrajectoryRenderer.GetComponent<LineRenderer>();
                    LineRenderer dash = f.Input.TrajectoryRenderer.transform.childCount > 0
                        ? f.Input.TrajectoryRenderer.transform.GetChild(0).GetComponent<LineRenderer>() : null;
                    bool hasLines = pull.enabled && pull.positionCount == 2 && dash != null && dash.enabled && dash.positionCount == 2;
                    attached &= hasLines && Vector3.Distance(pull.GetPosition(0), display) < 0.0001f &&
                                Vector3.Distance(dash.GetPosition(0), display) < 0.0001f;
                    if (hasLines)
                    {
                        Vector2 launch = MutinyPhysics.CalculateTwangVelocity(ActorPixels(f.Actor), fixedPointer,
                            MutinyWeaponFactory.GetTwangMaxForce(null));
                        Vector2 firstStep = MutinyTrajectoryRenderer.PredictVelocityTick(null, launch,
                            MutinyWeaponFactory.GetPredictionWeight(null));
                        Vector2 drawnStep = MutinyPhysics.UnityToPixel(dash.GetPosition(1)) - lastDisplay;
                        currentDirection &= Vector2.Dot(firstStep.normalized, drawnStep.normalized) > 0.9999f;
                    }
                    else currentDirection = false;
                };
                long startTick = f.Actor.PhysicsBody.SimulationTickCount;
                yield return WaitUntil(() => f.Actor.PhysicsBody.SimulationTickCount >= startTick + 4, 1f);
                m_ObserveAimFrame = null;
                result.Assert(frames >= 4 && Vector2.Distance(firstDisplay, lastDisplay) > 1f && attached,
                    "AIM-MOVE-01 stationary held pointer: pull/path track the actual mine-launched display pose across real ticks");
                result.Assert(currentDirection,
                    "AIM-MOVE-01 prediction uses the moving authoritative origin, not the initial press coordinates");

                f.Input.ResolveAimReleaseForVerification(f.Actor, ActorPixels(f.Actor));
                result.Assert(!f.Input.IsAiming && f.Actor.CanThrow && !f.Actor.IsSelfThrown && !f.Turn.ActionCommittedThisTurn,
                    "AIM-MOVE-02 release at current airborne origin cancels a short pull without consuming the jump");
                f.Input.TryBeginAimFromPrimaryPointerForVerification(f.Actor, ActorPixels(f.Actor));
                Vector2 expectedVelocity = MutinyPhysics.CalculateTwangVelocity(ActorPixels(f.Actor), fixedPointer,
                    MutinyWeaponFactory.GetTwangMaxForce(null));
                f.Input.ResolveAimReleaseForVerification(f.Actor, fixedPointer);
                result.Assert(Vector2.Distance(expectedVelocity, new Vector2(f.Actor.PhysicsBody.State.VelocityX,
                    f.Actor.PhysicsBody.State.VelocityY)) < 0.0001f,
                    "AIM-MOVE-02 airborne release commits the production Twang velocity from the current origin");
                result.Assert(f.Actor.IsSelfThrown && !f.Actor.CanThrow && f.Actor.CanShoot && f.Turn.ActionCommittedThisTurn,
                    "MIN-AIM-01 release in the air still commits the actual self throw");
                yield return WaitUntil(() => f.Turn.CurrentPhase == TurnPhase.TurnActive && !f.Turn.ActionCommittedThisTurn, 10f);
                result.Assert(f.Turn.CurrentTeam == f.Team && f.Turn.TurnCount == 0 && f.Actor.CanShoot,
                    "MIN-AIM-01 surviving jump settles to the same actor's remaining weapon action");
            }
            yield return null;

            Debug.Log("[TURN-DEATH-STAGE] unselected-death");
            using (var f = new Fixture(true))
            {
                yield return null;
                yield return new WaitForSeconds(0.6f);
                result.Assert(f.Turn.TurnCount == 0 && f.Team.SelectedCharacter == null,
                    "TURN-DEATH-03 idle turn with no selection must not auto-pass");
                f.Input.TrySelectCharacterForVerification(f.Team, ActorPixels(f.Actor));
                f.Teammate.TakeDamage(100f);
                yield return WaitUntil(() => !f.Teammate.IsResolvingHealthDisplay && f.Turn.CheckAllBodiesAtRest(), 8f);
                yield return new WaitForSeconds(0.6f);
                result.Assert(f.Turn.CurrentTeam == f.Team && f.Turn.TurnCount == 0 && !f.Turn.HasSelectedCharacterDied &&
                              f.Team.SelectedCharacter == f.Actor && f.Input.CanProcessCurrentTurnInputForVerification(),
                    "TURN-DEATH-03 death of an unselected teammate must not end the live actor's turn");
            }
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeout)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!condition()) throw new TimeoutException("Production state did not reach the expected transition.");
        }

        private static Vector2 ActorPixels(MutinyCharacter actor) =>
            new Vector2(actor.PhysicsBody.State.X, actor.PhysicsBody.State.Y);

        private sealed class Fixture : IDisposable
        {
            private readonly GameObject m_Host = new GameObject("SelectedDeathFixture");
            private readonly string[,] m_Terrain = new string[20, 50];
            private readonly MutinyMine m_Mine;
            public readonly MutinyTeam Team, EnemyTeam;
            public readonly MutinyCharacter Actor, Teammate, Enemy;
            public readonly MutinyTurnManager Turn;
            public readonly MutinyPlayerInput Input;
            public int Started, Ended;

            public Fixture(bool withTeammate)
            {
                for (int x = 0; x < 50; x++) m_Terrain[10, x] = "1";
                Team = Child("Team").AddComponent<MutinyTeam>(); Team.TeamNumber = 1;
                EnemyTeam = Child("EnemyTeam").AddComponent<MutinyTeam>();
                EnemyTeam.TeamNumber = 2; EnemyTeam.IsAiControlled = true; // Deterministic start; no AI component.
                Actor = Character("Actor", Team, 256f);
                if (withTeammate) Teammate = Character("Teammate", Team, 1000f);
                Enemy = Character("Enemy", EnemyTeam, 1300f);
                // Seed a mine from the previous round BEFORE a turn manager
                // exists. Fire normally commits an action to any live manager;
                // calling it mid-fixture would test a different, already-committed turn.
                m_Mine = CreateMine();
                Turn = Child("Turn").AddComponent<MutinyTurnManager>();
                Turn.Initialize(Team, EnemyTeam);
                Turn.OnTurnStarted += _ => Started++;
                Turn.OnTurnEnded += _ => Ended++;
                Input = Child("Input").AddComponent<MutinyPlayerInput>();
                Input.TurnManager = Turn;
            }

            private GameObject Child(string name)
            {
                var go = new GameObject(name); go.transform.SetParent(m_Host.transform, false); return go;
            }

            private MutinyCharacter Character(string name, MutinyTeam team, float x)
            {
                GameObject go = Child(name);
                go.transform.position = MutinyPhysics.PixelToUnity(x, 311.9f);
                MutinyCharacter actor = go.AddComponent<MutinyCharacter>();
                actor.Initialize("redPirate", team.TeamNumber, 0, 0, new Dictionary<string, string>());
                actor.PhysicsBody.SetTerrain(m_Terrain, 50, 20);
                actor.PhysicsBody.WaterPixelY = 1000f;
                team.RegisterCharacter(actor);
                return actor;
            }

            public MutinyMine PlaceMine() => m_Mine;

            private MutinyMine CreateMine()
            {
                GameObject go = Child("Mine");
                go.transform.position = MutinyPhysics.PixelToUnity(256f, 305.9f);
                MutinyMine mine = go.AddComponent<MutinyMine>();
                mine.Initialize(Enemy);
                // Initialize starts at its owner; seed the already placed mine
                // position before firing it through the production lifecycle.
                mine.PhysicsBody.State.X = 256f;
                mine.PhysicsBody.State.Y = 305.9f;
                go.transform.position = MutinyPhysics.PixelToUnity(256f, 305.9f);
                mine.PhysicsBody.SetTerrain(m_Terrain, 50, 20);
                mine.Fire(Vector2.zero);
                return mine;
            }

            public void Dispose()
            {
                m_Host.SetActive(false);
                foreach (MutinyExplosion blast in FindObjectsByType<MutinyExplosion>())
                    if (blast.Caster == Enemy) DestroyImmediate(blast.gameObject);
                // Cleanup only after assertions; explosion lifetime during each case
                // is driven by its real Update, never manually cleared to unblock a turn.
                DestroyImmediate(m_Host);
            }
        }
    }
}
