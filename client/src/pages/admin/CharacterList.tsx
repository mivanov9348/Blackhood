import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../../api'
import { Portrait } from '../../components/Portrait'
import '../../admin.css'
import type { CharacterKind, CharacterSummary } from './model'

type Filter = 'all' | CharacterKind

export default function CharacterList() {
  const navigate = useNavigate()
  const [items, setItems] = useState<CharacterSummary[] | null>(null)
  const [filter, setFilter] = useState<Filter>('all')
  const [query, setQuery] = useState('')
  const [message, setMessage] = useState<string | null>(null)

  useEffect(() => {
    api<CharacterSummary[]>('/api/admin/characters').then(setItems).catch((e) => setMessage(e.message))
  }, [])

  const shown = useMemo(() => {
    const q = query.trim().toLowerCase()
    return (items ?? []).filter(
      (c) => (filter === 'all' || c.kind === filter) && (!q || `${c.code} ${c.name} ${c.title}`.toLowerCase().includes(q)),
    )
  }, [items, filter, query])

  function nextCode(kind: CharacterKind) {
    const prefix = kind === 'Host' ? 'H' : 'P'
    const max = Math.max(0, ...(items ?? []).filter((c) => c.code.startsWith(prefix)).map((c) => Number(c.code.slice(1)) || 0))
    return `${prefix}${String(max + 1).padStart(2, '0')}`
  }

  async function exportAll() {
    try {
      const r = await api<{ count: number }>('/api/admin/characters/export-all', { method: 'POST' })
      setMessage(`Експортирани ${r.count} персонажа в content/characters/.`)
    } catch (e) {
      setMessage(e instanceof Error ? e.message : 'Грешка при експорта.')
    }
  }

  return (
    <div className="admin">
      <header className="admin__bar">
        <Link className="link" to="/">← Меню</Link>
        <h1>Персонажи</h1>
        <div className="admin__actions">
          <button className="btn btn--small" onClick={exportAll}>Експорт на всички в JSON</button>
          <button className="btn btn--small" onClick={() => navigate(`/admin/characters/new?kind=Host&code=${nextCode('Host')}`)}>+ Домакин</button>
          <button className="btn btn--small btn--primary" onClick={() => navigate(`/admin/characters/new?kind=Participant&code=${nextCode('Participant')}`)}>+ Участник</button>
        </div>
      </header>

      <div className="admin__filters">
        {(['all', 'Participant', 'Host'] as Filter[]).map((f) => (
          <button key={f} className={filter === f ? 'tab tab--active' : 'tab'} onClick={() => setFilter(f)}>
            {f === 'all' ? 'Всички' : f === 'Host' ? 'Домакини' : 'Участници'}
          </button>
        ))}
        <input className="admin__search" placeholder="Търси по код, име, титла…" value={query} onChange={(e) => setQuery(e.target.value)} />
      </div>

      {message && <p className="admin__message">{message}</p>}
      {!items && !message && <p className="loading">Зареждам…</p>}

      <div className="char-grid">
        {shown.map((c) => (
          <Link key={c.code} to={`/admin/characters/${c.code}`} className="char-tile">
            <Portrait hue={c.portraitHue} host={c.kind === 'Host'} code={c.code} hasImage={c.hasPortrait} />
            <div className="char-tile__body">
              <span className="char-tile__code">{c.code}</span>
              <strong>{c.name}</strong>
              <span className="char-tile__title">{c.title}</span>
              <div className="char-tile__meta">
                <span className={`pill pill--${c.rarity.toLowerCase()}`}>{c.rarity}</span>
                <span className={c.status === 'Published' ? 'pill pill--ok' : 'pill pill--draft'}>{c.status === 'Published' ? 'Публикуван' : 'Чернова'}</span>
                {c.warningCount > 0 && <span className="pill pill--warn" title="Предупреждения от проверките">⚠ {c.warningCount}</span>}
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  )
}
