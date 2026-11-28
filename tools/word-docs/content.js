// Content of the Word documents. Blocks: h1, h2, p, note, ul, ol, table.
// Inline: **bold**, *italic*, `code`.

const date = '25 Nov 2026'
const dateBg = '25.11.2026'

// =====================================================================================
// RULES — ENGLISH
// =====================================================================================
const rulesEn = {
  title: 'Gifts',
  subtitle: `The main game of the Blackhood Inn · rules v1.0 · ${date}`,
  header: 'Blackhood — Gifts: rules',
  blocks: [
    { t: 'h1', v: '1. The idea' },
    { t: 'p', v: 'Every night, every guest sends exactly **one gift** to someone at the table — anonymously, through the **Host**. Each gift is announced with a single word: *“Agnes, someone sends you Wine.”* Somewhere among the gifts the **Assassin** sends death, dressed up as something pleasant.' },
    { t: 'p', v: 'There is no talking. The bluff lives in the cards — what you send, what you announce, what you dare to take — and in the gold you visibly pay to the Host without anyone knowing why.' },
    { t: 'note', v: 'The Host is neutral but greedy. He never lies, but he never tells everything. He sells amulets, poison, mirrors and secrets — and the more frightened you are, the more he charges.' },

    { t: 'h1', v: '2. Players and roles' },
    { t: 'p', v: 'Five to eight players: you and the inn\'s regulars (NPCs). The single-player table currently seats **5**. Every player secretly receives a role.' },
    { t: 'table', head: ['Players', 'Assassins', 'Assassin\'s quota (deaths needed)'], widths: [2, 2, 4], rows: [['5', '1', '2'], ['6', '1', '2'], ['7', '2 (they know each other)', '3'], ['8', '2 (they know each other)', '3']] },
    { t: 'ul', v: [
      '**The Assassin** wins if the number of deaths reaches the quota before the sixth candle goes out.',
      '**The Guests** win as a team if the Assassin dies, or if the sixth candle goes out with fewer deaths than the quota. But only one guest is the **guest of honour**: the living guest with the highest score (see §8).',
      'The quota is public and the dead are announced every dawn, so everyone always knows how many more the Assassin needs.',
    ] },

    { t: 'h1', v: '3. Gold' },
    { t: 'ul', v: [
      'You sit down at the table with **10 gold from your character\'s purse** (the buy-in). Everyone starts the evening with 10.',
      'At the start of every candle everyone receives **+2 gold** from the Host.',
      'Gold is **visible**. But payments are revealed only at dawn, as one number per player: *“Silas −11”*. The table sees that someone paid — never for what.',
      'Gold is spent only at the Host. Paid gold leaves the game.',
      'You can give gold away as a gift: a **Purse** with any amount.',
      'When a guest dies, the Host keeps their purse.',
    ] },

    { t: 'h1', v: '4. The cards' },
    { t: 'h2', v: 'The deck (33 cards, drawn)' },
    { t: 'table', head: ['Card', 'Count', 'Effect'], widths: [2, 1, 7], rows: [
      ['Dagger', '8', 'Sent by the Assassin, the receiver dies at dawn. From anyone else it is harmless — but every dagger still in your hand at the end costs −2 gold. Daggers cannot be discarded, only given away.'],
      ['Wine', '6', 'When you receive it, you learn who sent **any one** gift announced tonight (your choice).'],
      ['Closed Door', '5', 'Play it when a gift is announced for you: the gift goes back to its sender unopened.'],
      ['Monocle', '4', 'Play it during the whisper: secretly see one player\'s whole hand.'],
      ['Bread', '7', 'Does nothing. An honest, empty gift.'],
    ] },
    { t: 'h2', v: 'The Host\'s chest (only through his offers, limited)' },
    { t: 'table', head: ['Card', 'Count', 'Effect'], widths: [2, 1, 7], rows: [
      ['Amulet', '4', 'Held at dawn, it stops one death (dagger or poison). The amulet and the weapon leave the game together.'],
      ['Poison', '3', 'Anyone may send it. The receiver dies at the **next** dawn unless they hold an amulet then.'],
      ['Mirror', '2', 'Play it when a gift is announced for you: the gift turns back on its sender. The Assassin\'s dagger finds his own heart.'],
    ] },
    { t: 'ul', v: [
      '**Free bread:** if you do not want to send anything, you send the Host\'s free bread. Sending is mandatory, so sending in itself gives nobody away.',
      '**Starting hand:** 3 cards. The Assassin\'s two daggers *replace* two of his dealt cards, so every hand has the same size.',
      '**Hand limit:** 5 cards. Excess cards are discarded at dawn — but never daggers while you have anything else.',
      'When the deck runs out, the discard pile is shuffled into a new deck.',
    ] },

    { t: 'h1', v: '5. One candle (six per evening)' },
    { t: 'ol', v: [
      '**Income.** Everyone gets +2 gold.',
      '**Draw.** Everyone draws 1 card.',
      '**The whisper.** The Host whispers 1–2 personal offers to each guest. You may accept, refuse, buy from his fixed price list, or play a Monocle.',
      '**The gifts.** Everyone secretly hands the Host one gift (a card, a purse or free bread), names the receiver and announces it with **one word**: Purse, Bread, Wine, Monocle, Closed Door, Amulet or Mirror. You may lie — but you can never announce a Dagger or Poison.',
      '**Delivery.** The Host shuffles the gifts and announces them one by one: *“Agnes, someone sends you Wine.”* The receiver may play a Closed Door or a Mirror; otherwise the gift goes into their hand and only they see it.',
      '**Wine.** Whoever received Wine now learns who sent one gift of their choice.',
      '**The last whisper.** Whoever just received a dagger or poison (and holds no amulet) gets an emergency offer: *“An amulet — 11 gold. Decide.”* The Host sells only **one** emergency amulet per evening.',
      '**Dawn.** Deaths, broken amulets and every player\'s gold change are revealed, then the heaviest purse and the quota counter.',
    ] },
    { t: 'note', v: 'The first candle is for getting acquainted: the Assassin\'s daggers cannot kill before the second candle. Poison can.' },

    { t: 'h1', v: '6. Dawn and death' },
    { t: 'ul', v: [
      'A guest dies if they accepted a dagger from the Assassin tonight, or if poison they received last night takes effect — unless they hold an amulet. One amulet stops one death.',
      'If an amulet saves someone, the Host only says *“An amulet broke tonight.”* Nobody learns whose.',
      'The dead reveal their role and their hand. Their gold goes to the Host.',
      '**Every death counts towards the quota**, whoever caused it — dagger, poison or mirror. *“The Host counts the candles that go out, not the hands that snuffed them.”*',
      '**Ghosts.** A dead player stays at the table as a ghost. Each night the ghost may point silently at one living player — everyone feels the cold breath — but cannot speak. Nobody knows whether the ghost *knows* or only *guesses*.',
      'The dead do not know who killed them, unless their Wine that night traced exactly the fatal gift.',
      'The Assassin is told privately what became of each dagger: harmless on the first candle, stopped by an amulet, sent back by a door, or fatal.',
    ] },

    { t: 'h1', v: '7. The Host' },
    { t: 'p', v: 'His personal offers depend only on your own situation and on public facts — never on other players\' secrets. He **never sells or hints at roles**.' },
    { t: 'table', head: ['Offer', 'When', 'Price'], widths: [2, 5, 3], rows: [
      ['Amulet', 'Often after deaths, or after you received a dagger', '6 + 2 per death (+2 after a dagger, +2 late in the evening)'],
      ['Saving the poisoned', 'When poison will kill you at this dawn', 'Amulet price + 3'],
      ['Emergency amulet', 'Right after you received a dagger or poison — only one per evening', '9 + 2 per death + candle ÷ 2'],
      ['Mirror', 'To the richest, or after a dagger', '8 + candle ÷ 2'],
      ['Poison', 'From the second candle, sometimes', '6 + deaths'],
      ['Closed Door', 'Sometimes, as a discount', '3'],
    ] },
    { t: 'h2', v: 'The fixed price list (any time during the whisper)' },
    { t: 'table', head: ['Service', 'Price'], widths: [7, 2], rows: [
      ['Buy a Closed Door', '4'],
      ['A rumour: one true thing about a chosen player\'s hand (never their role)', '3'],
      ['See a chosen player\'s whole hand', '4'],
      ['Learn who sent you one of last night\'s gifts', '4'],
      ['Diversion: tonight one gift meant for you goes to someone random', '5'],
    ] },

    { t: 'h1', v: '8. The end of the evening' },
    { t: 'ul', v: [
      '**The Assassin dies** (from a mirror or poison): the evening ends at once and the Guests win. The guest who caused it is paid **+5 gold** by the Host. With two Assassins, both must die.',
      '**The deaths reach the quota:** the evening ends at once and the Assassin wins.',
      '**The sixth candle goes out** with fewer deaths than the quota: every living guest secretly **names the Assassin**. Then all roles are revealed and the Guests win.',
    ] },
    { t: 'p', v: '**Score of a living guest** = gold on the table **+5** if they named the Assassin correctly **−2 for every dagger** in their hand. The highest score is the **guest of honour**.' },
    { t: 'h2', v: 'Your purse' },
    { t: 'ul', v: [
      'If you survive, the gold on your table (your score, never below 0) goes back to your character\'s purse.',
      'The guest of honour — or the winning Assassin — also receives **+10 gold** from the Host.',
      'If you die, you take nothing home.',
      'If your purse holds less than 10 gold, the house covers the rest of the buy-in.',
    ] },

    { t: 'h1', v: '9. Where the bluff comes from' },
    { t: 'ul', v: [
      'The Assassin must disguise every dagger as something pleasant — and hope the receiver does not close the door or raise a mirror.',
      'Guests have daggers too: they cost −2 at the end, so everyone wants to give them away. Whoever receives a dagger does not know whether it is deadly — that fear is the Host\'s best customer.',
      'Surviving a dagger without an amulet proves that its sender is innocent. Losing an amulet to one proves the opposite.',
      'Gold changes are visible: *“Oswin −6”* — an amulet? a rumour? a purse for someone?',
      'Counting works: 8 daggers, 4 amulets, 3 poisons, 2 mirrors. The chest shows how many are left.',
      'Every NPC plays by their character: the suspicious close doors, the greedy never pay, the vengeful send daggers back, the frightened buy amulets at any price — and a nervous liar sometimes shows a tell.',
    ] },

    { t: 'h1', v: '10. Balance (simulation)' },
    { t: 'p', v: '3,000 AI-against-AI evenings at a 5-player table with the numbers above:' },
    { t: 'table', head: ['Measure', 'Result'], widths: [5, 3], rows: [
      ['Assassin wins', '46%'],
      ['Guests win', '54%'],
      ['Assassin killed by a mirror or poison', '≈ 5%'],
      ['Average deaths per evening', '1.4'],
      ['Amulets sold / doors closed / mirrors raised per evening', '3.2 / 2.7 / 0.4'],
    ] },
    { t: 'note', v: 'All numbers live in one file (GiftsRules.cs). After any change the simulator in the test project reports the new balance.' },
  ],
}

