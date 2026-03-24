'use client';

import { useEffect, useRef } from 'react';
import { useLivestreamStore } from '../model/useLivestreamStore';
import { livestreamApi } from '../api/livestreamApi';
import {
  createLivestreamHubConnection,
  startLivestreamHub,
  stopLivestreamHub,
} from '@/lib/signalr/livestreamHub';
import { createChatHubConnection, startChatHub, stopChatHub } from '@/lib/signalr/chatHub';
import ViewerCountBadge from './ViewerCountBadge';
import RoomChatPanel from './RoomChatPanel';
import GiftPanel from './GiftPanel';
import HostControls from './HostControls';
import type { HubConnection } from '@microsoft/signalr';

interface Props {
  roomId: string;
  locale: string;
  isHost?: boolean;
}

export default function LivestreamRoom({ roomId, locale, isHost = false }: Props) {
  const { activeRoom, viewerCount, setActiveRoom, setViewerCount, addChatMessage, setIsConnected, clearRoom } =
    useLivestreamStore();

  const liveConnRef = useRef<HubConnection | null>(null);
  const chatConnRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    let mounted = true;

    const init = async () => {
      const room = await livestreamApi.getRoom(roomId);
      if (!mounted) return;
      setActiveRoom(room);

      if (!isHost) await livestreamApi.joinRoom(roomId);

      // Livestream hub
      const liveConn = createLivestreamHubConnection(roomId, {
        onViewerCountUpdated: ({ viewerCount: count }) => setViewerCount(count),
        onStreamEnded: () => clearRoom(),
      });
      liveConnRef.current = liveConn;
      await startLivestreamHub(liveConn);

      // Chat hub
      const chatConn = createChatHubConnection({
        onRoomMessageReceived: (msg) =>
          addChatMessage({
            messageId: msg.messageId,
            roomId: msg.roomId,
            senderId: msg.senderId,
            senderDisplayName: msg.senderDisplayName,
            content: msg.content,
            type: 'message',
            sentAt: msg.sentAt,
          }),
      });
      chatConnRef.current = chatConn;
      await startChatHub(chatConn);

      if (mounted) setIsConnected(true);
    };

    init().catch(console.error);

    return () => {
      mounted = false;
      if (liveConnRef.current) stopLivestreamHub(liveConnRef.current);
      if (chatConnRef.current) stopChatHub(chatConnRef.current);
      if (!isHost) livestreamApi.leaveRoom(roomId).catch(() => {});
      clearRoom();
    };
  }, [roomId, isHost]);

  if (!activeRoom) {
    return <div className="flex h-screen items-center justify-center text-white">Loading...</div>;
  }

  return (
    <div className="flex h-screen flex-col bg-black text-white">
      {/* Video area */}
      <div className="relative flex-1 bg-gray-900">
        <div className="absolute inset-0 flex items-center justify-center text-gray-600">
          {/* Agora video player renders here */}
          <span>Video Stream</span>
        </div>

        {/* Viewer count overlay */}
        <div className="absolute top-4 right-4">
          <ViewerCountBadge count={viewerCount} />
        </div>

        {/* Room title */}
        <div className="absolute top-4 left-4">
          <p className="text-sm font-semibold">{activeRoom.title}</p>
        </div>
      </div>

      {/* Gift panel */}
      <GiftPanel roomId={roomId} liveConn={liveConnRef.current} />

      {/* Host controls */}
      {isHost && <HostControls roomId={roomId} locale={locale} />}

      {/* Chat panel */}
      <div className="h-64">
        <RoomChatPanel roomId={roomId} chatConn={chatConnRef.current} />
      </div>
    </div>
  );
}
