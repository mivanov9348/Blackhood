import { useCallback, useEffect, useState, type ChangeEvent, type ReactNode } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { api, upload } from '../../api'
import { Portrait } from '../../components/Portrait'
import '../../admin.css'
import {
  driveDefs, emptyCharacter, hostAttrDefs, lineSituations, situationLabel, skillDefs, tagSuggestions, tellStateLabel, tellStates, textSections,
  type CharacterDoc, type CharacterKind, type CharacterSummary, type EditorResponse, type FieldDef, type LineDoc, type RelationDoc, type Section, type TellDoc, type Validation,
} from './model'

type Tab = 'basic' | 'appearance' | 'story' | 'psychology' | 'attributes' | 'behavior' | 'speech' | 'mechanics' | 'host' | 'notes'

const tabs: { id: Tab; label: string; hostOnly?: boolean }[] = [
  { id: 'basic', label: 'Basics' },
  { id: 'appearance', label: 'Appearance' },
  { id: 'story', label: 'Story' },
  { id: 'psychology', label: 'Psychology' },
  { id: 'attributes', label: 'Attributes' },
  { id: 'behavior', label: 'Behaviour & motives' },
  { id: 'speech', label: 'Speech & lines' },
  { id: 'mechanics', label: 'Tells & mechanics' },
  { id: 'host', label: 'Host', hostOnly: true },
  { id: 'notes', label: 'Notes' },
]

