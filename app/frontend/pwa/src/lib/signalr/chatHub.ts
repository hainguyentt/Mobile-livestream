import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';

export interface RoomMessageReceivedEvent {
  messageId: string;
  roomId: string;
  senderId: string;
  senderDisplayName: string;
  content: string;
  sentAt: string;
}

export interface DirectMessageReceivedEvent {
  messageId: string;
  conversationId: string;
  senderId: string;
  content: string;
  messageType: 'Text' | 'Emoji';
  sentAt: string;
}

export interface CallRequestEvent {
  requestId: string;
  viewerId: string;
  viewerName: string;
  viewerPhoto?: string;
}

export interface CallAcceptedEvent {
  sessionId: string;
  agoraToken: string;
  channelName: string;
}

export interface CallEndedEvent {
  sessionId: string;
  endedBy: string;
  totalCoinsCharged: number;
  durationSeconds: number;
}

export interface BalanceUpdatedEvent {
  balance: number;
}

export interface LowBalanceWarningEvent {
  balance: number;
  threshold: number;
}

export type ChatHubEventHandlers = {
  onRoomMessageReceived?: (event: RoomMessageReceivedEvent) => void;
  onDirectMessageReceived?: (event: DirectMessageReceivedEvent) => void;
  onCallRequest?: (event: CallRequestEvent) => void;
  onCallAccepted?: (event: CallAcceptedEvent) => void;
  onCallRejected?: (requestId: string) => void;
  onCallEnded?: (event: CallEndedEvent) => void;
  onBalanceUpdated?: (event: BalanceUpdatedEvent) => void;
  onLowBalanceWarning?: (event: LowBalanceWarningEvent) => void;
};

export function createChatHubConnection(handlers: ChatHubEventHandlers): HubConnection {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

  const conn = new HubConnectionBuilder()
    .withUrl(`${apiUrl}/hubs/chat`, { withCredentials: true })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(
      process.env.NODE_ENV === 'development' ? LogLevel.Information : LogLevel.Warning
    )
    .build();

  if (handlers.onRoomMessageReceived) conn.on('RoomMessageReceived', handlers.onRoomMessageReceived);
  if (handlers.onDirectMessageReceived) conn.on('DirectMessageReceived', handlers.onDirectMessageReceived);
  if (handlers.onCallRequest) conn.on('CallRequest', handlers.onCallRequest);
  if (handlers.onCallAccepted) conn.on('CallAccepted', handlers.onCallAccepted);
  if (handlers.onCallRejected) conn.on('CallRejected', handlers.onCallRejected);
  if (handlers.onCallEnded) conn.on('CallEnded', handlers.onCallEnded);
  if (handlers.onBalanceUpdated) conn.on('BalanceUpdated', handlers.onBalanceUpdated);
  if (handlers.onLowBalanceWarning) conn.on('LowBalanceWarning', handlers.onLowBalanceWarning);

  return conn;
}

export async function startChatHub(conn: HubConnection): Promise<void> {
  if (conn.state === HubConnectionState.Disconnected) {
    await conn.start();
  }
}

export async function stopChatHub(conn: HubConnection): Promise<void> {
  if (conn.state !== HubConnectionState.Disconnected) {
    await conn.stop();
  }
}

export async function sendRoomMessage(conn: HubConnection, roomId: string, text: string): Promise<void> {
  await conn.invoke('SendRoomMessage', roomId, text);
}

export async function sendDirectMessage(conn: HubConnection, conversationId: string, text: string): Promise<void> {
  await conn.invoke('SendDirectMessage', conversationId, text);
}

export async function markConversationRead(conn: HubConnection, conversationId: string): Promise<void> {
  await conn.invoke('MarkConversationRead', conversationId);
}
