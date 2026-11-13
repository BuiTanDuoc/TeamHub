# CLAUDE.md — TeamHub

Hệ thống quản lý software team (10–20 người, nhiều team riêng biệt): quản lý team, dự án, giao việc/tiến độ, bug tracking, báo cáo định kỳ.

> Giao tiếp, comment nghiệp vụ và tài liệu viết bằng **tiếng Việt**. Tên class, method, biến, commit message dùng **tiếng Anh**.

## 1. Tech stack

| Lớp | Công nghệ |
|---|---|
| Backend | C#, **.NET 10**, ASP.NET Core, EF Core, SQL Server |
| Kiến trúc | Clean Architecture + CQRS (source-generated **Mediator**) |
| Validation | FluentValidation |
| Realtime | SignalR |
| Background job | Hangfire (storage: SQL Server) |
| Auth | ASP.NET Core Identity + JWT (access + refresh token) + Google login |
| Web | React 19 + TypeScript + Vite |
| Mobile | React Native (Expo) + TypeScript |
| Log | Serilog |
| Test | xUnit, FluentAssertions, Testcontainers (SQL Server), Playwright (web) |

## 2. Cấu trúc solution

```
TeamHub/
├── CLAUDE.md
├── Directory.Build.props          # TargetFramework, Nullable, ImplicitUsings, TreatWarningsAsErrors
├── Directory.Packages.props       # Central Package Management
├── global.json                    # Pin .NET SDK 10
├── docker-compose.yml
├── src/
│   ├── Backend/
│   │   ├── TeamHub.Domain/
│   │   ├── TeamHub.Application/
│   │   ├── TeamHub.Infrastructure/
│   │   └── TeamHub.Api/
│   ├── Web/                       # React (Vite)
│   ├── Mobile/                    # React Native (Expo)
│   └── Shared/                    # TS: API client sinh từ OpenAPI, zod schemas, types
└── tests/
    ├── TeamHub.Domain.UnitTests/
    ├── TeamHub.Application.UnitTests/
    └── TeamHub.Api.IntegrationTests/
```

Quy tắc phụ thuộc (chỉ được trỏ vào trong):
`Api → Application → Domain`, `Infrastructure → Application → Domain`.
`Domain` không tham chiếu project nào. `Api` chỉ tham chiếu `Infrastructure` để đăng ký DI.

### 2.1 Domain
```
TeamHub.Domain/
├── Common/            # BaseEntity, IAuditable, ITeamScoped, DomainEvent
├── Teams/             # Team, TeamMember, TeamRole
├── Projects/          # Project, ProjectMember, Sprint (tùy chọn), Milestone
├── WorkItems/         # WorkItem, WorkItemType, WorkItemStatus, TimeLog, WorkItemComment
├── Bugs/              # Bug, BugSeverity, BugStatus, BugStatusHistory, BugWorkflow
├── Notifications/     # Notification, NotificationPreference
├── Reports/           # ReportSubscription, ReportSnapshot
└── Exceptions/        # DomainException, InvalidTransitionException
```

### 2.2 Application (tổ chức theo feature)
```
TeamHub.Application/
├── Common/
│   ├── Abstractions/  # IAppDbContext, ICurrentUser, IFileStorage, IEmailSender, IPushSender
│   ├── Behaviors/     # ValidationBehavior, AuthorizationBehavior, LoggingBehavior
│   ├── Exceptions/    # NotFoundException, ForbiddenException, ValidationException
│   └── Models/        # PagedResult<T>, Result<T>
├── Teams/
│   ├── Commands/CreateTeam/{CreateTeamCommand, CreateTeamHandler, CreateTeamValidator}.cs
│   ├── Queries/GetMyTeams/{GetMyTeamsQuery, GetMyTeamsHandler, TeamDto}.cs
│   └── TeamMappings.cs    # mapping thủ công (extension methods)
├── Projects/
├── WorkItems/
├── Bugs/
├── Reports/
├── Notifications/
└── DependencyInjection.cs
```

