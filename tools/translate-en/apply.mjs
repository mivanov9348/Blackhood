// Overlays the English text onto content/characters/*.json, keeping every number,
// code, enum and the structure. Reports any Cyrillic left behind.
//
//   node tools/translate-en/apply.mjs
import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import a from './en-p01-p07.mjs'
import b from './en-p08-p13.mjs'
import hosts from './en-hosts.mjs'

const here = path.dirname(fileURLToPath(import.meta.url))
const dir = path.resolve(here, '../../content/characters')
const en = { ...a, ...b, ...hosts }

// Characters whose sheet had no "Lying" tell get one, so the table can catch them in Gifts.
// (Bertram and Elias never lie — that is their character, so they get none.)
const extraTells = {
  P02: [{ state: 'Lying', description: 'Repeats your last word a beat too slowly', fakeable: true, visibleWithoutDossier: false }],
  P08: [{ state: 'Lying', description: 'Holds your gaze a moment too long', fakeable: true, visibleWithoutDossier: true }],
  P11: [{ state: 'Lying', description: 'Stammers worse than usual', fakeable: false, visibleWithoutDossier: true }],
  P12: [{ state: 'Lying', description: 'Complains about his poverty even louder', fakeable: true, visibleWithoutDossier: true }],
}

const sections = ['origin', 'appearance', 'backstory', 'psychology', 'behavior', 'motives', 'speech', 'mechanics', 'authorNotes', 'host']
const cyr = /[Ѐ-ӿ]/
let leftovers = 0

for (const file of fs.readdirSync(dir).filter((f) => f.endsWith('.json'))) {
  const doc = JSON.parse(fs.readFileSync(path.join(dir, file), 'utf8'))
  const t = en[doc.code]
  if (!t) throw new Error(`No English text for ${doc.code}`)

  for (const k of ['name', 'title', 'nickname', 'gender', 'age', 'archetype']) if (t[k] !== undefined) doc[k] = t[k]
  for (const s of sections) if (t[s] && doc[s]) Object.assign(doc[s], t[s])

  // lines: replace by situation; add any new ones (e.g. Death)
  for (const [situation, text] of Object.entries(t.lines ?? {})) {
    const line = doc.lines.find((l) => l.situation === situation)
    if (line) line.text = text
    else doc.lines.push({ situation, text })
  }
  for (const line of doc.lines) {
    const dir = t.directions?.[line.situation]
    if (dir) line.direction = dir
    else if (line.direction && cyr.test(line.direction)) delete line.direction
  }

  ;(t.tells ?? []).forEach((d, i) => { if (doc.tells[i]) doc.tells[i].description = d })
  for (const extra of extraTells[doc.code] ?? [])
    if (!doc.tells.some((x) => x.state === extra.state)) doc.tells.push(extra)

  ;(t.relations ?? []).forEach((r, i) => { if (doc.relations[i]) Object.assign(doc.relations[i], r) })

  // report leftovers
  const walk = (o, p) => {
    for (const [k, v] of Object.entries(o)) {
      if (typeof v === 'string' && cyr.test(v)) { leftovers++; console.warn(`${doc.code} ${p}${k}: ${v.slice(0, 60)}`) }
      else if (v && typeof v === 'object') walk(v, `${p}${k}.`)
    }
  }
  walk(doc, '')

  fs.writeFileSync(path.join(dir, file), JSON.stringify(doc, null, 2) + '\n', 'utf8')
}
console.log(leftovers === 0 ? 'All characters are now in English.' : `${leftovers} Bulgarian strings left.`)
