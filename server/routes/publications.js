import { Router } from 'express'
import db from '../db.js'

const router = Router()

function toObj(row) {
  return { ...JSON.parse(row.json_data), id: Number(row.id) }
}

router.get('/', async (req, res) => {
  try {
    const [rows] = await db.query('SELECT id, json_data FROM publications ORDER BY id DESC')
    res.json(rows.map(toObj))
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

router.post('/', async (req, res) => {
  try {
    const { id, ...data } = req.body
    const newId = id || Date.now()
    await db.query(
      'INSERT INTO publications (id, json_data) VALUES (?, ?)',
      [newId, JSON.stringify(data)]
    )
    res.status(201).json({ id: Number(newId), ...data })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

router.put('/:id', async (req, res) => {
  try {
    const id = Number(req.params.id)
    const { id: _omit, ...data } = req.body
    await db.query(
      'UPDATE publications SET json_data = ? WHERE id = ?',
      [JSON.stringify(data), id]
    )
    res.json({ id, ...data })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

router.delete('/:id', async (req, res) => {
  try {
    await db.query('DELETE FROM publications WHERE id = ?', [Number(req.params.id)])
    res.json({ ok: true })
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

export default router
