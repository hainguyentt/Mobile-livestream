'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import type { HubConnection } from '@microsoft/signalr';
import { sendDirectMessage } from '@/lib/signalr/chatHub';

const EMOJI_REACTIONS = ['❤️', '😂', '😮', '😢', '👏', '🔥'];

interface Props {
  conversationId: string;
  chatConn: HubConnection | null;
}

export default function MessageInput({ conversationId, chatConn }: Props) {
  const t = useTranslations('directChat');
  const [input, setInput] = useState('');
  const [showEmoji, setShowEmoji] = useState(false);

  const handleSend = async () => {
    if (!input.trim() || !chatConn) return;
    await sendDirectMessage(chatConn, conversationId, input.trim());
    setInput('');
  };

  const handleEmoji = async (emoji: string) => {
    if (!chatConn) return;
    await sendDirectMessage(chatConn, conversationId, emoji);
    setShowEmoji(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="border-t border-gray-800 p-3">
      {showEmoji && (
        <div className="mb-2 flex gap-2" role="group" aria-label={t('emojiReactions')}>
          {EMOJI_REACTIONS.map((emoji) => (
            <button
              key={emoji}
              onClick={() => handleEmoji(emoji)}
              className="text-2xl hover:scale-125 transition-transform"
              aria-label={`Send ${emoji}`}
            >
              {emoji}
            </button>
          ))}
        </div>
      )}

      <div className="flex items-center gap-2">
        <button
          onClick={() => setShowEmoji((v) => !v)}
          className="text-xl text-gray-400 hover:text-white"
          aria-label={t('toggleEmoji')}
          aria-expanded={showEmoji}
        >
          😊
        </button>

        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          maxLength={1000}
          placeholder={t('messagePlaceholder')}
          className="flex-1 rounded-full bg-gray-800 px-4 py-2 text-sm text-white placeholder-gray-500 outline-none"
          aria-label={t('messagePlaceholder')}
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