export default function CharacterEditor() {
  const { code } = useParams()
  const [search] = useSearchParams()
  const navigate = useNavigate()
  const isNew = !code

  const [doc, setDoc] = useState<CharacterDoc | null>(null)
  const [hasPortrait, setHasPortrait] = useState(false)
  const [portraitVersion, setPortraitVersion] = useState(0)
  const [validation, setValidation] = useState<Validation>({ errors: [], warnings: [] })
  const [skillBudget, setSkillBudget] = useState<number | null>(null)
  const [others, setOthers] = useState<CharacterSummary[]>([])
  const [tab, setTab] = useState<Tab>('basic')
  const [dirty, setDirty] = useState(false)
  const [saving, setSaving] = useState(false)
  const [message, setMessage] = useState<{ kind: 'ok' | 'error'; text: string } | null>(null)

  const applyResponse = useCallback((r: EditorResponse) => {
    setDoc(r.document)
    setValidation(r.validation)
    setHasPortrait(r.hasPortrait)
    setSkillBudget(r.skillBudget)
    setDirty(false)
  }, [])

  // ---- load ----
  useEffect(() => {
    setMessage(null)
    if (isNew) {
      const kind = (search.get('kind') as CharacterKind) ?? 'Participant'
      setDoc(emptyCharacter(kind, search.get('code') ?? (kind === 'Host' ? 'H99' : 'P99')))
      setHasPortrait(false)
      setDirty(true)
    } else {
      api<EditorResponse>(`/api/admin/characters/${code}`)
        .then(applyResponse)
        .catch((e) => setMessage({ kind: 'error', text: e.message }))
    }
    api<CharacterSummary[]>('/api/admin/characters').then(setOthers).catch(() => {})
  }, [code, isNew, search, applyResponse])

  // ---- live validation (debounced) ----
  useEffect(() => {
    if (!doc || !dirty) return
    const id = window.setTimeout(() => {
      api<{ validation: Validation; skillBudget: number | null }>('/api/admin/characters/validate', { method: 'POST', body: doc })
        .then((r) => {
          setValidation(r.validation)
          setSkillBudget(r.skillBudget)
        })
        .catch(() => {})
    }, 400)
    return () => window.clearTimeout(id)
  }, [doc, dirty])

  // ---- warn before leaving with unsaved changes ----
  useEffect(() => {
    if (!dirty) return
    const onBeforeUnload = (e: BeforeUnloadEvent) => e.preventDefault()
    window.addEventListener('beforeunload', onBeforeUnload)
    return () => window.removeEventListener('beforeunload', onBeforeUnload)
  }, [dirty])

  const update = useCallback((fn: (d: CharacterDoc) => void) => {
    setDoc((d) => {
      if (!d) return d
      const copy = structuredClone(d)
      fn(copy)
      return copy
    })
    setDirty(true)
  }, [])

  const save = useCallback(async () => {
    if (!doc || saving) return
    setSaving(true)
    setMessage(null)
    try {
      const r = isNew
        ? await api<EditorResponse>('/api/admin/characters', { method: 'POST', body: doc })
        : await api<EditorResponse>(`/api/admin/characters/${doc.code}`, { method: 'PUT', body: doc })
      applyResponse(r)
      setMessage({ kind: 'ok', text: 'Saved.' })
      if (isNew) navigate(`/admin/characters/${r.document.code}`, { replace: true })
    } catch (e) {
      setMessage({ kind: 'error', text: e instanceof Error ? e.message : 'Could not save.' })
    } finally {
      setSaving(false)
    }
  }, [doc, saving, isNew, applyResponse, navigate])

  // Ctrl+S
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
        e.preventDefault()
        save()
      }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [save])

  async function remove() {
    if (!doc || isNew) return
    if (!window.confirm(`Delete ${doc.code} · ${doc.name} from the database? This cannot be undone (the JSON file in content/ stays).`)) return
    try {
      await api(`/api/admin/characters/${doc.code}`, { method: 'DELETE' })
      setDirty(false)
      navigate('/admin')
    } catch (e) {
      setMessage({ kind: 'error', text: e instanceof Error ? e.message : 'Could not delete.' })
    }
  }

  async function exportJson() {
    if (!doc || isNew) return
    try {
      const r = await api<{ file: string }>(`/api/admin/characters/${doc.code}/export`, { method: 'POST' })
      setMessage({ kind: 'ok', text: `Written to ${r.file}` + (dirty ? ' (without your unsaved changes!)' : '') })
    } catch (e) {
      setMessage({ kind: 'error', text: e instanceof Error ? e.message : 'Could not export.' })
    }
  }

  async function onPortrait(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file || !doc) return
    try {
      await upload(`/api/admin/characters/${doc.code}/portrait`, file)
      setHasPortrait(true)
      setPortraitVersion(Date.now())
      setMessage({ kind: 'ok', text: 'Portrait uploaded.' })
    } catch (err) {
      setMessage({ kind: 'error', text: err instanceof Error ? err.message : 'Could not upload.' })
    }
  }

  async function removePortrait() {
    if (!doc) return
    await api(`/api/admin/characters/${doc.code}/portrait`, { method: 'DELETE' })
    setHasPortrait(false)
  }

  if (!doc)
    return (
      <div className="admin">
        {message ? <p className="form-error">{message.text}</p> : <p className="loading">Loading…</p>}
        <Link className="link" to="/admin">← Back to the list</Link>
      </div>
    )

  const isHost = doc.kind === 'Host'
  const skillSum = skillDefs.reduce((sum, s) => sum + doc.skills[s.key], 0)

  return (
    <div className="admin admin--editor">
      <header className="admin__bar">
        <Link className="link" to="/admin" onClick={(e) => dirty && !window.confirm('You have unsaved changes. Leave anyway?') && e.preventDefault()}>
          ← Characters
        </Link>
        <h1>
          <span className="admin__code">{doc.code}</span> {doc.name || 'New character'}
          {dirty && <span className="admin__dirty" title="Unsaved changes">●</span>}
        </h1>
        <div className="admin__actions">
          <label className="status-toggle">
            <input
              type="checkbox"
              checked={doc.status === 'Published'}
              onChange={(e) => update((d) => void (d.status = e.target.checked ? 'Published' : 'Draft'))}
            />
            <span>{doc.status === 'Published' ? 'Published' : 'Draft'}</span>
          </label>
          {!isNew && <button className="btn btn--small" onClick={exportJson}>Export to JSON</button>}
          {!isNew && <button className="btn btn--small btn--danger" onClick={remove}>Delete</button>}
          <button className="btn btn--small btn--primary" onClick={save} disabled={saving || validation.errors.length > 0}>
            {saving ? 'Saving…' : 'Save (Ctrl+S)'}
          </button>
        </div>
      </header>

      {message && <p className={message.kind === 'ok' ? 'admin__message admin__message--ok' : 'admin__message admin__message--error'}>{message.text}</p>}

      <div className="editor">
        <nav className="editor__tabs">
          {tabs.filter((t) => !t.hostOnly || isHost).map((t) => (
            <button key={t.id} className={tab === t.id ? 'editor__tab editor__tab--active' : 'editor__tab'} onClick={() => setTab(t.id)}>
              {t.label}
            </button>
          ))}
        </nav>

        <main className="editor__form">
          {tab === 'basic' && (
            <>
              <Group title="Identity">
                <div className="grid2">
                  <Field label="Code" value={doc.code} onChange={(v) => update((d) => void (d.code = v.toUpperCase()))} disabled={!isNew} hint={isNew ? 'P.. for a participant, H.. for a host. Cannot be changed after saving.' : undefined} />
                  <label className="field">
                    <span>Kind</span>
                    <select value={doc.kind} disabled={!isNew} onChange={(e) => update((d) => {
                      d.kind = e.target.value as CharacterKind
                      d.host = d.kind === 'Host' ? (d.host ?? { generosity: 3, cruelty: 3, verbosity: 3, capriciousness: 3, partiality: 3 }) : null
                    })}>
                      <option value="Participant">Table participant</option>
                      <option value="Host">Host</option>
                    </select>
                  </label>
                  <Field label="Name" value={doc.name} onChange={(v) => update((d) => void (d.name = v))} />
                  <Field label="Title (on the nameplate)" value={doc.title} onChange={(v) => update((d) => void (d.title = v))} hint="E.g. “The Haughty Lord”" />
                  <Field label="Nicknames" value={doc.nickname} onChange={(v) => update((d) => void (d.nickname = v))} />
                  <Field label="Archetype" value={doc.archetype} onChange={(v) => update((d) => void (d.archetype = v))} />
                  <Field label="Gender" value={doc.gender} onChange={(v) => update((d) => void (d.gender = v))} />
                  <Field label="Age" value={doc.age} onChange={(v) => update((d) => void (d.age = v))} />
                  <label className="field">
                    <span>Rarity</span>
                    <select value={doc.rarity} onChange={(e) => update((d) => void (d.rarity = e.target.value as CharacterDoc['rarity']))}>
                      <option value="Common">Common (budget 15)</option>
                      <option value="Uncommon">Uncommon (budget 17)</option>
                      <option value="Rare">Rare (budget 18)</option>
                      <option value="Unique">Unique (no budget)</option>
                    </select>
                  </label>
                  <Field
                    label="Tags"
                    value={doc.tags.join(', ')}
                    onChange={(v) => update((d) => void (d.tags = v.split(',').map((t) => t.trim()).filter(Boolean)))}
                    hint={`Comma-separated. E.g.: ${tagSuggestions.join(', ')}`}
                  />
                </div>
              </Group>
              <SectionFields section="origin" doc={doc} update={update} />
            </>
          )}

          {tab === 'appearance' && (
            <>
              <Group title="Portrait">
                <div className="portrait-editor">
                  <Portrait hue={Number(doc.appearance.portraitHue ?? 30)} host={isHost} code={doc.code} hasImage={hasPortrait} version={portraitVersion} />
                  <div className="portrait-editor__controls">
                    {isNew ? (
                      <p className="muted">Save the character before uploading a portrait.</p>
                    ) : (
                      <div className="row">
                        <label className="btn btn--small">
                          {hasPortrait ? 'Replace portrait' : 'Upload portrait'}
                          <input type="file" accept="image/png,image/jpeg,image/webp" hidden onChange={onPortrait} />
                        </label>
                        {hasPortrait && <button className="btn btn--small" onClick={removePortrait}>Remove</button>}
                      </div>
                    )}
                    <label className="field">
                      <span>Silhouette hue (until there is a portrait): {doc.appearance.portraitHue ?? 30}</span>
                      <input type="range" min={0} max={360} value={Number(doc.appearance.portraitHue ?? 30)} onChange={(e) => update((d) => void (d.appearance.portraitHue = Number(e.target.value)))} />
                    </label>
                    <p className="muted">PNG, JPEG or WebP up to 5 MB, portrait format (5:6). Stored in content/portraits/.</p>
                  </div>
                </div>
              </Group>
              <SectionFields section="appearance" doc={doc} update={update} />
            </>
          )}

          {tab === 'story' && <SectionFields section="backstory" doc={doc} update={update} />}
          {tab === 'psychology' && <SectionFields section="psychology" doc={doc} update={update} />}

          {tab === 'attributes' && (
            <>
              <Group title={`Skills (1–5) — total ${skillSum}${skillBudget ? ` / budget ${skillBudget}` : ''}`}>
                {skillBudget && (
                  <div className="budget">
                    <div className={`budget__bar ${skillSum === skillBudget ? 'budget__bar--ok' : skillSum > skillBudget ? 'budget__bar--over' : ''}`} style={{ width: `${Math.min(100, (skillSum / skillBudget) * 100)}%` }} />
                  </div>
                )}
                {skillDefs.map((s) => (
                  <Slider key={s.key} label={s.label} hint={s.hint} value={doc.skills[s.key]} onChange={(v) => update((d) => void (d.skills[s.key] = v))} />
                ))}
              </Group>
              <Group title="Drives (1–5, no budget — at least two extreme)">
                {driveDefs.map((s) => (
                  <Slider key={s.key} label={s.label} value={doc.drives[s.key]} onChange={(v) => update((d) => void (d.drives[s.key] = v))} />
                ))}
              </Group>
              {isHost && doc.host && (
                <Group title="Host attributes (1–5)">
                  {hostAttrDefs.map((s) => (
                    <Slider key={s.key} label={s.label} hint={'hint' in s ? s.hint : undefined} value={Number(doc.host![s.key] ?? 3)} onChange={(v) => update((d) => void (d.host![s.key] = v))} />
                  ))}
                </Group>
              )}
            </>
          )}

          {tab === 'behavior' && (
            <>
              <SectionFields section="behavior" doc={doc} update={update} />
              <SectionFields section="motives" doc={doc} update={update} />
            </>
          )}

          {tab === 'speech' && (
            <>
              <SectionFields section="speech" doc={doc} update={update} />
              <LinesEditor lines={doc.lines} isHost={isHost} update={update} />
            </>
          )}

          {tab === 'mechanics' && (
            <>
              <TellsEditor tells={doc.tells} update={update} />
              <SectionFields section="mechanics" doc={doc} update={update} />
              <RelationsEditor relations={doc.relations} self={doc.code} others={others} update={update} />
            </>
          )}

          {tab === 'host' && isHost && <SectionFields section="host" doc={doc} update={update} />}
          {tab === 'notes' && <SectionFields section="authorNotes" doc={doc} update={update} />}
        </main>

        <aside className="editor__side">
          <Preview doc={doc} hasPortrait={hasPortrait} portraitVersion={portraitVersion} />
          <div className="checks">
            <h3>Checks</h3>
            {validation.errors.length === 0 && validation.warnings.length === 0 && <p className="check check--ok">✓ All checks pass.</p>}
            {validation.errors.map((e, i) => <p key={`e${i}`} className="check check--error">✕ {e}</p>)}
            {validation.warnings.map((w, i) => <p key={`w${i}`} className="check check--warn">⚠ {w}</p>)}
          </div>
        </aside>
      </div>
    </div>
  )
}

