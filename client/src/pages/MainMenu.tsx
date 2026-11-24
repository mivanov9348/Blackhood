import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { Embers } from '../components/Embers'
import type { MatchView } from '../game/types'

export default function MainMenu() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function startSingleplayer() {
    setBusy(true)
    setError(null)
    try {
      const match = await api<MatchView>('/api/singleplayer/matches', { method: 'POST' })
      navigate(`/table/${match.id}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Масата не можа да се нареди.')
      setBusy(false)
    }
  }

  return (
    <div className="screen screen--center menu">
      <div className="menu__hearth" />
      <Embers count={36} />
      <div className="vignette" />

      <header className="menu__user">
        <span>{user?.userName}</span>
        {user?.isAdmin && <Link className="link" to="/admin">Персонажи</Link>}
        <button className="link" onClick={logout}>Излез</button>
      </header>

      <main className="menu__main">
        <h1 className="logo">Blackhood</h1>
        <p className="tagline">Шест свещи. Петима гости. Един Домакин, който знае всичко.</p>

        <nav className="menu__nav">
          <button className="btn btn--primary btn--wide" onClick={startSingleplayer} disabled={busy}>
            {busy ? 'Домакинът нарежда масата…' : 'Сингълплейър'}
            <small>Маса в таверната срещу четирима гости</small>
          </button>

          <button className="btn btn--wide" disabled title="Скоро">
            Мултиплейър
            <small>Скоро</small>
          </button>
        </nav>

        {error && <p className="form-error">{error}</p>}
      </main>

      <footer className="menu__footer">работно заглавие · прототип</footer>
    </div>
  )
}
