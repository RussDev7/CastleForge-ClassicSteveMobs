/*
SPDX-License-Identifier: GPL-3.0-or-later
Copyright (c) 2025 RussDev7
This file is part of https://github.com/RussDev7/CastleForge - see LICENSE for details.
*/

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;

using static ClassicSteveMobs.GamePatches;

namespace ClassicSteveMobs
{
    #region Runtime Settings

    /// <summary>
    /// Live config values used by patches, commands, and the renderer.
    /// </summary>
    internal static class ClassicSteveSettings
    {
        public static bool  Enabled              = true;
        public static bool  NaturalSpawns        = false;
        public static float NaturalSpawnChance   = 0.02f;
        public static int   CommandSpawnDistance = 5;

        public static float Health          = 2f;
        public static int   SpawnRadius     = 20;
        public static int   DistanceLimit   = 40;
        public static float SlowSpeed       = 1.75f;
        public static float RandomSlowSpeed = 0.5f;
        public static float FastSpeed       = 3.0f;
        public static bool  HasRunFast      = false;

        public static float WanderTurnIntervalMin   = 0.5f;
        public static float WanderTurnIntervalMax   = 2.0f;
        public static float WanderTurnAmountRadians = 1.5f;
        public static float WanderJumpSpeed         = 7.0f;
        public static float WanderJumpCooldown      = 0.35f;
        public static float WanderRandomJumpChance  = 1.25f;

        public static float ModelScale       = 0.065f;
        public static float AnimationSpeed   = 10f;
        public static float YawOffsetDegrees = 0f;
    }
    #endregion

    #region Config Loader

    /// <summary>
    /// Minimal INI config for ClassicSteveMobs.
    /// </summary>
    internal sealed class ClassicSteveConfig
    {
        #region Fields

        public static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "!Mods",
            "ClassicSteveMobs",
            "ClassicSteveMobs.ini");

        // Hotkey to reload this config at runtime.
        public static string ReloadConfigHotkey = "Ctrl+Shift+R";

        #endregion

        #region Public API

        /// <summary>
        /// Loads or creates ClassicSteveMobs.ini and applies it to runtime settings.
        /// </summary>
        public static void LoadApply()
        {
            try
            {
                EnsureConfigExists();
                SimpleIni ini = SimpleIni.Load(ConfigPath);

                ClassicSteveSettings.Enabled              = ini.GetBool("ClassicSteveMobs", "Enabled", true);
                ClassicSteveSettings.NaturalSpawns        = ini.GetBool("ClassicSteveMobs", "NaturalSpawns", false);
                ClassicSteveSettings.NaturalSpawnChance   = Clamp(ini.GetFloat("ClassicSteveMobs", "NaturalSpawnChance", 0.02f), 0f, 1f);
                ClassicSteveSettings.CommandSpawnDistance = Clamp(ini.GetInt("ClassicSteveMobs", "CommandSpawnDistance", 5), 1, 50);

                ClassicSteveSettings.Health          = Clamp(ini.GetFloat("EnemyStats", "Health", 2f), 0.1f, 500f);
                ClassicSteveSettings.SpawnRadius     = Clamp(ini.GetInt("EnemyStats", "SpawnRadius", 20), 1, 100);
                ClassicSteveSettings.DistanceLimit   = Clamp(ini.GetInt("EnemyStats", "DistanceLimit", 40), 1, 1000);
                ClassicSteveSettings.SlowSpeed       = Clamp(ini.GetFloat("EnemyStats", "SlowSpeed", 1.75f), 0.1f, 20f);
                ClassicSteveSettings.RandomSlowSpeed = Clamp(ini.GetFloat("EnemyStats", "RandomSlowSpeed", 0.5f), 0f, 20f);
                ClassicSteveSettings.FastSpeed       = Clamp(ini.GetFloat("EnemyStats", "FastSpeed", 3.0f), 0.1f, 30f);
                ClassicSteveSettings.HasRunFast      = ini.GetBool("EnemyStats", "HasRunFast", false);

                ClassicSteveSettings.WanderTurnIntervalMin   = Clamp(ini.GetFloat("Behavior", "WanderTurnIntervalMin", 0.5f), 0.05f, 30f);
                ClassicSteveSettings.WanderTurnIntervalMax   = Clamp(ini.GetFloat("Behavior", "WanderTurnIntervalMax", 2.0f), 0.05f, 30f);
                ClassicSteveSettings.WanderTurnAmountRadians = Clamp(ini.GetFloat("Behavior", "WanderTurnAmountRadians", 1.5f), 0f, 6.2831855f);
                ClassicSteveSettings.WanderJumpSpeed         = Clamp(ini.GetFloat("Behavior", "WanderJumpSpeed", 7.0f), 0f, 30f);
                ClassicSteveSettings.WanderJumpCooldown      = Clamp(ini.GetFloat("Behavior", "WanderJumpCooldown", 0.35f), 0.05f, 30f);
                ClassicSteveSettings.WanderRandomJumpChance  = Clamp(ini.GetFloat("Behavior", "WanderRandomJumpChance", 1.25f), 0f, 10f);

                ClassicSteveSettings.ModelScale       = Clamp(ini.GetFloat("Rendering", "ModelScale", 0.065f), 0.01f, 1f);
                ClassicSteveSettings.AnimationSpeed   = Clamp(ini.GetFloat("Rendering", "AnimationSpeed", 10f), 0f, 100f);
                ClassicSteveSettings.YawOffsetDegrees = ini.GetFloat("Rendering", "YawOffsetDegrees", 0f);

                ReloadConfigHotkey = ini.GetString("Hotkeys", "ReloadConfig", "Ctrl+Shift+R");
                CSMHotkeys.SetReloadBinding(ReloadConfigHotkey);
            }
            catch (Exception ex)
            {
                ModLoader.LogSystem.Log($"Failed to load config: {ex.Message}.");
            }
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Writes a default config with comments when missing.
        /// </summary>
        private static void EnsureConfigExists()
        {
            string dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(ConfigPath))
                return;

            File.WriteAllLines(ConfigPath, new[]
            {
                "; ClassicSteveMobs - test rd-132328-style mob config",
                "; Uses EnemyTypeEnum.TREASURE_ZOMBIE as the custom test slot.",
                "",
                "[ClassicSteveMobs]",
                "; Master toggle for patches, command spawning, and natural spawning.",
                "Enabled              = true",
                "; If true, some aboveground night spawns become classic Steve mobs.",
                "NaturalSpawns        = false",
                "; 0.02 = 2% chance when NaturalSpawns is enabled.",
                "NaturalSpawnChance   = 0.02",
                "; Distance in front of the local player for /spawnsteve.",
                "CommandSpawnDistance = 5",
                "",
                "[EnemyStats]",
                "; Low test health to match early-game/simple mob behavior.",
                "Health          = 2",
                "SpawnRadius     = 20",
                "DistanceLimit   = 40",
                "SlowSpeed       = 1.75",
                "RandomSlowSpeed = 0.5",
                "FastSpeed       = 3.0",
                "HasRunFast      = false",
                "",
                "[Behavior]",
                "; Random wandering values. The mob does not chase the player in this test version.",
                "WanderTurnIntervalMin   = 0.5",
                "WanderTurnIntervalMax   = 2.0",
                "; Maximum signed heading change when the wander timer rolls over. 1.5 radians is about 86 degrees.",
                "WanderTurnAmountRadians = 1.5",
                "; Vertical velocity added when the mob bumps a wall or randomly hops.",
                "WanderJumpSpeed         = 7.0",
                "WanderJumpCooldown      = 0.35",
                "; Chance per second to hop while walking on flat ground. 0 disables random hops.",
                "WanderRandomJumpChance  = 1.25",
                "",
                "[Rendering]",
                "; Classic model height is 32 texture/model units. 0.065 makes it about CMZ-player sized.",
                "ModelScale       = 0.065",
                "; Original rd-132328 render uses time * 10.",
                "AnimationSpeed   = 10",
                "; If the model faces backward in-game, try 180.",
                "YawOffsetDegrees = 0",
                "",
                "[Hotkeys]",
                "; Reload this config while in-game:",
                "ReloadConfig = Ctrl+Shift+R",
            });
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
        private static float Clamp(float v, float lo, float hi) => v < lo ? lo : (v > hi ? hi : v);

        #endregion
    }
    #endregion

