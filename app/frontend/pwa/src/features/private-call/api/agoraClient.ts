/**
 * Agora RTC SDK wrapper.
 * MockMode: when NEXT_PUBLIC_AGORA_MOCK=true, returns fake streams for dev.
 */

const MOCK_MODE = process.env.NEXT_PUBLIC_AGORA_MOCK === 'true';

export interface AgoraClientConfig {
  appId: string;
  channelName: string;
  token: string;
  uid: string;
}

export interface AgoraClient {
  join: (config: AgoraClientConfig) => Promise<void>;
  leave: () => Promise<void>;
  publishLocalVideo: () => Promise<void>;
  unpublishLocalVideo: () => Promise<void>;
}

export function createAgoraClient(): AgoraClient {
  if (MOCK_MODE) {
    return {
      join: async (config) => {
        console.log('[Agora Mock] Joined channel:', config.channelName);
      },
      leave: async () => {
        console.log('[Agora Mock] Left channel');
      },
      publishLocalVideo: async () => {
        console.log('[Agora Mock] Published local video');
      },
      unpublishLocalVideo: async () => {
        console.log('[Agora Mock] Unpublished local video');
      },
    };
  }

  // Real Agora RTC SDK integration
  // TODO: import AgoraRTC from 'agora-rtc-sdk-ng' and implement
  throw new Error('Real Agora client not yet implemented. Set NEXT_PUBLIC_AGORA_MOCK=true for dev.');
}
