import { Router } from 'express'
import db from '../db.js'
import { secondaryTopics as defaults } from '../../src/data/topics.js'

const router = Router()

router.get('/', async (req, res) => {
  try {
    const [rows] = await db.query("SELECT v FROM settings WHERE k = 'secondary_topics'")
    res.json(rows.length ? JSON.parse(rows[0].v) : defaults)
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

router.put('/', async (req, res) => {
  try {
    const val = JSON.stringify(req.body)
    await db.query(
      "INSERT INTO settings (k,v) VALUES ('secondary_topics',?) ON DUPLICATE KEY UPDATE v=?",
      [val, val]
    )
    res.json(req.body)
  } catch (e) {
    res.status(500).json({ error: e.message })
  }
})

export default router
