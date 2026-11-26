import type { Declaration, ItemType, Phase, Role } from './types'

export const itemInfo: Record<ItemType, { name: string; glyph: string; text: string; chest?: boolean }> = {
  Dagger: { name: 'Dagger', glyph: '†', text: "Sent by the Assassin, it kills at dawn. From anyone else it's harmless — but −2 gold at the end if you still hold it. Can't be discarded, only given away." },
  Wine: { name: 'Wine', glyph: '🍷', text: 'When you receive it, learn who sent any one gift tonight.' },
  ClosedDoor: { name: 'Closed Door', glyph: '🚪', text: 'Play when a gift is announced for you: it goes back to the sender unopened.' },
  Monocle: { name: 'Monocle', glyph: '🧐', text: "Play during the whisper: secretly see one player's hand." },
  Bread: { name: 'Bread', glyph: '🍞', text: 'Does nothing. An honest, empty gift.' },
  Amulet: { name: 'Amulet', glyph: '🧿', text: 'Held at dawn, it stops one death. Then the amulet and the weapon leave the game.', chest: true },
  Poison: { name: 'Poison', glyph: '⚗', text: 'The receiver dies at the NEXT dawn unless they hold an Amulet. Every death counts towards the Assassin.', chest: true },
  Mirror: { name: 'Mirror', glyph: '🪞', text: "Play when a gift is announced for you: its effect turns on the sender. The Assassin's dagger finds his own heart.", chest: true },
}

export const declarationName: Record<Declaration, string> = {
  Purse: 'a Purse',
  Bread: 'Bread',
  Wine: 'Wine',
  Monocle: 'a Monocle',
  ClosedDoor: 'a Closed Door',
  Amulet: 'an Amulet',
  Mirror: 'a Mirror',
}

export const declarations: Declaration[] = ['Purse', 'Bread', 'Wine', 'Monocle', 'ClosedDoor', 'Amulet', 'Mirror']

/** What you would honestly announce for a card (Dagger and Poison have no honest word). */
export const honestDeclaration: Partial<Record<ItemType, Declaration>> = {
  Bread: 'Bread', Wine: 'Wine', Monocle: 'Monocle', ClosedDoor: 'ClosedDoor', Amulet: 'Amulet', Mirror: 'Mirror',
}

export const roleInfo: Record<Role, { name: string; goal: string; hint: string }> = {
  Assassin: {
    name: 'The Assassin',
    goal: 'Let enough candles go out. Every death counts — however it happens.',
    hint: "Your daggers kill from the second candle. Disguise them as something pleasant. Beware of mirrors.",
  },
  Guest: {
    name: 'A Guest',
    goal: "Survive the six candles with fewer deaths than the Assassin needs. Then the living guest with the most gold is the guest of honour.",
    hint: 'Name the Assassin at the end for +5 gold. A mirror or poison that kills him pays +5 more.',
  },
}

export const phaseName: Record<Phase, string> = {
  Whisper: 'The whisper',
  Gifts: 'The gifts',
  Delivery: 'Delivery',
  WineChoice: 'Wine',
  LastWhisper: 'The last whisper',
  Dawn: 'Dawn',
  Discard: 'Dawn',
  Pointing: 'Name the Assassin',
  Finished: 'The end',
}
