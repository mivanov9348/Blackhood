import { useCallback, useEffect, useRef, useState, type CSSProperties, type Dispatch, type ReactNode, type SetStateAction } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { Candles } from '../components/Candles'
import { Embers } from '../components/Embers'
import { Portrait } from '../components/Portrait'
import { declarationName, declarations, honestDeclaration, itemInfo, phaseName, roleInfo } from '../game/text'
import type { Declaration, GiftKind, GiftsAction, GiftsResponse, GiftsView, Item, OfferKind, SeatView, ServiceView } from '../game/types'
import Arrival from './Arrival'
import '../tavern.css'
import '../gifts.css'

// Opponents sit clockwise from the player's left: near-left, far-left, far-right, near-right.
const seatSlots = ['seat--near-left', 'seat--far-left', 'seat--far-right', 'seat--near-right', 'seat--top-left', 'seat--top-right', 'seat--mid-left']

type Pick =
  | { purpose: 'gift' }
  | { purpose: 'monocle'; itemId: number }
  | { purpose: 'service'; service: ServiceView }
  | { purpose: 'point' }
  | { purpose: 'ghost' }

interface GiftDraft { kind: GiftKind; itemId?: number; purse: number; to?: number; declaration?: Declaration }

export default function GiftsTable() {
  const { matchId } = useParams()
  const { setGold } = useAuth()
  const navigate = useNavigate()
  const [data, setData] = useState<GiftsResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [pick, setPick] = useState<Pick | null>(null)
  const [draft, setDraft] = useState<GiftDraft>({ kind: 'FreeBread', purse: 1 })
  const [discard, setDiscard] = useState<number[]>([])
  const seenEvents = useRef(0)

  const load = useCallback((r: GiftsResponse) => {
    setData(r)
    setGold(r.profileGold)
    setPick(null)
    setDiscard([])
  }, [setGold])

  useEffect(() => {
    api<GiftsResponse>(`/api/gifts/matches/${matchId}`).then(load).catch((e) => setError(e instanceof Error ? e.message : 'The table was not found.'))
  }, [matchId, load])

  // a fresh gift draft every candle
  const candle = data?.match.candle
  useEffect(() => {
    setDraft({ kind: 'FreeBread', purse: 1 })
  }, [candle])

  const act = useCallback(async (action: GiftsAction) => {
    setBusy(true)
    setError(null)
    try {
      load(await api<GiftsResponse>(`/api/gifts/matches/${matchId}/act`, { method: 'POST', body: action }))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'The host shakes his head.')
    } finally {
      setBusy(false)
    }
  }, [matchId, load])

  if (!data)
    return (
      <div className="screen screen--center">
        {error ? <><p className="form-error">{error}</p><Link className="btn" to="/">Back to the hall</Link></> : <p className="loading">The host is setting the table…</p>}
      </div>
    )

  const m = data.match
  if (!m.introDone) return <Arrival match={m} onDone={load} />

  const me = m.players.find((p) => p.isMe)!
  const others = m.players.filter((p) => !p.isMe)
  const newFrom = seenEvents.current
  seenEvents.current = m.events.length

  const pickable = (s: SeatView) => pick !== null && s.alive && !s.isMe

  function onSeatClick(s: SeatView) {
    if (!pickable(s) || !pick) return
    switch (pick.purpose) {
      case 'gift': setDraft((d) => ({ ...d, to: s.seat })); setPick(null); break
      case 'monocle': act({ type: 'monocle', itemId: pick.itemId, target: s.seat }); break
      case 'service': act({ type: 'buyService', service: pick.service.kind, target: s.seat }); break
      case 'point': act({ type: 'point', target: s.seat }); break
      case 'ghost': act({ type: 'ghost', target: s.seat }); break
    }
  }

  const dyingNow = m.phase === 'Dawn' || m.phase === 'Finished' || m.phase === 'Pointing' ? (m.dawn?.candle === m.candle ? m.dawn.died : []) : []

  return (
    <div className={`tavern tavern--enter gifts gifts--${m.phase.toLowerCase()}`}>
      {/* Left — everything the table saw */}
      <aside className="panel panel--log">
        <h2 className="panel__title">Journal</h2>
        <Journal events={m.events} newFrom={newFrom} players={m.players} />
      </aside>

      <section className="stage">
        <div className="stage__wall" />
        <div className="stage__glow" />
        <Embers count={12} />

        <header className="stage__bar">
          <Link className="link" to="/">← The hall</Link>
          <span>
            Candle {Math.min(m.candle, m.totalCandles)} of {m.totalCandles} · <strong>{phaseName[m.phase]}</strong>
          </span>
          <span className="stage__stats" title="Deaths so far / deaths the Assassin needs">
            ☠ {m.deaths}/{m.quota} · <span title="Left in the host's chest">🧿{m.chest.amulets} ⚗{m.chest.poisons} 🪞{m.chest.mirrors}</span>
          </span>
        </header>

        <div className="host">
          <Portrait hue={m.host.portraitHue} host code={m.host.code} hasImage={m.host.hasPortrait} />
          <div className="nameplate nameplate--host">
            <strong>{m.host.name}</strong>
            <span>The host</span>
          </div>
          <Candles total={m.totalCandles} round={m.phase === 'Finished' ? m.totalCandles + 1 : m.candle} />
        </div>

        <div className="table">
          <div className="table__surface" />
        </div>

        {others.map((o, i) => (
          <Seat
            key={o.seat}
            seat={o}
            slot={seatSlots[i]}
            players={m.players}
            pickable={pickable(o)}
            dying={dyingNow.includes(o.seat)}
            showDelta={m.phase === 'Dawn' || m.phase === 'Discard'}
            draftTarget={draft.to === o.seat && m.phase === 'Gifts'}
            onClick={() => onSeatClick(o)}
          />
        ))}

        <div className="action">
          {error && <p className="form-error action__error">{error}</p>}
          <ActionPanel
            m={m}
            me={me}
            payout={data.payout}
            busy={busy}
            pick={pick}
            setPick={setPick}
            draft={draft}
            setDraft={setDraft}
            discard={discard}
            setDiscard={setDiscard}
            act={act}
            onNewEvening={async () => {
              const r = await api<GiftsResponse>('/api/gifts/matches', { method: 'POST' })
              setGold(r.profileGold)
              seenEvents.current = 0
              navigate(`/gifts/${r.match.id}`)
            }}
          />
        </div>

        {/* The player — first person: only hands, purse and role are visible */}
        <div className="me">
          <div className={`role-card role-card--${m.me.role.toLowerCase()}`}>
            <span className="role-card__label">Your role</span>
            <strong className="role-card__name">{roleInfo[m.me.role].name}</strong>
            <p>{roleInfo[m.me.role].goal}</p>
            {m.me.poisoned && <p className="role-card__warn">⚗ Poison is in your blood. Without an amulet you die at the next dawn.</p>}
          </div>

          <div className={`hand${m.me.hand.length > 5 ? ' hand--crowded' : ''}${m.me.alive ? '' : ' hand--dead'}`}>
            {m.me.hand.length === 0 && <span className="hand__empty">Your hands are empty.</span>}
            {m.me.hand.map((c, i) => {
              const offset = i - (m.me.hand.length - 1) / 2
              const selected = (m.phase === 'Gifts' && draft.kind === 'Card' && draft.itemId === c.id) || discard.includes(c.id)
              return (
                <ItemCard
                  key={c.id}
                  item={c}
                  selected={selected}
                  style={{ '--rot': `${offset * 6}deg`, '--lift': `${Math.abs(offset) * 8}px` } as CSSProperties}
                  onClick={() => {
                    if (m.phase === 'Gifts' && m.me.alive && !m.me.giftSent)
                      setDraft((d) => ({ ...d, kind: 'Card', itemId: c.id, declaration: honestDeclaration[c.type] ?? d.declaration }))
                    if (m.phase === 'Discard')
                      setDiscard((ds) => (ds.includes(c.id) ? ds.filter((x) => x !== c.id) : [...ds, c.id]))
                  }}
                />
              )
            })}
          </div>

          <div className="mypurse" title="Gold on your table">
            <span className="mypurse__coins">🪙</span>
            <strong>{m.me.gold}</strong>
            <span>gold</span>
          </div>
        </div>

        {!m.me.alive && m.phase !== 'Finished' && <div className="ghost-veil" aria-hidden="true" />}
      </section>

      {/* Right — what only you know */}
      <aside className="panel panel--whisper">
        <h2 className="panel__title">Whispers</h2>
        <ul className="whispers">
          {[...m.notes].reverse().map((n, i) => (
            <li key={`${n.candle}-${m.notes.length - i}`} className={n.candle === m.candle ? 'whispers__now' : ''}>
              <span className="whispers__from">Candle {n.candle}</span>
              {n.text}
            </li>
          ))}
        </ul>
      </aside>
    </div>
  )
}