/* ------------------------------------------------------------------ building blocks */

type Update = (fn: (d: CharacterDoc) => void) => void

function Group({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="group">
      <h2>{title}</h2>
      {children}
    </section>
  )
}

function Field({ label, value, onChange, long, hint, disabled }: { label: string; value: string; onChange: (v: string) => void; long?: boolean; hint?: string; disabled?: boolean }) {
  return (
    <label className="field">
      <span>{label}</span>
      {long ? (
        <textarea value={value} rows={3} onChange={(e) => onChange(e.target.value)} disabled={disabled} />
      ) : (
        <input value={value} onChange={(e) => onChange(e.target.value)} disabled={disabled} />
      )}
      {hint && <small>{hint}</small>}
    </label>
  )
}

function SectionFields({ section, doc, update }: { section: keyof typeof textSections; doc: CharacterDoc; update: Update }) {
  const def = textSections[section]
  const data = ((doc as unknown as Record<string, Section | null | undefined>)[section] ?? {}) as Section
  return (
    <Group title={def.title}>
      {def.fields.map((f: FieldDef) => (
        <Field
          key={f.key}
          label={f.label}
          long={f.long}
          hint={f.hint}
          value={String(data[f.key] ?? '')}
          onChange={(v) => update((d) => {
            const rec = d as unknown as Record<string, Section>
            rec[section] = { ...(rec[section] ?? {}), [f.key]: v }
          })}
        />
      ))}
    </Group>
  )
}

