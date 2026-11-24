// Mirrors Blackhood.Engine.MatchView (enums are serialized as strings).

export type CardType = 'Gold' | 'Dagger' | 'Shield' | 'Eye' | 'Hand' | 'Seal' | 'Devil'
export type RoleType = 'Assassin' | 'Inquisitor' | 'Thief' | 'Merchant' | 'Priest' | 'Sold' | 'Fool'
export type Phase = 'Draw' | 'Table' | 'Night' | 'Resolution' | 'Whisper' | 'Court' | 'Finished'

export interface Card {
  id: number
  type: CardType
  value: number
}

export interface GuestIntro {
  hostIntroduction: string
  line: string
  direction: string | null
  gesture: string
}

export interface OpponentView {
  seat: number
  code: string
  name: string
  title: string
  portraitHue: number
  hasPortrait: boolean
  metBefore: boolean
  cardCount: number
  takenFromMarket: CardType[]
  isAlive: boolean
  revealedRole: RoleType | null
  intro: GuestIntro | null
}

export interface HostView {
  code: string
  name: string
  title: string
  portraitHue: number
  hasPortrait: boolean
  metBefore: boolean
  intro: { greeting: string; replyWho: string; replyBegin: string; replySilent: string; roleDeal: string }
}

export interface MatchView {
  id: string
  round: number
  totalCandles: number
  phase: Phase
  introDone: boolean
  host: HostView
  me: { seat: number; name: string; role: RoleType; hand: Card[]; hasSoul: boolean }
  opponents: OpponentView[]
  market: Card[]
  deckCount: number
  log: { round: number; text: string }[]
  whispers: string[]
}
