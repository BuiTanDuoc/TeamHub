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
`src/features/{auth,account,admin,dashboard,teams,projects,tasks,bugs,notifications}`, `src/shared/{api,auth,components,hooks,realtime,stores,utils}`, `src/layout`, `src/i18n/locales/{vi,en}.json`.

## Push trình duyệt (Firebase Cloud Messaging)
1. Firebase Console → tạo project → thêm **Web app** → chép `apiKey`, `authDomain`, `projectId`, `messagingSenderId`, `appId` vào `.env` (`VITE_FIREBASE_*`).
2. Project settings → **Cloud Messaging** → Web configuration → *Generate key pair* → chép public key vào `VITE_FIREBASE_VAPID_KEY`.
3. Backend: Project settings → **Service accounts** → *Generate new private key*, rồi bật FCM:
   ```bash
   dotnet user-secrets set "Push:Fcm:Enabled" "true" -p src/Backend/TeamHub.Api
   dotnet user-secrets set "Push:Fcm:CredentialFile" "/đường/dẫn/service-account.json" -p src/Backend/TeamHub.Api
   ```
   Production: biến môi trường `Push__Fcm__Enabled=true` và `Push__Fcm__CredentialJson=<nội dung JSON>` (hoặc `GOOGLE_APPLICATION_CREDENTIALS`). Không commit file service account.
4. Chạy migration (`FcmDeviceToken`), mở **Cài đặt thông báo** → bật *Thông báo đẩy trên trình duyệt này*.

Push chỉ hoạt động trên HTTPS hoặc `localhost`. Service worker: `public/firebase-messaging-sw.js` (phải phục vụ ở gốc site).
