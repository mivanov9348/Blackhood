# Blackhood — Progress so far

*Working title. Last updated: 24 Nov 2026.*

Blackhood is a browser game with secret roles, played at a table in a strange medieval tavern. It works like Mafia, except you play alone against four AI characters, each with their own personality. A Host runs the table and has interests of their own. The design is based on *The Devil's Game — Concept v0.4* and the character sheet *Devils_Seat_NPC_Profili*.

This document summarises what has been built so far, how it fits together and what comes next.

---

## 1. Tech stack

| Layer | Technology | Opened with |
|---|---|---|
| Game logic | C# class library (`Blackhood.Engine`), .NET 10 | Visual Studio |
| Data | EF Core 10, **code-first**, SQL Server LocalDB | Visual Studio |
| API | ASP.NET Core minimal APIs, Identity (cookie auth) | Visual Studio |
| Client | React 19 + Vite + TypeScript | VS Code |
| Content | JSON files in `content/characters/`, portraits in `content/portraits/` | any editor / the admin UI |

The server is **authoritative**. The full match state never leaves the server. The client only receives a `MatchView`, which contains just what the player is allowed to see. Other players' roles and hands are never sent.

---

## 2. Solution layout

```
Blackhood/
├─ Blackhood.sln
├─ src/
│  ├─ Blackhood.Engine/      pure game logic — cards, roles, match setup, views
│  ├─ Blackhood.Data/        EF Core DbContext, entities, migrations, character content I/O
│  └─ Blackhood.Api/         HTTP endpoints, auth, admin, startup seeding
├─ client/                   React app (menu, arrival scene, tavern table, admin editor)
├─ content/
│  ├─ characters/            P01–P13 (participants), H01–H05 (hosts) as JSON
│  └─ portraits/             uploaded portrait images
├─ tools/import-sheet/       converts the character-sheet .docx into JSON
└─ docs/                     this document (EN + BG)
```

---

## 3. What works today

### 3.1 Accounts
- Registration, login (by user name or e-mail) and logout. Uses ASP.NET Core Identity with an HTTP-only cookie.
- Role **Admin**. Users listed in `Admin:Users` (appsettings) get it automatically. In development the test user `tester` is an admin.
- The admin check reads the role from the database on every request, so a promotion takes effect without logging in again.

### 3.2 Main menu
- **Single-player** starts a new evening at the tavern table.
- **Multiplayer** is visible but locked ("Скоро" / "Soon").
- Admins also see a **Персонажи** (Characters) link to the editor.

### 3.3 Characters in the database
Every character is one `Character` row:

- **Columns** for what gets queried or computed:
  - code, kind (participant / host), name, title, rarity, status (Draft / Published)
  - **Skills** (1–5, budgeted): Insight, Calculation, Cunning, Charisma, Composure
  - **Drives** (1–5, no budget): Greed, Boldness, Loyalty, Revenge, Patience, Ambition, Pride, Fear of Loss
- **One JSON column per section of the character sheet**, using EF Core complex types: Origin, Appearance, Backstory, Psychology, Behaviour, Motives, Speech, Mechanics, Author notes, Host profile. The sheet can grow without needing a migration for every new field.
- **Child tables**:
  - `CharacterLine`: lines per situation (first meeting, win, accused, host introduction, death, …)
  - `CharacterTell`: visible tells tied to an internal state (lying, calculating, afraid, …), each flagged as fakeable or not and visible without the dossier or not
  - `CharacterRelation`: special relationships with a story, from −100 to +100
  - `CharacterAsset`: the portrait file
- `PlayerEncounter` records how many times a player has sat with each character. It drives the short "met again" intro now and the dossier later.

**Attribute decision.** The character sheet and concept v0.4 used different attributes, so they were merged:
- *Temper* is dropped (it is the opposite of Composure).
- *Risk Tolerance* is merged into Boldness.
- *Calculation* and *Pride*, which the sheet lacked, were added.

**New field „Странното“ (The Strange).** Each character has one unexplained, slightly otherworldly trait. This is the fantasy layer: Silas's coin from a country that is on no map, the gravedigger who hears the dead, and so on.

### 3.4 Content pipeline (JSON ⇄ database)
- **The database is the source of truth.** JSON files are used to seed a fresh database, for backups and for history in git.
- On startup (in development) the API **imports every character from `content/characters/` that is not in the database yet**. It never overwrites characters that already exist, because the editor owns them.
- The admin editor can **export** one character, or all of them, back to JSON.
- `tools/import-sheet/` converts the .docx character sheet into the 18 JSON files. It merges in authored additions (`additions.mjs`):
  - Calculation and Pride values, The Strange, portrait hue and tags
  - tells mapped to states, special abilities
  - intro lines: how the host introduces each guest, "met again" greetings, the host's three replies, the role-deal line
  - host attributes and the one rule each host changes
  - a few relationships, e.g. Agnes ↔ Godfrey the moneylender, Bertram ↔ Noah from Ashford

