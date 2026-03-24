'use client';

import { useEffect } from 'react';
import { useTranslations } from 'next-intl';
import Link from 'next/link';
import { livestreamApi } from '../api/livestreamApi';
import { useLivestreamStore } from '../model/useLivestreamStore';
import type { RoomCategory } from '../model/types';
import ViewerCountBadge from './ViewerCountBadge';

interface Props {
  category?: RoomCategory;
}

export default function LivestreamGrid({ category }: Props) {
  const t = useTranslations('livestream');
  const { rooms, setRooms } = useLivestreamStore();

  useEffect(() => {
    livestreamApi.getRooms(category).then(({ items }) => setRooms(items));
  }, [category, setRooms]);

  if (rooms.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-gray-500">
        <p>{t('noActiveRooms')}</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 gap-3 p-4 sm:grid-cols-3">
      {rooms
        .sort((a, b) => b.viewerCount - a.viewerCount) // Sort by viewer count desc
        .map((room) => (
          <Link
            key={room.id}
            href={`/livestream/${room.id}`}
            className="relative overflow-hidden rounded-xl bg-gray-900 aspect-[3/4]"
          >
            {/* Room thumbnail placeholder */}
            <div className="absolute inset-0 bg-gradient-to-b from-transparent to-black/70" />

            {/* Live badge */}
            <div className="absolute top-2 left-2 rounded-full bg-red-500 px-2 py-0.5 text-xs font-bold text-white">
              LIVE
            </div>

            {/* Viewer count */}
            <div className="absolute top-2 right-2">
              <ViewerCountBadge count={room.viewerCount} />
            </div>

            {/* Room info */}
            <div className="absolute bottom-0 left-0 right-0 p-3">
              <p className="truncate text-sm font-semibold text-white">{room.title}</p>
              <p className="text-xs text-gray-300">{room.category}</p>
            </div>
          </Link>
        ))}
    </div>
  );
}
