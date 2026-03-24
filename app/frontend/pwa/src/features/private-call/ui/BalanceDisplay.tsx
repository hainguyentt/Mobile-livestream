'use client';

import { useTranslations } from 'next-intl';
import { usePrivateCallStore } from '../model/usePrivateCallStore';

const LOW_BALANCE_THRESHOLD = 100;

export default function BalanceDisplay() {
  const t = useTranslations('privateCall');
  const { balance } = usePrivateCallStore();
  const isLow = balance < LOW_BALANCE_THRESHOLD;

  return (
    <div
      className={`flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold ${
        isLow ? 'bg-red-500/20 text-red-400' : 'bg-white/10 text-white'
      }`}
      aria-live="polite"
      aria-label={`${t('balance')}: ${balance} coins${isLow ? ` (${t('lowBalance')})` : ''}`}
    >
      <span>💰</span>
      <span>{balance} {t('coins')}</span>
      {isLow && <span className="text-xs">({t('lowBalance')})</span>}
    </div>
  );
}
