import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';

export interface ViewerJoinedEvent {
  userId: string;
  viewerCount: number;
}

export interface ViewerLeftEvent {
  userId: string;
  viewerCount: number;
}

export interface ViewerCountUpdatedEvent {
  roomId: string;
  viewerCount: number;
}

export interface GiftReceivedEvent {
  senderId: string;
  giftId: string;
  timestamp: string;
}

export interface ViewerBannedEvent {
  viewerId: string;
  reason?: string;
}

export type LivestreamHubEventHandlers = {
  onViewerJoined?: (event: ViewerJoinedEvent) => void;
  onViewerLeft?: (event: ViewerLeftEvent) => void;
  onViewerCountUpdated?: (event: ViewerCountUpdatedEvent) => void;
  onStreamEnded?: (roomId: string) => void;
  onViewerBanned?: (event: ViewerBannedEvent) => void;
  onGiftReceived?: (event: GiftReceivedEvent) => void;
};

let connection: HubConnection | null = null;

export function createLivestreamHubConnection(
  roomId: string,
  handlers: LivestreamHubEventHandlers
): HubConnection {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

  connection = new HubConnectionBuilder()
    .withUrl(`${apiUrl}/hubs/livestream?roomId=${roomId}`, {
      withCredentials: true, // httpOnly cookie JWT
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(
      process.env.NODE_ENV === 'development' ? LogLevel.Information : LogLevel.Warning
    )
    .build();

  if (handlers.onViewerJoined) connection.on('ViewerJoined', handlers.onViewerJoined);
  if (handlers.onViewerLeft) connection.on('ViewerLeft', handlers.onViewerLeft);
  if (handlers.onViewerCountUpdated) connection.on('ViewerCountUpdated', handlers.onViewerCountUpdated);
  if (handlers.onStreamEnded) connection.on('StreamEnded', handlers.onStreamEnded);
  if (handlers.onViewerBanned) connection.on('ViewerBanned', handlers.onViewerBanned);
  if (handlers.onGiftReceived) connection.on('GiftReceived', handlers.onGiftReceived);

  return connection;
}

export async function startLivestreamHub(conn: HubConnection): Promise<void> {
  if (conn.state === HubConnectionState.Disconnected) {
    await conn.start();
  }
}

export async function stopLivestreamHub(conn: HubConnection): Promise<void> {
  if (conn.state !== HubConnectionState.Disconnected) {
    await conn.stop();
  }
}

export async function sendGift(conn: HubConnection, roomId: string, giftId: string): Promise<void> {
  await conn.invoke('SendGift', roomId, giftId);
}
