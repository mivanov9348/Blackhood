import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react'
import { api } from './api'

export interface User {
  id: string
  userName: string
  email: string
  isAdmin: boolean
  gold: number
}

interface AuthState {
  user: User | null
  loading: boolean
  login: (userName: string, password: string) => Promise<void>
  register: (userName: string, email: string, password: string) => Promise<void>
  logout: () => Promise<void>
  /** Update the purse shown in the UI after a game changed it. */
  setGold: (gold: number) => void
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api<User>('/api/auth/me')
      .then(setUser)
      .catch(() => setUser(null))
      .finally(() => setLoading(false))
  }, [])

  const login = useCallback(async (userName: string, password: string) => {
    setUser(await api<User>('/api/auth/login', { method: 'POST', body: { userName, password } }))
  }, [])

  const register = useCallback(async (userName: string, email: string, password: string) => {
    setUser(await api<User>('/api/auth/register', { method: 'POST', body: { userName, email, password } }))
  }, [])

  const logout = useCallback(async () => {
    await api('/api/auth/logout', { method: 'POST' })
    setUser(null)
  }, [])

  const setGold = useCallback((gold: number) => setUser((u) => (u ? { ...u, gold } : u)), [])

  return <AuthContext.Provider value={{ user, loading, login, register, logout, setGold }}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>')
  return ctx
}