### 2.3 Infrastructure
```
TeamHub.Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs           # global query filter theo TeamId
│   ├── Configurations/           # IEntityTypeConfiguration<T> cho từng entity
│   ├── Interceptors/             # AuditInterceptor, DomainEventInterceptor
│   └── Migrations/
├── Identity/                     # AppUser, JwtTokenService, GoogleAuthService, RefreshTokenStore
├── Files/                        # LocalFileStorage / BlobFileStorage
├── Email/                        # SmtpEmailSender, template
├── Push/                         # FcmPushSender
├── Jobs/                         # WeeklyReportJob, DueDateReminderJob, BugSlaJob
└── DependencyInjection.cs
```

### 2.4 Api
```
TeamHub.Api/
├── Endpoints/        # Minimal API, nhóm theo feature (TeamsEndpoints, BugsEndpoints, ...)
├── Hubs/             # NotificationHub, BoardHub
├── Middleware/       # GlobalExceptionHandler (IExceptionHandler → ProblemDetails)
├── Auth/             # CurrentUser, policy, requirement handlers
├── appsettings.json
└── Program.cs
```

## 3. Quy ước kiến trúc (BẮT BUỘC)

1. **Không dùng AutoMapper.** Mapping thủ công qua extension method hoặc projection `Select(...)` thẳng trong query.
2. **Không dùng Repository/UnitOfWork pattern.** Handler inject `IAppDbContext` và dùng EF Core trực tiếp.
3. **CQRS bằng Mediator (source generator)**, không dùng MediatR. Mỗi use case là 1 command/query + 1 handler + (nếu cần) 1 validator.
4. **FluentValidation** cho mọi command. Validate chạy trong `ValidationBehavior`, handler không tự validate lại.
5. **Giá trị cấu hình đặt trong `appsettings.json`** (bind bằng `IOptions<T>`), không hard-code hằng số nghiệp vụ: SLA theo severity, giới hạn dung lượng upload, thời gian sống token, lịch gửi báo cáo, v.v.
6. Query chỉ đọc dùng `AsNoTracking()` + projection sang DTO. Không trả entity ra ngoài Application.
7. Mọi danh sách phải phân trang (ưu tiên keyset pagination cho danh sách lớn).
8. Nullable reference types bật; không `!` bừa bãi.
9. `CancellationToken` truyền xuyên suốt đến EF Core.
10. Thời gian: inject `TimeProvider` (đã đăng ký `TimeProvider.System`), không dùng `DateTime.UtcNow`/`DateTimeOffset.UtcNow` trong Application/Infrastructure.
11. Ghi quyết định kiến trúc mới vào mục **Nhật ký quyết định** cuối file này.

## 4. Multi-team và phân quyền

- Một DB dùng chung. Mọi entity thuộc team implement `ITeamScoped` (có `TeamId`).
- Dùng **named query filters** của EF Core 10: `TeamScope` và `SoftDelete` (xem `QueryFilterNames`). Khi cần bỏ qua một filter, chỉ bỏ đúng filter đó: `.IgnoreQueryFilters([QueryFilterNames.SoftDelete])`.
- `TeamMember` là bảng nguồn để xác định quyền nên **không có** filter `TeamScope` (tránh đệ quy). Truy vấn `TeamMembers` trực tiếp phải lọc theo `TeamId` sau khi `AuthorizationBehavior` đã xác nhận user thuộc team.
- Mã hiển thị của task/bug là `{Project.Key}-{Number}`; `Number` cấp từ `Project.ReserveItemNumber()` (dùng chung bộ đếm cho WorkItem và Bug), `RowVersion` của Project chống cấp trùng. Gặp `DbUpdateConcurrencyException` thì thử lại.
- `AppDbContext` đăng ký **global query filter**: chỉ thấy dữ liệu của các team mà user hiện tại là thành viên. Chỉ dùng `IgnoreQueryFilters()` trong job nền/migration và phải có comment lý do.
- Vai trò gắn theo **(UserId, TeamId)** trong `TeamMember`, không gắn cố định vào user: `TeamLead`, `Developer`, `Tester`, `Viewer`. `SystemAdmin` là cờ riêng ở `AppUser`.
- Kiểm tra quyền bằng `AuthorizationBehavior` trong pipeline Mediator: command/query nào cần quyền thì khai báo qua interface `IRequireTeamRole` (khai báo `TeamId` + vai trò tối thiểu). Không check quyền rải rác trong handler.
- **Lưu ý bảo mật:** không bao giờ tin `TeamId` từ client mà bỏ qua kiểm tra thành viên.

