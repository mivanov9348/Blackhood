// Converts the extracted text of "Devils_Seat_NPC_Profili.docx" into
// content/characters/<CODE>.json, merged with authored additions from additions.mjs.
//
// Usage:
//   node tools/import-sheet/parse-sheet.mjs <extracted.txt> [--dry]
//
// The .txt is the docx body with one paragraph per line (see README in this folder).

import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { additions } from './additions.mjs'

const here = path.dirname(fileURLToPath(import.meta.url))
const outDir = path.resolve(here, '../../content/characters')
const [, , input, flag] = process.argv
if (!input) {
  console.error('usage: node parse-sheet.mjs <extracted.txt> [--dry]')
  process.exit(1)
}

const lines = fs.readFileSync(input, 'utf8').replace(/^﻿/, '').split(/\r?\n/).map((l) => l.trim())

// ---- label → target path ------------------------------------------------------------
const fields = {
  'Име': 'rawName',
  'Прякор / титла': 'nickname',
  'Пол': 'gender',
  'Възраст': 'age',
  'Произход / родно място': 'origin.birthplace',
  'Обществено положение': 'origin.socialStatus',
  'Професия / минало': 'origin.profession',
  'Архетип': 'archetype',
  'Рядкост (Common / Uncommon / Rare / Unique)': 'rarity',

  'Лице и отличителни черти': 'appearance.face',
  'Коса / брада / очи': 'appearance.hairAndEyes',
  'Облекло и аксесоари': 'appearance.clothing',
  'Телосложение и стойка': 'appearance.build',
  'Жестове и навици': 'appearance.gestures',
  'Глас / начин на говорене': 'appearance.voice',
  'Първо впечатление': 'appearance.firstImpression',
  'Допълнителни визуални бележки:': 'appearance.artistNotes',

  'Кратка биография (3–6 изречения):': 'backstory.biography',
  'Какво го довежда в страноприемницата?': 'backstory.whyHere',
  'Какво крие от останалите?': 'backstory.secret',

  'Основна черта на характера': 'psychology.coreTrait',
  'Втора характерна черта': 'psychology.secondTrait',
  'Най-голяма слабост': 'psychology.weakness',
  'Най-голям страх': 'psychology.fear',
  'Най-силно желание': 'psychology.desire',
  'Какво го разгневява?': 'psychology.angers',
  'Какво предизвиква съжаление / вина?': 'psychology.regret',
  'Морални граници': 'psychology.moralLimits',
  'Какво никога не би направил?': 'psychology.neverWould',
  'Какво би направил за достатъчно голяма награда?': 'psychology.wouldForReward',

  'Insight – Проницателност': 'n.insight',
  'Cunning – Хитрост': 'n.cunning',
  'Composure – Самообладание': 'n.composure',
  'Boldness – Дързост': 'n.boldness',
  'Greed – Алчност': 'n.greed',
  'Charisma – Влияние': 'n.charisma',
  'Temper – Избухливост': 'n.temper',
  'Patience – Търпение': 'n.patience',
  'Loyalty – Лоялност': 'n.loyalty',
  'Revenge – Отмъстителност': 'n.revenge',
  'Ambition – Амбиция': 'n.ambition',
  'Fear of Loss – Страх от загуба': 'n.fearOfLoss',
  'Risk Tolerance – Толерантност към риск': 'n.riskTolerance',

  'Предпочитан стил (агресивен / защитен / манипулативен / пресметлив / социален / хаотичен)': 'behavior.style',
  'Отношение към залозите': 'behavior.stakes',
  'Отношение към измамата': 'behavior.deception',
  'Отношение към дълговете': 'behavior.debts',
  'Отношение към съюзите': 'behavior.alliances',
  'Кога атакува?': 'behavior.whenAttacks',
  'Кога се оттегля?': 'behavior.whenRetreats',
  'Как реагира при загуба?': 'behavior.onLoss',
  'Как реагира при победа?': 'behavior.onWin',
  'Какво го кара да промени стратегията си?': 'behavior.strategyChange',

  'Публична цел': 'motives.publicGoal',
  'Истинска / скрита цел': 'motives.hiddenGoal',
  'Лична причина да участва': 'motives.personalReason',
  'Какво би го накарало да предаде останалите?': 'motives.betrayalTrigger',
  'Какво би го накарало да помогне на друг?': 'motives.helpTrigger',

  'Начин на говорене / речник': 'speech.register',
  'Любими думи / обръщения': 'speech.favoriteWords',
  'Теми, които избягва': 'speech.avoidedTopics',
  'Как лъже?': 'speech.howLies',
  'Как показва гняв или страх?': 'speech.angerAndFear',

  'При първа среща:': 'line.FirstMeeting',
  'При победа:': 'line.Win',
  'При загуба:': 'line.Loss',
  'При обвинение:': 'line.Accused',
  'При предложение за съюз:': 'line.AllianceOffer',
  'При предателство:': 'line.Betrayal',
  'При голям залог:': 'line.BigBet',

  'Какво прави персонажа интересен и различим?': 'authorNotes.distinctive',
  'Кои негови черти не трябва да се променят?': 'authorNotes.immutable',
  'Какво подлежи на редакция / баланс?': 'authorNotes.tunable',

  'Как се представя пред гостите?': 'host.presentation',
  'Каква е истинската му природа?': 'host.trueNature',
  'Какво иска от участниците?': 'host.wantsFromGuests',
  'Какво никога не разкрива?': 'host.neverReveals',
  'Как се отнася към правилата?': 'host.rulesAttitude',
  'Как наказва нарушителите?': 'host.punishment',
  'Какви изкушения предлага?': 'host.temptations',
  'Каква цена изисква?': 'host.price',
  'Какво го забавлява?': 'host.amused',
  'Какво го ядосва?': 'host.angered',
  'Какво би го накарало да наруши собствените си правила?': 'host.wouldBreakRules',
  'Характерен начин на водене на партията:': 'host.hostingStyle',
  'Примерни оферти / изкушения:': 'host.sampleOffers',
  'Истинска история / тайна на домакина:': 'host.trueStory',
}

