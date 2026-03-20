NEW: the prompt created for adding new languages

it is in the file:
 
 AddNewLanguagePrompt.md.

To add a new language in the future, just:
1.	Open the prompt
2.	Replace the placeholders LANGUAGE in the first line with whatever the new language is (e.g. German)
3.	Paste the prompt into Copilot Chat
 
That's it — one file to edit (Language.cs) + 7 AdminRes.de.resx files to create. The API, top-right picker, settings page, and request localization all pick it up automatically from All.





=====================================================



# Adding a New Language to Podium

## Overview

Podium uses `.resx` resource files for all UI text and a static list in the API for language metadata.
Adding a new language touches **4 areas** — all straightforward.

---

## Step 1 — Register the language code in the API

Open `Podium.Api/Endpoints/ProfileEndpoints.cs` and add one entry to `SupportedLanguages`:

```csharp
internal static readonly (string Code, string Name)[] SupportedLanguages =
[
    ("en", "English"),
    ("fr", "Français"),
    ("ru", "Русский"),
    ("de", "Deutsch"),   // ← new entry
];
```

This single array drives:
- The `GET /api/profile/languages` response (what the UI renders)
- The `POST /api/profile/language` validation (rejects unknown codes)
- The ASP.NET Core request-localization middleware in `Podium.Api/Program.cs`

---

## Step 2 — Create the resource files

For each resource file that exists, create a new `.<code>.resx` sibling.

All resource files live in `Podium.Shared/Resources/`:

| Base file | New file to create |
|---|---|
| `Common.resx` | `Common.de.resx` |
| `HomeRes.resx` | `HomeRes.de.resx` |
| `AuthRes.resx` | `AuthRes.de.resx` |
| `SettingsRes.resx` | `SettingsRes.de.resx` |
| `AdminRes.resx` | `AdminRes.de.resx` |
| `ApiMessages.resx` | `ApiMessages.de.resx` |
| *(any other `*.resx` in that folder)* | *(same pattern)* |

Each new file must:
1. Copy the structure of the base `.resx` (the English version)
2. Replace the `<value>` text with translations
3. Keep all `<data name="...">` keys identical to the base file

> **Missing keys fall back to English automatically** — you can ship a partial translation
> and fill in the rest later without breaking anything.

---

## Step 3 — Add the language to the MAUI bootstrap

Open `Podium.Native/MauiProgram.cs`. The default language is read from `Preferences`:

```csharp
var storedLanguage = Microsoft.Maui.Storage.Preferences.Default.Get("podium_language", "en");
```

No change needed here unless you want a different default for new installs.

---

## Step 4 — Add the nav button to MainLayout

Open `Podium.Shared/Layout/MainLayout.razor` and add a button alongside the existing ones:

```razor
<button class="lang-btn @(LangState.CurrentLanguageCode == "de" ? "lang-active" : "")"
        @onclick='() => ChangeLanguage("de")'>DE</button>
```

---

## Checklist

- [ ] Entry added to `ProfileEndpoints.SupportedLanguages`
- [ ] All `*.resx` files have a `*.<code>.resx` counterpart in `Podium.Shared/Resources/`
- [ ] Nav button added to `MainLayout.razor`
- [ ] Tested: language switcher shows the new language, selecting it reloads in the correct locale

---

## Notes

- The `LanguageCode` field on `PodiumUsers` stores the user's preference as a plain string (`"en"`, `"fr"`, etc.). No database schema change is needed.
- There is no `PodiumLanguages` table — the language list is static per deployment (adding a language always requires a code change + redeploy anyway).
- The `ApiMessages.resx` translations are used by the **server** (error messages returned from the API). The culture for a given API request is set from the `Accept-Language` header sent by the client.
