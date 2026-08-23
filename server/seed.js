import db from './db.js'
import { publications } from '../src/data/publications.js'
import { journalIssues } from '../src/data/journal.js'
import { events } from '../src/data/events.js'

export async function seed(force = false) {
  if (!force) {
    const [[{ c }]] = await db.query('SELECT COUNT(*) as c FROM publications')
    if (c > 0) return
  }

  await db.query('DELETE FROM publications')
  await db.query('DELETE FROM journal_issues')
  await db.query('DELETE FROM events')

  for (const pub of publications) {
    const { id, ...data } = pub
    await db.query('INSERT INTO publications (id, json_data) VALUES (?, ?)',
      [id, JSON.stringify(data)])
  }

  for (const issue of journalIssues) {
    const { id, ...data } = issue
    await db.query('INSERT INTO journal_issues (id, json_data) VALUES (?, ?)',
      [id, JSON.stringify(data)])
  }

  for (const event of events) {
    const { id, ...data } = event
    await db.query('INSERT INTO events (id, json_data) VALUES (?, ?)',
      [id, JSON.stringify(data)])
  }

  console.log('✅ DB seeded from static data')
}

async function fetchAll() {
  const [pubs]    = await db.query('SELECT id, json_data FROM publications ORDER BY id DESC')
  const [journal] = await db.query('SELECT id, json_data FROM journal_issues ORDER BY id ASC')
  const [evts]    = await db.query('SELECT id, json_data FROM events ORDER BY id ASC')
  const parse = r => ({ ...JSON.parse(r.json_data), id: Number(r.id) })
  return {
    publications:  pubs.map(parse),
    journalIssues: journal.map(parse),
    events:        evts.map(parse)
  }
}

export { fetchAll }
