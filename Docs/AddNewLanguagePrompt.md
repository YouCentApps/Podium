# Add New Language Prompt

Copy the prompt below, replace `LANGUAGE' with the desired language name. (in the first line) - leave the rest of the prompt as is, and follow the instructions to add a new language to the Podium app.




---

## Prompt

```
Add LANGUAGE  translation to the Podium app. Follow these steps in order:

everywhere below where you see Language Code or Native Language name placeholders you substitute it with correct value which you need to create yourself based on the language above.

### 1. Register the language
In `Podium.Shared/Models/Language.cs`, add a new entry to `SupportedLanguages.All`:
  new() { Code = "LANGUAGE CODE, Name = NATIVE LANGUAGE NAME, IsActive = true },

This is the single source of truth — the API, settings page fallback, and top-right picker all read from here. No other registration is needed.

### 2. Create all .resx translation files
For EACH of the following base resource files, create a corresponding `*.LANGUAGE_CODE.resx` file with all keys translated to LANGUAGE_NAME:

- `Podium.Shared/Resources/Common.resx`           → `Common.LANGUAGE_CODE.resx`
- `Podium.Shared/Resources/Auth.resx`              → `Auth.LANGUAGE_CODE.resx`
- `Podium.Shared/Resources/Predictions.resx`       → `Predictions.LANGUAGE_CODE.resx`
- `Podium.Shared/Resources/Leaderboard.resx`       → `Leaderboard.LANGUAGE_CODE.resx`
- `Podium.Shared/Resources/AdminRes.resx`          → `AdminRes.LANGUAGE_CODE.resx`
- `Podium.Shared/Resources/ApiMessages.resx`       → `ApiMessages.LANGUAGE_CODE.resx`
- `Podium.Shared/Resources/SettingsRes.resx`       → `SettingsRes.LANGUAGE_CODE.resx`

For each file:
- Read the base `.resx` to get ALL keys
- Create the new `.LANGUAGE_CODE.resx` with the same XML structure (resheaders + all data nodes)
- Translate every <value> to LANGUAGE_NAME
- Keep {0}, {1}, etc. format placeholders intact
- Keep &amp; XML entities intact
- Keep emoji prefixes (⏳, ✓, ❌) intact
- The new file MUST have the same number of <data> entries as the base file

### 3. Verify
- Run a build to confirm no errors
- Verify each new .resx file has the same line count as its base counterpart using:
  Get-ChildItem "Podium.Shared\Resources\*.resx" | ForEach-Object { $lines = (Get-Content $_.FullName).Count; "$($_.Name): $lines lines" } | Sort-Object

That's it — no changes needed in ProfileEndpoints.cs, Program.cs, MainLayout.razor, or LanguageSettings.razor since they all read from SupportedLanguages.All dynamically.
```
