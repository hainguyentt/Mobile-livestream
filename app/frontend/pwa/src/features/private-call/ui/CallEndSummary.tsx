'use client';

import { useTranslations } from 'next-intl';

interface Props {
  totalCoinsCharged: number;
  durationSeconds: number;
  onClose: () => void;
}

export default function CallEndSummary({ totalCoinsCharged, durationSeconds, onClose }: Props) {
  const t = useTranslations('privateCall');
  const minutes = Math.floor(durationSeconds / 60);
  const seconds = durationSeconds % 60;

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="call-summary-title"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
    >
      <div className="w-full max-w-sm rounded-2xl bg-gray-900 p-6 text-center text-white">
        <h2 id="call-summary-title" className="text-lg font-bold">{t('callEnded')}</h2>

        <div className="mt-4 space-y-2 text-gray-300">
          <p>{t('duration')}: {String(minutes).padStart(2, '0')}:{String(seconds).padStart(2, '0')}</p>
          <p>{t('coinsCharged')}: {totalCoinsCharged} 💰</p>
        </div>

        <button
          onClick={onClose}
          className="mt-6 w-full rounded-full bg-pink-500 py-3 font-semibold"
        >
          {t('close')}
        </button>
      </div>
    </div>
  );
}
