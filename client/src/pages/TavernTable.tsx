import { useEffect, useState, type CSSProperties } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api'
import { Candles } from '../components/Candles'
import { Embers } from '../components/Embers'
import { DeckPile, GameCard } from '../components/GameCard'
import { Portrait } from '../components/Portrait'
import { cardInfo, phaseName, roleInfo } from '../game/text'
import type { MatchView, OpponentView } from '../game/types'
import Arrival from './Arrival'
import '../tavern.css'

// Opponents sit clockwise from the player's left: near-left, far-left, far-right, near-right.
const seatSlots = ['seat--near-left', 'seat--far-left', 'seat--far-right', 'seat--near-right']

export default function TavernTable() {
  const { matchId } = useParams()
  const [match, setMatch] = useState<MatchView | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api<MatchView>(`/api/singleplayer/matches/${matchId}`)
      .then(setMatch)
      .catch((err) => setError(err instanceof Error ? err.message : 'Масата не е намерена.'))
  }, [matchId])

  if (error)
    return (
      <div className="screen screen--center">
        <p className="form-error">{error}</p>
        <Link className="btn" to="/">Към менюто</Link>
      </div>
    )
  if (!match)
    return (
      <div className="screen screen--center">
        <p className="loading">Домакинът нарежда масата…</p>
      </div>
    )

  if (!match.introDone) return <Arrival match={match} onDone={setMatch} />

  const role = roleInfo[match.me.role]
  const hand = match.me.hand

  return (
    <div className="tavern tavern--enter">
      {/* Left — the public journal */}
      <aside className="panel panel--log">
        <h2 className="panel__title">Дневник</h2>
        <ol className="log">
          {match.log.map((entry, i) => (
            <li key={i}>
              <span className="log__round">Свещ {entry.round}</span>
              {entry.text}
            </li>
          ))}
        </ol>
      </aside>

      {/* Centre — first-person view across the table */}
      <section className="stage">
        <div className="stage__wall" />
        <div className="stage__glow" />
        <Embers count={14} />

        <header className="stage__bar">
          <Link className="link" to="/">← Меню</Link>
          <span>
            Свещ {match.round} от {match.totalCandles} · <strong>{phaseName[match.phase]}</strong>
          </span>
        </header>

        <div className="host">
          <Portrait hue={match.host.portraitHue} host code={match.host.code} hasImage={match.host.hasPortrait} />
          <div className="nameplate nameplate--host">
            <strong>{match.host.name}</strong>
            <span>{match.host.title}</span>
          </div>
          <Candles total={match.totalCandles} round={match.round} />
        </div>

        <div className="table">
          <div className="table__surface" />
        </div>

        {match.opponents.map((o, i) => (
          <Seat key={o.seat} opponent={o} slot={seatSlots[i]} />
        ))}

        <div className="market">
          <div className="market__cards">
            {match.market.map((c) => (
              <GameCard key={c.id} card={c} role={match.me.role} size="market" />
            ))}
          </div>
          <DeckPile count={match.deckCount} />
          <span className="market__label">Пазарът</span>
        </div>

        {/* The player — first person: only hands and cards are visible */}
        <div className="me">
          <div className="role-card">
            <span className="role-card__label">Твоята роля</span>
            <strong className="role-card__name">{role.name}</strong>
            <p><em>Цел:</em> {role.goal}</p>
            <p><em>Умение:</em> {role.skill}</p>
            <p className="role-card__fear">Страхувай се от {role.fears}.</p>
          </div>

          <div className="hand">
            {hand.map((c, i) => {
              const offset = i - (hand.length - 1) / 2
              return (
                <GameCard
                  key={c.id}
                  card={c}
                  role={match.me.role}
                  style={{ '--rot': `${offset * 7}deg`, '--lift': `${Math.abs(offset) * 10}px` } as CSSProperties}
                />
              )
            })}
          </div>

          <div className={match.me.hasSoul ? 'soul' : 'soul soul--sold'} title="Душата ти — Домакинът я иска">
            <span className="soul__gem" />
            <span>{match.me.hasSoul ? 'Душата ти' : 'Продадена'}</span>
          </div>
        </div>
      </section>

      {/* Right — the host's whispers, only to you */}
      <aside className="panel panel--whisper">
        <h2 className="panel__title">Шепот</h2>
        <ul className="whispers">
          {match.whispers.map((w, i) => (
            <li key={i}>
              <span className="whispers__from">{match.host.name} ти прошепва:</span>
              <q>{w}</q>
            </li>
          ))}
        </ul>
      </aside>
    </div>
  )
}

function Seat({ opponent, slot }: { opponent: OpponentView; slot: string }) {
  return (
    <div className={`seat ${slot}${opponent.isAlive ? '' : ' seat--ghost'}`}>
      <Portrait hue={opponent.portraitHue} code={opponent.code} hasImage={opponent.hasPortrait} />
      <div className="nameplate">
        <strong>{opponent.name}</strong>
        <span>{opponent.title}</span>
      </div>
      <div className="seat__info">
        <span className="chip" title="Карти в ръката">🂠 {opponent.cardCount}</span>
        {opponent.takenFromMarket.length === 0 ? (
          <span className="chip chip--muted" title="Взети от пазара">нищо от пазара</span>
        ) : (
          opponent.takenFromMarket.map((t, i) => (
            <span key={i} className="chip" title={`Взе ${cardInfo[t].name} от пазара`}>{cardInfo[t].glyph}</span>
          ))
        )}
      </div>
    </div>
  )
}