function Slider({ label, hint, value, onChange }: { label: string; hint?: string; value: number; onChange: (v: number) => void }) {
  return (
    <div className="slider">
      <span className="slider__label">
        {label} {hint && <small>— {hint}</small>}
      </span>
      <div className="slider__pips" role="radiogroup" aria-label={label}>
        {[1, 2, 3, 4, 5].map((n) => (
          <button
            key={n}
            role="radio"
            aria-checked={value === n}
            className={n <= value ? `pip pip--on${n === 1 || n === 5 ? ' pip--extreme' : ''}` : 'pip'}
            onClick={() => onChange(n)}
          >
            {n}
          </button>
        ))}
      </div>
    </div>
  )
}

function LinesEditor({ lines, isHost, update }: { lines: LineDoc[]; isHost: boolean; update: Update }) {
  const order = new Map(lineSituations.map((s, i) => [s, i]))
  const indexed = lines.map((l, i) => ({ l, i })).sort((a, b) => (order.get(a.l.situation) ?? 0) - (order.get(b.l.situation) ?? 0))
  const defaultSituation = isHost ? 'IntroReplyWho' : 'FirstMeeting'

  return (
    <Group title={`Lines (${lines.length})`}>
      <p className="muted">The arrival uses: {isHost ? 'First meeting, the three replies, Role deal, Met again' : 'Host introduction, First meeting, Met again, Death'}.</p>
      {indexed.map(({ l, i }) => (
        <div key={i} className="line-row">
          <select value={l.situation} onChange={(e) => update((d) => void (d.lines[i].situation = e.target.value as LineDoc['situation']))}>
            {lineSituations.map((s) => <option key={s} value={s}>{situationLabel[s]}</option>)}
          </select>
          <textarea rows={2} value={l.text} placeholder="The line…" onChange={(e) => update((d) => void (d.lines[i].text = e.target.value))} />
          <input value={l.direction ?? ''} placeholder="Stage direction (optional)" onChange={(e) => update((d) => void (d.lines[i].direction = e.target.value || null))} />
          <button className="icon-btn" title="Remove" onClick={() => update((d) => void d.lines.splice(i, 1))}>✕</button>
        </div>
      ))}
      <button className="btn btn--small" onClick={() => update((d) => void d.lines.push({ situation: defaultSituation, text: '' }))}>+ Line</button>
    </Group>
  )
}

