'use client';

import { useEffect, useRef, useState } from 'react';
import { useDirectChatStore } from '../model/useDirectChatStore';
import { directChatApi } from '../api/directChatApi';
import MessageInput from './MessageInput';
import type { HubConnection } from '@microsoft/signalr';

interface Props {
  conversationId: string;
  currentUserId: string;
  chatConn: HubConnection | null;
}

export default function ConversationThread({ conversationId, currentUserId, chatConn }: Props) {
  const { messages, setMessages, markRead } = useDirectChatStore();
  const [loadingOlder, setLoadingOlder] = useState(false);
  const [oldestDate, setOldestDate] = useState<Date>(() => new Date(Date.now() - 30 * 86400_000));
  const bottomRef = useRef<HTMLDivElement>(null);
  const threadMessages = messages[conversationId] ?? [];

  useEffect(() => {
    directChatApi.getMessages(conversationId, oldestDate).then((msgs) => {
      setMessages(conversationId, msgs);
      markRead(conversationId);
      directChatApi.markAsRead(conversationId);
    });
  }, [conversationId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [threadMessages.length]);

  const loadOlderMessages = async () => {
    setLoadingOlder(true);
    const newFrom = new Date(oldestDate.getTime() - 30 * 86400_000);
    const older = await directChatApi.getMessages(conversationId, newFrom);
    setMessages(conversationId, [...older, ...threadMessages]);
    setOldestDate(newFrom);
    setLoadingOlder(false);
  };

  return (
    <div className="flex h-full flex-col bg-black text-white">
      {/* Load older messages */}
      <div className="flex justify-center p-2">
        <button
          onClick={loadOlderMessages}
          disabled={loadingOlder}
          className="text-xs text-gray-500 hover:text-gray-300 disabled:opacity-50"
        >
          {loadingOlder ? 'Loading...' : 'Load older messages'}
        </button>
      </div>

      {/* Message list */}
      <div className="flex-1 overflow-y-auto px-4 py-2 space-y-3">
        {threadMessages.map((msg) => {
          const isMine = msg.senderId === currentUserId;
          return (
            <div
              key={msg.messageId}
              className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}
            >
              <div
                className={`max-w-[75%] rounded-2xl px-4 py-2 text-sm ${
                  isMine ? 'bg-pink-500 text-white' : 'bg-gray-800 text-white'
                }`}
              >
                {msg.content}
              </div>
            </div>
          );
        })}
        <div ref={bottomRef} />
      </div>

      {/* Input */}
      <MessageInput conversationId={conversationId} chatConn={chatConn} />
    </div>
  );
}
