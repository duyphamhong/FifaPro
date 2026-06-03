# FIFA Prediction Editing Instructions

This repo contains a FIFA/Euro football prediction game with a .NET backend and an Angular frontend. Use this file as the shared editing guide for future work.

## Repo Map

- `Backend/FifaPrediction/BehindAGirl.sln`
  - Main prediction/game API solution.
  - `BehindAGirl/`: ASP.NET Core Web API for matches, predictions, rankings, champion picks, chat, crawler triggers, JWT-protected game endpoints, and SignalR hub `/chat`.
  - `BehindAGirl.Common/`: constants, score/point helpers, HTML parsing extensions.
  - `Bot/`: HtmlAgilityPack crawler support, including FIFA ranking crawler code.
- `Backend/FifaPrediction/Who/Who.sln`
  - Identity/auth API solution.
  - `Who/`: ASP.NET Core Identity, JWT login/register/change-password/admin lock endpoints.
  - `Who.Common/`: auth constants, lock/status enums, small helpers.
- `Frontend/Boss`
  - Angular 12 application using Angular Material, SCSS, RxJS, SignalR, and ngx-toastr.
  - The active user flow currently routes `/` to `/about-you`; `/login` exists and redirects logged-in users back to `/about-you`.

## Ignore Generated Output

Do not edit generated or compiled artifacts unless explicitly asked:

- `**/bin/**`
- `**/obj/**`
- `Frontend/Boss/dist/**`
- `Frontend/Boss/node_modules/**`
- Visual Studio publish artifacts under `Properties/PublishProfiles` and `Properties/ServiceDependencies`, unless the task is deployment-specific.

The repo currently contains many generated backend outputs. Treat source files and project files as the source of truth.

## Backend Architecture

Prediction/game API:

- Entrypoints: `Backend/FifaPrediction/BehindAGirl/Program.cs` and `Startup.cs`.
- Controllers:
  - `MatchController`: predict scores, next/previous match, all matches, teams, match predictions.
  - `UserController`: rankings, player info, champion pick, prediction history, user additional info seeding.
  - `DataController`: crawler-triggered standing/match updates and bot prediction.
  - `ChatController`: JSON-file chat persistence plus SignalR broadcast.
- Services:
  - `DataInformationService`: main match, prediction, scoring, previous-match update, and match prediction logic.
  - `UserService`: ranking, profile, champion, and history logic.
  - `CrawlerService`: Wikipedia scraping for standings and matches.
- Repositories:
  - `DataInformationRepository`: EF Core match/team/prediction persistence.
  - `UserRepository`: user extra info and champion persistence.
- Data context: `BehindAGirl/Data/ApplicationDbContext.cs`.
- Important constants:
  - `BehindAGirl.Common/Constants/Constants.cs` contains round labels and point conversion.
  - `BehindAGirl.Common/Constants/WonType.cs` defines `Lose`, `Winner`, and `WinnerAndScore`.

Identity API:

- Entrypoints: `Backend/FifaPrediction/Who/Who/Program.cs` and `Startup.cs`.
- `AuthenticationController`: login, register, lock-user, change-password, register-admin.
- `Who/Data/ApplicationDbContext.cs`: ASP.NET Identity context.
- `Who/Models/ApplicationUser.cs`: identity user extension.

Both APIs target `net10.0` in the current `.csproj` files and use SQL Server EF Core packages.

## Frontend Architecture

- `Frontend/Boss/src/app/app-routing.module.ts`: routes. `/about-you` is the current main page.
- `Frontend/Boss/src/app/modules/about-you`: main prediction, ranking, history, champion, and profile UI.
- `Frontend/Boss/src/app/modules/dash-board`: dashboard/chat-oriented page that is present but not currently routed by default.
- `Frontend/Boss/src/app/modules/log-in`: login screen.
- `Frontend/Boss/src/app/core/services/auth.service.ts`: calls identity API through `environment.baseAuthenUrl`.
- `Frontend/Boss/src/app/core/services/data-service.service.ts`: calls prediction API through `ServiceInvokerService`.
- `Frontend/Boss/src/app/core/services/bases/service-invoker.service.ts`: wraps prediction API HTTP calls using `environment.baseApiUrl`.
- `Frontend/Boss/src/app/core/services/bases/customHttpInterceptor.ts`: adds bearer token and handles common HTTP errors.
- `Frontend/Boss/src/app/shared/constants/constant.ts`: frontend API path constants. Update this whenever backend routes change.
- `Frontend/Boss/src/environments/environment.ts`: local API URLs.
- `Frontend/Boss/src/environments/environment.prod.ts`: production Azure API URLs.

## Local URLs

Local launch settings and frontend environment currently align as:

- Identity/auth API: `https://localhost:5001`
- Prediction/game API: `https://localhost:6001`
- Angular app: `http://localhost:4200`

Run both backend APIs before using the Angular app locally.

## Common Edit Paths

When adding or changing a prediction/game endpoint:

