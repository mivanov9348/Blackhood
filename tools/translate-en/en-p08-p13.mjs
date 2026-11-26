// English text for P08–P13.
export default {
  P08: {
    name: 'Mara Flint', title: 'The Shadow', nickname: 'The Shadow', gender: 'Female', age: '27', archetype: 'Sly',
    origin: { birthplace: 'The streets of Saltmarsh, no home', socialStatus: 'No home and no patron, lives on the road', profession: 'Courier and messenger; carries letters and parcels between towns without asking questions' },
    appearance: {
      face: 'A thin, sharp face, quick eyes, a thin scar across the lip', hairAndEyes: 'Short-cropped dark hair, grey-green eyes',
      clothing: 'A dark hood, a tight leather vest, many hidden pockets, a leather letter bag', build: 'Short, wiry, always sits near the exit',
      gestures: 'Counts the exits in the room; turns small objects from the table between her fingers without looking at them', voice: 'A quiet, husky voice; speaks briefly and mockingly',
      firstImpression: 'A harmless street girl who sees more than she shows', artistNotes: 'Wears three different cheap rings won at dice. Always sits where she can see the door.',
    },
    backstory: {
      biography: 'Mara never had a home. Her mother left her at the docks and the streets taught her to survive before she learned to read. Today she carries letters and parcels between towns: fast, quiet, no questions. She works alone and never gets attached to anyone.',
      whyHere: 'Money for a new life. She wants to win enough to stop running from town to town.',
      secret: 'Where she comes from, and how much she misses a place she could call home.',
    },
    psychology: {
      coreTrait: 'Cunning', secondTrait: 'Independence', weakness: 'Trusts no one and pushes away even those who want to help', fear: 'Being caged — by law, debt or a person',
      desire: 'Freedom, and enough gold to depend on no one', angers: 'Being treated like rubbish', regret: 'The people she left without saying goodbye',
      moralLimits: 'Does not cheat children or the sick', neverWould: 'Stay in one place for long', wouldForReward: 'Break almost any promise',
    },
    behavior: {
      style: 'Manipulative / sly', stakes: 'Small, quick stakes. Prefers to wait for others\' mistakes', deception: 'A way of life', debts: 'Never borrows. Wants no one to have power over her',
      alliances: 'Short-lived. Always the first to leave an alliance', whenAttacks: 'When the opponent is distracted', whenRetreats: 'When someone suspects her',
      onLoss: 'Drops out of focus, waits', onWin: 'Shows no joy', strategyChange: 'If she is exposed',
    },
    motives: { publicGoal: 'To win a little and disappear', hiddenGoal: 'To win enough for a new life and disappear', personalReason: 'Freedom', betrayalTrigger: 'Always, if the gain is big', helpTrigger: 'If someone helped her without asking for anything in return' },
    speech: { register: 'Short street phrases', favoriteWords: '"Hm", "normal"', avoidedTopics: 'Her childhood', howLies: 'Calmly, looking you in the eye', angerAndFear: 'Goes very quiet and withdraws' },
    mechanics: {
      strangeness: 'Nobody saw her come in. She is simply already seated when you turn around.', principle: 'Stay in one place for long',
      temptation: 'Trusts no one and pushes away even those who want to help', ability: 'Shadow: once per evening her night action cannot be seen with a Monocle.',
      hostRelation: 'Fear — the host is a cage, and she hates cages.',
    },
    authorNotes: { distinctive: 'Lonely, distrustful and cunning. Always one step from the door.', immutable: 'The cunning, the independence and the loneliness.', tunable: 'How early she leaves alliances.' },
    lines: {
      FirstMeeting: 'Mara. Just Mara. Don\'t ask where I\'m from.', Win: 'Hm. Fine.', Loss: 'I\'ll get it back. I always do.', Accused: 'Me? Look at your own cards before you accuse anyone.',
      AllianceOffer: 'An alliance? As long as it pays.', Betrayal: 'Normal. Everyone does it.', BigBet: 'Risky. I like it.',
      HostIntroduction: 'Mara Flint, the Shadow. She carries other people\'s letters and other people\'s secrets. Guard your purses.', MetAgain: '…You again.', Death: 'Should have… sat by the door.',
    },
    tells: ['Turns small objects from the table between her fingers without looking at them', 'Counts the exits in the room'],
  },

  P09: {
    name: 'Robin Sallow', title: 'The Pilgrim', nickname: 'The Pilgrim; "Brother Robin"', gender: 'Male', age: '34', archetype: 'Social manipulator',
    origin: { birthplace: 'Claims to come from a monastery in the north. Actually from Saltmarsh', socialStatus: 'A wandering pilgrim and preacher, lives on alms', profession: 'Wandering preacher, teller of parables and miracle stories' },
    appearance: {
      face: 'A meek, kind face, a broad smile, "good" eyes', hairAndEyes: 'Light brown hair, short beard, blue eyes', clothing: 'A simple grey pilgrim\'s cloak, a staff, a wooden medallion of a saint',
      build: 'Slim, slightly stooped, "humble"', gestures: 'Blesses everyone; touches the medallion when he exaggerates', voice: 'A soft, warm, persuasive voice',
      firstImpression: 'A good-natured, holy man anyone would confess to', artistNotes: 'The wooden medallion is worn smooth from touching; under the cloak he wears fine leather boots that do not suit his humble look.',
    },
    backstory: {
      biography: 'Robin grew up in Saltmarsh, the son of a poor washerwoman. He learned early that people give more to a holy man than to a poor child. He put on a pilgrim\'s cloak and took to the roads telling parables and miracle stories. His faith is half real, half role, and he has long stopped knowing which half is larger.',
      whyHere: 'Money and an audience. The table is full of people with something to lose who need someone to comfort them.',
      secret: 'That he is from Saltmarsh, that he loves comfort more than he preaches, and that sometimes he believes his own stories.',
    },
    psychology: {
      coreTrait: 'Charm', secondTrait: 'Hypocrisy', weakness: 'Believes his own stories far too much', fear: 'Being exposed in public',
      desire: 'To live richly and still be loved like a saint', angers: 'Being unmasked', regret: 'His mother, whom he abandoned',
      moralLimits: 'Does not hurt anyone physically', neverWould: 'Admit a lie voluntarily', wouldForReward: 'Play anyone, even a friend',
    },
    behavior: {
      style: 'Social / manipulative', stakes: 'Moderate; presents them as "God\'s will"', deception: 'The art of living', debts: 'Takes and rarely repays', alliances: 'Makes them easily, betrays them easily',
      whenAttacks: 'When the victim is emotionally vulnerable', whenRetreats: 'When someone starts questioning him', onLoss: '"God\'s will"', onWin: '"A blessing"', strategyChange: 'If he is exposed',
    },
    motives: { publicGoal: 'To "bless" the table', hiddenGoal: 'To win the trust and money of the richest, most frightened person at the table', personalReason: 'Money', betrayalTrigger: 'Always, if a bigger victim appears', helpTrigger: 'If helping builds his image' },
    speech: { register: 'Soft, religious, full of parables', favoriteWords: '"Brother", "sister", "God\'s will"', avoidedTopics: 'His origins, Saltmarsh', howLies: 'With full conviction and exaggerated sincerity', angerAndFear: 'Anger comes out as "holy wrath"; when afraid he prays' },
    mechanics: {
      strangeness: 'The candles near him burn brighter when he lies, and dimmer when he tells the truth.', principle: 'Admit a lie voluntarily',
      temptation: 'Believes his own stories far too much', ability: 'Blessing: once per evening he turns away one gift sent to him, without a door.',
      hostRelation: 'Ignorance — he believes he can convert the host.',
    },
    authorNotes: { distinctive: 'A charming hypocrite in a saint\'s robes. You never know when he believes and when he plays.', immutable: 'The hypocrisy, the charm and the blessings.', tunable: 'How convincing he is and how easily he is unmasked.' },
    lines: {
      FirstMeeting: 'Peace be with you, brothers and sisters. I am only a humble traveller.', Win: 'A blessing! Heaven was with me.', Loss: 'God\'s will. I accept it humbly.',
      Accused: 'How could… I am only a poor pilgrim!', AllianceOffer: 'An alliance is a sacred thing, brother.', Betrayal: 'I shall pray for your soul.', BigBet: 'Heaven will decide.',
      HostIntroduction: 'Brother Robin, a pilgrim. He has visited every shrine in the kingdom… at least in his stories.', MetAgain: 'Peace be with you again, brother. Heaven brings us together once more.',
      Death: 'Now… I will find out… whether it was true.',
    },
    tells: ['Touches his wooden medallion when he exaggerates', 'Blesses everyone'],
  },

  P10: {
    name: 'Bertram Cole', title: 'The Veteran', nickname: 'The Veteran; "Old Cole"', gender: 'Male', age: '56', archetype: 'Survivor',
    origin: { birthplace: 'The village of Oldbrook', socialStatus: 'A retired soldier, poor', profession: 'Infantry sergeant who served in several wars' },
    appearance: {
      face: 'A furrowed, scarred face; a missing eye under a patch', hairAndEyes: 'Grey hair, thick grey beard, one brown eye', clothing: 'An old military tunic, a heavy cloak, a sabre',
      build: 'Big, steady, limping', gestures: 'Counts the people in the room; rubs his old wound', voice: 'A rough, low soldier\'s voice',
      firstImpression: 'An old soldier who has seen too much', artistNotes: 'Carries a pouch of the soldier\'s tokens of his fallen comrades.',
    },
    backstory: {
      biography: 'Bertram served thirty years in the royal infantry. He survived every battle but lost all his comrades. He has seen enough wars to know that the heroes are rarely the ones in the songs. Today he lives alone on a small pension and drinks more than he should.',
      whyHere: 'He has nothing to live for. Here he can win enough to support the families of his fallen comrades.',
      secret: 'That once, in his hardest battle, he ran. He survived; the boy beside him did not.',
    },
    psychology: {
      coreTrait: 'Survival', secondTrait: 'Loyalty to comrades', weakness: 'Distrusts promises', fear: 'Dying for nothing', desire: 'To do something good before he dies',
      angers: 'Cowards and traitors', regret: 'The day he ran from the battlefield', moralLimits: 'Never abandons his comrades', neverWould: 'Betray a comrade', wouldForReward: 'Risk his life for a comrade',
    },
    behavior: {
      style: 'Defensive', stakes: 'Careful, measured stakes. Never more than he can lose', deception: 'Cannot stand it. If he finds a deception, he says it out loud',
      debts: 'Pays them at once. A debt is a wound that never heals for him', alliances: 'Extremely loyal to those who keep their word', whenAttacks: 'When a comrade is in trouble or someone lies openly',
      whenRetreats: 'When the risk is pointless. "The dead don\'t spend gold."', onLoss: 'Silently. Takes a sip and carries on', onWin: 'Without joy, just a nod', strategyChange: 'If someone keeps their word, he may trust them',
    },
    motives: { publicGoal: 'To win a decent sum', hiddenGoal: 'To provide money for the families of the fallen', personalReason: 'A debt to the dead', betrayalTrigger: 'Nothing', helpTrigger: 'If someone is in trouble' },
    speech: { register: 'Rough, short, soldierly', favoriteWords: '"Lad", "comrade"', avoidedTopics: 'The wars', howLies: 'Does not lie', angerAndFear: 'Goes silent and clenches his jaw' },
    mechanics: {
      strangeness: 'Sometimes he talks to the empty chair beside him. And the chair creaks in reply.', principle: 'Betray a comrade',
      temptation: 'Distrusts promises', ability: 'Veteran: survives the first killing blow against him if he holds a Closed Door.',
      hostRelation: 'Service — he has seen many commanders; the host is just one more.',
    },
    authorNotes: { distinctive: 'An old soldier for whom a word weighs more than gold.', immutable: 'The survival, the loyalty, the silent guilt.', tunable: 'How carefully he stakes and how easily he trusts.' },
    lines: {
      FirstMeeting: 'Bertram Cole. Old soldier. Don\'t expect much talk.', Win: 'Good. Let\'s see how long it lasts.', Loss: 'I\'ve seen worse.', Accused: 'Say that again, lad.',
      AllianceOffer: 'Your word. Nothing else interests me.', Betrayal: 'Knew it.', BigBet: 'Risk is for the young.',
      HostIntroduction: 'Bertram Cole. He has survived more battles than he can count. Tonight we learn whether it was luck or skill.', MetAgain: 'You again. Still alive. Good.',
      Death: 'Lads… save me a place.',
    },
    tells: ['Rubs his old wound', 'Counts the people in the room'],
    relations: [{ story: 'He served under Noah Thorn at Ashford and is alive because of the retreat.', effect: 'He never turns on Noah.' }],
  },

  P11: {
    name: 'Thomas Wycliffe', title: 'The Cowardly Heir', nickname: 'The Cowardly Heir; "Little Wycliffe"', gender: 'Male', age: '24', archetype: 'Cowardly but clever',
    origin: { birthplace: 'The Wycliffe estate near the capital', socialStatus: 'A rich heir of a merchant family that bought its nobility', profession: 'Heir who has never worked; studied mathematics and bookkeeping' },
    appearance: {
      face: 'A young, pale, smooth face; bites his lips', hairAndEyes: 'Curly blond hair, frightened blue eyes', clothing: 'Expensive clothes that do not fit him, a purse pressed to his chest, a small ledger',
      build: 'Thin, hunched, sits on the edge of the chair', gestures: 'Calculates in his head and moves his lips; glances at the door', voice: 'A quiet, unsure voice; stammers under pressure',
      firstImpression: 'A frightened rich boy who should not be here', artistNotes: 'In the ledger he keeps an exact record of every stake at the table. Wears his father\'s ring, which is too big for him.',
    },
    backstory: {
      biography: 'Thomas is the only son of a rich merchant who bought a noble title. His father decided everything for him. A month ago his father died and Thomas inherited a vast fortune and many enemies. He does not know whom to trust. He is clever and excellent with numbers, but has no courage to make a decision.',
      whyHere: 'His uncle (who wants the inheritance) sent him "to learn to be a man". Thomas suspects his uncle wants him to lose everything.',
      secret: 'That he is cleverer than he shows and keeps a full account of everyone at the table.',
    },
    psychology: {
      coreTrait: 'Fear', secondTrait: 'Intelligence', weakness: 'Cowardice', fear: 'Being killed or robbed', desire: 'To be safe', angers: 'Being ignored', regret: 'That he was never good enough for his father',
      moralLimits: 'Hurts no one', neverWould: 'Take a big risk alone', wouldForReward: 'Betray anyone if he feels in danger',
    },
    behavior: {
      style: 'Defensive', stakes: 'Small, careful, precisely calculated', deception: 'Does not use it, but recognises it in the numbers', debts: 'Hates owing and pays at once',
      alliances: 'Looks for a protector and gets strongly attached', whenAttacks: 'Only when the numbers are clearly in his favour', whenRetreats: 'At the first sign of danger or threat',
      onLoss: 'Panics and wants to leave the table', onWin: 'Relief and gratitude to his protector', strategyChange: 'If his protector abandons him or someone threatens him',
    },
    motives: { publicGoal: 'Not to lose', hiddenGoal: 'To find a protector he can trust', personalReason: 'Forced by his uncle', betrayalTrigger: 'If his protector abandons him', helpTrigger: 'If he feels protected' },
    speech: { register: 'Unsure, polite, sometimes precise with numbers', favoriteWords: '"Excuse me", "I… I don\'t know"', avoidedTopics: 'His father', howLies: 'Badly. Stammers', angerAndFear: 'Stammers, turns pale' },
    mechanics: {
      strangeness: 'His father\'s ring is far too big, yet it never falls off — however much his hand shakes.', principle: 'Take a big risk alone',
      temptation: 'Cowardice', ability: 'The Bookkeeper: sees the full list of every gift announced tonight and who received it.',
      hostRelation: 'Fear — the strongest at the table.',
    },
    authorNotes: { distinctive: 'A cowardly but clever character who can be a very useful ally.', immutable: 'The fear and the intelligence.', tunable: 'How easily he runs.' },
    lines: {
      FirstMeeting: 'I… I\'m Thomas. Wycliffe. Pleased to meet you.', Win: 'Oh! Really? Did I win?', Loss: 'I knew it… I knew this would happen.', Accused: 'N-no! It wasn\'t me!',
      AllianceOffer: 'Really? You\'ll protect me?', Betrayal: 'But… you promised!', BigBet: 'No… I can\'t… it\'s too much.',
      HostIntroduction: 'Thomas Wycliffe, the heir. He trembles like a leaf — but watch his ledger, not his hands.', MetAgain: 'Oh… you. H-hello again.', Death: 'I… I knew it…',
    },
    tells: ['Calculates in his head and moves his lips', 'Glances at the door'],
  },

  P12: {
    name: 'Godfrey Mott', title: 'The Miser', nickname: 'The Miser; "Old Mott"', gender: 'Male', age: '68', archetype: 'Greedy',
    origin: { birthplace: 'Saltmarsh', socialStatus: 'One of the richest men in the region, yet lives like a beggar', profession: 'Moneylender, creditor to half the town, owner of houses and land' },
    appearance: {
      face: 'A wrinkled grey face, sunken cheeks, small sharp eyes that follow every coin', hairAndEyes: 'Thin grey hair, an unkempt beard, watery grey eyes',
      clothing: 'A worn coat patched many times; a purse and a folder of papers sewn into the lining', build: 'Thin, hunched, as if guarding something under his arm',
      gestures: 'Counts coins even when it is not his turn; trembles and smacks his lips when he has to pay', voice: 'A creaky, quiet voice; sighs often and complains of poverty',
      firstImpression: 'A poor old man who seems about to starve — until he starts talking about money', artistNotes: 'In his coat lining he carries a folder of bonds and debt records, along with a locket holding a portrait of his late daughter.',
    },
    backstory: {
      biography: 'Godfrey started as a poor apprentice to a moneylender and built his fortune loan by loan, interest by interest. He could buy half the town but lives in a cold room eating bread and onions. Half the town owes him money and the other half hopes it never will. He trusts no banks, no safes and no people, so he carries his most important papers on him.',
      whyHere: 'Someone told him he could "double everything" here. He doesn\'t believe it, but cannot bear the thought of someone else doing it instead.',
      secret: 'That his daughter died of fever because he would not pay for a doctor. Her locket is the one thing he would never sell at any price.',
    },
    psychology: {
      coreTrait: 'Greed', secondTrait: 'Suspicion', weakness: 'Feels physical pain when he has to pay, and so misses good chances', fear: 'Dying poor',
      desire: 'To own everything and everyone', angers: 'Debtors who don\'t pay, and spendthrifts', regret: 'His daughter\'s death',
      moralLimits: 'Keeps to the letter of every contract', neverWould: 'Forgive a debt', wouldForReward: 'Almost anything, but only against a signed paper',
    },
    behavior: {
      style: 'Defensive / calculating', stakes: 'Small and safe. Big only if the opponent is desperate', deception: 'Accepts it if it profits and is legal on paper',
      debts: 'Loves them — as long as they are others\'. Keeps an exact record of every one', alliances: 'Rare. Only against interest or a signed paper', whenAttacks: 'When an opponent is weak and exposed',
      whenRetreats: 'When he has to pay more than planned', onLoss: 'Groans, complains and asks for a deferral', onWin: 'Quietly pleased. Immediately calculates the interest',
      strategyChange: 'The danger of losing more than a third of his wealth',
    },
    motives: { publicGoal: 'To win and make a loan at good interest', hiddenGoal: 'To multiply his money without risking anything of his own', personalReason: 'Greed, but also fear. Money is the only thing that makes him feel safe', betrayalTrigger: 'If an ally becomes more expensive than useful', helpTrigger: 'Only against interest — or if someone touches the memory of his daughter' },
    speech: { register: 'Creaky, petty, full of numbers', favoriteWords: '"How much?", "my money", "due date"', avoidedTopics: 'His daughter, his expenses', howLies: 'Pitifully. Pretends to be poor', angerAndFear: 'Trembles, his voice thins. When angry he talks about interest' },
    mechanics: {
      strangeness: 'The coins in his purse jingle even when he is perfectly still — as if they were counting themselves.', principle: 'Never forgives a debt.',
      temptation: 'Feels physical pain when he has to pay, and so misses good chances', ability: 'Loan: gives 2 gold now; you repay 3 at the end, or the host takes one of your cards.',
      hostRelation: 'Debt — the only debtor Godfrey is afraid will not pay him.',
    },
    authorNotes: { distinctive: 'A greedy old man with one human weakness: his daughter\'s locket.', immutable: 'The stinginess and the locket.', tunable: 'How dearly he lends and how easily he risks.' },
    lines: {
      FirstMeeting: 'Godfrey Mott. Don\'t expect gifts. Or drinks.', Win: 'Hm. Good. Good… Five percent. No, six.', Loss: 'Oh! Oh, my money! This is robbery!',
      Accused: 'Me? I\'m a poor old man! Look at my coat!', AllianceOffer: 'An alliance? What will it cost me? And how much will you pay me?', Betrayal: 'You will pay. Everyone pays in the end.',
      BigBet: 'No… it\'s too much… All right. But only once.',
      HostIntroduction: 'Godfrey Mott. He holds a bond on everyone in Saltmarsh — and probably on one of you.', MetAgain: 'You again. Do you owe me something? Probably.', Death: 'My… locket… don\'t let them…',
    },
    tells: ['Trembles and smacks his lips when he has to pay', 'Counts coins even when it is not his turn'],
  },

  P13: {
    name: 'Elias Rook', title: 'The Gravedigger', nickname: 'The Gravedigger; "the last companion"', gender: 'Male', age: '61', archetype: 'Grim observer',
    origin: { birthplace: 'The graveyards of Blackmoor', socialStatus: 'Poor and avoided, but secretly respected', profession: 'A gravedigger all his life, son and grandson of gravediggers' },
    appearance: {
      face: 'A gaunt, pale face, sunken cheeks; deep lines as if dug with a spade', hairAndEyes: 'Long thin white hair; a white film over the left eye, the right one pale grey and piercing',
      clothing: 'A threadbare black cloak, leather gloves, earth clinging to his boots', build: 'Tall, bony, slightly stooped',
      gestures: 'Looks people over as if measuring them for a coffin; drums on the table in a slow rhythm', voice: 'A slow, low, hoarse voice; long pauses',
      firstImpression: 'A grim old man who makes everyone uncomfortable', artistNotes: 'Around his neck on a chain hangs a tiny old spade the size of a thumb, a gift from his father, also a gravedigger.',
    },
    backstory: {
      biography: 'Elias has buried more people than most have met. He says the dead are more honest than the living because they have nothing to hide. He has seen heirs weep at the grave and quarrel over the estate on the way home. He cares nothing for money and does not fear death.',
      whyHere: 'Not wealth. He wants to see how the living reveal themselves when the stakes are high.',
      secret: 'That he is alone: no family, no friends, only graves. And that he comes to the table because here, at least, someone talks to him.',
    },
    psychology: {
      coreTrait: 'Grim honesty', secondTrait: 'Fearlessness', weakness: 'Indifferent to winning; sometimes gives up just when he could win', fear: 'Dying with no one to dig his grave',
      desire: 'For the truth to be told, at least once', angers: 'Lies over a grave: hypocritical eulogies and false tears', regret: 'That he spent his life among the dead, not the living',
      moralLimits: 'Does not lie. Does not rob the dead for profit', neverWould: 'Swear to a lie', wouldForReward: 'Almost nothing. Rewards do not interest him',
    },
    behavior: {
      style: 'Observant / passive', stakes: 'Indifferent to stakes. Bets the minimum to stay at the table', deception: 'Does not cheat. Recognises deception and sometimes says so aloud',
      debts: 'Has no debts and gives no loans', alliances: 'Rare. Only with people who have told him the truth', whenAttacks: 'Almost never. Attacks with words, not stakes',
      whenRetreats: 'When he is bored or has seen enough', onLoss: 'Without emotion: "The dead don\'t lose."', onWin: 'Without joy: "The dead don\'t win."', strategyChange: 'Only if someone asks him an honest question',
    },
    motives: { publicGoal: 'To watch', hiddenGoal: 'To see at least one person tell the truth when a lie would profit them', personalReason: 'Curiosity and loneliness', betrayalTrigger: 'Nothing. He is on no one\'s side', helpTrigger: 'If the person is honest with him' },
    speech: { register: 'Grim, slow, short aphorisms about death', favoriteWords: '"The dead…", "the earth", "everything ends"', avoidedTopics: 'His loneliness', howLies: 'Does not lie. When he doesn\'t want to say something, he stays silent', angerAndFear: 'Shows no fear. His anger is cold and quiet' },
    mechanics: {
      strangeness: 'He hears the dead. When someone at the table will die tonight, Elias already looks at them a little longer.', principle: 'Swear to a lie',
      temptation: 'Indifferent to winning; sometimes gives up just when he could win', ability: 'The last companion: learns the role of every dead player before the others and can speak with the Ghost.',
      hostRelation: 'Indifference — to the gravedigger the host is just one more he will dig a grave for someday.',
    },
    authorNotes: { distinctive: 'A grim observer who wants nothing, and is therefore hard to manipulate.', immutable: 'The indifference to money, the honesty, the dark humour.', tunable: 'How passively he plays and how often he comments aloud.' },
    lines: {
      FirstMeeting: 'Elias Rook. Gravedigger. We\'ll meet again someday — professionally.', Win: 'The dead don\'t win. But I\'m not dead yet.', Loss: 'The dead don\'t lose. That\'s why they\'re calm.',
      Accused: 'The dead don\'t lie. Neither do I.', AllianceOffer: 'Why? I want nothing from you.', Betrayal: 'Expected. The living always betray.', BigBet: 'It\'s all the same to me. Everything ends in the earth.',
      HostIntroduction: 'Elias Rook, the gravedigger. You play for gold. He plays for company.', MetAgain: 'Not yet. But soon.', Death: 'Finally… someone else digs.',
    },
    tells: ['Drums on the table in a slow rhythm', 'Looks people over as if measuring them for a coffin'],
    relations: [{ story: 'He buried Godfrey\'s daughter. He knows why she died.', effect: 'Once per evening he hints to the table that Godfrey is hiding something.' }],
  },
}
