# Blackhood *(работно заглавие)*

Браузърна игра в странна средновековна страноприемница с кръчмарски игри. Главната игра е **Gifts (Дарове)** — анонимни дарове, Убиец и алчен Домакин. Самата игра е изцяло на английски.

📜 **Правила и документация (Word):**
- Правила на Gifts: [English](docs/Blackhood-Gifts-Rules-EN.docx) · [Български](docs/Blackhood-Gifts-Rules-BG.docx)
- Документация на проекта: [English](docs/Blackhood-Documentation-EN.docx) · [Български](docs/Blackhood-Documentation-BG.docx)
- По-ранни бележки (преди Gifts): [BG](docs/progress.bg.md) · [EN](docs/progress.en.md)

## Структура

| Папка | Какво е | С какво се отваря |
|---|---|---|
| `Blackhood.sln` | C# решението | **Visual Studio** |
| `src/Blackhood.Engine` | Чиста логика: `Gifts/` (правила, NPC AI, Домакин, изглед, числа) | |
| `src/Blackhood.Data` | EF Core **code-first**: потребители + кесия, партии, персонажи; миграциите са в `Migrations/` | |
| `src/Blackhood.Api` | ASP.NET Core API: вход, таверна, Gifts, админ на персонажите | |
| `tests/Blackhood.Engine.Tests` | xUnit: правилата + симулация на хиляди вечери AI срещу AI | |
| `client/` | React + Vite + TypeScript: зала на таверната, Пристигане, маса за Gifts, админ | **VS Code** |
| `content/characters/` | Персонажите като JSON (P01–P13, H01–H05), на английски — пълнят празна база | |
| `content/portraits/` | Качените портрети | |
| `tools/` | Внос на листовете, английски превод, генератор на Word документите (`node tools/word-docs/build.js`) | |

## Пускане

**1. API** — във Visual Studio: стартов проект `Blackhood.Api`, профил **http**, F5. Или:

```powershell
dotnet run --project src/Blackhood.Api --launch-profile http
```

API-то слуша на `http://localhost:5125`. В Development при старт:
- прилага миграциите към SQL Server LocalDB `(localdb)\MSSQLLocalDB`, база `Blackhood`
- внася от `content/characters/` персонажите, които ги няма в базата
- дава роля Admin на потребителите от `Admin:Users`

**2. Клиент** — в отделен терминал (или в терминала на VS Code):

```powershell
cd client
npm install
npm run dev
```

Отвори `http://localhost:5173`. Vite проксира `/api` към API-то, така че cookie-то за вход работи без CORS.

## Нова миграция (code-first)

След промяна в ентититата (`src/Blackhood.Data`):

```powershell
dotnet ef migrations add ИмеНаПромяната -p src/Blackhood.Data -s src/Blackhood.Api
```

Базата се обновява сама при следващо пускане на API-то.

## Проблеми

**`Cannot create file 'C:\Users\<ти>\Blackhood.mdf' because it already exists`** при старт на API-то.
LocalDB инстанцията е „забравила“ базата, но файловете ѝ са още на диска. Закачи ги обратно (данните се запазват):

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "CREATE DATABASE [Blackhood] ON (FILENAME = N'$env:USERPROFILE\Blackhood.mdf'), (FILENAME = N'$env:USERPROFILE\Blackhood_log.ldf') FOR ATTACH;"
```

Ако не ти трябват данните, изтрий двата файла `Blackhood.mdf` и `Blackhood_log.ldf` от `%USERPROFILE%` — API-то ще създаде базата наново и ще внесе персонажите.

## Тестов акаунт (само локално)

| Име | Имейл | Парола | Роля |
|---|---|---|---|
| `tester` | `tester@blackhood.test` | `candlelight42` | Admin |
| `quick` | `quick@blackhood.test` | `1` | — |

В Development паролата при регистрация няма ограничения (може и „1“).

## Логове на игрите

Всяка партия (Gifts и Bones) пише ред по ред в таблицата `GameLogs`: начало с ролите, всеки ход на играча, събитията, грешките, тайните нощни отчети и резултата. Админите ги виждат на `GET /api/admin/logs` и `GET /api/admin/logs/{matchId}`.

Админите виждат линк **Персонажи** в главното меню → `/admin`.
