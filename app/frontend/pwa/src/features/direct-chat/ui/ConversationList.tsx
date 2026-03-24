'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useDirectChatStore } from '../model/useDirectChatStore';
import { directChatApi } from '../api/directChatApi';

interface Props {
  locale: string;
}

export default function ConversationList({ locale }: Props) {
  const t = useTranslations('directChat');
  const { conversations, setConversations } = useDirectChatStore();

  useEffect(() => {
    directChatApi.getConversations().then(setConversations);
  }, [setConversations]);

  if (conversations.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-gray-500">
        <p>{t('noConversations')}</p>
      </div>
    );
  }

  return (
    <ul className="divide-y divide-gray-800">
      {conversations.map((conv) => (
        <li key={conv.id}>
          <Link
            href={`/${locale}/messages/${conv.id}`}
            className="flex items-center gap-3 px-4 py-3 hover:bg-gray-900"
          >
            {/* Avatar placeholder */}
            <div className="h-12 w-12 rounded-full bg-gray-700 flex-shrink-0" aria-hidden="true" />

            <div className="flex-1 min-w-0">
              <p className="truncate font-semibold text-white">{conv.otherUserId}</p>
              {conv.lastMessagePreview && (
                <p className="truncate text-sm text-gray-400">{conv.lastMessagePreview}</p>
              )}
            </div>

            {conv.unreadCount > 0 && (
              <span
                className="rounded-full bg-pink-500 px-2 py-0.5 text-xs font-bold text-white"
                aria-label={`${conv.unreadCount} unread messages`}
              >
                {conv.unreadCount}
              </span>
            )}
          </Link>
        </li>
      ))}
    </ul>
  );
}