// =====================================================================================
// RULES — BULGARIAN
// =====================================================================================
const rulesBg = {
  title: 'Дарове (Gifts)',
  subtitle: `Главната игра на страноприемницата Blackhood · правила v1.0 · ${dateBg}`,
  header: 'Blackhood — Дарове: правила',
  blocks: [
    { t: 'h1', v: '1. Идеята' },
    { t: 'p', v: 'Всяка нощ всеки гост праща точно **един дар** на някого от масата — анонимно, чрез **Домакина**. Всеки дар се обявява с една дума: *„Агнес, някой ти праща Вино.“* Някъде сред даровете **Убиецът** праща смърт, облечена като нещо приятно.' },
    { t: 'p', v: 'Няма разговори. Блъфът е в картите — какво пращаш, какво обявяваш, какво смееш да приемеш — и в златото, което видимо плащаш на Домакина, без никой да знае защо.' },
    { t: 'note', v: 'Домакинът е неутрален, но алчен. Никога не лъже, но никога не казва всичко. Продава амулети, отрова, огледала и тайни — и колкото по-уплашен си, толкова по-скъпо взима.' },

    { t: 'h1', v: '2. Играчи и роли' },
    { t: 'p', v: 'От петима до осем играчи: ти и постоянните гости на страноприемницата (NPC). Масата за един играч засега е за **5**. Всеки тайно получава роля.' },
    { t: 'table', head: ['Играчи', 'Убийци', 'Квота на Убиеца (нужни смърти)'], widths: [2, 2, 4], rows: [['5', '1', '2'], ['6', '1', '2'], ['7', '2 (знаят се)', '3'], ['8', '2 (знаят се)', '3']] },
    { t: 'ul', v: [
      '**Убиецът** печели, ако смъртите стигнат квотата, преди да угасне шестата свещ.',
      '**Гостите** печелят като отбор, ако Убиецът умре или ако шестата свещ угасне с по-малко смърти от квотата. Но само един е **почетен гост**: живият гост с най-висок резултат (виж §8).',
      'Квотата е публична, а мъртвите се обявяват всяка сутрин, така че всички винаги знаят колко още му трябват.',
    ] },

    { t: 'h1', v: '3. Златото' },
    { t: 'ul', v: [
      'Сядаш на масата с **10 злато от кесията на героя си** (таксата за влизане). Всеки започва вечерта с 10.',
      'В началото на всяка свещ всеки получава **+2 злато** от Домакина.',
      'Златото е **видимо**. Но плащанията се показват чак на зазоряване, с по едно число на играч: *„Сайлъс −11“*. Масата вижда, че някой е платил — никога за какво.',
      'Злато се харчи само при Домакина. Платеното излиза от играта.',
      'Злато може да се подари като дар: **Кесия** с произволна сума.',
      'Когато гост умре, Домакинът прибира кесията му.',
    ] },

    { t: 'h1', v: '4. Картите' },
    { t: 'h2', v: 'Тестето (33 карти, теглят се)' },
    { t: 'table', head: ['Карта', 'Бр.', 'Ефект'], widths: [2, 1, 7], rows: [
      ['Кама', '8', 'Пратена от Убиеца, получателят умира на зазоряване. От всеки друг е безвредна — но всяка кама в ръката ти накрая е −2 злато. Камите не могат да се изхвърлят, само да се подарят.'],
      ['Вино', '6', 'Когато го получиш, научаваш кой е пратил **кой да е** дар, обявен тази нощ (по твой избор).'],
      ['Затворена врата', '5', 'Изиграва се, когато е обявен дар за теб: дарът се връща неотворен при подателя.'],
      ['Монокъл', '4', 'Изиграва се по време на шепота: тайно виждаш цялата ръка на един играч.'],
      ['Хляб', '7', 'Не прави нищо. Честен, празен дар.'],
    ] },
    { t: 'h2', v: 'Сандъкът на Домакина (само чрез оферти, ограничен брой)' },
    { t: 'table', head: ['Карта', 'Бр.', 'Ефект'], widths: [2, 1, 7], rows: [
      ['Амулет', '4', 'Ако е в ръката ти на зазоряване, спира една смърт (кама или отрова). Амулетът и оръжието излизат от играта заедно.'],
      ['Отрова', '3', 'Всеки може да я прати. Получателят умира на **следващото** зазоряване, ако тогава няма амулет.'],
      ['Огледало', '2', 'Изиграва се, когато е обявен дар за теб: дарът се обръща срещу подателя. Камата на Убиеца намира собственото му сърце.'],
    ] },
    { t: 'ul', v: [
      '**Безплатен хляб:** ако не искаш да пращаш нищо, пращаш безплатния хляб на Домакина. Пращането е задължително, затова самото пращане не издава никого.',
      '**Начална ръка:** 3 карти. Двете ками на Убиеца *заменят* две от неговите раздадени карти, така че всички ръце са с еднакъв брой карти.',
      '**Лимит в ръката:** 5 карти. Излишните се изхвърлят на зазоряване — но никога ками, докато имаш нещо друго.',
      'Когато тестето свърши, изхвърлените карти се разбъркват в ново тесте.',
    ] },

    { t: 'h1', v: '5. Една свещ (шест на вечер)' },
    { t: 'ol', v: [
      '**Доход.** Всеки получава +2 злато.',
      '**Теглене.** Всеки тегли 1 карта.',
      '**Шепотът.** Домакинът шепне на всеки 1–2 лични оферти. Можеш да приемеш, да откажеш, да купиш от фиксирания му ценоразпис или да изиграеш Монокъл.',
      '**Даровете.** Всеки тайно дава на Домакина един дар (карта, кесия или безплатен хляб), казва на кого е и го обявява с **една дума**: Кесия, Хляб, Вино, Монокъл, Затворена врата, Амулет или Огледало. Можеш да излъжеш — но никога не можеш да обявиш Кама или Отрова.',
      '**Доставката.** Домакинът разбърква даровете и ги обявява един по един: *„Агнес, някой ти праща Вино.“* Получателят може да изиграе Затворена врата или Огледало; иначе дарът отива в ръката му и само той го вижда.',
      '**Виното.** Който е получил Вино, научава кой е пратил един дар по негов избор.',
      '**Последният шепот.** Който току-що е получил кама или отрова (и няма амулет), получава спешна оферта: *„Амулет — 11 злато. Решавай.“* Домакинът продава само **един** спешен амулет на вечер.',
      '**Зазоряване.** Обявяват се смъртите, счупените амулети и промяната в златото на всеки играч, после най-тежката кесия и броячът на квотата.',
    ] },
    { t: 'note', v: 'Първата свещ е за запознанство: камите на Убиеца не могат да убиват преди втората свещ. Отровата може.' },

    { t: 'h1', v: '6. Зазоряване и смърт' },
    { t: 'ul', v: [
      'Гост умира, ако тази нощ е приел кама от Убиеца или ако отровата, получена предната нощ, подейства — освен ако държи амулет. Един амулет спира една смърт.',
      'Ако амулет спаси някого, Домакинът казва само: *„Тази нощ се счупи амулет.“* Никой не научава чий.',
      'Мъртвите разкриват роля и ръка. Златото им отива при Домакина.',
      '**Всяка смърт се брои към квотата**, независимо кой я е причинил — кама, отрова или огледало. *„Домакинът брои угасналите свещи, не ръцете, които са ги угасили.“*',
      '**Призраци.** Мъртвият остава на масата като призрак. Всяка нощ може безмълвно да посочи един жив играч — всички усещат студения полъх — но не може да говори. Никой не знае дали призракът *знае*, или само *гадае*.',
      'Мъртвите не знаят кой ги е убил, освен ако Виното им същата нощ не е проследило точно смъртоносния дар.',
      'Убиецът научава насаме какво е станало с всяка негова кама: безвредна на първата свещ, спряна от амулет, върната от врата или смъртоносна.',
    ] },

    { t: 'h1', v: '7. Домакинът' },
    { t: 'p', v: 'Личните му оферти зависят само от твоето положение и от публичните факти — никога от чуждите тайни. **Никога не продава роли и не намеква за тях.**' },
    { t: 'table', head: ['Оферта', 'Кога', 'Цена'], widths: [2, 5, 3], rows: [
      ['Амулет', 'Често след смърти или след като си получил кама', '6 + 2 за всяка смърт (+2 след кама, +2 късно вечерта)'],
      ['Спасение на отровения', 'Когато отровата ще те убие на това зазоряване', 'Цената на амулета + 3'],
      ['Спешен амулет', 'Веднага след като си получил кама или отрова — само един на вечер', '9 + 2 за всяка смърт + свещ ÷ 2'],
      ['Огледало', 'На най-богатия или след кама', '8 + свещ ÷ 2'],
      ['Отрова', 'От втората свещ, понякога', '6 + смъртите'],
      ['Затворена врата', 'Понякога, с отстъпка', '3'],
    ] },
    { t: 'h2', v: 'Фиксираният ценоразпис (по всяко време на шепота)' },
    { t: 'table', head: ['Услуга', 'Цена'], widths: [7, 2], rows: [
      ['Купуваш Затворена врата', '4'],
      ['Слух: едно вярно нещо за ръката на избран играч (никога ролята му)', '3'],
      ['Виждаш цялата ръка на избран играч', '4'],
      ['Научаваш кой ти е пратил един от снощните дарове', '4'],
      ['Отклоняване: тази нощ един дар към теб отива при случаен друг играч', '5'],
    ] },

    { t: 'h1', v: '8. Краят на вечерта' },
    { t: 'ul', v: [
      '**Убиецът умре** (от огледало или отрова): вечерта свършва веднага и Гостите печелят. Гостът, който го е причинил, получава **+5 злато** от Домакина. При двама Убийци трябва да умрат и двамата.',
      '**Смъртите стигнат квотата:** вечерта свършва веднага и Убиецът печели.',
      '**Шестата свещ угасне** с по-малко смърти от квотата: всеки жив гост тайно **посочва Убиеца**. После всички роли се разкриват и Гостите печелят.',
    ] },
    { t: 'p', v: '**Резултат на жив гост** = златото на масата **+5** при вярно посочване **−2 за всяка кама** в ръката. Най-високият резултат е **почетният гост**.' },
    { t: 'h2', v: 'Твоята кесия' },
    { t: 'ul', v: [
      'Ако оцелееш, златото на масата (резултатът ти, никога под 0) се връща в кесията на героя ти.',
      'Почетният гост — или победилият Убиец — получава и **+10 злато** от Домакина.',
      'Ако умреш, не прибираш нищо.',
      'Ако в кесията ти има по-малко от 10 злато, заведението покрива остатъка от таксата.',
    ] },

    { t: 'h1', v: '9. Откъде идва блъфът' },
    { t: 'ul', v: [
      'Убиецът трябва да маскира всяка кама като нещо приятно — и да се надява получателят да не затвори вратата или да не вдигне огледало.',
      'И Гостите имат ками: те са −2 накрая, затова всички искат да ги подарят. Който получи кама, не знае дали е смъртоносна — този страх е най-добрият клиент на Домакина.',
      'Оцелял с кама без амулет — значи подателят ѝ е невинен. Изгубил амулет заради нея — значи обратното.',
      'Промените в златото се виждат: *„Осуин −6“* — амулет? слух? кесия за някого?',
      'Броенето работи: 8 ками, 4 амулета, 3 отрови, 2 огледала. Сандъкът показва колко са останали.',
      'Всеки NPC играе според характера си: подозрителните затварят врати, алчните никога не плащат, отмъстителните връщат ками, уплашените купуват амулет на всяка цена — а нервният лъжец понякога издава знак.',
    ] },

    { t: 'h1', v: '10. Баланс (симулация)' },
    { t: 'p', v: '3000 вечери AI срещу AI на маса с 5 играчи и числата по-горе:' },
    { t: 'table', head: ['Показател', 'Резултат'], widths: [5, 3], rows: [
      ['Убиецът печели', '46%'],
      ['Гостите печелят', '54%'],
      ['Убиецът е убит с огледало или отрова', '≈ 5%'],
      ['Средно смърти на вечер', '1,4'],
      ['Продадени амулети / затворени врати / вдигнати огледала на вечер', '3,2 / 2,7 / 0,4'],
    ] },
    { t: 'note', v: 'Всички числа са в един файл (GiftsRules.cs). След всяка промяна симулаторът в тестовия проект показва новия баланс.' },
  ],
}

