import { create } from 'zustand';
import type { Conversation, DirectMessage } from './types';

interface DirectChatState {
  conversations: Conversation[];
  messages: Record<string, DirectMessage[]>; // conversationId → messages
  unreadCounts: Record<string, number>;

  setConversations: (conversations: Conversation[]) => void;
  setMessages: (conversationId: string, messages: DirectMessage[]) => void;
  addMessage: (conversationId: string, message: DirectMessage) => void;
  markRead: (conversationId: string) => void;
  updateUnreadCount: (conversationId: string, count: number) => void;
}

export const useDirectChatStore = create<DirectChatState>((set) => ({
  conversations: [],
  messages: {},
  unreadCounts: {},

  setConversations: (conversations) => set({ conversations }),
  setMessages: (conversationId, messages) =>
    set((s) => ({ messages: { ...s.messages, [conversationId]: messages } })),
  addMessage: (conversationId, message) =>
    set((s) => ({
      messages: {
        ...s.messages,
        [conversationId]: [...(s.messages[conversationId] ?? []), message],
      },
    })),
  markRead: (conversationId) =>
    set((s) => ({ unreadCounts: { ...s.unreadCounts, [conversationId]: 0 } })),
  updateUnreadCount: (conversationId, count) =>
    set((s) => ({ unreadCounts: { ...s.unreadCounts, [conversationId]: count } })),
}));