// Labels may use го/я/ѝ depending on the character's gender — normalise to "го".
const norm = (s) => s.replace(/\b(я|ѝ|й)\b/g, 'го')
const labelMap = new Map(Object.entries(fields).map(([k, v]) => [norm(k), v]))
const ignored = new Set(['ID / код на персонажа', 'Тип персонаж (участник / домакин)', 'Име / титла на домакина', 'Атрибут', 'Оценка', 'Как се проявява', 'Параметър', 'Значение'].map(norm))

// ---- split into blocks ----------------------------------------------------------------
const blocks = []
for (let i = 0; i < lines.length; i++) {
  const m = /^([PH]\d{2}) · (.+)$/.exec(lines[i])
  if (m && !lines[i].includes(' – ') && lines[i + 1]?.includes(' · ')) {
    blocks.push({ code: m[1], heading: m[2], sub: lines[i + 1], start: i + 2 })
  }
}
blocks.forEach((b, i) => (b.end = blocks[i + 1]?.start - 2 ?? lines.length))
blocks.at(-1).end = lines.length

const set = (obj, p, v) => {
  const parts = p.split('.')
  let o = obj
  for (const k of parts.slice(0, -1)) o = o[k] ??= {}
  o[parts.at(-1)] = v
}

function parseLine(raw) {
  // „Text.“ (direction)   |   „Text.“ After that, something   |   Text without quotes
  const m = /„([^“]+)“\s*(.*)$/.exec(raw)
  if (!m) return { text: raw }
  let direction = m[2].trim().replace(/^\((.*)\)$/, '$1').trim()
  return direction ? { text: m[1].trim(), direction } : { text: m[1].trim() }
}

const unknown = new Map()
const docs = []

for (const b of blocks) {
  const raw = { code: b.code, heading: b.heading, sub: b.sub, lines: {} }
  for (let i = b.start; i < b.end; i++) {
    const label = norm(lines[i])
    const target = labelMap.get(label)
    if (!target) {
      if (/[?:]$/.test(lines[i]) && !ignored.has(label) && lines[i].length < 120) unknown.set(lines[i], (unknown.get(lines[i]) ?? 0) + 1)
      continue
    }
    // value = next non-empty line that is not itself a label
    let j = i + 1
    while (j < b.end && lines[j] === '') j++
    const value = lines[j] ?? ''
    if (labelMap.has(norm(value))) continue
    if (target.startsWith('line.')) raw.lines[target.slice(5)] = parseLine(value)
    else set(raw, target, value)
  }
  docs.push(raw)
}

