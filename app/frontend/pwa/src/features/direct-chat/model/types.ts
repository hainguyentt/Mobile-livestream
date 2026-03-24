export interface Conversation {
  id: string;
  otherUserId: string;
  lastMessagePreview?: string;
  lastMessageAt?: string;
  unreadCount: number;
  createdAt: string;
}

export interface DirectMessage {
  messageId: string;
  senderId: string;
  content: string;
  messageType: 'Text' | 'Emoji';
  emojiCode?: string;
  isRead: boolean;
  sentAt: string;
}
