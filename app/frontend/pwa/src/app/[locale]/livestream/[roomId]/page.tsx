import LivestreamRoom from '@/features/livestream/ui/LivestreamRoom';

interface Props {
  params: Promise<{ roomId: string; locale: string }>;
}

export default async function LivestreamRoomPage({ params }: Props) {
  const { roomId, locale } = await params;
  return <LivestreamRoom roomId={roomId} locale={locale} />;
}