// =====================================================================================
// DOCUMENTATION — ENGLISH
// =====================================================================================
const docsEn = {
  title: 'Blackhood',
  subtitle: `Project documentation — what has been built so far · ${date}`,
  header: 'Blackhood — documentation',
  blocks: [
    { t: 'h1', v: '1. Overview' },
    { t: 'p', v: 'Blackhood (working title) is a browser game set in a strange medieval inn with fantasy touches. The inn is a hub of tavern games played with your character\'s gold. The main game is **Gifts** — a secret-role game of anonymous gifts, an Assassin and a greedy Host (see the separate rules document). Dice and fortune-telling tables are placed in the hall as “coming soon”.' },
    { t: 'p', v: 'The whole game is in **English**: interface, host, characters and content.' },
    { t: 'table', head: ['Layer', 'Technology', 'Opened with'], widths: [2, 5, 2], rows: [
      ['Game logic', 'C# class library `Blackhood.Engine`, .NET 10', 'Visual Studio'],
      ['Data', 'EF Core 10 code-first, SQL Server LocalDB', 'Visual Studio'],
      ['API', 'ASP.NET Core minimal APIs, Identity (cookie)', 'Visual Studio'],
      ['Client', 'React 19 + Vite + TypeScript', 'VS Code'],
      ['Tests', 'xUnit `Blackhood.Engine.Tests` (rules + AI simulation)', 'Visual Studio'],
      ['Content', 'JSON in `content/characters/`, portraits in `content/portraits/`', 'Admin editor'],
    ] },
    { t: 'note', v: 'The server is authoritative: the full match state never leaves it. The client receives only a view of what the player may see — other players\' roles, hands and live gold are never sent.' },

    { t: 'h1', v: '2. Solution layout' },
    { t: 'table', head: ['Folder', 'Contents'], widths: [3, 7], rows: [
      ['`src/Blackhood.Engine`', 'Pure game logic. `Gifts/` holds the rules (GiftsGame), the NPC AI (GiftsAi), the Host (GiftsHost), the client view (GiftsView), all numbers (GiftsRules) and a saveable random generator (Rng).'],
      ['`src/Blackhood.Data`', 'DbContext, entities, migrations, character model, validator and JSON import/export.'],
      ['`src/Blackhood.Api`', 'Endpoints: auth, tavern, gifts, characters, admin; startup migration and seeding.'],
      ['`tests/Blackhood.Engine.Tests`', '23 tests: every rule, plus thousands of simulated evenings.'],
      ['`client/`', 'Tavern hall, arrival scene, Gifts table, admin character editor.'],
      ['`content/`', '18 characters (P01–P13 participants, H01–H05 hosts) as JSON; portraits.'],
      ['`tools/`', 'Character-sheet importer, English translation overlay, this Word generator.'],
      ['`docs/`', 'These documents and the earlier progress notes.'],
    ] },

    { t: 'h1', v: '3. Running it' },
    { t: 'ol', v: [
      '**API** — in Visual Studio, `Blackhood.Api` is the startup project; choose the **http** profile and press F5. It migrates the LocalDB database, imports any missing characters and gives the Admin role to the configured users.',
      '**Client** — in VS Code: `cd client`, `npm install`, `npm run dev`, then open http://localhost:5173.',
      '**Test account** (local only): `tester` / `candlelight42`, Admin role.',
      '**Tests** — Test Explorer in Visual Studio, or `dotnet test`.',
    ] },
    { t: 'p', v: 'If LocalDB ever forgets the database (“Cannot create file Blackhood.mdf because it already exists”), the README explains how to re-attach it without losing data.' },

    { t: 'h1', v: '4. Accounts and the purse' },
    { t: 'ul', v: [
      'Registration, sign-in (name or e-mail) and sign-out with ASP.NET Core Identity and an HTTP-only cookie.',
      'Every account has a **purse** (`AppUser.Gold`, starting at 50). Tavern games are played with this gold: Gifts takes a 10-gold buy-in and pays out at the end.',
      'An **Admin** role (checked against the database on every request) unlocks the character editor.',
    ] },

    { t: 'h1', v: '5. The tavern hall (main menu)' },
    { t: 'ul', v: [
      'A text-based hall of the Blackhood Inn: drifting ambient lines (“For a moment the candles in the far corner burn blue.”), four regulars who are “around tonight” with one of their habits, your purse.',
      'Tables: **The host\'s table — Gifts** (playable, with a rules summary and “return to your unfinished evening”), **Bones** (dice, coming soon), **The Cards of Fate** (fortune telling, coming soon), **Private tables** (multiplayer, coming soon).',
      'Your last five evenings with buy-in and payout.',
      'A 3D map of the inn is planned for later.',
    ] },

    { t: 'h1', v: '6. Characters' },
    { t: 'h2', v: 'Data model' },
    { t: 'ul', v: [
      '`Character`: code, kind (participant / host), name, title, rarity, status (Draft / Published), tags.',
      '**Skills** (budgeted by rarity: Common 15, Uncommon 17, Rare 18): Insight, Calculation, Cunning, Charisma, Composure. **Drives**: Greed, Boldness, Loyalty, Revenge, Patience, Ambition, Pride, Fear of Loss — columns, read by the AI.',
      'One **JSON column per section** of the character sheet (EF Core complex types): origin, appearance, backstory, psychology, behaviour, motives, speech, mechanics (incl. **“The Strange”**), author notes, host profile.',
      'Child tables: lines per situation, tells tied to inner states, relations, portrait assets; `PlayerEncounter` remembers whom the player has met.',
    ] },
    { t: 'h2', v: 'Content pipeline and language' },
    { t: 'ul', v: [
      'The database is the source of truth; `content/characters/*.json` seeds a fresh database and is exported back from the editor.',
      'All 18 characters were translated into English (`tools/translate-en/`). Death lines were added for every participant; characters without a “Lying” tell got one (except Bertram and Elias, who never lie); host rules were rewritten for Gifts.',
    ] },
    { t: 'h2', v: 'Admin editor (`/admin`)' },
    { t: 'p', v: 'List with filters and warnings; editor with tabs following the sheet, skill sliders with a budget bar, line/tell/relation editors, portrait upload, live design checks, table preview, draft/published, Ctrl+S, optimistic concurrency, export to JSON. Fully in English.' },

    { t: 'h1', v: '7. The arrival' },
    { t: 'p', v: 'Every evening opens with an interactive scene: the door in the rain → the Host (The Gentleman) greets you and you choose a reply (“Who are you?” / “Let\'s begin.” / silence) → the six candles light → the four guests arrive one by one, introduced by the Host, with their own first line and one visible habit → the Host slides your secret role across the table and you turn it over. Returning guests get a short “met again” greeting. Skippable at any time.' },

    { t: 'h1', v: '8. Gifts — implementation' },
    { t: 'ul', v: [
      '**GiftsGame** — the rules as a state machine: Whisper → Gifts → Delivery → Wine → Last whisper → Dawn → Discard → (Pointing) → Finished. NPC seats are played automatically; the human seat waits for actions. A match without a human plays itself to the end (used by the simulator).',
      '**GiftsAi** — every NPC decision is a probability shaped by its traits: who buys an amulet, who closes a door, which word the Assassin uses for a dagger, whom he targets (he remembers who just spent an amulet), who dumps a dagger on whom, whom a ghost points at, whom a guest names at the end. Beliefs come only from what that seat could know (wine, monocle, rumours, survived daggers, revealed deaths, ghosts).',
      '**Tells** — when an NPC lies with a gift it may betray itself in the journal (“Silas Crowe spins the coin between his knuckles.”); cunning characters sometimes fake the tell.',
      '**GiftsHost** — offers and prices built only from the player\'s own situation and public facts.',
      '**GiftsView** — what the human may see. **Rng** — deterministic, saved in the match, so a match continues identically after a reload.',
    ] },
    { t: 'h2', v: 'The table (client)' },
    { t: 'p', v: 'First-person table: Host and candles at the far end, four guests with portrait, visible gold (+/− at dawn), hand size, ghost state and revealed role; a central panel for the current phase (offers and price list, three-step gift builder with honest/lie indicator, delivery with Take / Close the door / Raise the mirror, wine tracing, emergency offer, dawn report, discard, naming the Assassin, final scores and payout); your hand, role and purse; the public journal on the left and private whispers on the right. Death shakes the portrait, releases smoke and cracks it; ghosts float in cold colours.' },

    { t: 'h1', v: '9. API' },
    { t: 'table', head: ['Endpoint', 'Purpose'], widths: [4, 6], rows: [
      ['`POST /api/auth/register · login · logout`, `GET /api/auth/me`', 'Accounts (the user includes `gold` and `isAdmin`)'],
      ['`GET /api/tavern`', 'Purse, patrons in the hall, unfinished evening, recent evenings'],
      ['`POST /api/gifts/matches`', 'Sit down: buy-in, four random guests, encounters recorded'],
      ['`GET /api/gifts/matches/{id}`', 'The player\'s view of the evening'],
      ['`POST /api/gifts/matches/{id}/intro-complete`', 'The arrival was watched or skipped'],
      ['`POST /api/gifts/matches/{id}/act`', 'One move: endWhisper, acceptOffer, declineOffer, buyService, monocle, sendGift, ghost, respond, wine, seeDawn, discard, point'],
      ['`GET /api/characters/{code}/portrait`', 'Portrait image'],
      ['`/api/admin/characters…`', 'List, read, create, update, delete, validate, export, portrait upload (Admin only)'],
    ] },

    { t: 'h1', v: '10. Database' },
    { t: 'p', v: 'Migrations: `Initial` (Identity, matches), `Characters` (character system), `GiftsAndGold` (user gold, match game kind, buy-in, payout, settled). Matches from the first prototype are kept but marked `Legacy` and are no longer playable.' },

    { t: 'h1', v: '11. Tests and simulation' },
    { t: 'ul', v: [
      '16 rule tests: setup, quota, lethal and harmless daggers, the safe first candle, amulets, doors, mirrors (and the slayer bonus), poison timing, “every death counts”, discard rules, gift validation, hidden information in the view, wine.',
      'Simulation: 3,000 AI evenings at 5 players (Assassin 44% / Guests 56%), every table size from 5 to 8, determinism, and a scripted human who takes random legal actions through every phase for 400 evenings.',
      'The simulator found real problems during development: an empty chest crashing offers, and a 21% Assassin win rate that led to the current numbers.',
    ] },

    { t: 'h1', v: '12. Source control' },
    { t: 'p', v: 'GitHub: https://github.com/mivanov9348/Blackhood — `master` holds the state before Gifts; the Gifts work is ready to be committed.' },

    { t: 'h1', v: '13. Known limits and next steps' },
    { t: 'ul', v: [
      'Only the main host (The Gentleman) runs the table; the other four hosts and their changed rules are written but not active yet.',
      'Character special abilities are described but not yet part of the Gifts rules.',
      'Portraits are tinted silhouettes until real art exists; expressions (smug, tilted, afraid, dead) come with the art.',
      'Most participants show skill-budget warnings in the editor — the sheet numbers predate the budget rule.',
      'Next: sound and more cinematic deaths, the dossier (“Wall of Records”), the dice table, fortune telling, multiplayer, a 3D inn.',
    ] },
  ],
}

