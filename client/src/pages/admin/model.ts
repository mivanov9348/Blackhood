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
  FirstMeeting: 'Първа среща',
  Win: 'Победа',
  Loss: 'Загуба',
  Accused: 'Обвинение',
  AllianceOffer: 'Предложение за съюз',
  Betrayal: 'Предателство',
  BigBet: 'Голям залог',
  HostIntroduction: 'Домакинът го представя',
  MetAgain: 'Повторна среща',
  IntroReplyWho: 'Домакин: отговор на „Кой сте вие?“',
  IntroReplyBegin: 'Домакин: отговор на „Да започваме“',
  IntroReplySilent: 'Домакин: отговор на мълчание',
  RoleDeal: 'Домакин: раздаване на ролята',
  Claim: 'Заявка за роля',
  Defend: 'Защита',
  Robbed: 'Ограбен',
  Tilted: 'Нажежен',
  Death: 'Смърт',
  RoleReveal: 'Разкриване на ролята',
  ToHost: 'Към домакина',
}

export const tellStates = ['Habit', 'Lying', 'Calculating', 'Confident', 'Afraid', 'Tilted', 'PlanningBetrayal'] as const
export type TellState = (typeof tellStates)[number]

export const tellStateLabel: Record<TellState, string> = {
  Habit: 'Навик (без значение)',
  Lying: 'Лъже',
  Calculating: 'Пресмята',
  Confident: 'Сигурен е',
  Afraid: 'Страх',
  Tilted: 'Нажежен',
  PlanningBetrayal: 'Готви предателство',
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
    title: 'Произход',
    fields: [
      { key: 'birthplace', label: 'Произход / родно място' },
      { key: 'socialStatus', label: 'Обществено положение' },
      { key: 'profession', label: 'Професия / минало' },
    ],
  },
  appearance: {
    title: 'Външен вид',
    fields: [
      { key: 'face', label: 'Лице и отличителни черти', long: true },
      { key: 'hairAndEyes', label: 'Коса / брада / очи' },
      { key: 'clothing', label: 'Облекло и аксесоари', long: true },
      { key: 'build', label: 'Телосложение и стойка' },
      { key: 'gestures', label: 'Жестове и навици', long: true, hint: 'Суровината за издайническите знаци.' },
      { key: 'voice', label: 'Глас / начин на говорене' },
      { key: 'firstImpression', label: 'Първо впечатление' },
      { key: 'artistNotes', label: 'Бележки за художника', long: true },
    ],
  },
  backstory: {
    title: 'История',
    fields: [
      { key: 'biography', label: 'Кратка биография (3–6 изречения)', long: true },
      { key: 'whyHere', label: 'Какво го довежда в страноприемницата?', long: true },
      { key: 'secret', label: 'Какво крие от останалите?', long: true },
    ],
  },
  psychology: {
    title: 'Психология',
    fields: [
      { key: 'coreTrait', label: 'Основна черта' },
      { key: 'secondTrait', label: 'Втора черта' },
      { key: 'weakness', label: 'Най-голяма слабост', long: true },
      { key: 'fear', label: 'Най-голям страх' },
      { key: 'desire', label: 'Най-силно желание' },
      { key: 'angers', label: 'Какво го разгневява?' },
      { key: 'regret', label: 'Съжаление / вина' },
      { key: 'moralLimits', label: 'Морални граници', long: true },
      { key: 'neverWould', label: 'Какво никога не би направил?' },
      { key: 'wouldForReward', label: 'Какво би направил за достатъчно голяма награда?' },
    ],
  },
  behavior: {
    title: 'Поведение',
    fields: [
      { key: 'style', label: 'Предпочитан стил' },
      { key: 'stakes', label: 'Отношение към залозите' },
      { key: 'deception', label: 'Отношение към измамата' },
      { key: 'debts', label: 'Отношение към дълговете' },
      { key: 'alliances', label: 'Отношение към съюзите' },
      { key: 'whenAttacks', label: 'Кога атакува?' },
      { key: 'whenRetreats', label: 'Кога се оттегля?' },
      { key: 'onLoss', label: 'Как реагира при загуба?' },
      { key: 'onWin', label: 'Как реагира при победа?' },
      { key: 'strategyChange', label: 'Какво го кара да промени стратегията си?' },
    ],
  },
  motives: {
    title: 'Мотиви',
    fields: [
      { key: 'publicGoal', label: 'Публична цел' },
      { key: 'hiddenGoal', label: 'Истинска / скрита цел' },
      { key: 'personalReason', label: 'Лична причина да участва' },
      { key: 'betrayalTrigger', label: 'Какво би го накарало да предаде останалите?' },
      { key: 'helpTrigger', label: 'Какво би го накарало да помогне на друг?' },
    ],
  },
  speech: {
    title: 'Речев стил',
    fields: [
      { key: 'register', label: 'Начин на говорене / речник' },
      { key: 'favoriteWords', label: 'Любими думи / обръщения' },
      { key: 'avoidedTopics', label: 'Теми, които избягва' },
      { key: 'howLies', label: 'Как лъже?' },
      { key: 'angerAndFear', label: 'Как показва гняв или страх?' },
    ],
  },
  mechanics: {
    title: 'Механика',
    fields: [
      { key: 'strangeness', label: '„Странното“', long: true, hint: 'Едно необяснимо, леко отвъдно нещо за персонажа.' },
      { key: 'principle', label: 'Принцип (вето) — какво никога не прави', long: true },
      { key: 'temptation', label: 'Изкушение — кога се държи ирационално', long: true },
      { key: 'ability', label: 'Специално умение (веднъж на партия или пасивно)', long: true },
      { key: 'hostRelation', label: 'Отношение към домакина' },
    ],
  },
  authorNotes: {
    title: 'Бележки на автора',
    fields: [
      { key: 'distinctive', label: 'Какво прави персонажа интересен и различим?', long: true },
      { key: 'immutable', label: 'Кои черти не трябва да се променят?', long: true },
      { key: 'tunable', label: 'Какво подлежи на редакция / баланс?', long: true },
    ],
  },
  host: {
    title: 'Профил на домакина',
    fields: [
      { key: 'presentation', label: 'Как се представя пред гостите?' },
      { key: 'trueNature', label: 'Каква е истинската му природа?', long: true },
      { key: 'wantsFromGuests', label: 'Какво иска от участниците?' },
      { key: 'neverReveals', label: 'Какво никога не разкрива?' },
      { key: 'rulesAttitude', label: 'Как се отнася към правилата?' },
      { key: 'punishment', label: 'Как наказва нарушителите?' },
      { key: 'temptations', label: 'Какви изкушения предлага?' },
      { key: 'price', label: 'Каква цена изисква?' },
      { key: 'amused', label: 'Какво го забавлява?' },
      { key: 'angered', label: 'Какво го ядосва?' },
      { key: 'wouldBreakRules', label: 'Какво би го накарало да наруши собствените си правила?' },
      { key: 'hostingStyle', label: 'Характерен начин на водене на партията', long: true },
      { key: 'sampleOffers', label: 'Примерни оферти / изкушения', long: true },
      { key: 'trueStory', label: 'Истинска история / тайна', long: true },
      { key: 'changedRule', label: 'Правилото, което променя', long: true },
    ],
  },
}