/* ================================================================== the action panel */

interface PanelProps {
  m: GiftsView
  me: SeatView
  payout: number | null
  busy: boolean
  pick: Pick | null
  setPick: (p: Pick | null) => void
  draft: GiftDraft
  setDraft: Dispatch<SetStateAction<GiftDraft>>
  discard: number[]
  setDiscard: (d: number[]) => void
  act: (a: GiftsAction) => void
  onNewEvening: () => void
}

function ActionPanel(p: PanelProps) {
  const { m } = p
  switch (m.phase) {
    case 'Whisper': return <WhisperPanel {...p} />
    case 'Gifts': return m.me.alive ? <GiftPanel {...p} /> : <GhostPanel {...p} />
    case 'Delivery': return <DeliveryPanel {...p} />
    case 'WineChoice': return <WinePanel {...p} />
    case 'LastWhisper': return <LastWhisperPanel {...p} />
    case 'Dawn': return <DawnPanel {...p} />
    case 'Discard': return <DiscardPanel {...p} />
    case 'Pointing': return <PointPanel {...p} />
    case 'Finished': return <ResultPanel {...p} />
  }
}

function Box({ title, children, wide }: { title: string; children: ReactNode; wide?: boolean }) {
  return (
    <div className={wide ? 'box box--wide' : 'box'}>
      <h3 className="box__title">{title}</h3>
      {children}
    </div>
  )
}