function TellsEditor({ tells, update }: { tells: TellDoc[]; update: Update }) {
  return (
    <Group title={`Tells (${tells.length})`}>
      <p className="muted">A visible action tied to an inner state. The first visible tell is shown in the arrival; Lying tells can flash when the character lies in Gifts.</p>
      {tells.map((t, i) => (
        <div key={i} className="tell-row">
          <select value={t.state} onChange={(e) => update((d) => void (d.tells[i].state = e.target.value as TellDoc['state']))}>
            {tellStates.map((s) => <option key={s} value={s}>{tellStateLabel[s]}</option>)}
          </select>
          <input value={t.description} placeholder="E.g. “Spins the coin between his knuckles”" onChange={(e) => update((d) => void (d.tells[i].description = e.target.value))} />
          <label className="check-inline" title="Can show it falsely (high Cunning)">
            <input type="checkbox" checked={t.fakeable} onChange={(e) => update((d) => void (d.tells[i].fakeable = e.target.checked))} /> fakeable
          </label>
          <label className="check-inline" title="Visible from the first meeting, without the dossier">
            <input type="checkbox" checked={t.visibleWithoutDossier} onChange={(e) => update((d) => void (d.tells[i].visibleWithoutDossier = e.target.checked))} /> visible
          </label>
          <button className="icon-btn" title="Remove" onClick={() => update((d) => void d.tells.splice(i, 1))}>✕</button>
        </div>
      ))}
      <button className="btn btn--small" onClick={() => update((d) => void d.tells.push({ state: 'Habit', description: '', fakeable: false, visibleWithoutDossier: true }))}>+ Tell</button>
    </Group>
  )
}