## 5. Xác thực

- Đăng nhập nội bộ (email + mật khẩu) và **Google** (OAuth 2.0/OIDC). Cả hai cùng quy về một `AppUser`; liên kết theo email đã xác minh.
- Web/Mobile nhận JWT access token (ngắn hạn) + refresh token (xoay vòng, lưu hash trong DB, có thể thu hồi).
- Mobile lưu token trong `expo-secure-store`; Google login dùng PKCE.
- Chính sách mật khẩu và thời gian token cấu hình trong `appsettings.json` (mục `Identity`, `Jwt`).
- **Google:** client (web/mobile) lấy Google ID token rồi gọi `POST /api/v1/auth/google`; server xác thực chữ ký/audience bằng `Google.Apis.Auth`. Chỉ liên kết/tạo tài khoản theo email đã được Google xác minh; có thể giới hạn tên miền qua `Auth:Google:AllowedDomains`.
- **Refresh token:** xoay vòng, chỉ lưu hash SHA-256. Dùng lại token đã thu hồi → thu hồi toàn bộ phiên của user.
- **JWT:** `MapInboundClaims = false`, claim user id là `sub` (xem `AppClaims`). `Jwt:SigningKey` KHÔNG nằm trong appsettings.json: dùng user-secrets (dev) hoặc biến môi trường `Jwt__SigningKey` (prod).
- Mọi endpoint yêu cầu đăng nhập theo mặc định (fallback policy); endpoint công khai phải khai báo `.AllowAnonymous()`.
- SignalR nhận token qua query `access_token` chỉ cho đường dẫn `/hubs`; không log query string.
- Tiến trình nền không có user: dùng `.IgnoreQueryFilters([QueryFilterNames.TeamScope])` tường minh kèm comment lý do.
- Tài khoản mới chỉ vào được team khi được mời (invite link/email) hoặc Admin thêm.

## 6. Quy trình nghiệp vụ

### 6.1 Task (WorkItem)
Trạng thái: `Todo → InProgress → InReview → Testing → Done` (cho phép lùi lại khi bị trả).
Mỗi lần đổi trạng thái/assignee/estimate ghi vào lịch sử. Dự án mặc định dùng **Kanban**; bật **Sprint** theo từng project bằng cờ `UseSprints` (Sprint chỉ là nhóm task theo khoảng thời gian, không thay đổi workflow).

### 6.2 Bug
State machine nằm ở `Domain/Bugs/BugWorkflow.cs`, là nơi **duy nhất** định nghĩa chuyển trạng thái hợp lệ:

```
New → Confirmed → Assigned → InProgress → Fixed → ReadyToRetest → Verified → Closed
                                              ↑                       │
                                              └──── Reopened ◄────────┘
Nhánh phụ: WontFix, Duplicate, CannotReproduce (kèm lý do bắt buộc)
```

- Chuyển trạng thái sai → ném `InvalidTransitionException` (API trả 409).
- Mỗi chuyển trạng thái ghi `BugStatusHistory` (ai, lúc nào, ghi chú).
- Chỉ `Tester` được `Verified/Closed`; chỉ `Developer` được đặt `Fixed`. Đặt `Reopened` bắt buộc có lý do.
- SLA theo severity đọc từ `appsettings.json` (`Bugs:Sla:Critical`, ...); `BugSlaJob` cảnh báo khi quá hạn.

### 6.3 Báo cáo định kỳ
- `ReportSnapshot` chốt số liệu theo kỳ để báo cáo cũ không bị đổi khi dữ liệu thay đổi.
- Job Hangfire sinh báo cáo tuần/tháng theo team/dự án và gửi email (PDF/Excel). Lịch (cron) cấu hình trong `appsettings.json`.

## 7. Dữ liệu (EF Core / SQL Server)