// =====================================================================================
// DOCUMENTATION — BULGARIAN
// =====================================================================================
const docsBg = {
  title: 'Blackhood',
  subtitle: `Документация на проекта — какво е направено досега · ${dateBg}`,
  header: 'Blackhood — документация',
  blocks: [
    { t: 'h1', v: '1. Общ преглед' },
    { t: 'p', v: 'Blackhood (работно заглавие) е браузърна игра в странна средновековна страноприемница с фентъзи елементи. Страноприемницата е център на кръчмарски игри, които се играят със златото на героя ти. Главната игра е **Дарове (Gifts)** — игра с тайни роли, анонимни дарове, Убиец и алчен Домакин (виж отделния документ с правилата). Масите за зарове и гадаене са поставени в залата като „скоро“.' },
    { t: 'p', v: 'Цялата игра е на **английски**: интерфейс, домакин, персонажи и съдържание.' },
    { t: 'table', head: ['Слой', 'Технология', 'С какво се отваря'], widths: [2, 5, 2], rows: [
      ['Логика на играта', 'C# библиотека `Blackhood.Engine`, .NET 10', 'Visual Studio'],
      ['Данни', 'EF Core 10 code-first, SQL Server LocalDB', 'Visual Studio'],
      ['API', 'ASP.NET Core minimal APIs, Identity (cookie)', 'Visual Studio'],
      ['Клиент', 'React 19 + Vite + TypeScript', 'VS Code'],
      ['Тестове', 'xUnit `Blackhood.Engine.Tests` (правила + AI симулация)', 'Visual Studio'],
      ['Съдържание', 'JSON в `content/characters/`, портрети в `content/portraits/`', 'Админ едитора'],
    ] },
    { t: 'note', v: 'Сървърът е авторитетен: пълното състояние на партията не го напуска. Клиентът получава само изглед с това, което играчът има право да вижда — чужди роли, ръце и истинско злато никога не се изпращат.' },

    { t: 'h1', v: '2. Структура на решението' },
    { t: 'table', head: ['Папка', 'Съдържание'], widths: [3, 7], rows: [
      ['`src/Blackhood.Engine`', 'Чиста логика. `Gifts/` съдържа правилата (GiftsGame), AI-то на NPC-тата (GiftsAi), Домакина (GiftsHost), изгледа за клиента (GiftsView), всички числа (GiftsRules) и генератор на случайни числа, който се пази в партията (Rng).'],
      ['`src/Blackhood.Data`', 'DbContext, ентитита, миграции, моделът на персонажите, валидатор, внос/износ в JSON.'],
      ['`src/Blackhood.Api`', 'Endpoint-и: вход, таверна, дарове, персонажи, админ; миграция и внос при старт.'],
      ['`tests/Blackhood.Engine.Tests`', '23 теста: всяко правило плюс хиляди симулирани вечери.'],
      ['`client/`', 'Залата на таверната, Пристигането, масата за Дарове, админ едиторът.'],
      ['`content/`', '18 персонажа (P01–P13 участници, H01–H05 домакини) в JSON; портрети.'],
      ['`tools/`', 'Внос на листовете, английски превод, този генератор на Word документи.'],
      ['`docs/`', 'Тези документи и по-ранните бележки.'],
    ] },

    { t: 'h1', v: '3. Пускане' },
    { t: 'ol', v: [
      '**API** — във Visual Studio `Blackhood.Api` е стартовият проект; избери профил **http** и натисни F5. Мигрира LocalDB базата, внася липсващите персонажи и дава роля Admin на настроените потребители.',
      '**Клиент** — във VS Code: `cd client`, `npm install`, `npm run dev`, после отвори http://localhost:5173.',
      '**Тестов акаунт** (само локално): `tester` / `candlelight42`, роля Admin.',
      '**Тестове** — Test Explorer във Visual Studio или `dotnet test`.',
    ] },
    { t: 'p', v: 'Ако LocalDB „забрави“ базата („Cannot create file Blackhood.mdf because it already exists“), README-то обяснява как да я закачиш обратно, без да губиш данни.' },

    { t: 'h1', v: '4. Профили и кесията' },
    { t: 'ul', v: [
      'Регистрация, вход (с име или имейл) и изход с ASP.NET Core Identity и HTTP-only cookie.',
      'Всеки профил има **кесия** (`AppUser.Gold`, започва от 50). Кръчмарските игри се играят с това злато: Даровете взимат 10 злато за влизане и изплащат накрая.',
      'Роля **Admin** (проверява се в базата при всяка заявка) отключва едитора на персонажи.',
    ] },

    { t: 'h1', v: '5. Залата на таверната (главно меню)' },
    { t: 'ul', v: [
      'Текстова зала на страноприемницата Blackhood: сменящи се атмосферни редове („За миг свещите в далечния ъгъл горят синьо.“), четирима постоянни гости, които „са тук тази вечер“, с по един свой навик, и кесията ти.',
      'Маси: **Масата на домакина — Дарове** (играе се, с кратки правила и „върни се към недовършената вечер“), **Bones** (зарове, скоро), **The Cards of Fate** (гадаене, скоро), **Private tables** (мултиплейър, скоро).',
      'Последните ти пет вечери с такса и изплащане.',
      '3D карта на страноприемницата е планирана за по-късно.',
    ] },

    { t: 'h1', v: '6. Персонажите' },
    { t: 'h2', v: 'Модел на данните' },
    { t: 'ul', v: [
      '`Character`: код, вид (участник / домакин), име, титла, рядкост, статус (Чернова / Публикуван), етикети.',
      '**Умения** (с бюджет по рядкост: Common 15, Uncommon 17, Rare 18): Проницателност, Пресметливост, Хитрост, Влияние, Самообладание. **Подбуди**: Алчност, Дързост, Лоялност, Отмъстителност, Търпение, Амбиция, Гордост, Страх от загуба — колони, които AI-то чете.',
      '**По една JSON колона за всяка секция** от листа на персонажа (EF Core complex types): произход, външен вид, история, психология, поведение, мотиви, реч, механика (вкл. **„Странното“**), бележки на автора, профил на домакина.',
      'Дъщерни таблици: реплики по ситуации, знаци, вързани с вътрешни състояния, връзки, портрети; `PlayerEncounter` помни кого е срещал играчът.',
    ] },
    { t: 'h2', v: 'Съдържание и език' },
    { t: 'ul', v: [
      'Базата е източникът на истината; `content/characters/*.json` пълни нова база и се експортира обратно от едитора.',
      'Всички 18 персонажа са преведени на английски (`tools/translate-en/`). Добавени са реплики за смърт на всеки участник; персонажите без знак „лъже“ получиха такъв (освен Бъртрам и Илайъс, които никога не лъжат); правилата на домакините са пренаписани за Даровете.',
    ] },
    { t: 'h2', v: 'Админ едитор (`/admin`)' },
    { t: 'p', v: 'Списък с филтри и предупреждения; едитор с табове по листа, плъзгачи за уменията с лента на бюджета, редактори за реплики, знаци и връзки, качване на портрет, проверки на живо, преглед на масата, чернова/публикуван, Ctrl+S, защита при едновременна редакция, експорт в JSON. Изцяло на английски.' },

    { t: 'h1', v: '7. Пристигането' },
    { t: 'p', v: 'Всяка вечер започва с интерактивна сцена: вратата в дъжда → Домакинът (Джентълменът) те поздравява и избираш отговор („Кой сте вие?“ / „Да започваме.“ / мълчание) → шестте свещи се палят → четиримата гости влизат един по един, представени от Домакина, със своя реплика и един видим навик → Домакинът плъзга към теб тайната ти роля и я обръщаш. Вече срещнатите гости получават кратък поздрав. Може да се пропусне по всяко време.' },

    { t: 'h1', v: '8. Дарове — реализация' },
    { t: 'ul', v: [
      '**GiftsGame** — правилата като машина на състоянията: Шепот → Дарове → Доставка → Вино → Последен шепот → Зазоряване → Изхвърляне → (Посочване) → Край. NPC местата се играят автоматично; мястото на човека чака действия. Партия без човек се изиграва сама докрай (така работи симулаторът).',
      '**GiftsAi** — всяко решение на NPC е вероятност, оформена от характера му: кой купува амулет, кой затваря врата, с каква дума Убиецът маскира камата, кого напада (помни кой току-що е похарчил амулета си), кой на кого подхвърля кама, кого сочи призракът, кого посочва гостът накрая. Вярванията идват само от това, което това място може да знае (вино, монокъл, слухове, оцелели ками, разкрити мъртви, призраци).',
      '**Знаци** — когато NPC излъже с дар, може да се издаде в Дневника („Silas Crowe spins the coin between his knuckles.“); хитрите понякога показват фалшив знак.',
      '**GiftsHost** — офертите и цените се строят само от положението на играча и публичните факти.',
      '**GiftsView** — какво може да види човекът. **Rng** — детерминиран, пази се в партията, така че тя продължава еднакво след презареждане.',
    ] },
    { t: 'h2', v: 'Масата (клиент)' },
    { t: 'p', v: 'Маса от първо лице: Домакинът и свещите в дъното, четиримата гости с портрет, видимо злато (+/− на зазоряване), брой карти, призрачно състояние и разкрита роля; централен панел за текущата фаза (оферти и ценоразпис, дар в три стъпки с индикатор „честно/лъжа“, доставка с „Вземи / Затвори вратата / Вдигни огледалото“, проследяване с вино, спешна оферта, отчет на зазоряването, изхвърляне, посочване на Убиеца, крайни резултати и изплащане); твоята ръка, роля и кесия; публичният Дневник вляво и тайният Шепот вдясно. При смърт портретът се тресе, изпуска дим и се пропуква; призраците плуват в студени цветове.' },

    { t: 'h1', v: '9. API' },
    { t: 'table', head: ['Endpoint', 'За какво'], widths: [4, 6], rows: [
      ['`POST /api/auth/register · login · logout`, `GET /api/auth/me`', 'Профили (потребителят съдържа `gold` и `isAdmin`)'],
      ['`GET /api/tavern`', 'Кесия, гости в залата, недовършена вечер, последни вечери'],
      ['`POST /api/gifts/matches`', 'Сядане на масата: такса, четирима случайни гости, записване на срещите'],
      ['`GET /api/gifts/matches/{id}`', 'Изгледът на играча за вечерта'],
      ['`POST /api/gifts/matches/{id}/intro-complete`', 'Пристигането е изгледано или пропуснато'],
      ['`POST /api/gifts/matches/{id}/act`', 'Един ход: endWhisper, acceptOffer, declineOffer, buyService, monocle, sendGift, ghost, respond, wine, seeDawn, discard, point'],
      ['`GET /api/characters/{code}/portrait`', 'Портрет'],
      ['`/api/admin/characters…`', 'Списък, четене, създаване, промяна, изтриване, проверка, експорт, качване на портрет (само Admin)'],
    ] },

    { t: 'h1', v: '10. База данни' },
    { t: 'p', v: 'Миграции: `Initial` (Identity, партии), `Characters` (системата за персонажи), `GiftsAndGold` (злато на потребителя, вид игра, такса, изплащане, уреждане). Партиите от първия прототип са запазени, но са маркирани `Legacy` и вече не се играят.' },

    { t: 'h1', v: '11. Тестове и симулация' },
    { t: 'ul', v: [
      '16 теста за правилата: начало, квота, смъртоносни и безвредни ками, безопасната първа свещ, амулети, врати, огледала (и наградата за убиване на Убиеца), времето на отровата, „всяка смърт се брои“, правила за изхвърляне, проверка на даровете, скрита информация в изгледа, виното.',
      'Симулация: 3000 вечери AI при 5 играчи (Убиецът 44% / Гостите 56%), всеки размер на масата от 5 до 8, детерминизъм и скриптиран човек, който прави случайни позволени ходове през всяка фаза в 400 вечери.',
      'Симулаторът хвана истински проблеми по време на разработката: празен сандък, който чупеше офертите, и 21% победи за Убиеца, които доведоха до сегашните числа.',
    ] },

    { t: 'h1', v: '12. Контрол на версиите' },
    { t: 'p', v: 'GitHub: https://github.com/mivanov9348/Blackhood — `master` съдържа състоянието преди Даровете; работата по Даровете е готова за commit.' },

    { t: 'h1', v: '13. Известни ограничения и следващи стъпки' },
    { t: 'ul', v: [
      'Масата се води само от главния домакин (Джентълмена); другите четирима домакини и променените им правила са описани, но още не са активни.',
      'Специалните умения на персонажите са описани, но още не са част от правилата на Даровете.',
      'Портретите са оцветени силуети, докато няма истински рисунки; изразите (самодоволен, нажежен, уплашен, мъртъв) идват с рисунките.',
      'Повечето участници показват предупреждения за бюджета на уменията в едитора — числата в листовете са писани преди това правило.',
      'Следва: звук и още по-кинематографични смърти, досието („Стената на записите“), масата за зарове, гадаенето, мултиплейър, 3D страноприемница.',
    ] },
  ],
}

module.exports = {
  'Blackhood-Gifts-Rules-EN.docx': rulesEn,
  'Blackhood-Gifts-Rules-BG.docx': rulesBg,
  'Blackhood-Documentation-EN.docx': docsEn,
  'Blackhood-Documentation-BG.docx': docsBg,
}
