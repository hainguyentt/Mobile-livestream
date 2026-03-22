export default function DashboardPage() {
  return (
    <main className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-semibold mb-6">Admin Dashboard</h1>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="bg-white rounded-lg shadow p-4">
            <h2 className="text-sm font-medium text-gray-500">Total Users</h2>
            <p className="text-2xl font-bold mt-1">—</p>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <h2 className="text-sm font-medium text-gray-500">Pending Verifications</h2>
            <p className="text-2xl font-bold mt-1">—</p>
          </div>
          <div className="bg-white rounded-lg shadow p-4">
            <h2 className="text-sm font-medium text-gray-500">Active Streams</h2>
            <p className="text-2xl font-bold mt-1">—</p>
          </div>
        </div>
      </div>
    </main>
  )
}