if (unknown.size) {
  console.warn('Unmapped label-like lines:')
  for (const [k, n] of unknown) console.warn(`  ${n}× ${k}`)
}

// ---- build the CharacterDocument -----------------------------------------------------
const clamp = (v) => Math.max(1, Math.min(5, Math.round(v)))
const stripPrefix = (s = '', ...prefixes) => prefixes.reduce((acc, p) => (acc.startsWith(p) ? acc.slice(p.length).trim() : acc), s)

const out = []
for (const r of docs) {
  const add = additions[r.code]
  if (!add) throw new Error(`No authored additions for ${r.code}`)
  const isHost = r.code.startsWith('H')
  const n = r.n ?? {}
  const need = ['insight', 'cunning', 'composure', 'boldness', 'greed', 'charisma', 'patience', 'loyalty', 'revenge', 'ambition', 'fearOfLoss', 'riskTolerance']
  for (const k of need) if (!(Number(n[k]) >= 1)) throw new Error(`${r.code}: missing number ${k}`)
  const num = Object.fromEntries(Object.entries(n).map(([k, v]) => [k, Number(v)]))

  const sheetLines = Object.entries(r.lines).map(([situation, l]) => ({ situation, ...l }))

  const doc = {
    code: r.code,
    kind: isHost ? 'Host' : 'Participant',
    name: r.heading.trim(),
    title: r.sub.split(' · ')[0].trim(),
    nickname: r.nickname ?? '',
    gender: r.gender ?? '',
    age: r.age ?? '',
    archetype: r.archetype ?? r.sub.split(' · ')[2]?.trim() ?? '',
    rarity: r.rarity ?? 'Common',
    status: 'Published',
    tags: add.tags ?? [],
    skills: {
      insight: num.insight,
      calculation: add.calculation,
      cunning: num.cunning,
      charisma: num.charisma,
      composure: num.composure,
    },
    drives: {
      greed: num.greed,
      // "Risk Tolerance" is merged into Boldness
      boldness: clamp((num.boldness + num.riskTolerance) / 2 + 0.01),
      loyalty: num.loyalty,
      revenge: num.revenge,
      patience: num.patience,
      ambition: num.ambition,
      pride: add.pride,
      fearOfLoss: num.fearOfLoss,
    },
    origin: r.origin ?? {},
    appearance: {
      ...r.appearance,
      artistNotes: stripPrefix(r.appearance?.artistNotes, 'Детайл за художника:', 'Детайл:'),
      portraitHue: add.hue,
    },
    backstory: r.backstory ?? {},
    psychology: r.psychology ?? {},
    behavior: r.behavior ?? {},
    motives: r.motives ?? {},
    speech: { ...r.speech, register: stripPrefix(r.speech?.register, 'Начин на говорене:') },
    mechanics: {
      strangeness: add.strangeness,
      principle: add.principle ?? r.psychology?.neverWould ?? '',
      temptation: add.temptation ?? r.psychology?.weakness ?? '',
      ability: add.ability ?? '',
      hostRelation: add.hostRelation ?? '',
    },
    authorNotes: r.authorNotes ?? {},
    host: isHost ? { ...r.host, ...add.host } : undefined,
    lines: [...sheetLines, ...(add.lines ?? [])],
    tells: add.tells,
    relations: add.relations ?? [],
  }
  out.push(doc)
}

console.log(`Parsed ${out.length} characters: ${out.map((d) => d.code).join(', ')}`)
if (flag === '--dry') {
  console.log(JSON.stringify(out[0], null, 2).slice(0, 3000))
} else {
  fs.mkdirSync(outDir, { recursive: true })
  for (const d of out) fs.writeFileSync(path.join(outDir, `${d.code}.json`), JSON.stringify(d, null, 2) + '\n', 'utf8')
  console.log(`Wrote ${out.length} files to ${outDir}`)
}