function RelationsEditor({ relations, self, others, update }: { relations: RelationDoc[]; self: string; others: CharacterSummary[]; update: Update }) {
  const options = others.filter((o) => o.code !== self && o.kind === 'Participant')
  return (
    <Group title={`Special relations (${relations.length})`}>
      <p className="muted">Only relations with a story. Value from −100 (feud) to +100 (alliance).</p>
      {relations.map((r, i) => (
        <div key={i} className="relation-row">
          <select value={r.to} onChange={(e) => update((d) => void (d.relations[i].to = e.target.value))}>
            <option value="">— choose —</option>
            {options.map((o) => <option key={o.code} value={o.code}>{o.code} · {o.name}</option>)}
          </select>
          <input type="number" min={-100} max={100} value={r.value} onChange={(e) => update((d) => void (d.relations[i].value = Number(e.target.value)))} />
          <input value={r.story} placeholder="Story" onChange={(e) => update((d) => void (d.relations[i].story = e.target.value))} />
          <input value={r.effect} placeholder="Effect at the table" onChange={(e) => update((d) => void (d.relations[i].effect = e.target.value))} />
          <button className="icon-btn" title="Remove" onClick={() => update((d) => void d.relations.splice(i, 1))}>✕</button>
        </div>
      ))}
      <button className="btn btn--small" onClick={() => update((d) => void d.relations.push({ to: '', value: 0, story: '', effect: '' }))}>+ Relation</button>
    </Group>
  )
}

function Preview({ doc, hasPortrait, portraitVersion }: { doc: CharacterDoc; hasPortrait: boolean; portraitVersion: number }) {
  const line = (s: LineDoc['situation']) => doc.lines.find((l) => l.situation === s)
  const gesture = doc.tells.find((t) => t.visibleWithoutDossier)?.description
  const first = line('FirstMeeting')
  const hostIntro = line('HostIntroduction')

  return (
    <div className="preview">
      <h3>Preview — at the table</h3>
      <div className="preview__seat">
        <Portrait hue={Number(doc.appearance.portraitHue ?? 30)} host={doc.kind === 'Host'} code={doc.code} hasImage={hasPortrait} version={portraitVersion} />
        <div className="nameplate">
          <strong>{doc.name || '—'}</strong>
          <span>{doc.title || '—'}</span>
        </div>
      </div>
      {doc.kind === 'Participant' ? (
        <div className="preview__intro">
          {hostIntro && <p className="preview__host">The host: <q>{hostIntro.text}</q></p>}
          {first && <p><q>{first.text}</q>{first.direction && <small> ({first.direction})</small>}</p>}
          {gesture && <p className="gesture">*{gesture}*</p>}
        </div>
      ) : (
        <div className="preview__intro">{first && <p><q>{first.text}</q></p>}</div>
      )}
      {doc.mechanics.strangeness && <p className="preview__strange"><span>The Strange:</span> {String(doc.mechanics.strangeness)}</p>}
    </div>
  )
}
