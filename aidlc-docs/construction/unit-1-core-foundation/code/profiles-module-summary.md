# Profiles Module Summary — Unit 1: Core Foundation

**Module**: `LivestreamApp.Profiles`  
**Ngày hoàn thành**: 2026-03-22  
**Stories**: US-02-01, US-02-02

---

## Files Created

### Domain Entities
| File | Mô tả |
|---|---|
| `Domain/Entities/UserProfile.cs` | Profile người dùng (DisplayName, DateOfBirth, Bio, Interests) |
| `Domain/Entities/HostProfile.cs` | Verification badge cho Host (Pending/Approved/Rejected) |
| `Domain/Entities/UserPhoto.cs` | Ảnh người dùng, max 6 ảnh, DisplayIndex 0-5 |

### Repositories (Interfaces)
| File | Mô tả |
|---|---|
| `Repositories/IProfileRepository.cs` | CRUD + IsDisplayNameTaken + GetWithPhotos |
| `Repositories/IHostProfileRepository.cs` | CRUD + GetByUserId |
| `Repositories/IPhotoRepository.cs` | CRUD + GetByUserId + CountByUserId |

### Services
| File | Mô tả |
|---|---|
| `Services/IProfileService.cs` | Interface: Create/Update/Get/InvalidateCache |
| `Services/ProfileService.cs` | Cache-aside pattern (Redis 15min TTL) |
| `Services/IPhotoService.cs` | Interface: Presign/Confirm/Delete/Reorder |
| `Services/PhotoService.cs` | S3 presigned URL upload flow |
| `Services/IHostVerificationService.cs` | Interface: Request/Approve/Reject |
| `Services/HostVerificationService.cs` | Admin approval workflow |
| `Services/CacheKeys.cs` | Centralized cache key: `user:profile:{userId}` |

### Shared (thêm mới)
| File | Mô tả |
|---|---|
| `Shared/Interfaces/IStorageService.cs` | S3 abstraction (GeneratePresignedUrl/ObjectExists/Delete) |
| `Shared/Events/HostVerificationRequestedEvent.cs` | Domain event khi Host submit request |

---

## Unit Tests (18 tests — all passing)

| Test Class | Tests |
|---|---|
| `ProfileServiceTests` | 6 tests: Create (success/duplicate/exists), Update (success/notfound), Get (cache hit/miss) |
| `PhotoServiceTests` | 5 tests: Presign (success/invalid index), Confirm (success/not found), Reorder (success/mismatch) |
| `HostVerificationServiceTests` | 5 tests: Request (new/existing), Approve (success/not found), Reject (success) |

---

## Key Design Decisions

- `UserProfile.Id == User.Id` (1-1 relationship, shared PK)
- `HostProfile.Id == User.Id` (1-1 optional, chỉ tồn tại khi Role=Host)
- Cache invalidation: write-invalidate pattern (DEL sau write, lazy populate)
- Photo upload: 3-phase flow (Presign → Client PUT → Confirm)
- Max 6 photos per user, DisplayIndex 0 là avatar chính
