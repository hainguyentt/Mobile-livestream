'use client';

import React, { useEffect, useRef, useState } from 'react';
import ConversationThread from '@/features/direct-chat/ui/ConversationThread';
import { createChatHubConnection, startChatHub, stopChatHub } from '@/lib/signalr/chatHub';
import { useDirectChatStore } from '@/features/direct-chat/model/useDirectChatStore';
import type { HubConnection } from '@microsoft/signalr';

interface Props {
  params: Promise<{ conversationId: string; locale: string }>;
}

export default function ConversationPage({ params }: Props) {
  const { addMessage } = useDirectChatStore();
  const chatConnRef = useRef<HubConnection | null>(null);
  const [resolvedParams, setResolvedParams] = useState<{ conversationId: string; locale: string } | null>(null);

  // TODO: get currentUserId from auth context
  const currentUserId = 'current-user-id';

  useEffect(() => {
    params.then(setResolvedParams);
  }, [params]);

  useEffect(() => {
    if (!resolvedParams) return;
    const conn = createChatHubConnection({
      onDirectMessageReceived: (msg) => {
        if (msg.conversationId === resolvedParams.conversationId) {
          addMessage(resolvedParams.conversationId, {
            messageId: msg.messageId,
            senderId: msg.senderId,
            content: msg.content,
            messageType: msg.messageType,
            isRead: false,
            sentAt: msg.sentAt,
          });
        }
      },
    });
    chatConnRef.current = conn;
    startChatHub(conn).catch(console.error);

    return () => {
      stopChatHub(conn).catch(() => {});
    };
  }, [resolvedParams?.conversationId]);

  if (!resolvedParams) return null;

  return (
    <main className="flex h-screen flex-col bg-black">
      <ConversationThread
        conversationId={resolvedParams.conversationId}
        currentUserId={currentUserId}
        chatConn={chatConnRef.current}
      />
    </main>
  );
}