function WhisperPanel({ m, busy, pick, setPick, act }: PanelProps) {
  const [showServices, setShowServices] = useState(false)
  const [giftForService, setGiftForService] = useState<number | null>(null)
  const monocle = m.me.hand.find((i) => i.type === 'Monocle')

  return (
    <Box title="The host leans in…">
      {m.offers.length === 0 && <p className="muted">He has nothing for you tonight. Not yet.</p>}
      {m.offers.map((o) => (
        <div key={o.id} className="offer">
          <q>{o.text}</q>
          <div className="offer__actions">
            <button className="btn btn--small btn--primary" disabled={busy || m.me.gold < o.price} onClick={() => act({ type: 'acceptOffer', offerId: o.id })}>
              Pay {o.price}
            </button>
            <button className="btn btn--small" disabled={busy} onClick={() => act({ type: 'declineOffer', offerId: o.id })}>No</button>
          </div>
        </div>
      ))}

      {monocle && (
        <button className="btn btn--small" disabled={busy} onClick={() => setPick(pick?.purpose === 'monocle' ? null : { purpose: 'monocle', itemId: monocle.id })}>
          {pick?.purpose === 'monocle' ? 'Choose whose hand to see…' : '🧐 Use the Monocle'}
        </button>
      )}

      <button className="link services-toggle" onClick={() => setShowServices((s) => !s)}>
        {showServices ? 'Hide the price list' : "The host's price list ▾"}
      </button>
      {showServices && (
        <ul className="services">
          {m.services.map((s) => (
            <li key={s.kind}>
              <span>{s.text}</span>
              {s.needsGift ? (
                <span className="services__gift">
                  <select value={giftForService ?? ''} onChange={(e) => setGiftForService(Number(e.target.value) || null)}>
                    <option value="">last night's gift…</option>
                    {m.myLastNightGifts.map((g) => <option key={g.id} value={g.id}>“{declarationName[g.declared]}” — {g.contents}</option>)}
                  </select>
                  <button className="btn btn--small" disabled={busy || !giftForService || m.me.gold < s.price} onClick={() => act({ type: 'buyService', service: s.kind, giftId: giftForService! })}>
                    {s.price} 🪙
                  </button>
                </span>
              ) : s.needsTarget ? (
                <button className="btn btn--small" disabled={busy || m.me.gold < s.price} onClick={() => setPick(pick?.purpose === 'service' && pick.service.kind === s.kind ? null : { purpose: 'service', service: s })}>
                  {pick?.purpose === 'service' && pick.service.kind === s.kind ? 'choose a player…' : `${s.price} 🪙`}
                </button>
              ) : (
                <button className="btn btn--small" disabled={busy || m.me.gold < s.price} onClick={() => act({ type: 'buyService', service: s.kind as OfferKind })}>
                  {s.price} 🪙
                </button>
              )}
            </li>
          ))}
        </ul>
      )}

      <button className="btn btn--primary btn--next" disabled={busy} onClick={() => act({ type: 'endWhisper' })}>
        Enough whispering →
      </button>
    </Box>
  )
}

