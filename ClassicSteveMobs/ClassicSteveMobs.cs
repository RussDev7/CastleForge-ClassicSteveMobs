/*
SPDX-License-Identifier: GPL-3.0-or-later
Copyright (c) 2025 RussDev7
This file is part of https://github.com/RussDev7/CastleForge - see LICENSE for details.

Test-only note:
This test mod embeds the original rd-132328 char.png from the provided preclassic source archive.
Keep that bundled texture private/local unless you replace it with an original CastleForge asset.
*/

using Microsoft.Xna.Framework;
using System.Reflection;
using DNA.CastleMinerZ;
using ModLoaderExt;
using DNA.Input;
using ModLoader;
using System.IO;
using System;

using static ModLoader.LogSystem;

namespace ClassicSteveMobs
{
    /// <summary>
    /// Test mod that registers the unused TREASURE_ZOMBIE enemy slot as a classic
    /// rd-132328-style blocky Steve mob and renders it with the original char.png.
    /// </summary>
    [Priority(Priority.Normal)]
    [RequiredDependencies("ModLoaderExtensions")]
    public class ClassicSteveMobs : ModBase
    {
        /// <summary>
        /// Entrypoint for the ClassicSteveMobs mod: Sets up command dispatching, patches, and world lookup.
        /// </summary>
        #region Mod Initiation

        private readonly CommandDispatcher _dispatcher; // Dispatcher that routes incoming "/commands" to attributed methods.
        // private object                  _world;      // Holds the reference to the game's world object once it becomes available.

        // Mod constructor: Invoked by the ModLoader when instantiating your mod.
        public ClassicSteveMobs() : base("ClassicSteveMobs", new Version("0.0.1.0"))
        {
            EmbeddedResolver.Init();                    // Load any native & managed DLLs embedded as resources (e.g., Harmony, cimgui, other libs).
            _dispatcher = new CommandDispatcher(this);  // Create the command dispatcher, pointing it at this instance so it can find [Command]-annotated methods.

            var game = CastleMinerZGame.Instance;       // Hook into the game's shutdown event to clean up patches and resources on exit.
            if (game != null)
                game.Exiting += (s, e) => Shutdown();
        }

        /// <summary>
        /// Called once when the mod is first loaded by the ModLoader.
        /// Good place to:
        /// 1) Verify the game is running.
        /// 2) Install any Harmony patches or interceptors.
        /// 3) Create and load the config.
        /// 4) Register your command handlers.
        /// </summary>
        public override void Start()
        {
            // Acquire game and world references.
            var game = CastleMinerZGame.Instance;
            if (game == null)
            {
                Log("Game instance is null.");
                return;
            }

            // Extract embedded resources for this mod into the
            // !Mods/<Namespace> folder; skipped if nothing embedded.
            var ns = typeof(ClassicSteveMobs).Namespace;
            var dest = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "!Mods", ns);
            var wrote = EmbeddedExporter.ExtractFolder(ns, dest);
            if (wrote > 0) Log($"Extracted {wrote} file(s) to {dest}.");

            // Apply game patches.
            GamePatches.ApplyAllPatches();

            // Load or create config.
            ClassicSteveConfig.LoadApply();

            // Register this plugin's command dispatcher with the interceptor.
            // Each time a player types "/command", our dispatcher will be invoked.
            // Also register this plugin's command list to the global help registry.
            ChatInterceptor.RegisterHandler(raw => _dispatcher.TryInvoke(raw));
            HelpRegistry.Register(this.Name, commands);

            // Notify in log that the mod is ready.
            // Lazy: Use this namespace as the 'mods' name.
            Log($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace} loaded.");
        }

        /// <summary>
        /// Called when the game exits or mod is unloaded.
        /// Used to safely dispose patches and resources.
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                try { GamePatches.DisableAll();       } catch (Exception ex) { Log($"Disable hooks failed: {ex.Message}.");    } // Unpatch Harmony.
                try { ClassicSteveRenderer.Dispose(); } catch (Exception ex) { Log($"Renderer dispose failed: {ex.Message}."); } // Unpatch classic steve renderer.

                // Notify in log that the mod teardown was complete.
                // Lazy: Use this namespace as the 'mods' name.
                Log($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace} shutdown complete.");
            }
            catch (Exception ex)
            { Log($"Error shutting down mod: {ex}."); }
        }

        /// <summary>
        /// Called once per game tick.
        /// Not used by this mod (but required by ModBase).
        /// </summary>
        public override void Tick(InputManager inputManager, GameTime gameTime) { }

        #endregion

        /// <summary>
        /// This is the main command logic for the mod.
        /// </summary>
        #region Chat Command Functions

        #region Help Command List

        private static readonly (string command, string description)[] commands = new (string, string)[]
        {
            ("spawnsteve", "Spawn a test rd-132328-style Steve mob in front of you."),
            ("stevemob reload", "Reload the ClassicSteveMobs config."),
            ("stevemob info", "Show ClassicSteveMobs runtime info.")
        };
        #endregion

        #region Chat Commands

        [Command("/spawnsteve")]
        [Command("/steve")]
        private static void ExecuteSpawnSteve(string[] args)
        {
            try
            {
                if (!ClassicSteveSettings.Enabled)
                {
                    SendFeedback("Mod is disabled in config.");
                    return;
                }

                int amount = 1;
                if (args.Length >= 1 && (!int.TryParse(args[0], out amount) || amount < 1))
                {
                    SendFeedback("ERROR: Command usage /spawnsteve [amount]");
                    return;
                }

                amount = Math.Min(amount, 1000);
                int spawned = ClassicSteveSpawner.SpawnNearLocalPlayer(amount);
                SendFeedback($"Spawned {spawned} classic Steve mob(s).");
            }
            catch (Exception ex)
            {
                SendFeedback($"ERROR: {ex.Message}");
                Log($"/spawnsteve failed: {ex}.");
            }
        }

        [Command("/stevemob")]
        private static void ExecuteSteveMob(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    SendFeedback("ERROR: Command usage /stevemob [reload|info]");
                    return;
                }

                switch (args[0].ToLowerInvariant())
                {
                    case "reload":
                        ClassicSteveConfig.LoadApply();
                        ClassicSteveRegistry.EnsureRegistered();
                        SendFeedback("Config reloaded.");
                        break;

                    case "info":
                        SendFeedback($"Enabled={ClassicSteveSettings.Enabled}, NaturalSpawns={ClassicSteveSettings.NaturalSpawns}, Chance={ClassicSteveSettings.NaturalSpawnChance:0.###}, Scale={ClassicSteveSettings.ModelScale:0.###}.");
                        SendFeedback($"EnemyType={ClassicSteveRegistry.ClassicSteveEnemyType}, VanillaSafeMode={ClassicSteveSettings.VanillaSafeMode}.");
                        break;

                    default:
                        SendFeedback("ERROR: Command usage /stevemob [reload|info]");
                        break;
                }
            }
            catch (Exception ex)
            {
                SendFeedback($"ERROR: {ex.Message}");
                Log($"/stevemob failed: {ex}.");
            }
        }
        #endregion

        #endregion
    }
}