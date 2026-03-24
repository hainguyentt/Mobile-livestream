export type RoomCategory = 'Talk' | 'Music' | 'Game' | 'Cooking' | 'Study' | 'Other';
export type RoomStatus = 'Scheduled' | 'Live' | 'Ended';

export interface LivestreamRoom {
  id: string;
  hostId: string;
  title: string;
  category: RoomCategory;
  status: RoomStatus;
  agoraChannelName: string;
  viewerCount: number;
  peakViewerCount: number;
  startedAt?: string;
  createdAt: string;
}

export interface RoomChatMessage {
  messageId: string;
  roomId: string;
  senderId: string;
  senderDisplayName: string;
  content: string;
  type: 'message' | 'gift' | 'system';
  giftId?: string;
  sentAt: string;
}

export interface JoinRoomResponse {
  roomId: string;
  sessionId: string;
  agoraChannelName: string;
}