function GiftPanel({ m, busy, pick, setPick, draft, setDraft, act }: PanelProps) {
  const card = draft.kind === 'Card' ? m.me.hand.find((i) => i.id === draft.itemId) : undefined
  const to = m.players.find((p) => p.seat === draft.to)
  const mustLie = card && !honestDeclaration[card.type]
  const ready = draft.to !== undefined && draft.declaration !== undefined && (draft.kind !== 'Card' || card) && (draft.kind !== 'Purse' || (draft.purse >= 1 && draft.purse <= m.me.gold))

  const what = draft.kind === 'Purse' ? `a purse of ${draft.purse} gold` : draft.kind === 'FreeBread' ? 'the host’s free bread' : card ? itemInfo[card.type].name : '…'
  const honest =
    draft.declaration &&
    ((draft.kind === 'Purse' && draft.declaration === 'Purse') || (draft.kind === 'FreeBread' && draft.declaration === 'Bread') || (card && honestDeclaration[card.type] === draft.declaration))

  if (m.me.giftSent) return <Box title="The gifts"><p className="muted">Your gift is with the host. The others are choosing theirs…</p></Box>

  return (
    <Box title="Send one gift" wide>
      <div className="gift-step">
        <span className="gift-step__n">1</span>
        <div>
          <strong>What?</strong> Click a card in your hand, or
          <div className="row">
            <button className={draft.kind === 'FreeBread' ? 'chipbtn chipbtn--on' : 'chipbtn'} onClick={() => setDraft((d) => ({ ...d, kind: 'FreeBread', itemId: undefined, declaration: d.declaration ?? 'Bread' }))}>🍞 Free bread</button>
            <button className={draft.kind === 'Purse' ? 'chipbtn chipbtn--on' : 'chipbtn'} onClick={() => setDraft((d) => ({ ...d, kind: 'Purse', itemId: undefined, declaration: 'Purse' }))} disabled={m.me.gold < 1}>🪙 A purse</button>
            {draft.kind === 'Purse' && (
              <input className="purse-input" type="number" min={1} max={m.me.gold} value={draft.purse} onChange={(e) => setDraft((d) => ({ ...d, purse: Number(e.target.value) }))} />
            )}
          </div>
        </div>
      </div>

      <div className="gift-step">
        <span className="gift-step__n">2</span>
        <div>
          <strong>To whom?</strong>{' '}
          <button className={pick?.purpose === 'gift' ? 'chipbtn chipbtn--on' : 'chipbtn'} onClick={() => setPick(pick?.purpose === 'gift' ? null : { purpose: 'gift' })}>
            {to ? to.name : pick?.purpose === 'gift' ? 'click a guest at the table…' : 'choose a guest'}
          </button>
        </div>
      </div>

      <div className="gift-step">
        <span className="gift-step__n">3</span>
        <div>
          <strong>Announce it as…</strong> {mustLie && <em className="lie-note">You cannot announce a {itemInfo[card!.type].name}. You must lie.</em>}
          <div className="row row--wrap">
            {declarations.map((d) => (
              <button key={d} className={draft.declaration === d ? 'chipbtn chipbtn--on' : 'chipbtn'} onClick={() => setDraft((x) => ({ ...x, declaration: d }))}>
                {declarationName[d]}
              </button>
            ))}
          </div>
        </div>
      </div>

      <p className="gift-summary">
        You send <strong>{what}</strong> to <strong>{to?.name ?? '…'}</strong>, announced as <strong>“{draft.declaration ? declarationName[draft.declaration] : '…'}”</strong>
        {draft.declaration && <span className={honest ? 'honest' : 'liar'}> — {honest ? 'honest' : 'a lie'}</span>}.
      </p>
      <button
        className="btn btn--primary btn--next"
        disabled={busy || !ready}
        onClick={() => act({ type: 'sendGift', kind: draft.kind, itemId: draft.itemId, purse: draft.kind === 'Purse' ? draft.purse : undefined, target: draft.to, declaration: draft.declaration })}
      >
        Hand it to the host →
      </button>
    </Box>
  )
}

