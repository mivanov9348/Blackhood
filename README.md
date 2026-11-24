# Blackhood *(работно заглавие)*

Браузърна игра с тайни роли на маса в странна средновековна таверна, по концепцията *The Devil's Game v0.4*.

📜 **Какво сме направили дотук:** [на български](docs/progress.bg.md) · [in English](docs/progress.en.md)

## Структура

| Папка | Какво е | С какво се отваря |
|---|---|---|
| `Blackhood.sln` | C# решението | **Visual Studio** |
| `src/Blackhood.Engine` | Чиста логика на играта: роли, тесте, пазар, начало на партия. Без уеб и без база. | |
| `src/Blackhood.Data` | EF Core **code-first**: потребители (Identity), партии, персонажи; миграциите са в `Migrations/` | |
| `src/Blackhood.Api` | ASP.NET Core API: вход, сингълплейър, админ на персонажите | |
| `client/` | React + Vite + TypeScript | **VS Code** |
| `content/characters/` | Персонажите като JSON (P01–P13, H01–H05) — пълнят празна база | |
| `content/portraits/` | Качените портрети | |
| `tools/import-sheet/` | Превръща .docx листовете на персонажите в JSON ([README](tools/import-sheet/README.md)) | |

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

## Тестов акаунт (само локално)

| Име | Имейл | Парола | Роля |
|---|---|---|---|
| `tester` | `tester@blackhood.test` | `candlelight42` | Admin |

Админите виждат линк **Персонажи** в главното меню → `/admin`.