- Mỗi entity có `IEntityTypeConfiguration<T>` riêng, không dùng data annotation cho cấu hình DB.
- Khoá chính `Guid` (dùng `Guid.CreateVersion7()` để index thân thiện hơn) cho entity chính; enum lưu dạng string hoặc smallint (thống nhất toàn dự án).
- Có `RowVersion` (`rowversion`) cho `WorkItem`, `Bug` để chống ghi đè đồng thời; xung đột → API trả 409.
- Thời gian lưu UTC (`DateTimeOffset`); chuyển múi giờ ở client.
- Index tối thiểu: `(TeamId, ProjectId, Status)`, `(AssigneeId, Status)`, `(DueDate)`.
- Migration: `dotnet ef migrations add <Name> -p src/Backend/TeamHub.Infrastructure -s src/Backend/TeamHub.Api`. Không sửa migration đã áp dụng ở môi trường chung.
- Soft delete bằng `IsDeleted` + query filter cho entity cần khôi phục (Project, WorkItem, Bug).

## 8. API

- Minimal API theo nhóm feature, version `/api/v1/...`.
- Lỗi trả về `ProblemDetails` (RFC 9457); lỗi validate trả 400 kèm danh sách lỗi theo field.
- OpenAPI sinh tự động; `src/Shared` sinh TypeScript client từ OpenAPI (`pnpm gen:api`). **Không sửa tay file generated.**
- Realtime: SignalR group theo `team-{teamId}` và `project-{projectId}`; client chỉ join group của team mình là thành viên.

## 9. Frontend (Web)

- Cấu trúc theo feature: `src/features/{teams,projects,tasks,bugs,reports,notifications}` và `src/shared/{api,components,hooks,auth}`.
- Server state: TanStack Query. UI state nhẹ: Zustand. Form: React Hook Form + Zod.
- Không gọi `fetch`/`axios` trực tiếp trong component; dùng API client từ `Shared`.
- Component dùng TypeScript strict, không dùng `any`.
- Kéo thả board: `dnd-kit`. Biểu đồ: Recharts/ECharts.

## 10. Mobile (React Native)

- Phạm vi: xem/cập nhật việc của tôi, đổi trạng thái task/bug, comment, chụp ảnh báo bug, nhận push, dashboard rút gọn. Quản trị và báo cáo chi tiết để web đảm nhiệm.
- Dùng chung `Shared` (API client, Zod schema) với web.
- Push qua FCM (Expo Notifications). Cache offline bằng TanStack Query persist.

## 11. Test

- Domain/Application: unit test (xUnit + FluentAssertions); bắt buộc test `BugWorkflow` và các validator.
- Integration test dùng Testcontainers (SQL Server thật), không dùng InMemory provider.
- Đặt tên test: `Method_Scenario_ExpectedResult`.
- Web E2E: Playwright cho luồng chính (đăng nhập, tạo task, báo bug → fix → verify).

## 12. Lệnh thường dùng

```bash
# Backend
dotnet build
dotnet test
dotnet run --project src/Backend/TeamHub.Api
dotnet ef database update -p src/Backend/TeamHub.Infrastructure -s src/Backend/TeamHub.Api

# Khởi tạo secrets cho Api (dev)
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)" -p src/Backend/TeamHub.Api
dotnet user-secrets set "Seed:Admin:Email" "admin@example.com" -p src/Backend/TeamHub.Api
dotnet user-secrets set "Seed:Admin:Password" "<mật khẩu mạnh>" -p src/Backend/TeamHub.Api

# Môi trường dev (SQL Server, ...)
docker compose up -d

# Web / Mobile / Shared
pnpm install
pnpm --filter web dev
pnpm --filter mobile start
pnpm gen:api
```

## 13. Quy tắc cho Claude khi làm việc trong repo này

- Đọc file này trước khi sửa code; làm đúng theo mục 3 (không AutoMapper, không repository).
- Thêm tính năng mới theo cấu trúc feature (Command/Query + Handler + Validator + mapping thủ công), không tạo "service" tùy tiện.
- Mọi thay đổi schema phải đi kèm migration và cập nhật mục 7 nếu có quy ước mới.
- Không hard-code giá trị nghiệp vụ; đưa vào `appsettings.json` và class Options tương ứng.
- Không bỏ qua kiểm tra quyền theo team; mọi query/command liên quan dữ liệu team phải đi qua `AuthorizationBehavior`.
- Comment nghiệp vụ bằng tiếng Việt, ngắn gọn, giải thích *vì sao* thay vì *cái gì*.
- Khi chưa chắc về nghiệp vụ, hỏi lại thay vì tự suy đoán.

