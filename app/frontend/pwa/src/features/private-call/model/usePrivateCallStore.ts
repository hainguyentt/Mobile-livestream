import { create } from 'zustand';
import type { CallSession, CallRequestStatus } from './types';

interface PrivateCallState {
  callStatus: CallRequestStatus | 'Idle';
  activeSession: CallSession | null;
  balance: number;
  elapsedSeconds: number;
  incomingRequest: { requestId: string; viewerId: string; viewerName: string } | null;

  setCallStatus: (status: CallRequestStatus | 'Idle') => void;
  setActiveSession: (session: CallSession | null) => void;
  setBalance: (balance: number) => void;
  incrementElapsed: () => void;
  setIncomingRequest: (req: PrivateCallState['incomingRequest']) => void;
  reset: () => void;
}

export const usePrivateCallStore = create<PrivateCallState>((set) => ({
  callStatus: 'Idle',
  activeSession: null,
  balance: 0,
  elapsedSeconds: 0,
  incomingRequest: null,

  setCallStatus: (status) => set({ callStatus: status }),
  setActiveSession: (session) => set({ activeSession: session }),
  setBalance: (balance) => set({ balance }),
  incrementElapsed: () => set((s) => ({ elapsedSeconds: s.elapsedSeconds + 1 })),
  setIncomingRequest: (req) => set({ incomingRequest: req }),
  reset: () =>
    set({ callStatus: 'Idle', activeSession: null, elapsedSeconds: 0, incomingRequest: null }),
}));
