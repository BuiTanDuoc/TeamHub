# TeamHub Web

React 19 + Vite + TypeScript (strict), CoreUI Free React Admin Template 5.x, TanStack Query, Zustand, React Hook Form + Zod, dnd-kit, SignalR, react-i18next (vi/en), dark/light/auto theme.

## Chạy
```bash
cp .env.example .env     # chỉnh nếu cần
npm install
npm run dev              # http://localhost:5173 (khớp Cors:AllowedOrigins)
```
Dev dùng proxy Vite: `/api` và `/hubs` → `DEV_API_TARGET` (mặc định `https://localhost:31034`, xem launchSettings.json).
Chạy backend trước: `dotnet run --project src/Backend/TeamHub.Api`, và seed Admin qua user-secrets (CLAUDE.md §12).

## Cấu trúc
`src/features/{auth,dashboard,teams,projects,tasks,bugs,notifications}`, `src/shared/{api,auth,components,hooks,realtime,stores,utils}`, `src/layout`, `src/i18n/locales/{vi,en}.json`.
