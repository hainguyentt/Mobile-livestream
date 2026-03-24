import { create } from 'zustand';
import type { LivestreamRoom, RoomChatMessage } from './types';

interface LivestreamState {
  activeRoom: LivestreamRoom | null;
  rooms: LivestreamRoom[];
  viewerCount: number;
  chatMessages: RoomChatMessage[];
  isHost: boolean;
  isConnected: boolean;

  setActiveRoom: (room: LivestreamRoom | null) => void;
  setRooms: (rooms: LivestreamRoom[]) => void;
  setViewerCount: (count: number) => void;
  addChatMessage: (message: RoomChatMessage) => void;
  setIsHost: (isHost: boolean) => void;
  setIsConnected: (connected: boolean) => void;
  clearRoom: () => void;
}

export const useLivestreamStore = create<LivestreamState>((set) => ({
  activeRoom: null,
  rooms: [],
  viewerCount: 0,
  chatMessages: [],
  isHost: false,
  isConnected: false,

  setActiveRoom: (room) => set({ activeRoom: room }),
  setRooms: (rooms) => set({ rooms }),
  setViewerCount: (count) => set({ viewerCount: count }),
  addChatMessage: (message) =>
    set((state) => ({
      chatMessages: [...state.chatMessages.slice(-199), message], // Keep last 200
    })),
  setIsHost: (isHost) => set({ isHost }),
  setIsConnected: (connected) => set({ isConnected: connected }),
  clearRoom: () =>
    set({ activeRoom: null, viewerCount: 0, chatMessages: [], isConnected: false }),
}));
