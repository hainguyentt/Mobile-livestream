import type { JoinRoomResponse, LivestreamRoom, RoomCategory } from '../model/types';

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
}

export const livestreamApi = {
  getRooms: (category?: RoomCategory, page = 1, pageSize = 20) =>
    apiFetch<{ items: LivestreamRoom[]; total: number }>(
      `/api/v1/livestream/rooms?${new URLSearchParams({
        ...(category ? { category } : {}),
        page: String(page),
        pageSize: String(pageSize),
      })}`
    ),

  getRoom: (roomId: string) =>
    apiFetch<LivestreamRoom>(`/api/v1/livestream/rooms/${roomId}`),

  createRoom: (title: string, category: RoomCategory) =>
    apiFetch<LivestreamRoom>('/api/v1/livestream/rooms', {
      method: 'POST',
      body: JSON.stringify({ title, category }),
    }),

  startStream: (roomId: string) =>
    apiFetch<void>(`/api/v1/livestream/rooms/${roomId}/start`, { method: 'POST' }),

  endStream: (roomId: string) =>
    apiFetch<void>(`/api/v1/livestream/rooms/${roomId}/end`, { method: 'POST' }),

  joinRoom: (roomId: string) =>
    apiFetch<JoinRoomResponse>(`/api/v1/livestream/rooms/${roomId}/join`, { method: 'POST' }),

  leaveRoom: (roomId: string) =>
    apiFetch<void>(`/api/v1/livestream/rooms/${roomId}/leave`, { method: 'POST' }),

  banViewer: (roomId: string, viewerId: string, reason?: string) =>
    apiFetch<void>(`/api/v1/livestream/rooms/${roomId}/ban/${viewerId}`, {
      method: 'POST',
      body: JSON.stringify({ reason }),
    }),
};
