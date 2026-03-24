'use client';

import { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { livestreamApi } from '../api/livestreamApi';

interface Props {
  roomId: string;
  locale: string;
}

export default function HostControls({ roomId, locale }: Props) {
  const t = useTranslations('livestream');
  const router = useRouter();
  const [ending, setEnding] = useState(false);

  const handleEndStream = async () => {
    if (!confirm(t('confirmEndStream'))) return;
    setEnding(true);
    try {
      await livestreamApi.endStream(roomId);
      router.push(`/${locale}/livestream`);
    } finally {
      setEnding(false);
    }
  };

  return (
    <div className="flex items-center gap-3 p-3">
      <button
        onClick={handleEndStream}
        disabled={ending}
        className="rounded-full bg-red-600 px-5 py-2 text-sm font-semibold text-white disabled:opacity-50"
      >
        {ending ? t('ending') : t('endStream')}
      </button>
    </div>
  );
}