function GhostPanel({ busy, pick, setPick, act }: PanelProps) {
  return (
    <Box title="You are a ghost">
      <p>The living cannot hear you. But each night you may point at one of them — everyone feels the cold breath.</p>
      <div className="row">
        <button className={pick?.purpose === 'ghost' ? 'chipbtn chipbtn--on' : 'chipbtn'} onClick={() => setPick(pick?.purpose === 'ghost' ? null : { purpose: 'ghost' })}>
          {pick?.purpose === 'ghost' ? 'click a living guest…' : '👉 Point at someone'}
        </button>
        <button className="btn btn--small" disabled={busy} onClick={() => act({ type: 'ghost' })}>Stay still →</button>
      </div>
    </Box>
  )
}

function DeliveryPanel({ m, busy, act }: PanelProps) {
  const pending = m.pending
  return (
    <Box title="The host delivers the gifts">
      <Announcements m={m} />
      {pending && (
        <div className="incoming">
          <p className="incoming__line">“{m.me.name}, someone sends you <strong>{declarationName[pending.declared]}</strong>.”</p>
          <div className="row">
            <button className="btn btn--primary" disabled={busy} onClick={() => act({ type: 'respond', response: 'Accept' })}>Take it</button>
            {pending.canCloseDoor && <button className="btn" disabled={busy} onClick={() => act({ type: 'respond', response: 'CloseDoor' })}>🚪 Close the door</button>}
            {pending.canMirror && <button className="btn" disabled={busy} onClick={() => act({ type: 'respond', response: 'Mirror' })}>🪞 Raise the mirror</button>}
          </div>
        </div>
      )}
    </Box>
  )
}

function WinePanel({ m, busy, act }: PanelProps) {
  return (
    <Box title="The wine loosens tongues">
      <p>Whose gift do you want to trace tonight? You will learn who sent it.</p>
      <ul className="trace">
        {m.tonightGifts.filter((g) => !g.sentByMe).map((g) => (
          <li key={g.id}>
            <button className="chipbtn" disabled={busy} onClick={() => act({ type: 'wine', giftId: g.id })}>
              {nameOf(m, g.to)} ← “{declarationName[g.declared]}” <Outcome o={g.outcome} />
            </button>
          </li>
        ))}
      </ul>
    </Box>
  )
}

function LastWhisperPanel({ m, busy, act }: PanelProps) {
  const o = m.offers.find((x) => x.emergency)
  if (!o) return <Box title="The last whisper"><p className="muted">…</p></Box>
  return (
    <Box title="The last whisper">
      <div className="offer offer--urgent">
        <q>{o.text}</q>
        <div className="offer__actions">
          <button className="btn btn--primary" disabled={busy || m.me.gold < o.price} onClick={() => act({ type: 'acceptOffer', offerId: o.id })}>Pay {o.price}</button>
          <button className="btn" disabled={busy} onClick={() => act({ type: 'declineOffer', offerId: o.id })}>Refuse</button>
        </div>
      </div>
    </Box>
  )
}

function DawnPanel({ m, busy, act }: PanelProps) {
  return (
    <Box title={`Dawn of the ${ordinal(m.candle)} candle`}>
      <DawnReport m={m} />
      <button className="btn btn--primary btn--next" disabled={busy} onClick={() => act({ type: 'seeDawn' })}>
        Light the next candle →
      </button>
    </Box>
  )
}

function DiscardPanel({ m, busy, discard, act }: PanelProps) {
  return (
    <Box title="Your hands are too full">
      <p>Discard {m.discardCount} card{m.discardCount > 1 ? 's' : ''} — click them in your hand. Daggers cannot be discarded, only given away.</p>
      <button className="btn btn--primary btn--next" disabled={busy || discard.length !== m.discardCount} onClick={() => act({ type: 'discard', itemIds: discard })}>
        Discard {discard.length}/{m.discardCount}
      </button>
    </Box>
  )
}

function PointPanel({ m, pick, setPick }: PanelProps) {
  return (
    <Box title="The sixth candle gutters out">
      <DawnReport m={m} />
      <p><strong>Who is the Assassin?</strong> Tell the host quietly. A correct guess is worth 5 gold.</p>
      <button className={pick?.purpose === 'point' ? 'chipbtn chipbtn--on' : 'chipbtn'} onClick={() => setPick(pick?.purpose === 'point' ? null : { purpose: 'point' })}>
        {pick?.purpose === 'point' ? 'click the guest you accuse…' : '👉 Name the Assassin'}
      </button>
    </Box>
  )
}

