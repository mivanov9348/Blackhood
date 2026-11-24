import type { ReactNode } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth'
import { Embers } from './components/Embers'
import CharacterEditor from './pages/admin/CharacterEditor'
import CharacterList from './pages/admin/CharacterList'
import AuthPage from './pages/AuthPage'
import MainMenu from './pages/MainMenu'
import TavernTable from './pages/TavernTable'

function RequireAuth({ children, admin = false }: { children: ReactNode; admin?: boolean }) {
  const { user, loading } = useAuth()
  if (loading) return <Loading />
  if (!user) return <Navigate to="/login" replace />
  if (admin && !user.isAdmin) return <Navigate to="/" replace />
  return children
}

function Loading() {
  return (
    <div className="screen screen--center">
      <Embers />
      <p className="loading">Свещите се палят…</p>
    </div>
  )
}

export default function App() {
  const { user, loading } = useAuth()

  return (
    <Routes>
      <Route path="/login" element={loading ? <Loading /> : user ? <Navigate to="/" replace /> : <AuthPage />} />
      <Route path="/" element={<RequireAuth><MainMenu /></RequireAuth>} />
      <Route path="/table/:matchId" element={<RequireAuth><TavernTable /></RequireAuth>} />
      <Route path="/admin" element={<RequireAuth admin><CharacterList /></RequireAuth>} />
      <Route path="/admin/characters/new" element={<RequireAuth admin><CharacterEditor /></RequireAuth>} />
      <Route path="/admin/characters/:code" element={<RequireAuth admin><CharacterEditor /></RequireAuth>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
