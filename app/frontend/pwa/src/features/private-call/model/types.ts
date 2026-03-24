export type CallRequestStatus = 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled' | 'TimedOut';
export type CallSessionStatus = 'Active' | 'Ended';

export interface CallRequest {
  requestId: string;
  status: CallRequestStatus;
  expiresAt: string;
}

export interface CallSession {
  sessionId: string;
  status: CallSessionStatus;
  agoraChannelName: string;
  startedAt: string;
  endedAt?: string;
  totalCoinsCharged: number;
  totalTicks: number;
}

export interface AgoraTokenResponse {
  token: string;
  channelName: string;
  expiresAt: string;
}
