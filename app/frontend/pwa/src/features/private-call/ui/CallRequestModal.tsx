'use client';

import { useTranslations } from 'next-intl';
import { usePrivateCallStore } from '../model/usePrivateCallStore';
import { privateCallApi } from '../api/privateCallApi';

export default function CallRequestModal() {
  const t = useTranslations('privateCall');
  const { incomingRequest, setIncomingRequest, setActiveSession, setCallStatus } =
    usePrivateCallStore();

  if (!incomingRequest) return null;

  const handleAccept = async () => {
    const result = await privateCallApi.acceptCall(incomingRequest.requestId);
    setActiveSession({
      sessionId: result.sessionId,
      status: 'Active',
      agoraChannelName: result.agoraChannelName,
      startedAt: new Date().toISOString(),
      totalCoinsCharged: 0,
      totalTicks: 0,
    });
    setCallStatus('Accepted');
    setIncomingRequest(null);
  };

  const handleReject = async () => {
    await privateCallApi.rejectCall(incomingRequest.requestId);
    setIncomingRequest(null);
  };

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="call-request-title"
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/60 p-4"
    >
      <div className="w-full max-w-sm rounded-2xl bg-gray-900 p-6 text-white">
        <h2 id="call-request-title" className="text-lg font-bold">
          {t('incomingCall')}
        </h2>
        <p className="mt-2 text-gray-300">
          {t('callFrom', { name: incomingRequest.viewerName })}
        </p>

        <div className="mt-6 flex gap-3">
          <button
            onClick={handleReject}
            className="flex-1 rounded-full bg-gray-700 py-3 font-semibold"
          >
            {t('reject')}
          </button>
          <button
            onClick={handleAccept}
            className="flex-1 rounded-full bg-green-500 py-3 font-semibold"
          >
            {t('accept')}
          </button>
        </div>
      </div>
    </div>
  );
}
