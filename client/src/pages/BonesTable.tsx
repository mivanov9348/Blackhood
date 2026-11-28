import { useCallback, useEffect, useRef, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { Embers } from '../components/Embers'
import { Portrait } from '../components/Portrait'
import '../bones.css'

interface Bid { quantity: number; face: number; seat: number }
interface Seat { seat: number; code: string; name: string; title: string; portraitHue: number; hasPortrait: boolean; isMe: boolean; diceCount: number; out: boolean }
interface Reveal { round: number; bid: Bid; callerSeat: number; count: number; bidWasTrue: boolean; loserSeat: number; dice: Record<string, number[]> }
interface BonesView {
  id: string
  round: number
  phase: 'Bidding' | 'Reveal' | 'Finished'
  pot: number
  totalDice: number
  turnSeat: number
  currentBid: Bid | null
  roundBids: Bid[]
  myDice: number[]
  players: Seat[]
  lastReveal: Reveal | null
  events: { round: number; kind: string; text: string; seat: number | null }[]
  winnerSeat: number | null
  hintChance: number | null
}
interface BonesResponse { match: BonesView; profileGold: number; payout: number | null }

const pips = ['', '⚀', '⚁', '⚂', '⚃', '⚄', '⚅']
const faceWord = (f: number, q: number) => ['', q === 1 ? 'one' : 'ones', q === 1 ? 'two' : 'twos', q === 1 ? 'three' : 'threes', q === 1 ? 'four' : 'fours', q === 1 ? 'five' : 'fives', q === 1 ? 'six' : 'sixes'][f]

export default function BonesTable() {
  const { matchId } = useParams()
  const { setGold } = useAuth()
  const navigate = useNavigate()
  const [data, setData] = useState<BonesResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [quantity, setQuantity] = useState(1)
  const [face, setFace] = useState(2)
  const log = useRef<HTMLOListElement>(null)

  const load = useCallback((r: BonesResponse) => {
    setData(r)
    setGold(r.profileGold)
    // Suggest the smallest legal raise.
    const b = r.match.currentBid
    if (b) {
      if (b.face < 6) { setQuantity(b.quantity); setFace(b.face + 1) }
      else { setQuantity(Math.min(b.quantity + 1, r.match.totalDice)); setFace(2) }
    } else { setQuantity(1); setFace(2) }
  }, [setGold])

  useEffect(() => {
    api<BonesResponse>(`/api/bones/matches/${matchId}`).then(load).catch((e) => setError(e instanceof Error ? e.message : 'The table was not found.'))
  }, [matchId, load])

  useEffect(() => {
    const el = log.current
    if (el) el.scrollTop = el.scrollHeight
  }, [data?.match.events.length])

  const act = useCallback(async (body: object) => {
    setBusy(true)
    setError(null)
    try {
      load(await api<BonesResponse>(`/api/bones/matches/${matchId}/act`, { method: 'POST', body }))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'The dice refuse.')
    } finally {
      setBusy(false)
    }
  }, [matchId, load])

  if (!data)
    return (
      <div className="screen screen--center">
        {error ? <><p className="form-error">{error}</p><Link className="btn" to="/">Back to the hall</Link></> : <p className="loading">The cups are being shaken…</p>}
      </div>
    )

  const m = data.match
  const me = m.players.find((p) => p.isMe)!
  const others = m.players.filter((p) => !p.isMe)
  const myTurn = m.phase === 'Bidding' && m.turnSeat === me.seat && !me.out
  const name = (seat: number) => (seat === me.seat ? 'You' : m.players.find((p) => p.seat === seat)?.name ?? '?')
  const lastBidOf = (seat: number) => [...m.roundBids].reverse().find((b) => b.seat === seat)
  const b = m.currentBid
  const legal = !b || quantity > b.quantity || (quantity === b.quantity && face > b.face)

  return (
    <div className="bones">
      <div className="bones__fire" />
      <Embers count={22} />
      <div className="vignette" />

      <header className="bones__bar">
        <Link className="link" to="/">← The hall</Link>
        <span><strong>Bones</strong> · round {m.round}</span>
        <span>🪙 pot {m.pot} · 🎲 {m.totalDice} dice on the table</span>
      </header>

      <div className="bones__layout">
        <aside className="bones__log">
          <h2 className="panel__title">By the fire</h2>
          <ol ref={log}>
            {m.events.map((e, i) => <li key={i} className={`bl bl--${e.kind}`}>{e.text}</li>)}
          </ol>
        </aside>

        <main className="bones__table">
          <div className="bones__seats">
            {others.map((p) => {
              const last = lastBidOf(p.seat)
              return (
                <div key={p.seat} className={`bseat${m.turnSeat === p.seat && m.phase === 'Bidding' ? ' bseat--turn' : ''}${p.out ? ' bseat--out' : ''}`}>
                  <Portrait hue={p.portraitHue} code={p.code} hasImage={p.hasPortrait} />
                  <strong>{p.name}</strong>
                  <span className="bseat__title">{p.title}</span>
                  <span className="bseat__cup" title="Dice under the cup">{p.out ? 'out' : '🥃 ' + '▪'.repeat(p.diceCount)}</span>
                  {last && m.phase === 'Bidding' && <span className="bseat__bid">“{last.quantity} {faceWord(last.face, last.quantity)}”</span>}
                </div>
              )
            })}
          </div>

          <div className="bones__center">
            {m.phase === 'Bidding' && (
              <div className="bid-now">
                {b ? (
                  <>
                    <span className="bid-now__who">{name(b.seat)} {b.seat === me.seat ? 'bid' : 'bids'}</span>
                    <span className="bid-now__bid">{b.quantity} × <span className={b.face === 1 ? 'pip pip--wild' : 'pip'}>{pips[b.face]}</span></span>
                    <span className="bid-now__words">{b.quantity} {faceWord(b.face, b.quantity)}</span>
                  </>
                ) : (
                  <span className="bid-now__who">No bid yet. {name(m.turnSeat)} {m.turnSeat === me.seat ? 'open' : 'opens'}.</span>
                )}
                {!myTurn && !me.out && <span className="bid-now__wait">{name(m.turnSeat)} is thinking…</span>}
              </div>
            )}

            {(m.phase === 'Reveal' || m.phase === 'Finished') && m.lastReveal && <RevealPanel r={m.lastReveal} name={name} />}

            {m.phase === 'Reveal' && (
              <button className="btn btn--primary" disabled={busy} onClick={() => act({ type: 'next' })}>
                {me.out ? 'Let them finish →' : 'Next round →'}
              </button>
            )}

            {m.phase === 'Finished' && (
              <div className="bones__result">
                <h2>{m.winnerSeat === me.seat ? 'You take the pot!' : `${name(m.winnerSeat!)} takes the pot.`}</h2>
                <p>{m.winnerSeat === me.seat ? `+${data.payout ?? m.pot} gold to your purse.` : 'Your 5 gold stays on the table.'}</p>
                <div className="row">
                  <Link className="btn" to="/">Back to the hall</Link>
                  <button className="btn btn--primary" onClick={async () => {
                    const r = await api<BonesResponse>('/api/bones/matches', { method: 'POST' })
                    setGold(r.profileGold)
                    navigate(`/bones/${r.match.id}`)
                  }}>Another game (5 gold)</button>
                </div>
              </div>
            )}

            {error && <p className="form-error">{error}</p>}
          </div>

          <div className="bones__me">
            <div className="my-dice" aria-label="Your dice">
              {me.out ? <span className="muted">You are out of dice. Watch the others.</span> : m.myDice.map((d, i) => (
                <span key={i} className={d === 1 ? 'die die--wild' : 'die'} title={d === 1 ? "The Witch's eye — counts as any face" : `${d}`}>{pips[d]}</span>
              ))}
            </div>

            {myTurn && (
              <div className="bid-controls">
                <div className="stepper">
                  <button className="chipbtn" onClick={() => setQuantity((q) => Math.max(1, q - 1))}>−</button>
                  <strong>{quantity}</strong>
                  <button className="chipbtn" onClick={() => setQuantity((q) => Math.min(m.totalDice, q + 1))}>+</button>
                </div>
                <span className="times">×</span>
                <div className="faces">
                  {[1, 2, 3, 4, 5, 6].map((f) => (
                    <button key={f} className={`facebtn${face === f ? ' facebtn--on' : ''}${f === 1 ? ' facebtn--wild' : ''}`} onClick={() => setFace(f)} title={f === 1 ? "Ones (the Witch's eye) — a bid on ones counts only ones" : faceWord(f, 2)}>
                      {pips[f]}
                    </button>
                  ))}
                </div>
                <button className="btn btn--primary" disabled={busy || !legal} onClick={() => act({ type: 'bid', quantity, face })}>
                  Bid {quantity} {faceWord(face, quantity)}
                </button>
                {b && (
                  <button className="btn btn--liar" disabled={busy} onClick={() => act({ type: 'liar' })}>
                    “Liar!”
                  </button>
                )}
                {m.hintChance !== null && <span className="hint-odds">By the numbers, the current bid is true {Math.round(m.hintChance * 100)}% of the time.</span>}
              </div>
            )}
          </div>
        </main>
      </div>
    </div>
  )
}