export const skillDefs = [
  { key: 'insight', label: 'Проницателност', hint: 'чете хора' },
  { key: 'calculation', label: 'Пресметливост', hint: 'чете числа' },
  { key: 'cunning', label: 'Хитрост', hint: 'лъже добре' },
  { key: 'charisma', label: 'Влияние', hint: 'убеждава' },
  { key: 'composure', label: 'Самообладание', hint: 'не се нажежава' },
] as const

export const driveDefs = [
  { key: 'greed', label: 'Алчност' },
  { key: 'boldness', label: 'Дързост' },
  { key: 'loyalty', label: 'Лоялност' },
  { key: 'revenge', label: 'Отмъстителност' },
  { key: 'patience', label: 'Търпение' },
  { key: 'ambition', label: 'Амбиция' },
  { key: 'pride', label: 'Гордост' },
  { key: 'fearOfLoss', label: 'Страх от загуба' },
] as const

export const hostAttrDefs = [
  { key: 'generosity', label: 'Щедрост' },
  { key: 'cruelty', label: 'Жестокост' },
  { key: 'verbosity', label: 'Словоохотливост' },
  { key: 'capriciousness', label: 'Капризност' },
  { key: 'partiality', label: 'Пристрастност', hint: '1 = към водещия, 5 = към изоставащия' },
] as const

export const rarityLabel: Record<Rarity, string> = { Common: 'Common', Uncommon: 'Uncommon', Rare: 'Rare', Unique: 'Unique' }
export const tagSuggestions = ['noble', 'street', 'soldier', 'church', 'money', 'craft', 'debtor', 'death', 'host']