## 14. Nhật ký quyết định

| Ngày | Quyết định | Lý do |
|---|---|---|
| 2026-11-12 | Tự xây thay vì dùng Jira/Azure DevOps | Quy trình riêng, kiểm soát dữ liệu |
| 2026-11-12 | Một DB dùng chung, tách theo `TeamId` + query filter | Quy mô 10–20 người, vận hành đơn giản |
| 2026-11-12 | Đăng nhập nội bộ + Google | Tiện cho cả tài khoản nội bộ lẫn Google |
| 2026-11-12 | `AppUser` đặt trong Domain (package `Microsoft.Extensions.Identity.Stores`) | Application join được tên/avatar user mà không phụ thuộc Infrastructure |
| 2026-11-12 | Named query filters (`TeamScope`, `SoftDelete`) | Bỏ qua từng filter riêng lẻ, không phải tắt hết |
| 2026-11-12 | Google login qua ID token (client lấy token, server xác thực) | Dùng chung cho web và mobile, không cần cookie/redirect phía server |
| 2026-11-12 | Fallback policy yêu cầu đăng nhập cho mọi endpoint | An toàn mặc định, quên khai báo cũng không bị lộ |
| 2026-11-12 | Kanban mặc định, Sprint bật tùy project | Phù hợp team nhỏ, linh hoạt, tránh nặng quy trình |
| 2026-11-12 | OpenAPI built-in (`AddOpenApi`) + Swagger UI tại `/swagger` (chỉ Development), khai báo JWT Bearer qua transformer | Thử API có token ngay trên Swagger; `Shared` sinh client từ `/openapi/v1.json` |
| 2026-11-12 | Pipeline Mediator: `AuthorizationBehavior` → `ValidationBehavior`; handler đăng ký Scoped | Kiểm tra quyền trước khi lộ lỗi validate; handler cần `IAppDbContext` (scoped) |
| 2026-11-12 | Chỉ SystemAdmin tạo team (kèm TeamLead đầu tiên) và ngừng/kích hoạt team; TeamLead quản lý thành viên | Khớp quy tắc "chỉ vào team khi được mời/Admin thêm"; team luôn có ít nhất một lead |
| 2026-11-12 | Thêm thành viên theo email của user đã có tài khoản (chưa làm invite link/email) | Giữ MVP gọn; invite sẽ bổ sung cùng module Thông báo |
| 2026-11-12 | `PagedResult<T>` + offset pagination cho danh sách nhỏ (team, thành viên); `Paging:MaxPageSize` trong appsettings | Keyset chỉ cần cho danh sách lớn (task, bug) |
| 2026-11-12 | Route dự án lồng dưới team (`/api/v1/teams/{teamId}/projects`); TeamLead tạo/sửa/xóa mềm/đổi trạng thái, mọi thành viên team xem | `AuthorizationBehavior` kiểm tra quyền theo `TeamId` thống nhất; dự án hiển thị cho cả team |
| 2026-11-12 | `ProjectMember` = tập người được giao task/bug của dự án, phải là thành viên team; Key dự án 2-10 ký tự, không đổi sau khi tạo | Key nằm trong mã task/bug (`TH-123`) nên đổi sẽ làm sai mã đã dùng |
| 2026-11-12 | Task dưới `/api/v1/teams/{teamId}/projects/{projectId}/tasks`; tạo/sửa/đổi trạng thái/giao việc cho mọi vai trò từ Tester trở lên, xóa mềm chỉ TeamLead | Viewer chỉ đọc; xóa là thao tác khó hoàn tác |
| 2026-11-12 | Cấp `Number` bằng `Project.ReserveItemNumber()` trong vòng thử lại tối đa 5 lần khi `DbUpdateConcurrencyException` (gọi `IAppDbContext.ClearChangeTracker()` trước mỗi lần) | Nhiều người tạo task cùng lúc không được trùng mã |
| 2026-11-12 | Thêm bảng `WorkItemHistories` (Status/Assignee/Estimate, lưu giá trị hiển thị, không lưu Id) | Thực hiện mục 6.1: mỗi lần đổi trạng thái/assignee/estimate ghi lịch sử |
| 2026-11-12 | Danh sách task dùng keyset pagination theo `Number` (cursor); người được giao phải là thành viên dự án | `Number` duy nhất trong dự án nên cursor ổn định, không trùng/sót khi có task mới |
| 2026-11-12 | Kanban board: một cột mỗi trạng thái, tối đa `Kanban:MaxCardsPerColumn` thẻ/cột kèm `totalCount`; không hiển thị Epic; chưa có thứ tự kéo thả trong cột | Tránh trả cột Done quá dài; sắp xếp trong cột theo ưu tiên rồi số thứ tự (Done theo ngày hoàn thành) |
| 2026-11-12 | Bình luận (`WorkItemComment`) dưới `.../tasks/{taskId}/comments`: Tester trở lên được viết; chỉ tác giả sửa; tác giả/TeamLead/SystemAdmin xóa mềm; liệt kê cũ nhất trước | Viewer chỉ đọc; chỉ người viết mới được đổi lời mình, người quản lý vẫn gỡ được nội dung không phù hợp |
| 2026-11-12 | Thông báo trong app (`Notification`) gửi theo người nhận, không áp filter team: mọi truy vấn lọc `RecipientId`; thông báo của người khác trả NotFound | Người rời team vẫn đọc được thông báo cũ; không lộ thông báo của người khác |
| 2026-11-12 | Ba sự kiện sinh thông báo: được giao task, có bình luận mới, đổi trạng thái (gửi cho người được giao + người tạo task, bỏ qua người gây ra) | Phạm vi tối thiểu của MVP (1); thêm @mention và bug ở giai đoạn sau |
| 2026-11-12 | `INotificationDispatcher`: Stage (cùng giao dịch) → SaveChanges → Publish; đẩy realtime qua `INotificationPusher` (SignalR, sự kiện `notification` tới group `user-{id}`), lỗi đẩy chỉ ghi log | Thông báo không bị mất khi SignalR lỗi và không đẩy tin của giao dịch đã rollback |
| 2026-11-12 | Danh sách thông báo dùng keyset pagination với cursor mờ `(CreatedAt, Id)` mã hóa base64url (`NextCursor` → `?cursor=`); sắp CreatedAt rồi Id giảm dần | Thông báo mới đẩy realtime lên đầu không làm trang sau bị lặp/sót như offset |
| 2026-11-12 | Job dọn thông báo (`NotificationCleanupJob`, Hangfire, cron UTC `0 20 * * *`): xóa thông báo đã đọc > 30 ngày và mọi thông báo > 180 ngày, theo lô 1000; logic ở `PurgeOldNotificationsCommand`; `Hangfire:Enabled=false` để tắt | Bảng chỉ tăng; ngưỡng đặt trong `Notifications:Cleanup`; job chỉ gọi command nên test được mà không cần Hangfire |
| 2026-11-12 | OpenAPI built-in (`AddOpenApi`) + Swagger UI tại `/swagger` (chỉ Development), khai báo JWT Bearer qua transformer | Thử API có token ngay trên Swagger; `Shared` sinh client từ `/openapi/v1.json` |

## 15. Lộ trình gợi ý
|----------------+---------------------------------------------------------------------------------------------------+----------------------|
| Giai đoạn      | Nội dung                                                                                          | Thời gian ước tính   |
|----------------|---------------------------------------------------------------------------------------------------|----------------------|
| MVP (1)        | Auth, Team, Member/Role, Project, Task + Kanban, Comment, Thông báo trong app                     | 6–8 tuần             |
| MVP (2)        | Bug tracking + workflow, Attachment, Dashboard cá nhân, Mobile bản đầu (xem/cập nhật việc, push)  | 4–6 tuần             |
| Giai đoạn 3    | Báo cáo định kỳ (Hangfire + email/PDF), Time log, Sprint/burndown, SLA bug                        | 4 tuần               |
| Giai đoạn 4    | Tích hợp Git/CI, test case, import Jira, tối ưu                                                   | Tùy nhu cầu          |
|----------------+---------------------------------------------------------------------------------------------------+----------------------|