1. Update the correct controller in `BehindAGirl/Controllers`.
2. Add or update the service interface in `BehindAGirl/Services/Interfaces`.
3. Implement behavior in `BehindAGirl/Services/Implements`.
4. Add persistence changes in `BehindAGirl/Repositoties/Interfaces` and `BehindAGirl/Repositoties/Implements` if needed. Keep the existing misspelled folder name unless doing a deliberate rename.
5. Update frontend path constants in `Frontend/Boss/src/app/shared/constants/constant.ts`.
6. Update the relevant frontend service in `Frontend/Boss/src/app/core/services`.
7. Update the Angular component/template/SCSS that owns the visible flow.

When changing login, password, user status, or JWT behavior:

1. Start in `Backend/FifaPrediction/Who/Who/Controllers/AuthenticationController.cs`.
2. Update `Who/Services` or `Who/Models` if the auth contract changes.
3. Mirror frontend changes in `Frontend/Boss/src/app/core/services/auth.service.ts`, the login component, or the HTTP interceptor.

When changing scoring or ranking rules:

1. Update `BehindAGirl.Common/Constants/Constants.cs` for point values or round labels.
2. Update `BehindAGirl/Services/Implements/DataInformationService.cs` for win/loss score evaluation.
3. Update `BehindAGirl/Services/Implements/UserService.cs` for ranking/profile presentation.
4. Check Angular ranking and history rendering under `modules/about-you`.

When changing crawler behavior:

1. Start in `BehindAGirl/Services/Implements/CrawlerService.cs`.
2. Keep scraping rules resilient because Wikipedia markup may change.
3. Update `BehindAGirl.Common/Extensions/Extension.cs` only for shared parsing helpers.
4. Verify `DataController` trigger endpoints still call the intended crawler methods.

When changing chat:

1. Backend HTTP persistence is in `BehindAGirl/Controllers/ChatController.cs`.
2. SignalR hub mapping is in `Startup.cs` and hub classes under `BehindAGirl/HubConfig`.
3. Frontend SignalR use is split between `modules/dash-board/dash-board.component.ts` and `core/services/signal-r.service.ts`.
4. Chat files are written to `BehindAGirl/ChatData/chat-{matchId}.json`.

## Build And Run

Backend restore/build:

```powershell
dotnet restore Backend\FifaPrediction\BehindAGirl.sln
dotnet build Backend\FifaPrediction\BehindAGirl.sln
dotnet restore Backend\FifaPrediction\Who\Who.sln
dotnet build Backend\FifaPrediction\Who\Who.sln
```

Backend local run:

```powershell
dotnet run --project Backend\FifaPrediction\Who\Who\Who.csproj
dotnet run --project Backend\FifaPrediction\BehindAGirl\BehindAGirl.csproj
```

Frontend install/build/run:

```powershell
Set-Location Frontend\Boss
npm install
npm run build
npm run start
```

The Angular scripts already use `--openssl-legacy-provider`, which is important for older Angular/Webpack tooling on newer Node versions.

## Verification Rules

- For backend-only changes, build the touched solution at minimum.
- For frontend-only changes, run `npm run build` from `Frontend/Boss`.
- For API contract changes, build both backend and frontend.
- For crawler changes, manually exercise `api/Data/update-standings` or `api/Data/update-matches` only after confirming the target website markup and local database are ready.
- For auth-protected API calls, verify a login token is present because the frontend interceptor always attaches `Authorization: Bearer <token>`.

## Known Risks And Cleanup Candidates

- Sensitive configuration values are currently committed in backend appsettings and scaffolded context code. Do not copy secrets into new files. Prefer user secrets, environment variables, or safe placeholders when cleaning this up.
- `BehindAGirl/Data/ApplicationDbContext.cs` contains a scaffold warning and fallback connection string. Prefer configuration-driven connection strings.
- `Startup.ConfigureServices` calls `AddCustomDbContext(Configuration)` twice in both APIs. Check this before debugging duplicate context registration behavior.
- CORS is permissive with `AllowAnyOrigin`, `AllowAnyMethod`, and `AllowAnyHeader`.
- The source contains old names and typos such as `BehindAGirl`, `Repositoties`, `Addtional`, and `CommingSoon`. Preserve them for targeted edits unless the task is an intentional rename.
- Several visible messages are informal and multilingual. Treat UI/API wording as product behavior and only change it when requested.
- `ChatController` uses local JSON files for chat history. This may not be durable in cloud/container deployments.
- The crawler is coupled to Wikipedia HTML structure and tournament-specific round counts.

## Next Phase Editing Workflow

For the next phase, start each requested change by identifying which surface it touches:

- UI only: edit Angular module/component/service and run `npm run build`.
- Prediction/game API: edit `BehindAGirl` controller/service/repository/model and build `BehindAGirl.sln`.
- Auth/user API: edit `Who` controller/service/model and build `Who.sln`.
- Cross-contract change: update backend route/model, frontend `Api` constant, frontend service, component usage, then build both sides.
- Database change: update entity/context/migrations deliberately and confirm the target SQL Server database before running migration commands.

Prefer small, traceable edits. After every meaningful change, verify with the narrowest command that proves the touched surface still works.
