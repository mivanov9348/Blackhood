import type { CSSProperties } from 'react'
import { cardInfo, roleReading } from '../game/text'
import type { Card, RoleType } from '../game/types'

interface Props {
  card: Card
  /** The viewer's role — the card shows only what it does for this role. */
  role: RoleType
  size?: 'hand' | 'market'
  style?: CSSProperties
}

export function GameCard({ card, role, size = 'hand', style }: Props) {
  const info = cardInfo[card.type]
  const special = roleReading[role]?.[card.type]

  return (
    <div className={`card card--${size} card--${card.type.toLowerCase()}`} style={style}>
      <div className="card__corner">
        <span className="card__glyph">{info.glyph}</span>
        {card.type === 'Gold' && <span className="card__value">{card.value}</span>}
      </div>
      <div className="card__center">
        {card.type === 'Gold' ? <span className="card__big">{card.value}</span> : <span className="card__big">{info.glyph}</span>}
      </div>
      <div className="card__name">{info.name}</div>
      <div className={special ? 'card__text card__text--role' : 'card__text'}>{special ?? info.base}</div>
    </div>
  )
}

/** Face-down draw pile. No card back art yet — just a plain stack. */
export function DeckPile({ count }: { count: number }) {
  return (
    <div className="deck" title={`${count} карти в тестето`}>
      <div className="deck__layer" />
      <div className="deck__layer" />
      <div className="deck__layer deck__top">
        <span>Тесте</span>
        <strong>{count}</strong>
      </div>
    </div>
  )
}
