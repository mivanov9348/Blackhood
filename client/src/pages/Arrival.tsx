import { useCallback, useEffect, useMemo, useState } from 'react'
import { api } from '../api'
import { Embers } from '../components/Embers'
import { Portrait } from '../components/Portrait'
import { useTypewriter } from '../components/useTypewriter'
import { roleInfo } from '../game/text'
import type { MatchView, OpponentView } from '../game/types'
import '../arrival.css'

type Choice = 'who' | 'begin' | 'silent'

type Step =
  | { kind: 'door' }
  | { kind: 'host' }
  | { kind: 'reply'; choice: Choice }
  | { kind: 'guest'; index: number }
  | { kind: 'role' }

const choices: { id: Choice; label: string }[] = [
  { id: 'who', label: '„Кой сте вие?“' },
  { id: 'begin', label: '„Да започваме.“' },
  { id: 'silent', label: '…мълчиш' },
]

/**
 * The arrival scene: the door, the host, the four guests one by one, then the secret role.
 * Purely presentational — everything shown here is public except the player's own role.
 */
export default function Arrival({ match, onDone }: { match: MatchView; onDone: (view: MatchView) => void }) {
  const [step, setStep] = useState<Step>({ kind: 'door' })
  const [busy, setBusy] = useState(false)

  const finishIntro = useCallback(async () => {
    if (busy) return
    setBusy(true)
    try {
      onDone(await api<MatchView>(`/api/singleplayer/matches/${match.id}/intro-complete`, { method: 'POST' }))
    } catch {
      setBusy(false)
    }
  }, [busy, match.id, onDone])

  const next = useCallback(() => {
    setStep((s) => {
      switch (s.kind) {
        case 'door':
          return { kind: 'host' }
        case 'reply':
          return { kind: 'guest', index: 0 }
        case 'guest':
          return s.index + 1 < match.opponents.length ? { kind: 'guest', index: s.index + 1 } : { kind: 'role' }
        default:
          return s
      }
    })
  }, [match.opponents.length])

  const seated = step.kind === 'guest' ? match.opponents.slice(0, step.index) : step.kind === 'role' ? match.opponents : []

  return (
    <div className={`arrival arrival--${step.kind}`}>
      <div className="arrival__wall" />
      {step.kind !== 'door' && <div className="arrival__glow" />}
      {step.kind !== 'door' && <Embers count={18} />}
      <div className="vignette" />

      <button className="arrival__skip link" onClick={finishIntro} disabled={busy}>
        Пропусни ›
      </button>

      {step.kind === 'door' && <Door onNext={next} />}
      {step.kind === 'host' && <HostGreeting match={match} onChoose={(choice) => setStep({ kind: 'reply', choice })} />}
      {step.kind === 'reply' && <HostReply match={match} choice={step.choice} onNext={next} />}
      {step.kind === 'guest' && <GuestEntrance key={step.index} match={match} guest={match.opponents[step.index]} onNext={next} />}
      {step.kind === 'role' && <RoleDeal match={match} onSit={finishIntro} busy={busy} />}

      {seated.length > 0 && (
        <div className="arrival__seated" aria-label="Вече седнали">
          {seated.map((o) => (
            <div key={o.seat} className="arrival__seated-guest">
              <Portrait hue={o.portraitHue} code={o.code} hasImage={o.hasPortrait} />
              <span>{o.name}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

/* ---------------------------------------------------------------------------------- */

/** Space / Enter / click: first completes the current line, then advances. */
function useAdvance(done: boolean, finish: () => void, onNext: (() => void) | null) {
  const advance = useCallback(() => {
    if (!done) finish()
    else onNext?.()
  }, [done, finish, onNext])

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === ' ' || e.key === 'Enter') {
        e.preventDefault()
        advance()
      }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [advance])

  return advance
}

function Door({ onNext }: { onNext: () => void }) {
  const text = 'Страноприемницата не е на никоя карта. И все пак ти я намери.'
  const { shown, done, finish } = useTypewriter(text, 45)
  const advance = useAdvance(done, finish, onNext)

  return (
    <div className="door" onClick={advance}>
      <div className="rain" aria-hidden="true" />
      <div className="door__light" aria-hidden="true" />
      <p className="door__text">{shown}</p>
      {done && <span className="hint">натисни, за да влезеш</span>}
    </div>
  )
}

function HostFigure({ match, small = false }: { match: MatchView; small?: boolean }) {
  const h = match.host
  return (
    <div className={small ? 'host-figure host-figure--small' : 'host-figure'}>
      <div className="host-figure__candle" aria-hidden="true" />
      <Portrait hue={h.portraitHue} host code={h.code} hasImage={h.hasPortrait} />
      <div className="nameplate nameplate--host">
        <strong>{h.name}</strong>
        <span>{h.title}</span>
      </div>
    </div>
  )
}

function HostGreeting({ match, onChoose }: { match: MatchView; onChoose: (c: Choice) => void }) {
  const { shown, done, finish } = useTypewriter(match.host.intro.greeting)
  useAdvance(done, finish, null)

  return (
    <div className="stage-center">
      <HostFigure match={match} />
      <Speech speaker={match.host.name} text={shown} onClick={finish} />
      <div className={done ? 'choices choices--visible' : 'choices'}>
        {choices.map((c) => (
          <button key={c.id} className="btn choice" onClick={() => onChoose(c.id)} disabled={!done}>
            {c.label}
          </button>
        ))}
      </div>
    </div>
  )
}

function HostReply({ match, choice, onNext }: { match: MatchView; choice: Choice; onNext: () => void }) {
  const i = match.host.intro
  const reply = choice === 'who' ? i.replyWho : choice === 'begin' ? i.replyBegin : i.replySilent
  const { shown, done, finish } = useTypewriter(reply)
  const advance = useAdvance(done, finish, onNext)

  return (
    <div className="stage-center" onClick={advance}>
      <HostFigure match={match} />
      <p className="player-said">Ти: {choices.find((c) => c.id === choice)!.label}</p>
      <Speech speaker={match.host.name} text={shown} />
      <CandleRow lit={done} total={match.totalCandles} />
      {done && <span className="hint">Гостите пристигат…</span>}
    </div>
  )
}

function GuestEntrance({ match, guest, onNext }: { match: MatchView; guest: OpponentView; onNext: () => void }) {
  const intro = guest.intro!
  // Returning guests skip the host's introduction — they get a short greeting.
  const lines = useMemo(
    () => (guest.metBefore ? [intro.line] : [intro.hostIntroduction, intro.line]),
    [guest.metBefore, intro.hostIntroduction, intro.line],
  )
  const [lineIndex, setLineIndex] = useState(0)
  const isHostLine = !guest.metBefore && lineIndex === 0
  const { shown, done, finish } = useTypewriter(lines[lineIndex])
  const last = lineIndex === lines.length - 1
  const advance = useAdvance(done, finish, last ? onNext : () => setLineIndex((n) => n + 1))

  return (
    <div className="stage-center guest-stage" onClick={advance}>
      <HostFigure match={match} small />
      <div className="guest-card">
        <Portrait hue={guest.portraitHue} code={guest.code} hasImage={guest.hasPortrait} className="guest-card__portrait" />
        <div className="nameplate guest-card__plate">
          <strong>{guest.name}</strong>
          <span>{guest.title}</span>
        </div>
        {guest.metBefore && <span className="badge">Срещали сте се</span>}
      </div>

      <Speech
        speaker={isHostLine ? match.host.name : guest.name}
        text={shown}
        variant={isHostLine ? 'host' : 'guest'}
        direction={!isHostLine && done ? intro.direction : null}
      />
      {!isHostLine && done && intro.gesture && <p className="gesture">*{intro.gesture.charAt(0).toLowerCase() + intro.gesture.slice(1)}*</p>}
      {done && <span className="hint">{last ? 'следващ гост' : 'продължи'}</span>}
    </div>
  )
}

function RoleDeal({ match, onSit, busy }: { match: MatchView; onSit: () => void; busy: boolean }) {
  const [flipped, setFlipped] = useState(false)
  const { shown, done, finish } = useTypewriter(match.host.intro.roleDeal)
  const role = roleInfo[match.me.role]
  useAdvance(done, finish, flipped ? onSit : () => setFlipped(true))

  return (
    <div className="stage-center role-stage">
      <HostFigure match={match} small />
      <Speech speaker={match.host.name} text={shown} onClick={finish} />

      <button className={flipped ? 'role-flip role-flip--open' : 'role-flip'} onClick={() => setFlipped(true)} disabled={!done || flipped} aria-label="Обърни картата">
        <span className="role-flip__inner">
          <span className="role-flip__back">Тайна роля</span>
          <span className="role-flip__front">
            <small>Ти си</small>
            <strong>{role.name}</strong>
            <em>{role.goal}</em>
            <span>{role.skill}</span>
          </span>
        </span>
      </button>

      {flipped ? (
        <button className="btn btn--primary" onClick={onSit} disabled={busy}>
          Седни на масата
        </button>
      ) : (
        done && <span className="hint">обърни картата</span>
      )}
    </div>
  )
}

function Speech({ speaker, text, variant = 'host', direction, onClick }: { speaker: string; text: string; variant?: 'host' | 'guest'; direction?: string | null; onClick?: () => void }) {
  return (
    <div className={`speech speech--${variant}`} onClick={onClick}>
      <span className="speech__speaker">{speaker}</span>
      <q className="speech__text">{text}</q>
      {direction && <span className="speech__direction">({direction})</span>}
    </div>
  )
}

function CandleRow({ lit, total }: { lit: boolean; total: number }) {
  return (
    <div className="candles candles--intro">
      {Array.from({ length: total }, (_, i) => (
        <div key={i} className="candle candle--lit">
          <span className={lit ? 'candle__flame candle__flame--ignite' : 'candle__flame candle__flame--off'} style={{ animationDelay: `${i * 0.35}s` }} />
          <span className="candle__wax" />
        </div>
      ))}
    </div>
  )
}
