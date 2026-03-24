'use client';

import { useEffect } from 'react';
import { usePrivateCallStore } from '../model/usePrivateCallStore';

interface Props {
  coinRatePerMinute: number;
}

export default function CallTimer({ coinRatePerMinute }: Props) {
  const { elapsedSeconds, incrementElapsed, callStatus } = usePrivateCallStore();

  useEffect(() => {
    if (callStatus !== 'Accepted') return;
    const timer = setInterval(incrementElapsed, 1000);
    return () => clearInterval(timer);
  }, [callStatus, incrementElapsed]);

  const minutes = Math.floor(elapsedSeconds / 60);
  const seconds = elapsedSeconds % 60;
  const formatted = `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

  return (
    <div className="flex flex-col items-center text-white">
      <span className="text-2xl font-mono font-bold" aria-live="polite" aria-label={`Call duration: ${formatted}`}>
        {formatted}
      </span>
      <span className="text-xs text-gray-400">{coinRatePerMinute} coins/min</span>
    </div>
  );
}
