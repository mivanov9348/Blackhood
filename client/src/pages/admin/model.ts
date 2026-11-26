// Mirrors Blackhood.Data.Characters.CharacterDocument and the admin API.

export type CharacterKind = 'Participant' | 'Host'
export type Rarity = 'Common' | 'Uncommon' | 'Rare' | 'Unique'
export type ContentStatus = 'Draft' | 'Published'

export const lineSituations = [
  'FirstMeeting', 'Win', 'Loss', 'Accused', 'AllianceOffer', 'Betrayal', 'BigBet',
  'HostIntroduction', 'MetAgain', 'IntroReplyWho', 'IntroReplyBegin', 'IntroReplySilent', 'RoleDeal',
  'Claim', 'Defend', 'Robbed', 'Tilted', 'Death', 'RoleReveal', 'ToHost',
] as const
export type LineSituation = (typeof lineSituations)[number]

export const situationLabel: Record<LineSituation, string> = {
  FirstMeeting: 'First meeting',
  Win: 'Win',
  Loss: 'Loss',
  Accused: 'Accused',
  AllianceOffer: 'Alliance offer',
  Betrayal: 'Betrayal',
  BigBet: 'Big bet',
  HostIntroduction: 'Host introduces them',
  MetAgain: 'Met again',
  IntroReplyWho: 'Host: reply to “Who are you?”',
  IntroReplyBegin: 'Host: reply to “Let’s begin”',
  IntroReplySilent: 'Host: reply to silence',
  RoleDeal: 'Host: dealing the role',
  Claim: 'Role claim',
  Defend: 'Defence',
  Robbed: 'Robbed',
  Tilted: 'Tilted',
  Death: 'Death',
  RoleReveal: 'Role reveal',
  ToHost: 'To the host',
}

export const tellStates = ['Habit', 'Lying', 'Calculating', 'Confident', 'Afraid', 'Tilted', 'PlanningBetrayal'] as const
export type TellState = (typeof tellStates)[number]

export const tellStateLabel: Record<TellState, string> = {
  Habit: 'Habit (no meaning)',
  Lying: 'Lying',
  Calculating: 'Calculating',
  Confident: 'Confident',
  Afraid: 'Afraid',
  Tilted: 'Tilted',
  PlanningBetrayal: 'Planning betrayal',
}

export interface LineDoc { situation: LineSituation; text: string; direction?: string | null }
export interface TellDoc { state: TellState; description: string; fakeable: boolean; visibleWithoutDossier: boolean }
export interface RelationDoc { to: string; value: number; story: string; effect: string }

export type Section = Record<string, string | number>

export interface CharacterDoc {
  code: string
  kind: CharacterKind
  name: string
  title: string
  nickname: string
  gender: string
  age: string
  archetype: string
  rarity: Rarity
  status: ContentStatus
  tags: string[]
  version: number
  skills: { insight: number; calculation: number; cunning: number; charisma: number; composure: number }
  drives: { greed: number; boldness: number; loyalty: number; revenge: number; patience: number; ambition: number; pride: number; fearOfLoss: number }
  origin: Section
  appearance: Section
  backstory: Section
  psychology: Section
  behavior: Section
  motives: Section
  speech: Section
  mechanics: Section
  authorNotes: Section
  host?: Section | null
  lines: LineDoc[]
  tells: TellDoc[]
  relations: RelationDoc[]
}

export interface Validation { errors: string[]; warnings: string[] }

export interface EditorResponse { document: CharacterDoc; validation: Validation; hasPortrait: boolean; skillBudget: number | null }

export interface CharacterSummary {
  code: string
  name: string
  title: string
  kind: CharacterKind
  rarity: Rarity
  status: ContentStatus
  portraitHue: number
  hasPortrait: boolean
  warningCount: number
  updatedAt: string
}