    #region SimpleIni

    /// <summary>
    /// Tiny, case-insensitive INI reader.
    /// Supports [Section], key=value, ';' or '#' comments. No escaping, no multi-line.
    /// </summary>
    internal sealed class SimpleIni
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Loads an INI file from disk into a simple nested dictionary:
        ///   section -> (key -> value).
        /// Unknown / malformed lines are ignored.
        /// </summary>
        public static SimpleIni Load(string path)
        {
            var ini = new SimpleIni();
            string section = "";

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith(";") || line.StartsWith("#")) continue;

                // Section header: [SectionName].
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2).Trim();
                    if (!ini._data.ContainsKey(section))
                        ini._data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }

                // Key/value pair: key = value.
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;

                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();

                if (!ini._data.TryGetValue(section, out var dict))
                {
                    dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    ini._data[section] = dict;
                }
                dict[key] = val;
            }

            return ini;
        }

        /// <summary>
        /// Reads an int from the INI and clamps it to the inclusive range [min..max].
        /// Returns <paramref name="def"/> if missing/invalid before clamping.
        /// </summary>
        public int GetClamp(string sec, string key, int def, int min, int max)
        {
            var v = GetInt(sec, key, def);
            if (v < min) v = min;
            if (v > max) v = max;
            return v;
        }

        /// <summary>
        /// Reads a string value from [section] key=... and returns <paramref name="def"/> if missing.
        /// </summary>
        public string GetString(string section, string key, string def)
            => (_data.TryGetValue(section, out var d) && d.TryGetValue(key, out var v)) ? v : def;

        /// <summary>
        /// Reads an int value from [section] key=... using invariant culture; returns <paramref name="def"/> on failure.
        /// </summary>
        public int GetInt(string section, string key, int def)
            => int.TryParse(GetString(section, key, def.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out var v) ? v : def;

        /// <summary>
        /// Reads a double value from [section] key=... using invariant culture; returns <paramref name="def"/> on failure.
        /// </summary>
        public double GetDouble(string section, string key, double def)
            => double.TryParse(GetString(section, key, def.ToString(CultureInfo.InvariantCulture)),
                               NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : def;

        /// <summary>
        /// Reads a float value from [section] key=... using invariant culture; returns <paramref name="def"/> on failure.
        /// </summary>
        public float GetFloat(string section, string key, float def)
            => float.TryParse(GetString(section, key, def.ToString(CultureInfo.InvariantCulture)), NumberStyles.Float,
                              CultureInfo.InvariantCulture, out var v) ? v : def;

        /// <summary>
        /// Reads a double value from [section] key=... using invariant culture; returns <paramref name="def"/> on failure.
        /// </summary>
        public bool GetBool(string section, string key, bool def)
        {
            var s = GetString(section, key, def ? "true" : "false");
            if (bool.TryParse(s, out var b)) return b;
            if (int.TryParse(s, out var i)) return i != 0;
            return def;
        }
    }
    #endregion
}