function RevealPanel({ r, name }: { r: Reveal; name: (s: number) => string }) {
  const matches = (d: number) => d === r.bid.face || (r.bid.face !== 1 && d === 1)
  return (
    <div className="reveal">
      <p className="reveal__line">
        {name(r.callerSeat)} called {name(r.bid.seat)}’s bid of <strong>{r.bid.quantity} {faceWord(r.bid.face, r.bid.quantity)}</strong>.
      </p>
      <div className="reveal__cups">
        {Object.entries(r.dice).map(([seat, dice]) => (
          <div key={seat} className="reveal__cup">
            <span>{name(Number(seat))}</span>
            <div>{dice.map((d, i) => <span key={i} className={matches(d) ? 'die die--small die--hit' : 'die die--small'}>{pips[d]}</span>)}</div>
          </div>
        ))}
      </div>
      <p className={r.bidWasTrue ? 'reveal__verdict reveal__verdict--true' : 'reveal__verdict reveal__verdict--lie'}>
        {r.count} on the table — {r.bidWasTrue ? 'the bid was true' : 'it was a lie'}. {name(r.loserSeat)} {r.loserSeat === r.callerSeat && name(r.loserSeat) === 'You' ? 'lose' : name(r.loserSeat) === 'You' ? 'lose' : 'loses'} a die.
      </p>
    </div>
  )
}
