'use client';

import { useTranslations } from 'next-intl';
import { usePrivateCallStore } from '../model/usePrivateCallStore';
import { privateCallApi } from '../api/privateCallApi';
import CallTimer from './CallTimer';
import BalanceDisplay from './BalanceDisplay';

interface Props {
  coinRatePerMinute: number;
}

export default function CallScreen({ coinRatePerMinute }: Props) {
  const t = useTranslations('privateCall');
  const { activeSession, reset } = usePrivateCallStore();

  if (!activeSession) return null;

  const handleEndCall = async () => {
    await privateCallApi.endCall(activeSession.sessionId);
    reset();
  };

  return (
    <div className="fixed inset-0 z-40 flex flex-col bg-black text-white">
      {/* Video area */}
      <div className="relative flex-1 bg-gray-900">
        <div className="absolute inset-0 flex items-center justify-center text-gray-600">
          {/* Agora video streams render here */}
          <span>Private Call Video</span>
        </div>
      </div>

      {/* Controls bar */}
      <div className="flex items-center justify-between bg-black/80 px-6 py-4">
        <BalanceDisplay />
        <CallTimer coinRatePerMinute={coinRatePerMinute} />
        <button
          onClick={handleEndCall}
          className="rounded-full bg-red-600 px-5 py-2 font-semibold"
          aria-label={t('endCall')}
        >
          {t('endCall')}
        </button>
      </div>
    </div>
  );
}