export function emptyCharacter(kind: CharacterKind, code: string): CharacterDoc {
  return {
    code, kind, name: '', title: '', nickname: '', gender: '', age: '', archetype: '',
    rarity: kind === 'Host' ? 'Unique' : 'Common', status: 'Draft', tags: [], version: 0,
    skills: { insight: 3, calculation: 3, cunning: 3, charisma: 3, composure: 3 },
    drives: { greed: 3, boldness: 3, loyalty: 3, revenge: 3, patience: 3, ambition: 3, pride: 3, fearOfLoss: 3 },
    origin: {}, appearance: { portraitHue: 30 }, backstory: {}, psychology: {}, behavior: {}, motives: {},
    speech: {}, mechanics: {}, authorNotes: {},
    host: kind === 'Host' ? { generosity: 3, cruelty: 3, verbosity: 3, capriciousness: 3, partiality: 3 } : null,
    lines: [], tells: [], relations: [],
  }
}

/** Text fields per section — labels follow the character sheet template. */
export interface FieldDef { key: string; label: string; long?: boolean; hint?: string }

export const textSections: Record<string, { title: string; fields: FieldDef[] }> = {
  origin: {
    title: 'Origin',
    fields: [
      { key: 'birthplace', label: 'Origin / birthplace' },
      { key: 'socialStatus', label: 'Social standing' },
      { key: 'profession', label: 'Profession / past' },
    ],
  },
  appearance: {
    title: 'Appearance',
    fields: [
      { key: 'face', label: 'Face and distinctive features', long: true },
      { key: 'hairAndEyes', label: 'Hair / beard / eyes' },
      { key: 'clothing', label: 'Clothing and accessories', long: true },
      { key: 'build', label: 'Build and posture' },
      { key: 'gestures', label: 'Gestures and habits', long: true, hint: 'Raw material for the tells.' },
      { key: 'voice', label: 'Voice / manner of speaking' },
      { key: 'firstImpression', label: 'First impression' },
      { key: 'artistNotes', label: 'Notes for the artist', long: true },
    ],
  },
  backstory: {
    title: 'Story',
    fields: [
      { key: 'biography', label: 'Short biography (3–6 sentences)', long: true },
      { key: 'whyHere', label: 'What brings them to the inn?', long: true },
      { key: 'secret', label: 'What do they hide from the others?', long: true },
    ],
  },
  psychology: {
    title: 'Psychology',
    fields: [
      { key: 'coreTrait', label: 'Core trait' },
      { key: 'secondTrait', label: 'Second trait' },
      { key: 'weakness', label: 'Greatest weakness', long: true },
      { key: 'fear', label: 'Greatest fear' },
      { key: 'desire', label: 'Strongest desire' },
      { key: 'angers', label: 'What angers them?' },
      { key: 'regret', label: 'Regret / guilt' },
      { key: 'moralLimits', label: 'Moral limits', long: true },
      { key: 'neverWould', label: 'What would they never do?' },
      { key: 'wouldForReward', label: 'What would they do for a big enough reward?' },
    ],
  },
  behavior: {
    title: 'Behaviour',
    fields: [
      { key: 'style', label: 'Preferred style' },
      { key: 'stakes', label: 'Attitude to stakes' },
      { key: 'deception', label: 'Attitude to deception' },
      { key: 'debts', label: 'Attitude to debts' },
      { key: 'alliances', label: 'Attitude to alliances' },
      { key: 'whenAttacks', label: 'When do they attack?' },
      { key: 'whenRetreats', label: 'When do they retreat?' },
      { key: 'onLoss', label: 'How do they react to a loss?' },
      { key: 'onWin', label: 'How do they react to a win?' },
      { key: 'strategyChange', label: 'What makes them change strategy?' },
    ],
  },
  motives: {
    title: 'Motives',
    fields: [
      { key: 'publicGoal', label: 'Public goal' },
      { key: 'hiddenGoal', label: 'True / hidden goal' },
      { key: 'personalReason', label: 'Personal reason to play' },
      { key: 'betrayalTrigger', label: 'What would make them betray the others?' },
      { key: 'helpTrigger', label: 'What would make them help someone?' },
    ],
  },
  speech: {
    title: 'Speech',
    fields: [
      { key: 'register', label: 'Manner of speech / vocabulary' },
      { key: 'favoriteWords', label: 'Favourite words / forms of address' },
      { key: 'avoidedTopics', label: 'Topics they avoid' },
      { key: 'howLies', label: 'How do they lie?' },
      { key: 'angerAndFear', label: 'How do they show anger or fear?' },
    ],
  },
  mechanics: {
    title: 'Mechanics',
    fields: [
      { key: 'strangeness', label: '“The Strange”', long: true, hint: 'One unexplained, slightly otherworldly thing about the character.' },
      { key: 'principle', label: 'Principle (veto) — what they never do', long: true },
      { key: 'temptation', label: 'Temptation — when they act irrationally', long: true },
      { key: 'ability', label: 'Special ability (once per evening, or passive)', long: true },
      { key: 'hostRelation', label: 'Attitude to the host' },
    ],
  },
  authorNotes: {
    title: 'Author notes',
    fields: [
      { key: 'distinctive', label: 'What makes the character interesting and distinct?', long: true },
      { key: 'immutable', label: 'Which traits must not change?', long: true },
      { key: 'tunable', label: 'What can be tuned / balanced?', long: true },
    ],
  },
  host: {
    title: 'Host profile',
    fields: [
      { key: 'presentation', label: 'How do they present themselves to guests?' },
      { key: 'trueNature', label: 'What is their true nature?', long: true },
      { key: 'wantsFromGuests', label: 'What do they want from the guests?' },
      { key: 'neverReveals', label: 'What do they never reveal?' },
      { key: 'rulesAttitude', label: 'Attitude to the rules' },
      { key: 'punishment', label: 'How do they punish offenders?' },
      { key: 'temptations', label: 'What temptations do they offer?' },
      { key: 'price', label: 'What price do they ask?' },
      { key: 'amused', label: 'What amuses them?' },
      { key: 'angered', label: 'What angers them?' },
      { key: 'wouldBreakRules', label: 'What would make them break their own rules?' },
      { key: 'hostingStyle', label: 'How they run the evening', long: true },
      { key: 'sampleOffers', label: 'Sample offers / temptations', long: true },
      { key: 'trueStory', label: 'True story / secret', long: true },
      { key: 'changedRule', label: 'The rule they change', long: true },
    ],
  },
}

