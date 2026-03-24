import type { AgoraTokenResponse, CallRequest, CallSession } from '../model/types';

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const privateCallApi = {
  requestCall: (hostId: string) =>
    apiFetch<CallRequest>('/api/v1/livestream/calls/request', {
      method: 'POST',
      body: JSON.stringify({ hostId }),
    }),

  acceptCall: (requestId: string) =>
    apiFetch<{ sessionId: string; agoraChannelName: string; hostToken: AgoraTokenResponse }>(
      `/api/v1/livestream/calls/${requestId}/accept`,
      { method: 'POST' }
    ),

  rejectCall: (requestId: string, reason?: string) =>
    apiFetch<void>(`/api/v1/livestream/calls/${requestId}/reject`, {
      method: 'POST',
      body: JSON.stringify({ reason }),
    }),

  endCall: (sessionId: string) =>
    apiFetch<void>(`/api/v1/livestream/calls/${sessionId}/end`, { method: 'POST' }),

  getToken: (sessionId: string) =>
    apiFetch<AgoraTokenResponse>(`/api/v1/livestream/calls/${sessionId}/token`),

  getStatus: (sessionId: string) =>
    apiFetch<CallSession>(`/api/v1/livestream/calls/${sessionId}/status`),
};