### 3.5 Admin character editor (`/admin`)
- **List view** with a portrait, code, rarity, status and a warning counter. Filters by kind and a search box. Creates new participants or hosts with the next free code.
- **Editor** with tabs that follow the character-sheet template:
  - Basic, Appearance, Story, Psychology, Attributes, Behaviour & motives, Speech & lines, Tells & mechanics, Host (hosts only), Notes
  - Skills and drives are set with 5-pip selectors. A **budget bar** shows the skill budget per rarity: Common 15 / Uncommon 17 / Rare 18 / Unique none.
  - Editors for lines, tells and relationships
  - **Portrait upload** (PNG, JPEG or WebP, up to 5 MB)
  - **Live checks** from the concept's rules, for example "at least one skill ≤ 2", "at least two extreme drives", "a first-meeting line exists" and "a host introduction exists". Errors block saving; warnings only inform.
  - **Preview**: how the character looks at the table and in the intro
  - Draft / Published toggle (only published characters appear in games), Ctrl+S to save, optimistic concurrency (two people editing the same character get a conflict instead of overwriting each other), export to JSON and delete
- Portrait placeholders: until real art exists, each character is a hooded silhouette tinted by their own hue.

### 3.6 The arrival (intro scene)
Every new single-player game starts with an interactive arrival:

1. **The door.** Darkness, rain and a sliver of light under the door. A line of text types out.
2. **The Host** (The Gentleman for now) steps out of the dark by candlelight and greets you. You choose a reply: *"Who are you?"*, *"Let's begin."* or silence. He answers differently to each, then the six candles light one by one.
3. **Four random guests** arrive one by one. For each one:
   - the host introduces them with a line about who they are
   - they say their first-meeting line
   - one visible **tell** is shown (*"spins the coin between his knuckles"*), which teaches tell-reading without a tutorial
   - guests the player has met before get a short "met again" greeting and a badge
4. **The role.** The host slides a face-down card across. You flip it to see your secret role, then sit at the table.

Every line types out letter by letter. Click, Space or Enter finishes the line and then moves on. **Skip** is always available. Whether the intro has been seen is saved with the match, so reloading the page goes straight to the table.

### 3.7 The tavern table (first person)
- The Host and the six candles (the match timer) sit at the far end of the table.
- The four guests sit around the table, each with a portrait, name, title, card count and the cards they took from the market.
- The **market** (3 face-up cards) and the **deck** are in the middle.
- At the bottom are your hand, your secret role (goal, skill, who to fear) and your soul.
- The **Journal** (public log) is on the left. The host's **Whispers** (private) are on the right.
- Cards show what they do **for your role**, following the "one card, a different action per role" table. They have no art or card back yet.

### 3.8 Engine
- The standard 40-card deck (14 gold, 4 dagger, 5 shield, 5 eye, 5 hand, 3 seal, 4 devil).
- The 5 prototype roles (Assassin, Inquisitor, Thief, Merchant, Priest).
- Seeded randomness: the same seed gives the same match.
- `MatchFactory.CreateSingleplayer` picks 4 random published guests and the default host, deals roles, 3 cards each and the market.
- `MatchView.ForSeat` projects only what a seat may see.

---

## 4. Running it

1. **API.** In Visual Studio, set `Blackhood.Api` as the startup project, choose the **http** profile and press F5. Or run:
   ```
   dotnet run --project src/Blackhood.Api --launch-profile http
   ```
   It migrates the LocalDB database and imports the characters on first start.
2. **Client.**
   ```
   cd client
   npm install
   npm run dev
   ```
   Then open http://localhost:5173.
3. **Test account** (local only): `tester` / `candlelight42`. It has the Admin role.

---

## 5. Not done yet / next steps

1. **Round phases in the engine**: draw → table → night → resolution → whisper → court. This is the first real game mechanic.
2. **NPC AI**: a belief model (who probably has which role) plus the utility AI described in the concept.
3. **Simulator**: AI-vs-AI matches to balance the characters. Most current balance warnings are the skill budget, because the sheet's numbers predate the budget rule.
4. **Portraits and expressions**: real art, then expressions (neutral, smug, tilted, afraid, dead).
5. **Deaths with effects**: a candle snuffed out, the portrait cracking, a cause-specific effect, the death line, the role revealed and a ghost.
6. **Dossier** ("Wall of Records"), built on `PlayerEncounter`.
7. **More hosts**: unlocking H02–H05, each with the rule they change.
8. **Multiplayer**: later.
