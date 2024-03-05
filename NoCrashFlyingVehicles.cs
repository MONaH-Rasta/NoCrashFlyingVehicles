using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System;

namespace Oxide.Plugins
{
    [Info("No Crash Flying Vehicles", "MON@H", "1.0.0")]
    [Description("Prevents flying vehicles from crashing.")]
    public class NoCrashFlyingVehicles : CovalencePlugin
    {
        #region Class Fields

        private const string PermissionUse = "nocrashflyingvehicles.use";
        private const uint _MiniCopterID = 2278499844;
        private const uint _ScrapCopterID = 3484163637;
        private const uint _CH47ID = 1675349834;
        private bool _enabled;

        #endregion Class Fields

        #region Initialization

        private void Init()
        {
            Unsubscribe(nameof(OnEntityTakeDamage));

            if (_configData.GlobalSettings.Commands.Length == 0)
            {
                _configData.GlobalSettings.Commands = new[] { "nocrash" };
                SaveConfig();
            }

            permission.RegisterPermission(PermissionUse, this);
            foreach (string command in _configData.GlobalSettings.Commands)
            {
                AddCovalenceCommand(command, nameof(CmdNoCrash));
            }                
        }

        private void OnServerInitialized()
        {
            if (!_CH47ID.Equals(StringPool.Get("assets/prefabs/npc/ch47/ch47.entity.prefab")))
            {
                PrintError($"CH47ID has changed! Report to developer: {StringPool.Get("assets/prefabs/npc/ch47/ch47.entity.prefab")}");
                return;
            }

            if (!_MiniCopterID.Equals(StringPool.Get("assets/content/vehicles/minicopter/minicopter.entity.prefab")))
            {
                PrintError($"MinicopterID has changed! Report to developer: {StringPool.Get("assets/content/vehicles/minicopter/minicopter.entity.prefab")}");
                return;
            }

            if (!_ScrapCopterID.Equals(StringPool.Get("assets/content/vehicles/scrap heli carrier/scraptransporthelicopter.prefab")))
            {
                PrintError($"ScrapcopterID has changed! Report to developer: {StringPool.Get("assets/content/vehicles/scrap heli carrier/scraptransporthelicopter.prefab")}");
                return;
            }

            _enabled = _configData.GlobalSettings.DefaultEnabled;

            if (_enabled)
            {
                Subscribe(nameof(OnEntityTakeDamage));
            }
        }

        #endregion Initialization

        #region Configuration

        private ConfigData _configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Global settings")]
            public GlobalSettings GlobalSettings = new GlobalSettings();

            [JsonProperty(PropertyName = "Chat settings")]
            public ChatSettings ChatSettings = new ChatSettings();

            [JsonProperty(PropertyName = "No Crash settings")]
            public NoCrashSettings NoCrashSettings = new NoCrashSettings();
        }

        private class GlobalSettings
        {
            [JsonProperty(PropertyName = "Enabled on start?")]
            public bool DefaultEnabled = true;

            [JsonProperty(PropertyName = "Commands list")]
            public string[] Commands = new[] { "nocrash", "ncfv" };
        }

        private class ChatSettings
        {
            [JsonProperty(PropertyName = "Chat steamID icon")]
            public ulong SteamIDIcon = 0;

            [JsonProperty(PropertyName = "Notify admins only")]
            public bool NotifyAdminsOnly = true;
        }

        private class NoCrashSettings
        {
            [JsonProperty(PropertyName = "MiniCopter enabled?")]
            public bool EnabledMiniCopter = true;

            [JsonProperty(PropertyName = "ScrapTransportHelicopter enabled?")]
            public bool EnabledScrapCopter = true;

            [JsonProperty(PropertyName = "CH47Helicopter enabled?")]
            public bool EnabledChinook = true;
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _configData = Config.ReadObject<ConfigData>();
                if (_configData == null)
                {
                    LoadDefaultConfig();
                    SaveConfig();
                }
            }
            catch
            {
                PrintError("The configuration file is corrupted");
                LoadDefaultConfig();
                SaveConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            _configData = new ConfigData();
        }

        protected override void SaveConfig() => Config.WriteObject(_configData);

        #endregion Configuration

        #region Localization

        private string Lang(string key, string userIDString = "", params object[] args)
        {
            try
            {
                return string.Format(lang.GetMessage(key, this, userIDString), args);
            }
            catch (Exception ex)
            {
                PrintError($"Lang Key '{key}' threw exception:\n{ex}");
                throw;
            }
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Disabled"] = "<color=#B22222>Disabled</color>",
                ["Enabled"] = "<color=#228B22>Enabled</color>",
                ["NotAllowed"] = "You do not have permission to use this command",
                ["Prefix"] = "<color=#00FFFF>[No Crash Flying Vehicles]</color>: ",
                ["State"] = "<color=#FFA500>{0}</color> {1} No Crash Flying Vehicles",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Disabled"] = "<color=#B22222>Отключил</color>",
                ["Enabled"] = "<color=#228B22>Включил</color>",
                ["NotAllowed"] = "У вас нет разрешения на использование этой команды",
                ["Prefix"] = "<color=#00FFFF>[Летающий транспорт без аварий]</color>: ",
                ["State"] = "<color=#FFA500>{0}</color> {1} летающий транспорт без аварий",
            }, this, "ru");
        }

        #endregion Localization

        #region Commands

        private void CmdNoCrash(IPlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.Id, PermissionUse))
            {
                PlayerSendMessage(player, Lang("NotAllowed", player.Id));
                return;
            }

            _enabled = !_enabled;

            switch (_enabled)
            {
                case true:
                    Subscribe(nameof(OnEntityTakeDamage));
                    break;
                case false:
                    Unsubscribe(nameof(OnEntityTakeDamage));
                    break;
            }

            foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
            {
                if (activePlayer.IsAlive())
                {
                    if (_configData.ChatSettings.NotifyAdminsOnly && !activePlayer.IsAdmin)
                    {
                        continue;
                    }

                    PlayerSendMessage(player, Lang("State", player.Id, player.Name, _enabled ? Lang("Enabled", player.Id) : Lang("Disabled", player.Id)));       
                }
            }
        }

        #endregion Commands

        #region OxideHooks

        private object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (!_enabled || info == null || entity == null || info.damageTypes.Has(Rust.DamageType.Decay))
            {
                return null;
            }

            BaseEntity attacker = info.Initiator;
            if (attacker == null)
            {
                return null;
            }

            switch (attacker.prefabID)
            {
                case _CH47ID:
                    if (_configData.NoCrashSettings.EnabledChinook) return true;
                    break;
                case _MiniCopterID:
                    if (_configData.NoCrashSettings.EnabledMiniCopter) return true;
                    break;
                case _ScrapCopterID:
                    if (_configData.NoCrashSettings.EnabledScrapCopter) return true;
                    break;
            }

            return null;
        }

        #endregion OxideHooks

        #region Helpers

        private void PlayerSendMessage(IPlayer player, string message)
        {
            (player.Object as BasePlayer).SendConsoleCommand("chat.add", 2, _configData.ChatSettings.SteamIDIcon, string.IsNullOrEmpty(Lang("Prefix", player.Id)) ? message : Lang("Prefix", player.Id) + message);
        }

        #endregion Helpers
    }
}