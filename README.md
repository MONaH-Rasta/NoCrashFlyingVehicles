Prevents flying vehicles from crushing. To prevent players from being abused as a weapon, the damage done by this vehicles is also disabled.

## Permissions

* `nocrashflyingvehicles.use` -- Allows players to toggle protection flying vehicles from crushing

## Commands

* `/nocrash` - Enable/disable protection flying vehicles from crushing

## Configuration

```json
{
  "Global settings": {
    "Enabled on start?": true,
    "Commands list": [
      "nocrash",
      "ncfv"
    ]
  },
  "Chat settings": {
    "Chat steamID icon": 0,
    "Notify admins only": true
  },
  "No Crash settings": {
    "MiniCopter enabled?": true,
    "ScrapTransportHelicopter enabled?": true,
    "CH47Helicopter enabled?": true
  }
}
```

## Localization

```json
{
  "Disabled": "<color=#B22222>Disabled</color>",
  "Enabled": "<color=#228B22>Enabled</color>",
  "NotAllowed": "You do not have permission to use this command",
  "Prefix": "<color=#00FFFF>[No Crash Flying Vehicles]</color>: ",
  "State": "<color=#FFA500>{0}</color> {1} No Crash Flying Vehicles"
}
```