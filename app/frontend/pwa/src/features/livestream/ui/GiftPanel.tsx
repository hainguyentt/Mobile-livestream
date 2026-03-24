'use client';

import { useTranslations } from 'next-intl';
import type { HubConnection } from '@microsoft/signalr';
import { sendGift } from '@/lib/signalr/livestreamHub';

const GIFTS = [
  { id: 'heart', emoji: '❤️', label: 'Heart' },
  { id: 'star', emoji: '⭐', label: 'Star' },
  { id: 'fire', emoji: '🔥', label: 'Fire' },
  { id: 'crown', emoji: '👑', label: 'Crown' },
  { id: 'diamond', emoji: '💎', label: 'Diamond' },
];

interface Props {
  roomId: string;
  liveConn: HubConnection | null;
}

export default function GiftPanel({ roomId, liveConn }: Props) {
  const t = useTranslations('livestream');

  const handleGift = async (giftId: string) => {
    if (!liveConn) return;
    await sendGift(liveConn, roomId, giftId);
  };

  return (
    <div className="flex gap-3 p-3">
      {GIFTS.map((gift) => (
        <button
          key={gift.id}
          onClick={() => handleGift(gift.id)}
          className="flex flex-col items-center gap-1 rounded-xl bg-white/10 p-3 text-2xl transition hover:bg-white/20 active:scale-95"
          aria-label={`${t('sendGift')}: ${gift.label}`}
        >
          {gift.emoji}
          <span className="text-xs text-gray-300">{gift.label}</span>
        </button>
      ))}
    </div>
  );
}
