import { useTranslations } from 'next-intl';
import ConversationList from '@/features/direct-chat/ui/ConversationList';

interface Props {
  params: { locale: string };
}

export default function MessagesPage({ params }: Props) {
  const t = useTranslations('directChat');

  return (
    <main className="min-h-screen bg-black">
      <div className="sticky top-0 z-10 bg-black/80 backdrop-blur-sm px-4 py-3">
        <h1 className="text-lg font-bold text-white">{t('title')}</h1>
      </div>
      <ConversationList locale={params.locale} />
    </main>
  );
}
