import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { Embers } from '../components/Embers'
import { Portrait } from '../components/Portrait'
import type { GiftsResponse } from '../game/types'
import '../hall.css'

interface Patron { code: string; name: string; title: string; portraitHue: number; hasPortrait: boolean; doing: string }
interface Recent { id: string; game: string; startedAt: string; finished: boolean; buyIn: number; payout: number }
interface Tavern { gold: number; patrons: Patron[]; unfinishedGifts: string | null; unfinishedBones: string | null; recent: Recent[] }

const gamePath = (game: string, id: string) => (game === 'Bones' ? '/bones/' : '/gifts/') + id

const ambience = [
  'Rain drums on the shutters. Nobody remembers the last time it stopped.',
  'Someone laughs too loudly by the fire, then stops, as if they forgot why.',
  'A dog under a table growls at an empty chair.',
  'For a moment the candles in the far corner burn blue.',
  'The stairs creak, though no one is on them.',
  'A lute plays somewhere upstairs. There is no upstairs.',
  'The innkeeper polishes a glass that is already clean, watching you.',
]

export default function TavernHall() {
  const { user, logout, setGold } = useAuth()
  const navigate = useNavigate()
  const [tavern, setTavern] = useState<Tavern | null>(null)
  const [line, setLine] = useState(0)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showRules, setShowRules] = useState(false)

  useEffect(() => {
    api<Tavern>('/api/tavern')
      .then((t) => {
        setTavern(t)
        setGold(t.gold)
      })
      .catch((e) => setError(e.message))
  }, [setGold])

  useEffect(() => {
    const id = window.setInterval(() => setLine((n) => (n + 1) % ambience.length), 9000)
    return () => window.clearInterval(id)
  }, [])

  async function sitDownBones() {
    setBusy(true)
    setError(null)
    try {
      const r = await api<{ match: { id: string }; profileGold: number }>('/api/bones/matches', { method: 'POST' })
      setGold(r.profileGold)
      navigate(`/bones/${r.match.id}`)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'The cups would not shake.')
      setBusy(false)
    }
  }

  async function sitDown() {
    setBusy(true)
    setError(null)
    try {
      const r = await api<GiftsResponse>('/api/gifts/matches', { method: 'POST' })
      setGold(r.profileGold)
      navigate(`/gifts/${r.match.id}`)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'The host could not set the table.')
      setBusy(false)
    }
  }

  return (
    <div className="hall">
      <div className="hall__hearth" />
      <Embers count={30} />
      <div className="vignette" />

      <header className="hall__bar">
        <div className="hall__brand">
          <span className="logo logo--tiny">Blackhood</span>
          <span className="hall__inn">The Blackhood Inn</span>
        </div>
        <div className="hall__user">
          <span className="purse" title="Your purse — every game is played with this gold">🪙 {user?.gold ?? '…'}</span>
          <span>{user?.userName}</span>
          {user?.isAdmin && <Link className="link" to="/admin">Characters</Link>}
          <button className="link" onClick={logout}>Leave</button>
        </div>
      </header>

      <main className="hall__main">
        <p className="hall__ambience" key={line}>{ambience[line]}</p>

        {tavern && tavern.patrons.length > 0 && (
          <section className="patrons" aria-label="Tonight in the hall">
            <h2 className="hall__heading">Tonight in the hall</h2>
            <div className="patrons__row">
              {tavern.patrons.map((p) => (
                <div key={p.code} className="patron">
                  <Portrait hue={p.portraitHue} code={p.code} hasImage={p.hasPortrait} />
                  <div>
                    <strong>{p.name}</strong>
                    <span className="patron__title">{p.title}</span>
                    {p.doing && <em>{p.doing.charAt(0).toLowerCase() + p.doing.slice(1)}.</em>}
                  </div>
                </div>
              ))}
            </div>
          </section>
        )}

        <section className="tables">
          <article className="table-card table-card--main">
            <div className="table-card__glyph">🕯</div>
            <div className="table-card__body">
              <span className="table-card__where">The host's table</span>
              <h2>Gifts</h2>
              <p>
                Six candles. Five guests. One of them is an Assassin. Every night each guest sends one gift through the host — anonymously,
                with a single word: <em>“someone sends you Wine.”</em> Somewhere among the gifts is a dagger dressed as something kind.
              </p>
              <p className="table-card__meta">Buy-in: 10 gold from your purse · Survive, name the Assassin, leave richer.</p>
              <div className="table-card__actions">
                {tavern?.unfinishedGifts ? (
                  <>
                    <button className="btn btn--primary" onClick={() => navigate(`/gifts/${tavern.unfinishedGifts}`)}>Return to your table</button>
                    <button className="btn" onClick={sitDown} disabled={busy}>New evening</button>
                  </>
                ) : (
                  <button className="btn btn--primary" onClick={sitDown} disabled={busy}>
                    {busy ? 'The host is setting the table…' : 'Sit down'}
                  </button>
                )}
                <button className="link" onClick={() => setShowRules((s) => !s)}>{showRules ? 'Hide the rules' : 'How is it played?'}</button>
              </div>
              {showRules && <RulesSummary />}
              {error && <p className="form-error">{error}</p>}
            </div>
          </article>

          <article className="table-card">
            <div className="table-card__glyph">⚄</div>
            <div className="table-card__body">
              <span className="table-card__where">By the fire</span>
              <h2>Bones</h2>
              <p>Four players, dice hidden under cups. Bid how many of a face lie on the table — ones are the Witch’s eye — or call “Liar!”. The last one with dice takes the pot.</p>
              <p className="table-card__meta">Buy-in: 5 gold · winner takes 20</p>
              <div className="table-card__actions">
                {tavern?.unfinishedBones && <button className="btn" onClick={() => navigate(`/bones/${tavern.unfinishedBones}`)}>Return</button>}
                <button className="btn btn--primary" onClick={sitDownBones} disabled={busy}>Roll the bones</button>
              </div>
            </div>
          </article>

          <article className="table-card table-card--locked">
            <div className="table-card__glyph">🔮</div>
            <div className="table-card__body">
              <span className="table-card__where">The corner behind the curtain</span>
              <h2>The Cards of Fate</h2>
              <p>An old woman reads your future. For a price, she will change it.</p>
              <span className="soon">Coming soon</span>
            </div>
          </article>

          <article className="table-card table-card--locked">
            <div className="table-card__glyph">🚪</div>
            <div className="table-card__body">
              <span className="table-card__where">The back room</span>
              <h2>Private tables</h2>
              <p>Gifts with other travellers instead of the inn's regulars.</p>
              <span className="soon">Multiplayer — coming soon</span>
            </div>
          </article>
        </section>

        {tavern && tavern.recent.length > 0 && (
          <section className="recent">
            <h2 className="hall__heading">Your last evenings</h2>
            <ul>
              {tavern.recent.map((r) => (
                <li key={r.id}>
                  <span>{r.game} · {new Date(r.startedAt).toLocaleString()}</span>
                  {r.finished ? (
                    <span className={r.payout > r.buyIn ? 'gain' : r.payout < r.buyIn ? 'loss' : ''}>
                      paid {r.buyIn}, took home {r.payout}
                    </span>
                  ) : (
                    <Link className="link" to={gamePath(r.game, r.id)}>unfinished — return</Link>
                  )}
                </li>
              ))}
            </ul>
          </section>
        )}
      </main>
    </div>
  )
}

function RulesSummary() {
  return (
    <div className="rules">
      <ol>
        <li><strong>Roles.</strong> One Assassin, the rest are Guests. The Assassin needs <strong>2 deaths</strong> before the sixth candle goes out. <em>Every</em> death counts, however it happens.</li>
        <li><strong>Gold.</strong> Everyone starts with 10 and gets +2 each candle. Gold is visible; at dawn you see who paid the host — never for what.</li>
        <li><strong>The whisper.</strong> The host offers you things: amulets, mirrors, poison, rumours. Prices rise with fear.</li>
        <li><strong>The gift.</strong> Send one gift to someone and announce it with one word. You may lie — except you can never announce a Dagger or Poison.</li>
        <li><strong>Delivery.</strong> Gifts are delivered one by one. You can close the door on one, or raise a mirror. Otherwise you take it.</li>
        <li><strong>Dawn.</strong> A dagger from the Assassin kills — from the second candle on. Poison kills a night later. An amulet stops one death.</li>
        <li><strong>The end.</strong> If the Assassin dies, the guests win at once. After six candles, the guests name the Assassin (+5 if right); the living guest with the most gold is the guest of honour.</li>
      </ol>
    </div>
  )
}
