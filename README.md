# AdsConnect API

ASP.NET Core Web API over the existing **`Ads_Connect`** PostgreSQL 18 database.
It is the only thing that talks to Postgres — the TanStack Start frontend calls
this API over HTTP from inside its server functions.

```
TanStack Start (:8080) --HTTP--> AdsConnect.api (:5036) --EF Core--> Postgres (:5432)
```

## Layout

Mirrors the PMS / ProjectMgnt solution at `D:\PMS\back-end` — two projects, a
web layer and a data layer, with the same folder names.

```
backend/
  AdsConnect.sln
  AdsConnect.api/            <- Member.api equivalent
    AuthService/             (empty - token issuing lives in Utils/, as in PMS)
    Controllers/
    Properties/launchSettings.json
    Utils/                   JwtService.cs, JwtSettings.cs
    wwwroot/                 (empty)
    appsettings.json
    Program.cs
    ResponseData.cs
  AdsConnect.data/           <- Member.data equivalent
    Dtos/
    Enum/                    (empty)
    Hubs/                    (empty)
    Interface/
    MappingClass/MappingProfile.cs
    Migrations/              (empty - the database is scaffolded, not migrated)
    Model/                   33 scaffolded entities
    Repository/
    Utils/                   (empty)
    AdsConnectContext.cs     <- PMSContext equivalent
```

Empty folders carry a `.gitkeep` so the structure survives a clone. Conventions
follow PMS: interfaces `IXxxService` in `Interface/`, implementations `XxxService`
in `Repository/`, registered with `AddTransient`, and every controller action
wrapped in `ResponseData<T>`.

## Differences from PMS, and why

| | PMS | AdsConnect |
|---|---|---|
| Database | SQL Server | **PostgreSQL** (Npgsql) — this is the existing `Ads_Connect` DB |
| Entities | hand-written + migrations | **scaffolded** from the live database |
| Auth | ASP.NET Identity + JWT | **JWT over the existing `AppUser` / `Role` / `UserRole` tables** — no Identity, no `UserManager`; passwords are BCrypt digests in `AppUser.PasswordHash` |
| 2FA / password reset | TOTP challenge token, `IMailSender` | **not implemented** — there is no mail or SMS sender here |
| Connection string | full value in `appsettings.json` | key in `appsettings.json`, **value empty** - supplied by user-secrets or environment |
| JWT signing key | value in `appsettings.json` | same treatment as the connection string — **empty** in the committed file |

## Configuration

The connection string is **never** committed. `appsettings.json` ships an empty
placeholder; supply the real value one of two ways:

```sh
cd AdsConnect.api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:PostgresConnectionString" "Host=localhost;Port=5432;Database=Ads_Connect;Username=postgres;Password=YOUR_PASSWORD"
```

```sh
# or via environment variable (note the double underscore)
export ConnectionStrings__PostgresConnectionString="Host=localhost;Port=5432;Database=Ads_Connect;Username=postgres;Password=YOUR_PASSWORD"
```

Startup fails with an explicit message if neither is set.

### JWT signing key

`Jwt:Key` is a credential, so it gets the same treatment — declared empty in
`appsettings.json`, supplied out of band:

```sh
dotnet user-secrets set "Jwt:Key" "<at least 32 random characters>"
# or: export Jwt__Key="<at least 32 random characters>"
```

**In Development a missing key is not fatal**: the API generates a random one for
the process and logs a warning. That keeps `bun run api` working with nothing to
configure, at the cost of invalidating every issued token when the API restarts —
you just sign in again. Outside Development a missing key stops startup.

`Jwt:Issuer`, `Jwt:Audience` and `Jwt:TokenExpiryTimeInHour` (default 24) are not
secrets and do live in `appsettings.json`.

## Running

```sh
dotnet run --project AdsConnect.api
```

- API: <http://localhost:5036>
- Swagger UI: <http://localhost:5036/swagger>
- Health: <http://localhost:5036/health>

The frontend reads the base URL from `API_BASE_URL` in
`velorix-clarity-bridge-main/.env` (default `http://localhost:5036`).

## Endpoints

All return `ResponseData<T>` — `{ data, errMessage, success }` — with HTTP 200
even on failure, per the PMS convention.

