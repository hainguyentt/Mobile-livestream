import type { Conversation, DirectMessage } from '../model/types';

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

export const directChatApi = {
  getConversations: () =>
    apiFetch<Conversation[]>('/api/v1/direct-chat/conversations'),

  getConversation: (id: string) =>
    apiFetch<Conversation>(`/api/v1/direct-chat/conversations/${id}`),

  getMessages: (conversationId: string, from?: Date) => {
    const params = new URLSearchParams();
    if (from) params.set('from', from.toISOString());
    return apiFetch<DirectMessage[]>(
      `/api/v1/direct-chat/conversations/${conversationId}/messages?${params}`
    );
  },

  markAsRead: (conversationId: string) =>
    apiFetch<void>(`/api/v1/direct-chat/conversations/${conversationId}/read`, { method: 'POST' }),

  blockUser: (userId: string) =>
    apiFetch<void>(`/api/v1/direct-chat/block/${userId}`, { method: 'POST' }),

  unblockUser: (userId: string) =>
    apiFetch<void>(`/api/v1/direct-chat/block/${userId}`, { method: 'DELETE' }),
};
