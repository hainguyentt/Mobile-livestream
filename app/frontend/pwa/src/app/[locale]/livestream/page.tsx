import { useTranslations } from 'next-intl';
import LivestreamGrid from '@/features/livestream/ui/LivestreamGrid';

export default function LivestreamPage() {
  const t = useTranslations('livestream');

  return (
    <main className="min-h-screen bg-black">
      <div className="sticky top-0 z-10 bg-black/80 backdrop-blur-sm px-4 py-3">
        <h1 className="text-lg font-bold text-white">{t('title')}</h1>
      </div>
      <LivestreamGrid />
    </main>
  );
}