function ResultPanel({ m, payout, onNewEvening }: PanelProps) {
  const r = m.result!
  const assassins = m.players.filter((p) => p.role === 'Assassin')
  return (
    <Box title={r.winner === 'Guests' ? 'The guests win' : 'The Assassin wins'} wide>
      <p className={`verdict ${r.youWon ? 'verdict--won' : ''}`}>
        {r.youWon ? 'You won the evening.' : r.youSurvived ? 'You survived the night.' : 'Your candle went out tonight.'}{' '}
        The Assassin was <strong>{assassins.map((a) => a.name).join(' and ')}</strong>.
        {r.slayerSeat !== null && <> {nameOf(m, r.slayerSeat)} killed the Assassin and was paid 5 gold.</>}
      </p>
      {r.scores.length > 0 && (
        <table className="scores">
          <thead><tr><th>Guest</th><th>Gold</th><th>Named the Assassin</th><th>Daggers</th><th>Score</th></tr></thead>
          <tbody>
            {r.scores.map((s) => (
              <tr key={s.seat} className={r.winnerSeats.includes(s.seat) ? 'scores__win' : ''}>
                <td>{nameOf(m, s.seat)}{r.winnerSeats.includes(s.seat) && ' 🏆'}</td>
                <td>{s.gold}</td>
                <td>{s.pointedRight ? '+5' : '—'}</td>
                <td>{s.daggerPenalty ? `−${s.daggerPenalty}` : '—'}</td>
                <td><strong>{s.score}</strong></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      <p className="payout">You took home <strong>{payout ?? 0} gold</strong> {r.youWon && '(including the host’s 10 gold reward)'}.</p>
      <div className="row">
        <Link className="btn" to="/">Back to the hall</Link>
        <button className="btn btn--primary" onClick={onNewEvening}>Another evening (10 gold)</button>
      </div>
    </Box>
  )
}

/* ================================================================== pieces */

function Announcements({ m }: { m: GiftsView }) {
  if (m.tonightGifts.length === 0) return null
  return (
    <ol className="announcements">
      {m.tonightGifts.map((g, i) => (
        <li key={g.id} style={{ animationDelay: `${Math.min(i, 8) * 0.25}s` }}>
          <span>“{nameOf(m, g.to)}, someone sends you {declarationName[g.declared]}.”</span>
          <Outcome o={g.outcome} />
          {g.sentByMe && <em className="mine"> (yours)</em>}
        </li>
      ))}
    </ol>
  )
}

function Outcome({ o }: { o: string }) {
  if (o === 'DoorClosed') return <span className="outcome outcome--door">🚪 door closed</span>
  if (o === 'Mirrored') return <span className="outcome outcome--mirror">🪞 mirrored</span>
  if (o === 'Accepted') return <span className="outcome">taken</span>
  return <span className="outcome outcome--pending">…</span>
}

function DawnReport({ m }: { m: GiftsView }) {
  const d = m.dawn
  if (!d) return null
  const deltas = Object.entries(d.goldDeltas).filter(([seat]) => Number(seat) !== m.me.seat || true)
  return (
    <div className="dawn">
      {d.died.length === 0 ? <p>Every candle still burns.</p> : d.died.map((s) => (
        <p key={s} className="dawn__death">🕯 {s === m.me.seat ? <strong>Your candle goes out.</strong> : <><strong>{nameOf(m, s)}</strong>’s candle goes out.</>}</p>
      ))}
      {d.amuletsBroken > 0 && <p>🧿 {d.amuletsBroken === 1 ? 'An amulet broke' : `${d.amuletsBroken} amulets broke`} in the night.</p>}
      {deltas.length > 0 && (
        <p className="dawn__gold">
          {deltas.map(([seat, delta]) => (
            <span key={seat} className={delta > 0 ? 'gain' : 'loss'}>{nameOf(m, Number(seat))} {delta > 0 ? `+${delta}` : delta}</span>
          ))}
        </p>
      )}
      {d.richestSeat !== null && <p className="muted">The heaviest purse: {nameOf(m, d.richestSeat)}.</p>}
      <p className="muted">Dead {m.deaths} of the {m.quota} the Assassin needs.</p>
    </div>
  )
}

function Journal({ events, newFrom, players }: { events: GiftsView['events']; newFrom: number; players: SeatView[] }) {
  // Scroll only the journal itself — scrollIntoView would also scroll the table scene.
  const list = useRef<HTMLOListElement>(null)
  useEffect(() => {
    const el = list.current
    if (el) el.scrollTop = el.scrollHeight
  }, [events.length])
  let lastCandle = 0
  return (
    <ol className="log" ref={list}>
      {events.map((e, i) => {
        const header = e.candle !== lastCandle
        lastCandle = e.candle
        const isNew = i >= newFrom && newFrom > 0
        return (
          <li
            key={i}
            className={`log__item log__item--${e.kind}${isNew ? ' log__item--new' : ''}`}
            style={isNew ? { animationDelay: `${Math.min(i - newFrom, 12) * 0.35}s` } : undefined}
          >
            {header && <span className="log__round">Candle {e.candle}</span>}
            {e.kind === 'line' && e.seat !== null ? <q>{players.find((p) => p.seat === e.seat)?.name}: {e.text}</q> : e.text}
          </li>
        )
      })}
    </ol>
  )
}

function Seat({ seat, slot, players, pickable, dying, showDelta, draftTarget, onClick }: {
  seat: SeatView; slot: string; players: SeatView[]; pickable: boolean; dying: boolean; showDelta: boolean; draftTarget: boolean; onClick: () => void
}) {
  const pointsAt = seat.ghostPoint !== null ? players.find((p) => p.seat === seat.ghostPoint)?.name : null
  const cls = ['seat', slot, seat.alive ? '' : 'seat--ghost', dying ? 'seat--dying' : '', pickable ? 'seat--pickable' : '', draftTarget ? 'seat--target' : ''].join(' ')
  return (
    <div className={cls} onClick={onClick} role={pickable ? 'button' : undefined} tabIndex={pickable ? 0 : undefined}>
      <div className="seat__portrait">
        <Portrait hue={seat.portraitHue} code={seat.code} hasImage={seat.hasPortrait} />
        {!seat.alive && <span className="seat__crack" aria-hidden="true" />}
      </div>
      <div className="nameplate">
        <strong>{seat.name}</strong>
        <span>{seat.title}</span>
      </div>
      <div className="seat__info">
        {seat.alive ? (
          <>
            <span className="chip chip--gold" title="Gold on the table">🪙 {seat.gold}</span>
            {showDelta && seat.lastDelta !== 0 && <span className={`chip ${seat.lastDelta > 0 ? 'gain' : 'loss'}`}>{seat.lastDelta > 0 ? `+${seat.lastDelta}` : seat.lastDelta}</span>}
            <span className="chip" title="Cards in hand">🂠 {seat.handCount}</span>
          </>
        ) : (
          <span className="chip chip--dead">{seat.deathCause === 'Poison' ? '⚗ poisoned' : '† stabbed'} · {seat.role === 'Assassin' ? 'the Assassin' : 'a Guest'}</span>
        )}
        {seat.role && seat.alive && <span className={`chip chip--role chip--${seat.role.toLowerCase()}`}>{seat.role === 'Assassin' ? 'Assassin' : 'Guest'}</span>}
      </div>
      {seat.revealedHand && seat.revealedHand.length > 0 && (
        <div className="seat__hand" title="Their hand">{seat.revealedHand.map((t, i) => <span key={i}>{itemInfo[t].glyph}</span>)}</div>
      )}
      {pointsAt && <div className="seat__point">👉 points at {pointsAt}</div>}
    </div>
  )
}

function ItemCard({ item, selected, style, onClick }: { item: Item; selected: boolean; style?: CSSProperties; onClick: () => void }) {
  const info = itemInfo[item.type]
  return (
    <button className={`card card--hand card--${item.type.toLowerCase()}${selected ? ' card--selected' : ''}${info.chest ? ' card--chest' : ''}`} style={style} onClick={onClick} title={info.text}>
      <span className="card__corner"><span className="card__glyph">{info.glyph}</span></span>
      <span className="card__center"><span className="card__big">{info.glyph}</span></span>
      <span className="card__name">{info.name}</span>
      <span className="card__text">{info.text}</span>
    </button>
  )
}

function nameOf(m: GiftsView, seat: number) {
  const p = m.players.find((x) => x.seat === seat)
  return p ? (p.isMe ? 'You' : p.name) : '?'
}

function ordinal(n: number) {
  return ['first', 'second', 'third', 'fourth', 'fifth', 'sixth'][n - 1] ?? `${n}th`
}