| Method | Route | Auth | Returns |
|---|---|---|---|
| POST | `/api/SignIn/Login` | — | `{ token, user }`, or `success: false` with the reason |
| POST | `/api/SignIn/Register` | — | Creates the account **and signs it in**: same `{ token, user }` |
| GET | `/api/SignIn/Me` | Bearer | The user behind the token |
| POST | `/api/SignIn/ChangePassword` | Bearer | `success` plus a message |
| GET | `/api/Reference/GetReferenceData` | Bearer | Industries, locations, provider types, channels, pricing units, campaign objectives |
| GET | `/api/Provider/GetProviders` | Bearer | All active providers as directory cards |
| GET | `/api/Provider/GetProviderById?id={guid}` | Bearer | One provider, or `success: false` |
| GET | `/api/Profile/GetMyProfile` | Bearer | `{ advertiser, provider }`, either or both null |
| POST | `/api/Profile/SaveAdvertiserProfile` | Advertiser | Creates the Advertiser row, then updates it |
| POST | `/api/Profile/SaveProviderProfile` | Provider | Provider + primary ProviderLocation + primary ProviderChannel |
| GET | `/api/Campaign/GetMyCampaigns` | Advertiser | The caller's campaigns, newest first |
| POST | `/api/Campaign/CreateCampaign` | Advertiser | The created campaign |
| GET | `/api/Campaign/GetAllCampaigns` | Admin | Every campaign, with the advertiser's name |
| POST | `/api/Request/CreateRequest` | Advertiser | The new conversation's id |
| GET | `/api/Request/GetProviderRequests` | Provider | The caller's inbox |
| GET | `/api/Request/GetMyRequests` | Advertiser | Requests the caller has sent |
| POST | `/api/Request/RespondToRequest` | Provider | Accept or decline |
| GET | `/api/Inventory/GetMyInventory` | Provider | Slots with their default price |
| POST | `/api/Inventory/SaveInventory` | Provider | Creates without `id`, updates with one |
| POST | `/api/Inventory/DeleteInventory?id={guid}` | Provider | Archives the slot |
| GET | `/api/Message/GetConversations` | Bearer | Threads the caller is in, with unread counts |
| GET | `/api/Message/GetMessages?conversationId={guid}` | Bearer | Thread history; **also marks it read** |
| POST | `/api/Message/SendMessage` | Bearer | The created message |
| GET | `/api/Admin/GetMetrics` | Admin | Platform counts and committed budget |
| GET | `/api/Admin/GetUsers?search=` | Admin | Users with their roles |
| GET | `/health` | — | Liveness probe (not wrapped) |

"Auth" names the role required via `[Authorize(Roles = "...")]`; "Bearer" means any
signed-in user. Every service takes the caller's id from the token rather than from
the request body, which is what keeps one provider out of another's inventory —
an id you do not own reads as "not found" rather than as a permission error.

A rejected sign-in is still HTTP 200 with `success: false`, per the envelope
convention; the **401s come from the JwtBearer middleware** on the protected
endpoints, not from the controllers.

> Two consequences of the always-200 envelope worth knowing. A failure and a
> not-found are indistinguishable to the caller, so a server error on a detail
> endpoint surfaces in the UI as "not found". And a malformed `id` fails ASP.NET
> model binding *before* the action runs, so it returns a 400 ProblemDetails
> rather than the envelope — the frontend guards the GUID shape for that reason.

## Things the schema forces on the API

Four places where the database shape drives an endpoint's behaviour rather than
the other way round. Worth reading before adding to these services.

- **A request cannot stand alone.** `CampaignProviderRequest` requires a
  `CampaignRequirement`, whose `ChannelId` is NOT NULL. `CreateRequest` therefore
  creates a requirement per request — the provider's primary channel supplies the
  channel, and `BudgetMin`/`BudgetMax` carry the proposed budget, which is why the
  inbox can show a per-provider figure. It also opens the `Conversation` and its
  two `ConversationParticipant` rows, so one button does the whole job.
- **Counters are the database's job.** `Campaign.ResponsesCount`,
  `CampaignRequirement.RequestsCount`, `Conversation.MessageCount` and
  `LastMessageDate` are maintained by the `RefreshRequestCounters`,
  `RefreshRequirementCount` and `BumpConversationActivity` triggers. The services
  never set them; doing so would fight the trigger.
- **`Message.MessageSeq` is an identity-always column** and `UQ_Message_Seq` is
  global, not per-conversation. It is never assigned in C#, and
  `ConversationParticipant.LastReadSeq` is a watermark over it — which is what
  makes "unread" a single `COUNT` rather than a join.
