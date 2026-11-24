import { useState, type FormEvent } from 'react'
import { useAuth } from '../auth'
import { Embers } from '../components/Embers'

type Mode = 'login' | 'register'

export default function AuthPage() {
  const { login, register } = useAuth()
  const [mode, setMode] = useState<Mode>('login')
  const [userName, setUserName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  async function submit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setBusy(true)
    try {
      if (mode === 'login') await login(userName, password)
      else await register(userName, email, password)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Нещо се обърка.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="screen screen--center">
      <Embers />
      <div className="vignette" />

      <div className="auth">
        <h1 className="logo logo--small">Blackhood</h1>
        <p className="tagline">Вратата на таверната е отворена. Кажи си името.</p>

        <div className="tabs" role="tablist">
          <button role="tab" aria-selected={mode === 'login'} className={mode === 'login' ? 'tab tab--active' : 'tab'} onClick={() => { setMode('login'); setError(null) }}>
            Вход
          </button>
          <button role="tab" aria-selected={mode === 'register'} className={mode === 'register' ? 'tab tab--active' : 'tab'} onClick={() => { setMode('register'); setError(null) }}>
            Регистрация
          </button>
        </div>

        <form className="auth__form" onSubmit={submit}>
          <label>
            <span>{mode === 'login' ? 'Име или имейл' : 'Име'}</span>
            <input value={userName} onChange={(e) => setUserName(e.target.value)} autoComplete="username" required />
          </label>

          {mode === 'register' && (
            <label>
              <span>Имейл</span>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} autoComplete="email" required />
            </label>
          )}

          <label>
            <span>Парола</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              minLength={mode === 'register' ? 8 : undefined}
              required
            />
          </label>

          {error && <p className="form-error">{error}</p>}

          <button className="btn btn--primary" type="submit" disabled={busy}>
            {busy ? '…' : mode === 'login' ? 'Влез в таверната' : 'Запиши ме в книгата'}
          </button>
        </form>
      </div>
    </div>
  )
}
