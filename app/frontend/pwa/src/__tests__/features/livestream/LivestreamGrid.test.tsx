import { render, screen, waitFor } from '@testing-library/react'
import { NextIntlClientProvider } from 'next-intl'
import LivestreamGrid from '@/features/livestream/ui/LivestreamGrid'
import { livestreamApi } from '@/features/livestream/api/livestreamApi'
import { useLivestreamStore } from '@/features/livestream/model/useLivestreamStore'

// Mock API
jest.mock('@/features/livestream/api/livestreamApi', () => ({
  livestreamApi: {
    getRooms: jest.fn(),
  },
}))

// Mock store
jest.mock('@/features/livestream/model/useLivestreamStore', () => ({
  useLivestreamStore: jest.fn(),
}))

const messages = {
  livestream: {
    title: 'Live',
    noActiveRooms: 'No live streams right now',
    chatPlaceholder: 'Say something...',
    send: 'Send',
    sendGift: 'Send gift',
    endStream: 'End Stream',
    ending: 'Ending...',
    confirmEndStream: 'Are you sure you want to end the stream?',
  },
}

function renderWithIntl(ui: React.ReactElement) {
  return render(
    <NextIntlClientProvider locale="en" messages={messages}>
      {ui}
    </NextIntlClientProvider>
  )
}

describe('LivestreamGrid', () => {
  const mockSetRooms = jest.fn()

  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('shows empty state when no rooms', async () => {
    ;(useLivestreamStore as unknown as jest.Mock).mockReturnValue({
      rooms: [],
      setRooms: mockSetRooms,
    })
    ;(livestreamApi.getRooms as jest.Mock).mockResolvedValue({ items: [] })

    renderWithIntl(<LivestreamGrid />)

    await waitFor(() => {
      expect(screen.getByText('No live streams right now')).toBeInTheDocument()
    })
  })

  it('renders room cards when rooms exist', async () => {
    const mockRooms = [
      {
        id: 'room-1',
        title: 'Test Room',
        category: 'general',
        viewerCount: 42,
        hostId: 'host-1',
        hostName: 'Host One',
        status: 'live',
        startedAt: new Date().toISOString(),
      },
    ]

    ;(useLivestreamStore as unknown as jest.Mock).mockReturnValue({
      rooms: mockRooms,
      setRooms: mockSetRooms,
    })
    ;(livestreamApi.getRooms as jest.Mock).mockResolvedValue({ items: mockRooms })

    renderWithIntl(<LivestreamGrid />)

    await waitFor(() => {
      expect(screen.getByText('Test Room')).toBeInTheDocument()
      expect(screen.getByText('LIVE')).toBeInTheDocument()
    })
  })

  it('calls getRooms with category when provided', async () => {
    ;(useLivestreamStore as unknown as jest.Mock).mockReturnValue({
      rooms: [],
      setRooms: mockSetRooms,
    })
    ;(livestreamApi.getRooms as jest.Mock).mockResolvedValue({ items: [] })

    renderWithIntl(<LivestreamGrid category="music" />)

    await waitFor(() => {
      expect(livestreamApi.getRooms).toHaveBeenCalledWith('music')
    })
  })

  it('sorts rooms by viewer count descending', async () => {
    const mockRooms = [
      {
        id: 'room-1',
        title: 'Low Viewers',
        category: 'general',
        viewerCount: 10,
        hostId: 'host-1',
        hostName: 'Host One',
        status: 'live',
        startedAt: new Date().toISOString(),
      },
      {
        id: 'room-2',
        title: 'High Viewers',
        category: 'general',
        viewerCount: 500,
        hostId: 'host-2',
        hostName: 'Host Two',
        status: 'live',
        startedAt: new Date().toISOString(),
      },
    ]

    ;(useLivestreamStore as unknown as jest.Mock).mockReturnValue({
      rooms: mockRooms,
      setRooms: mockSetRooms,
    })
    ;(livestreamApi.getRooms as jest.Mock).mockResolvedValue({ items: mockRooms })

    renderWithIntl(<LivestreamGrid />)

    await waitFor(() => {
      const titles = screen.getAllByText(/Viewers/)
      // High Viewers should appear before Low Viewers
      expect(titles[0].textContent).toBe('High Viewers')
    })
  })
})