- **NoTracking is the default query behaviour**, set in `Program.cs`. Anything that
  updates an existing row needs `.AsTracking()`, and so does any entity attached to
  a new one's navigation: an untracked `Location` added to `Campaign.Locations` is
  taken for a new row and trips `Location_pkey`.

Inventory is archived rather than deleted — `BookingItem`, `ProposalItem` and
`CampaignProviderRequest` all point at `ProviderInventory`.

Still not implemented: proposals, bookings, invoices and payments. `Booking`,
`Proposal`, `Invoice` and `Payment` have no endpoints, which is why the admin
"committed budget" tile reports budget rather than revenue.

## Authentication

Email and password only, modelled on PMS's `SignInController` but without ASP.NET
Identity — AdsConnect already has `AppUser`, `Role` and `UserRole` tables, so
`AuthService` uses them directly and hashes with BCrypt into
`AppUser.PasswordHash`, the column whose own comment says "Argon2id/bcrypt digest
only".

The token carries `sub` (user id), `email`, `name` and one `role` claim per row in
`UserRole`, so `[Authorize(Roles = "Admin")]` works against it. It is valid for 24
hours, and `ClockSkew` is set to zero rather than the default five minutes.

Three deliberate departures from PMS:

- **Failed sign-ins all say the same thing.** PMS answers "User not found!" and
  "Incorrect password" separately, which turns the endpoint into an email
  enumerator. `AuthService` returns one message for a missing account, a wrong
  password, and an account with no password set.
- **No 2FA, no password reset, no Google sign-in.** These need a mail or SMS
  sender and an OAuth client; the frontend screens for them are disabled rather
  than faked.
- **The 12 seeded demo users cannot sign in.** They have `PasswordHash IS NULL` —
  sign up a fresh account at `/auth/signup` instead. Nothing in this repo carries
  a usable password.

## Known warnings

The build is clean apart from two deliberate warnings:

- **NETSDK1138 — `net7.0` is out of support.** Chosen to match PMS. .NET 7 stopped
  receiving security updates in May 2024.
- **NU1903 — AutoMapper 13.0.1 has a high-severity advisory**
  ([GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x), DoS via
  uncontrolled recursion). This is the same warning PMS shows. The fix is
  AutoMapper ≥ 15.1.1, but 14.0.0 dropped net7.0 and 13.0.1 is the newest release
  targeting net6.0. It cannot be resolved without moving off .NET 7. Practical
  exposure here is low — the only mappings are flat lookup entities, not recursive
  graphs from untrusted input.

## Re-scaffolding the entities

The database is the source of truth and is managed in pgAdmin. We only scaffold
from it and never generate migrations against it — hence the empty `Migrations/`.

```sh
dotnet ef dbcontext scaffold "$ConnectionStrings__PostgresConnectionString" Npgsql.EntityFrameworkCore.PostgreSQL \
  --project AdsConnect.data \
  --startup-project AdsConnect.data \
  --context AdsConnectContext \
  --output-dir Model \
  --namespace AdsConnect.data.Model \
  --context-namespace AdsConnect.data \
  --schema public --no-onconfiguring --force
```

`--no-onconfiguring` is what keeps the connection string out of the generated
context. Do not drop it.

### Two things the scaffold gets wrong

**1. Partial unique indexes are read as one-to-one relationships.**
`UX_ProviderChannel_Primary` and `UX_ProviderLocation_Primary` are partial
(`... WHERE "IsPrimary"`). EF Core cannot see the filter, treats them as plain
unique indexes on `ProviderId`, and generates `Provider.ProviderChannel` and
`Provider.ProviderLocation` as **single references** instead of collections. That
is wrong — a provider may have many channels and many locations, only one of each
being primary — and those navigations will break as soon as a second row exists.

`ProviderService` deliberately queries `ProviderChannels` / `ProviderPricings`
through the DbSets instead of those navigations. Do the same in new code until
the model is corrected.

**2. Expression indexes are not scaffolded.**
Six `lower(...)` unique indexes (`UX_AppUser_Email`, `UX_Industry_Name`,
`UX_ProviderType_Name`, `UX_AdvertisingChannel_Name`, `UX_PricingUnit_Code`,
`UX_Role_RoleName`) are skipped with a warning. The database still enforces them;
EF simply does not know they exist.

`CampaignLocation` and `CampaignRequirementLocation` have no entity class on
purpose — EF correctly modelled them as many-to-many join tables.
