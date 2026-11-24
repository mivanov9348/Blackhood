// Prints a compact summary of each character block (for authoring additions.mjs).
import fs from 'node:fs'
const lines = fs.readFileSync(process.argv[2], 'utf8').replace(/^﻿/, '').split(/\r?\n/).map((l) => l.trim())
const want = ['Име', 'Прякор / титла', 'Рядкост (Common / Uncommon / Rare / Unique)', 'Жестове и навици', 'Допълнителни визуални бележки:', 'Най-голяма слабост', 'Какво никога не би направил?', 'Какво крие от останалите?', 'Insight – Проницателност', 'Cunning – Хитрост', 'Composure – Самообладание', 'Charisma – Влияние', 'Ambition – Амбиция', 'Основна черта на характера', 'Втора характерна черта', 'Най-голям страх', 'При първа среща:', 'Как се представя пред гостите?', 'Какво го ядосва?', 'Каква цена изисква?']
for (let i = 0; i < lines.length; i++) {
  const m = /^([PH]\d{2}) · (.+)$/.exec(lines[i])
  if (m && !lines[i].includes(' – ') && lines[i + 1]?.includes(' · ')) console.log(`\n### ${lines[i]} | ${lines[i + 1]}`)
  if (want.includes(lines[i])) {
    let j = i + 1
    while (lines[j] === '') j++
    console.log(`  ${lines[i].slice(0, 22)}: ${lines[j]}`)
  }
}
