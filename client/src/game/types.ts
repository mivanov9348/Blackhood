// Mirrors Blackhood.Engine.Gifts.GiftsView (enums are serialized as strings).

export type ItemType = 'Dagger' | 'Wine' | 'ClosedDoor' | 'Monocle' | 'Bread' | 'Amulet' | 'Poison' | 'Mirror'
export type Declaration = 'Purse' | 'Bread' | 'Wine' | 'Monocle' | 'ClosedDoor' | 'Amulet' | 'Mirror'
export type Role = 'Guest' | 'Assassin'
export type Phase = 'Whisper' | 'Gifts' | 'Delivery' | 'WineChoice' | 'LastWhisper' | 'Dawn' | 'Discard' | 'Pointing' | 'Finished'
export type OfferKind = 'Amulet' | 'Poison' | 'Mirror' | 'Door' | 'Rumor' | 'LearnSender' | 'Diversion' | 'Peek'
export type GiftKind = 'Card' | 'Purse' | 'FreeBread'
export type GiftOutcome = 'Pending' | 'Accepted' | 'DoorClosed' | 'Mirrored'
export type DeliveryResponse = 'Accept' | 'CloseDoor' | 'Mirror'
export type Winner = 'None' | 'Guests' | 'Assassin'

export interface Item { id: number; type: ItemType }

export interface GuestIntro { hostIntroduction: string; line: string; direction: string | null; gesture: string }

export interface SeatView {
  seat: number
  code: string
  name: string
  title: string
  portraitHue: number
  hasPortrait: boolean
  metBefore: boolean
  isMe: boolean
  alive: boolean
  gold: number
  lastDelta: number
  handCount: number
  role: Role | null
  revealedHand: ItemType[] | null
  ghostPoint: number | null
  deathCause: 'Dagger' | 'Poison' | null
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

export interface GiftsEvent { candle: number; kind: string; text: string; seat: number | null; target: number | null }
export interface PrivateNote { candle: number; seat: number; text: string }
export interface OfferView { id: number; kind: OfferKind; price: number; text: string; emergency: boolean }
export interface ServiceView { kind: OfferKind; price: number; text: string; needsTarget: boolean; needsGift: boolean }
export interface AnnouncedGift { id: number; to: number; declared: Declaration; outcome: GiftOutcome; sentByMe: boolean }
export interface MyGift { id: number; declared: Declaration; outcome: GiftOutcome; contents: string }
export interface DawnReport { candle: number; died: number[]; amuletsBroken: number; richestSeat: number | null; goldDeltas: Record<string, number> }
export interface FinalScore { seat: number; gold: number; pointedRight: boolean; daggerPenalty: number; score: number }

export interface GiftsView {
  id: string
  candle: number
  totalCandles: number
  phase: Phase
  introDone: boolean
  quota: number
  deaths: number
  host: HostView
  me: { seat: number; name: string; role: Role; gold: number; hand: Item[]; alive: boolean; poisoned: boolean; giftSent: boolean; pendingWine: number; finalPoint: number | null }
  players: SeatView[]
  chest: { amulets: number; poisons: number; mirrors: number }
  deckCount: number
  offers: OfferView[]
  services: ServiceView[]
  events: GiftsEvent[]
  notes: PrivateNote[]
  pending: { giftId: number; declared: Declaration; canCloseDoor: boolean; canMirror: boolean } | null
  tonightGifts: AnnouncedGift[]
  myLastNightGifts: MyGift[]
  discardCount: number
  dawn: DawnReport | null
  result: { winner: Winner; winnerSeats: number[]; scores: FinalScore[]; slayerSeat: number | null; youWon: boolean; youSurvived: boolean } | null
}

export interface GiftsResponse { match: GiftsView; profileGold: number; payout: number | null }

export interface GiftsAction {
  type: string
  offerId?: number
  service?: OfferKind
  target?: number
  giftId?: number
  itemId?: number
  kind?: GiftKind
  declaration?: Declaration
  purse?: number
  response?: DeliveryResponse
  itemIds?: number[]
}