export const skillDefs = [
  { key: 'insight', label: 'Insight', hint: 'reads people' },
  { key: 'calculation', label: 'Calculation', hint: 'reads numbers' },
  { key: 'cunning', label: 'Cunning', hint: 'lies well' },
  { key: 'charisma', label: 'Charisma', hint: 'persuades' },
  { key: 'composure', label: 'Composure', hint: 'keeps calm' },
] as const

export const driveDefs = [
  { key: 'greed', label: 'Greed' },
  { key: 'boldness', label: 'Boldness' },
  { key: 'loyalty', label: 'Loyalty' },
  { key: 'revenge', label: 'Revenge' },
  { key: 'patience', label: 'Patience' },
  { key: 'ambition', label: 'Ambition' },
  { key: 'pride', label: 'Pride' },
  { key: 'fearOfLoss', label: 'Fear of loss' },
] as const

export const hostAttrDefs = [
  { key: 'generosity', label: 'Generosity' },
  { key: 'cruelty', label: 'Cruelty' },
  { key: 'verbosity', label: 'Verbosity' },
  { key: 'capriciousness', label: 'Capriciousness' },
  { key: 'partiality', label: 'Partiality', hint: '1 = favours the leader, 5 = favours the one behind' },
] as const

export const rarityLabel: Record<Rarity, string> = { Common: 'Common', Uncommon: 'Uncommon', Rare: 'Rare', Unique: 'Unique' }
export const tagSuggestions = ['noble', 'street', 'soldier', 'church', 'money', 'craft', 'debtor', 'death', 'host']
