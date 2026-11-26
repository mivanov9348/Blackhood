// Builds the Word documents in docs/:
//   Blackhood-Gifts-Rules-EN.docx / -BG.docx
//   Blackhood-Documentation-EN.docx / -BG.docx
// Usage: node tools/word-docs/build.js
const fs = require('fs')
const path = require('path')
const {
  AlignmentType, BorderStyle, Document, Footer, Header, HeadingLevel, LevelFormat, Packer, PageNumber,
  Paragraph, ShadingType, Table, TableCell, TableRow, TextRun, WidthType,
} = require('docx')
const content = require('./content')

const OUT = path.resolve(__dirname, '../../docs')
const FONT = 'Calibri'
const HEAD_FONT = 'Cambria'
const ACCENT = '7A1A12'
const PAGE_W = 11906 // A4
const MARGIN = 1134 // 2 cm
const TEXT_W = PAGE_W - 2 * MARGIN

// ---- inline markup: **bold**, *italic*, `code` ----
function runs(text, base = {}) {
  const out = []
  const re = /(\*\*[^*]+\*\*|\*[^*]+\*|`[^`]+`)/g
  let last = 0
  let m
  while ((m = re.exec(text))) {
    if (m.index > last) out.push(new TextRun({ text: text.slice(last, m.index), ...base }))
    const t = m[0]
    if (t.startsWith('**')) out.push(new TextRun({ text: t.slice(2, -2), bold: true, ...base }))
    else if (t.startsWith('`')) out.push(new TextRun({ text: t.slice(1, -1), font: 'Consolas', size: 20, ...base }))
    else out.push(new TextRun({ text: t.slice(1, -1), italics: true, ...base }))
    last = m.index + t.length
  }
  if (last < text.length) out.push(new TextRun({ text: text.slice(last), ...base }))
  return out
}

const P = (text, opts = {}) => new Paragraph({ children: runs(text), spacing: { after: 120 }, ...opts })
const H1 = (text) => new Paragraph({ text, heading: HeadingLevel.HEADING_1 })
const H2 = (text) => new Paragraph({ text, heading: HeadingLevel.HEADING_2 })
const Bullets = (items) => items.map((t) => new Paragraph({ children: runs(t), numbering: { reference: 'bullets', level: 0 }, spacing: { after: 60 } }))
let numberedId = 0
const Numbered = (items) => {
  const ref = `numbered-${numberedId++}`
  numberingConfigs.push({ reference: ref, levels: [{ level: 0, format: LevelFormat.DECIMAL, text: '%1.', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 540, hanging: 360 } } } }] })
  return items.map((t) => new Paragraph({ children: runs(t), numbering: { reference: ref, level: 0 }, spacing: { after: 80 } }))
}
const Note = (text) => new Paragraph({
  children: runs(text, { italics: true, color: '5A4128' }),
  spacing: { before: 60, after: 160 },
  border: { left: { style: BorderStyle.SINGLE, size: 12, color: ACCENT, space: 8 } },
  indent: { left: 200 },
})

function Grid(header, rows, widths) {
  const total = widths.reduce((a, b) => a + b, 0)
  const scaled = widths.map((w) => Math.floor((w / total) * TEXT_W))
  scaled[scaled.length - 1] += TEXT_W - scaled.reduce((a, b) => a + b, 0)
  const border = { style: BorderStyle.SINGLE, size: 4, color: 'BFAF95' }
  const borders = { top: border, bottom: border, left: border, right: border }
  const cell = (text, i, head) => new TableCell({
    width: { size: scaled[i], type: WidthType.DXA },
    borders,
    shading: head ? { type: ShadingType.CLEAR, color: 'auto', fill: 'EFE4CC' } : undefined,
    margins: { top: 60, bottom: 60, left: 100, right: 100 },
    children: [new Paragraph({ children: runs(String(text), head ? { bold: true } : {}) })],
  })
  return new Table({
    width: { size: TEXT_W, type: WidthType.DXA },
    columnWidths: scaled,
    rows: [
      new TableRow({ tableHeader: true, children: header.map((h, i) => cell(h, i, true)) }),
      ...rows.map((r) => new TableRow({ children: r.map((c, i) => cell(c, i, false)) })),
    ],
  })
}

const Spacer = () => new Paragraph({ text: '', spacing: { after: 60 } })

/** Turns the plain content description into docx blocks. */
function render(blocks) {
  const out = []
  for (const b of blocks) {
    switch (b.t) {
      case 'h1': out.push(H1(b.v)); break
      case 'h2': out.push(H2(b.v)); break
      case 'p': out.push(P(b.v)); break
      case 'note': out.push(Note(b.v)); break
      case 'ul': out.push(...Bullets(b.v)); break
      case 'ol': out.push(...Numbered(b.v)); break
      case 'table': out.push(Grid(b.head, b.rows, b.widths), Spacer()); break
    }
  }
  return out
}

let numberingConfigs = []

function buildDoc({ title, subtitle, header, blocks }) {
  numberingConfigs = [{ reference: 'bullets', levels: [{ level: 0, format: LevelFormat.BULLET, text: '•', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 540, hanging: 300 } } } }] }]
  const body = render(blocks)
  return new Document({
    creator: 'Blackhood',
    title,
    styles: {
      default: { document: { run: { font: FONT, size: 22 } } },
      paragraphStyles: [
        { id: 'Title', name: 'Title', basedOn: 'Normal', run: { font: HEAD_FONT, size: 56, bold: true, color: ACCENT }, paragraph: { spacing: { before: 0, after: 80 }, keepNext: true } },
        { id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Normal', quickFormat: true, run: { font: HEAD_FONT, size: 32, bold: true, color: ACCENT }, paragraph: { spacing: { before: 320, after: 120 }, keepNext: true, outlineLevel: 0 } },
        { id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Normal', quickFormat: true, run: { font: HEAD_FONT, size: 26, bold: true, color: '3A2617' }, paragraph: { spacing: { before: 220, after: 80 }, keepNext: true, outlineLevel: 1 } },
      ],
    },
    numbering: { config: numberingConfigs },
    sections: [{
      properties: { page: { size: { width: PAGE_W, height: 16838 }, margin: { top: MARGIN, bottom: MARGIN, left: MARGIN, right: MARGIN } } },
      headers: { default: new Header({ children: [new Paragraph({ alignment: AlignmentType.RIGHT, children: [new TextRun({ text: header, size: 16, color: '8A7558' })] })] }) },
      footers: { default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ children: [PageNumber.CURRENT], size: 18, color: '8A7558' })] })] }) },
      children: [
        new Paragraph({ text: title, heading: HeadingLevel.TITLE }),
        new Paragraph({ children: [new TextRun({ text: subtitle, italics: true, color: '5A4128' })], spacing: { after: 240 }, border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: ACCENT, space: 6 } } }),
        ...body,
      ],
    }],
  })
}

;(async () => {
  fs.mkdirSync(OUT, { recursive: true })
  for (const [file, doc] of Object.entries(content)) {
    const buffer = await Packer.toBuffer(buildDoc(doc))
    fs.writeFileSync(path.join(OUT, file), buffer)
    console.log('wrote docs/' + file)
  }
})()
