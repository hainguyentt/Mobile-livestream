import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import ConversationThread from '@/features/direct-chat/ui/ConversationThread'
import { useDirectChatStore } from '@/features/direct-chat/model/useDirectChatStore'
import { directChatApi } from '@/features/direct-chat/api/directChatApi'

// Mock API
jest.mock('@/features/direct-chat/api/directChatApi', () => ({
  directChatApi: {
    getMessages: jest.fn(),
    markAsRead: jest.fn(),
  },
}))

// Mock store
jest.mock('@/features/direct-chat/model/useDirectChatStore', () => ({
  useDirectChatStore: jest.fn(),
}))

// Mock MessageInput to isolate ConversationThread
jest.mock('@/features/direct-chat/ui/MessageInput', () => ({
  __esModule: true,
  default: () => <div data-testid="message-input" />,
}))

const mockMessages = [
  {
    messageId: 'msg-1',
    conversationId: 'conv-1',
    senderId: 'user-1',
    content: 'Hello there',
    sentAt: new Date().toISOString(),
    isRead: true,
  },
  {
    messageId: 'msg-2',
    conversationId: 'conv-1',
    senderId: 'user-2',
    content: 'Hi back',
    sentAt: new Date().toISOString(),
    isRead: false,
  },
]

describe('ConversationThread', () => {
  const mockSetMessages = jest.fn()
  const mockMarkRead = jest.fn()

  beforeEach(() => {
    jest.clearAllMocks()
    ;(useDirectChatStore as unknown as jest.Mock).mockReturnValue({
      messages: { 'conv-1': mockMessages },
      setMessages: mockSetMessages,
      markRead: mockMarkRead,
    })
    ;(directChatApi.getMessages as jest.Mock).mockResolvedValue(mockMessages)
    ;(directChatApi.markAsRead as jest.Mock).mockResolvedValue(undefined)
  })

  it('renders messages in the thread', async () => {
    render(
      <ConversationThread
        conversationId="conv-1"
        currentUserId="user-1"
        chatConn={null}
      />
    )

    await waitFor(() => {
      expect(screen.getByText('Hello there')).toBeInTheDocument()
      expect(screen.getByText('Hi back')).toBeInTheDocument()
    })
  })

  it('aligns own messages to the right', async () => {
    render(
      <ConversationThread
        conversationId="conv-1"
        currentUserId="user-1"
        chatConn={null}
      />
    )

    await waitFor(() => {
      const myMessage = screen.getByText('Hello there').closest('div[class*="justify-end"]')
      expect(myMessage).toBeInTheDocument()
    })
  })

  it('aligns other messages to the left', async () => {
    render(
      <ConversationThread
        conversationId="conv-1"
        currentUserId="user-1"
        chatConn={null}
      />
    )

    await waitFor(() => {
      const otherMessage = screen.getByText('Hi back').closest('div[class*="justify-start"]')
      expect(otherMessage).toBeInTheDocument()
    })
  })

  it('calls markAsRead on mount', async () => {
    render(
      <ConversationThread
        conversationId="conv-1"
        currentUserId="user-1"
        chatConn={null}
      />
    )

    await waitFor(() => {
      expect(mockMarkRead).toHaveBeenCalledWith('conv-1')
      expect(directChatApi.markAsRead).toHaveBeenCalledWith('conv-1')
    })
  })

  it('renders message input', () => {
    render(
      <ConversationThread
        conversationId="conv-1"
        currentUserId="user-1"
        chatConn={null}
      />
    )

    expect(screen.getByTestId('message-input')).toBeInTheDocument()
  })

  it('loads older messages when button clicked', async () => {
    render(
      <ConversationThread
        conversationId="conv-1"
        currentUserId="user-1"
        chatConn={null}
      />
    )

    const loadButton = screen.getByText('Load older messages')
    fireEvent.click(loadButton)

    await waitFor(() => {
      // getMessages called twice: once on mount, once on load older
      expect(directChatApi.getMessages).toHaveBeenCalledTimes(2)
    })
  })
})
