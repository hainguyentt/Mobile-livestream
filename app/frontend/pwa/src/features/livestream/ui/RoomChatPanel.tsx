'use client';

import { useRef, useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { useLivestreamStore } from '../model/useLivestreamStore';
import type { HubConnection } from '@microsoft/signalr';
import { sendRoomMessage } from '@/lib/signalr/chatHub';

interface Props {
  roomId: string;
  chatConn: HubConnection | null;
}

export default function RoomChatPanel({ roomId, chatConn }: Props) {
  const t = useTranslations('livestream');
  const { chatMessages } = useLivestreamStore();
  const [input, setInput] = useState('');
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [chatMessages]);

  const handleSend = async () => {
    if (!input.trim() || !chatConn) return;
    try {
      await sendRoomMessage(chatConn, roomId, input.trim());
      setInput('');
    } catch {
      // Rate limit or connection error — silently ignore
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="flex h-full flex-col bg-black/40">
      {/* Message list */}
      <div className="flex-1 overflow-y-auto p-3 space-y-1">
        {chatMessages.map((msg) => (
          <div key={msg.messageId} className="text-sm">
            {msg.type === 'gift' ? (
              <span className="text-yellow-400">
                🎁 <strong>{msg.senderDisplayName}</strong> {msg.content}
              </span>
            ) : (
              <span className="text-white">
                <strong className="text-pink-400">{msg.senderDisplayName}</strong>{' '}
                {msg.content}
              </span>
            )}
          </div>
        ))}
        <div ref={bottomRef} />
      </div>

      {/* Input */}
      <div className="flex gap-2 border-t border-white/10 p-3">
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          maxLength={200}
          placeholder={t('chatPlaceholder')}
          className="flex-1 rounded-full bg-white/10 px-4 py-2 text-sm text-white placeholder-gray-400 outline-none"
          aria-label={t('chatPlaceholder')}
        />
        <button
          onClick={handleSend}
          disabled={!input.trim()}
          className="rounded-full bg-pink-500 px-4 py-2 text-sm font-semibold text-white disabled:opacity-50"
          aria-label={t('send')}
        >
          {t('send')}
        </button>
      </div>
    </div>
  );
}
