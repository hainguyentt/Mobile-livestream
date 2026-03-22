# Admin Frontend Summary — app/frontend/admin

**Module**: Frontend Admin Dashboard  
**Phase**: Construction — Unit 1 Core Foundation  
**Ngày hoàn thành**: 2026-03-22

---

## Tech Stack

- Next.js 14 (App Router)
- TypeScript strict mode
- Tailwind CSS
- Zustand (state management)
- Axios (HTTP client)

---

## Cấu trúc thư mục

```
app/frontend/admin/src/
├── app/
│   ├── layout.tsx
│   ├── globals.css
│   ├── login/page.tsx          # Admin login
│   └── dashboard/page.tsx      # Dashboard placeholder
├── store/
│   └── adminStore.ts           # Zustand admin store
└── lib/api/
    ├── client.ts               # Axios instance
    └── auth.ts                 # Auth API calls
```

---

## Pages

| Route | Mô tả |
|---|---|
| `/login` | Admin login (email + password) |
| `/dashboard` | Dashboard placeholder (stats cards) |

---

## State Management

`adminStore` — `isLoading`, `error`, `login()`, `logout()`

---

## Cách chạy

```bash
# Trong app/frontend/admin/
npm install
npm run dev
# Admin UI: http://localhost:3001
```

---

## Ghi chú

Admin dashboard ở Unit 1 là skeleton cơ bản. Các tính năng admin đầy đủ (quản lý user, duyệt host verification, xem reports) sẽ được implement ở các unit tiếp theo.
